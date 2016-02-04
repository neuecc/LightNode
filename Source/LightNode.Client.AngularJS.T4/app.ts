
namespace LightNode {
    "use strict";

    angular.module("app", ["lightNode"])
        .constant("rootEndPoint", "http://localhost:24028/")
        .config(
        ["lightNodeClientProvider", "rootEndPoint",
            (lightNodeClientProvider: LightNode.ILightNodeClientProvider, rootEndPoint: string) => {
                lightNodeClientProvider.rootEndPoint = rootEndPoint;
                lightNodeClientProvider.timeout = 10 * 1000;
            }])
        .directive(
        "main",
        [() => {
            return {
                controller: ["$scope", "lightNodeClient", MainController],
                controllerAs: "main"
            };
        }]);

    export class MainController {

        constructor($scope: ng.IScope, private lightNodeClient: LightNode.LightNodeClient) { }

        public logs: any[] = [];

        private cancellationTokenSource: LightNode.CancellationTokenSource;

        public echo() {

            if (this.cancellationTokenSource) {
                this.cancellationTokenSource.cancel();
            }

            this.cancellationTokenSource = this.lightNodeClient.createCancellationTokenSource();

            this.lightNodeClient.perf.echo("Hello", 1, 2, MyEnum.A, this.cancellationTokenSource.token)
                .then(
                (x: LightNode.MyClass) => {
                    this.logs.unshift(x);
                    console.log(x);
                })
                .catch((x: any) => {
                    this.logs.unshift(x);
                    console.log(x);
                });
        }

    }

}
