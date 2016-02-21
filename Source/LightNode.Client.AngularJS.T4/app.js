var LightNode;
(function (LightNode) {
    "use strict";
    angular.module("app", ["lightNode"])
        .constant("rootEndPoint", "http://localhost:12345/api")
        .config(["lightNodeClientProvider", "rootEndPoint",
        function (lightNodeClientProvider, rootEndPoint) {
            lightNodeClientProvider.rootEndPoint = rootEndPoint;
            lightNodeClientProvider.timeout = 2 * 1000;
        }]);
    var MainController = (function () {
        function MainController(mainService) {
            this.mainService = mainService;
        }
        Object.defineProperty(MainController.prototype, "isBoolTest", {
            get: function () {
                return this.mainService.isBoolTest;
            },
            set: function (value) {
                this.mainService.isBoolTest = value;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(MainController.prototype, "logs", {
            get: function () {
                return this.mainService.logs;
            },
            enumerable: true,
            configurable: true
        });
        MainController.prototype.random = function () {
            this.mainService.random();
        };
        MainController.prototype.city = function () {
            this.mainService.city();
        };
        return MainController;
    })();
    var MainService = (function () {
        function MainService(lightNodeClient) {
            this.lightNodeClient = lightNodeClient;
            this.logs = [];
        }
        MainService.prototype.random = function () {
            var _this = this;
            if (this.cancellationTokenSource) {
                this.cancellationTokenSource.cancel();
            }
            this.cancellationTokenSource = this.lightNodeClient.createCancellationTokenSource();
            this.lightNodeClient.member.random(1, this.cancellationTokenSource.token)
                .then(function (x) {
                _this.logs.unshift(x);
                console.log(x);
            })
                .catch(function (x) {
                _this.logs.unshift(x);
                console.log(x);
            });
        };
        MainService.prototype.city = function () {
            // isBoolTest property has undefined when not $durty.
            // but, lightNodeClient will send boolean.
            // if send undefined, recieve internal server error.
            var _this = this;
            this.lightNodeClient.member.city(this.isBoolTest)
                .then(function (x) {
                _this.logs.unshift(x);
                console.log(x);
            })
                .catch(function (x) {
                _this.logs.unshift(x);
                console.log(x);
            });
        };
        return MainService;
    })();
    angular.module("app")
        .service("mainService", MainService)
        .directive("main", [function () {
            return {
                controller: ["mainService", MainController],
                controllerAs: "main"
            };
        }]);
})(LightNode || (LightNode = {}));
//# sourceMappingURL=app.js.map