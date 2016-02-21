
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
