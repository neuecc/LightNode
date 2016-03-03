
namespace LightNode {
    "use strict";

    angular.module("app", ["lightNode"])
        .constant("rootEndPoint", "http://localhost:12345/api")
        .config(
        ["lightNodeClientProvider", "rootEndPoint",
            (lightNodeClientProvider: LightNode.ILightNodeClientProvider, rootEndPoint: string) => {
                lightNodeClientProvider.rootEndPoint = rootEndPoint;
                lightNodeClientProvider.timeout = 2 * 1000;
            }]);

    class MainController {

        constructor(private mainService: MainService) { }

        public get isBoolTest() {
            return this.mainService.isBoolTest;
        }
        public set isBoolTest(value: boolean) {
            this.mainService.isBoolTest = value;
        }

        public get logs() {
            return this.mainService.logs;
        }

        public voidMethod() {
            this.mainService.voidMethod();
        }

        public download() {
            this.mainService.download();
        }

        public random() {
            this.mainService.random();
        }

        public city() {
            this.mainService.city();
        }

    }

    class MainService {

        constructor(private lightNodeClient: LightNode.LightNodeClient) { }

        public isBoolTest: boolean;

        public logs: any[] = [];

        private cancellationTokenSource: LightNode.CancellationTokenSource;

        public voidMethod() {

            this.lightNodeClient.foo.voidMethod()
                .then((x: void) => this.logs.unshift(x));

        }

        public download() {

            this.lightNodeClient.foo.echoByte("FooBar")
                .then((x: ArrayBuffer) => {

                    let blob = new Blob([x], { type: "text/plain" });

                    if (navigator.msSaveBlob) {
                        navigator.msSaveOrOpenBlob(blob, "FooBar.txt");
                    } else {
                        let reader = new FileReader();
                        let link = document.createElement("a");
                        let click = (element: HTMLElement) => {
                            let evt = document.createEvent("MouseEvents");
                            evt.initMouseEvent("click", true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
                            element.dispatchEvent(evt);
                        };
                        reader.onload = () => {
                            (<any>link).download = "FooBar.txt";
                            link.href = reader.result;
                            link.target = "_blank";
                            click(link);
                            link = null;
                        };
                        reader.readAsDataURL(blob);
                    }

                });

        }

        public random() {

            if (this.cancellationTokenSource) {
                this.cancellationTokenSource.cancel();
            }

            this.cancellationTokenSource = this.lightNodeClient.createCancellationTokenSource();

            this.lightNodeClient.member.random(1, this.cancellationTokenSource.token)
                .then(
                (x: LightNode.Person) => {
                    this.logs.unshift(x);
                    console.log(x);
                })
                .catch((x: any) => {
                    this.logs.unshift(x);
                    console.log(x);
                });

        }

        public city() {

            // isBoolTest property has undefined when not $durty.
            // but, lightNodeClient will send boolean.
            // if send undefined, recieve internal server error.

            this.lightNodeClient.member.city(this.isBoolTest)
                .then(
                (x: LightNode.City) => {
                    this.logs.unshift(x);
                    console.log(x);
                })
                .catch((x: any) => {
                    this.logs.unshift(x);
                    console.log(x);
                });

        }

    }

    angular.module("app")
        .service("mainService", MainService)
        .directive(
        "main",
        [() => {
            return {
                controller: ["mainService", MainController],
                controllerAs: "main"
            };
        }]);

}
