var LightNode;
(function (LightNode) {
    "use strict";
    angular.module("app", ["lightNode"])
        .constant("rootEndPoint", "http://localhost:24028/")
        .config(["lightNodeClientProvider", "rootEndPoint",
        function (lightNodeClientProvider, rootEndPoint) {
            lightNodeClientProvider.rootEndPoint = rootEndPoint;
            lightNodeClientProvider.timeout = 10 * 1000;
        }])
        .directive("main", [function () {
            return {
                controller: ["$scope", "lightNodeClient", MainController],
                controllerAs: "main"
            };
        }]);
    var MainController = (function () {
        function MainController($scope, lightNodeClient) {
            this.lightNodeClient = lightNodeClient;
            this.logs = [];
        }
        MainController.prototype.echo = function () {
            var _this = this;
            if (this.cancellationTokenSource) {
                this.cancellationTokenSource.cancel();
            }
            this.cancellationTokenSource = this.lightNodeClient.createCancellationTokenSource();
            this.lightNodeClient.perf.echo("Hello", 1, 2, LightNode.MyEnum.A, this.cancellationTokenSource.token)
                .then(function (x) {
                _this.logs.unshift(x);
                console.log(x);
            })
                .catch(function (x) {
                _this.logs.unshift(x);
                console.log(x);
            });
        };
        return MainController;
    })();
    LightNode.MainController = MainController;
})(LightNode || (LightNode = {}));
//# sourceMappingURL=app.js.map