var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
/* tslint:disable */
var LightNode;
(function (LightNode) {
    "use strict";
    //#region LightNode
    angular.module("lightNode", [])
        .provider("lightNodeClientHandler", [function () {
            this.defaultRequestHeaders = true;
            this.lightNodeClientHandlerFactory = function ($http) {
                var handler = new LightNodeClientHandler($http);
                return handler;
            };
            this.$get = ["$http", this.lightNodeClientHandlerFactory];
        }])
        .provider("lightNodeClient", [function () {
            this.lightNodeClientFactory =
                function ($q, rootEndPoint, lightNodeClientHandler) {
                    var _this = this;
                    var client = new LightNodeClient($q, rootEndPoint, lightNodeClientHandler);
                    client.timeout = this.timeout;
                    Object.keys(this.defaultRequestHeaders || {}).forEach(function (x) {
                        client.defaultRequestHeaders[x] = _this.defaultRequestHeaders[x];
                    });
                    return client;
                };
            this.$get = [
                "$q", "lightNodeClientHandler",
                function ($q, lightNodeClientHandler) {
                    return this.lightNodeClientFactory($q, this.rootEndPoint, lightNodeClientHandler);
                }];
        }]);
    var ArgumentError = (function (_super) {
        __extends(ArgumentError, _super);
        function ArgumentError(message, paramName) {
            if (message === void 0) { message = "Value does not fall within the expected range."; }
            _super.call(this);
            this.name = "ArgumentError";
            this.message = message + (paramName ? ": " + paramName : "");
        }
        return ArgumentError;
    })(Error);
    LightNode.ArgumentError = ArgumentError;
    var ArgumentNullError = (function (_super) {
        __extends(ArgumentNullError, _super);
        function ArgumentNullError(paramName, message) {
            if (message === void 0) { message = "Value cannot be null."; }
            _super.call(this, message, paramName);
            this.name = "ArgumentNullError";
        }
        return ArgumentNullError;
    })(ArgumentError);
    LightNode.ArgumentNullError = ArgumentNullError;
    var ArgumentOutOfRangeError = (function (_super) {
        __extends(ArgumentOutOfRangeError, _super);
        function ArgumentOutOfRangeError(paramName, message) {
            if (message === void 0) { message = "Specified argument was out of the range of valid values."; }
            _super.call(this, message, paramName);
            this.name = "ArgumentOutOfRangeError";
        }
        return ArgumentOutOfRangeError;
    })(ArgumentError);
    LightNode.ArgumentOutOfRangeError = ArgumentOutOfRangeError;
    var LightNodeClientHandler = (function () {
        function LightNodeClientHandler($http) {
            this.$http = $http;
            if (!$http) {
                throw new ArgumentNullError("$http");
            }
        }
        LightNodeClientHandler.prototype.serializeToFormData = function (data) {
            return Object.keys(data || {})
                .map(function (x) {
                var value = data[x];
                if (value === void 0 || value === null) {
                    value = "";
                }
                return encodeURIComponent(x) + "=" + encodeURIComponent(value);
            })
                .join("&");
        };
        LightNodeClientHandler.prototype.post = function (url, data, timeout, requestHeaders, transformResponse) {
            if (!url) {
                throw new ArgumentNullError("url");
            }
            var config = {
                timeout: timeout,
                headers: requestHeaders,
                responseType: "json",
                transformResponse: transformResponse
            };
            return this.$http.post(url, this.serializeToFormData(data), config);
        };
        return LightNodeClientHandler;
    })();
    LightNode.LightNodeClientHandler = LightNodeClientHandler;
    var CancellationTokenSource = (function () {
        function CancellationTokenSource(deferred) {
            this.deferred = deferred;
            if (!deferred) {
                throw new ArgumentNullError("deferred");
            }
        }
        Object.defineProperty(CancellationTokenSource.prototype, "token", {
            get: function () {
                return this.deferred.promise;
            },
            enumerable: true,
            configurable: true
        });
        CancellationTokenSource.prototype.cancel = function () {
            this.deferred.resolve();
        };
        return CancellationTokenSource;
    })();
    LightNode.CancellationTokenSource = CancellationTokenSource;
    var LightNodeClientBase = (function () {
        function LightNodeClientBase($q, rootEndPoint, innerHandler) {
            this.$q = $q;
            this.rootEndPoint = rootEndPoint;
            this.innerHandler = innerHandler;
            if (!$q) {
                throw new ArgumentNullError("$q");
            }
            if (!rootEndPoint) {
                throw new ArgumentNullError("rootEndPoint");
            }
            if (!innerHandler) {
                throw new ArgumentNullError("innerHandler");
            }
            this.$q = $q;
            this.rootEndPoint = rootEndPoint.replace(/\/$/, "");
            if (!this.rootEndPoint) {
                throw new ArgumentOutOfRangeError("rootEndPoint");
            }
            this.innerHandler = innerHandler;
            this._defaultRequestHeaders = {
                "Content-Type": "application/x-www-form-urlencoded",
                "X-Requested-With": "XMLHttpRequest"
            };
        }
        Object.defineProperty(LightNodeClientBase.prototype, "timeout", {
            get: function () {
                return this._timeout;
            },
            set: function (value) {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeError();
                }
                this._timeout = value;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(LightNodeClientBase.prototype, "defaultRequestHeaders", {
            get: function () {
                return this._defaultRequestHeaders;
            },
            enumerable: true,
            configurable: true
        });
        ;
        LightNodeClientBase.prototype.post = function (method, data, cancellationToken, transformResponse) {
            if (!method) {
                throw new ArgumentNullError("method");
            }
            return this.innerHandler.post(this.rootEndPoint + method, data, cancellationToken || this.timeout, this.defaultRequestHeaders, transformResponse)
                .then(function (x) { return x.data; });
        };
        LightNodeClientBase.prototype.createCancellationTokenSource = function () {
            return new CancellationTokenSource(this.$q.defer());
        };
        ;
        LightNodeClientBase.prototype.validStatusCode = function (status) {
            return 200 <= status && status <= 299;
        };
        LightNodeClientBase.prototype.parseJSON = function (json) {
            return typeof json === "string"
                ? JSON.parse(json)
                : json;
        };
        return LightNodeClientBase;
    })();
    LightNode.LightNodeClientBase = LightNodeClientBase;
    //#endregion
    (function (MyEnum) {
        MyEnum[MyEnum["A"] = 2] = "A";
        MyEnum[MyEnum["B"] = 3] = "B";
        MyEnum[MyEnum["C"] = 4] = "C";
    })(LightNode.MyEnum || (LightNode.MyEnum = {}));
    var MyEnum = LightNode.MyEnum;
    (function (MyEnum2) {
        MyEnum2[MyEnum2["A"] = 100] = "A";
        MyEnum2[MyEnum2["B"] = 3000] = "B";
        MyEnum2[MyEnum2["C"] = 50000] = "C";
    })(LightNode.MyEnum2 || (LightNode.MyEnum2 = {}));
    var MyEnum2 = LightNode.MyEnum2;
    var MyClass = (function () {
        function MyClass(json) {
            this.name = json.Name;
            this.sum = json.Sum;
        }
        return MyClass;
    })();
    LightNode.MyClass = MyClass;
    var LightNodeClient = (function (_super) {
        __extends(LightNodeClient, _super);
        function LightNodeClient($q, rootEndPoint, innerHandler) {
            _super.call(this, $q, rootEndPoint, innerHandler);
        }
        Object.defineProperty(LightNodeClient.prototype, "perf", {
            get: function () {
                if (!this._perf) {
                    this._perf = {
                        echo: this.perfEcho.bind(this),
                        test: this.perfTest.bind(this),
                        te: this.perfTe.bind(this),
                        testArray: this.perfTestArray.bind(this),
                        teVoid: this.perfTeVoid.bind(this),
                        te4: this.perfTe4.bind(this),
                        postString: this.perfPostString.bind(this)
                    };
                }
                return this._perf;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(LightNodeClient.prototype, "debugOnlyTest", {
            get: function () {
                if (!this._debugOnlyTest) {
                    this._debugOnlyTest = {
                        hoge: this.debugOnlyTestHoge.bind(this)
                    };
                }
                return this._debugOnlyTest;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(LightNodeClient.prototype, "debugOnlyMethodTest", {
            get: function () {
                if (!this._debugOnlyMethodTest) {
                    this._debugOnlyMethodTest = {
                        hoge: this.debugOnlyMethodTestHoge.bind(this)
                    };
                }
                return this._debugOnlyMethodTest;
            },
            enumerable: true,
            configurable: true
        });
        LightNodeClient.prototype.perfEcho = function (name, x, y, e, cancellationToken) {
            var _this = this;
            var data = {
                "name": name,
                "x": x,
                "y": y,
                "e": e
            };
            return this.post("/Perf/Echo", data, cancellationToken, function (data, headersGetter, status) {
                if (!_this.validStatusCode(status)) {
                    return data;
                }
                var json = _this.parseJSON(data);
                return json ? new MyClass(json) : null;
            });
        };
        LightNodeClient.prototype.perfTest = function (a, x, z, cancellationToken) {
            var _this = this;
            var data = {
                "a": a,
                "x": x,
                "z": z
            };
            return this.post("/Perf/Test", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        LightNodeClient.prototype.perfTe = function (cancellationToken) {
            var _this = this;
            var data = {};
            return this.post("/Perf/Te", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        LightNodeClient.prototype.perfTestArray = function (array, array2, array3, cancellationToken) {
            var _this = this;
            var data = {
                "array": array,
                "array2": array2,
                "array3": array3
            };
            return this.post("/Perf/TestArray", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        LightNodeClient.prototype.perfTeVoid = function (cancellationToken) {
            var _this = this;
            var data = {};
            return this.post("/Perf/TeVoid", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        LightNodeClient.prototype.perfTe4 = function (xs, cancellationToken) {
            var _this = this;
            var data = {
                "xs": xs
            };
            return this.post("/Perf/Te4", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        LightNodeClient.prototype.perfPostString = function (hoge, cancellationToken) {
            var _this = this;
            var data = {
                "hoge": hoge
            };
            return this.post("/Perf/PostString", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        LightNodeClient.prototype.debugOnlyTestHoge = function (cancellationToken) {
            var _this = this;
            var data = {};
            return this.post("/DebugOnlyTest/Hoge", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        LightNodeClient.prototype.debugOnlyMethodTestHoge = function (cancellationToken) {
            var _this = this;
            var data = {};
            return this.post("/DebugOnlyMethodTest/Hoge", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        return LightNodeClient;
    })(LightNodeClientBase);
    LightNode.LightNodeClient = LightNodeClient;
})(LightNode || (LightNode = {}));
/* tslint:enable */
//# sourceMappingURL=light-node-client.js.map