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
    (function (Gender) {
        Gender[Gender["Male"] = 0] = "Male";
        Gender[Gender["Female"] = 1] = "Female";
    })(LightNode.Gender || (LightNode.Gender = {}));
    var Gender = LightNode.Gender;
    (function (Test) {
        Test[Test["Foo"] = 0] = "Foo";
        Test[Test["Bar"] = 1] = "Bar";
    })(LightNode.Test || (LightNode.Test = {}));
    var Test = LightNode.Test;
    var NoReferenceClass = (function () {
        function NoReferenceClass(json) {
            this.foo = json.Foo;
        }
        return NoReferenceClass;
    })();
    LightNode.NoReferenceClass = NoReferenceClass;
    var Person = (function () {
        function Person(json) {
            this.age = json.Age;
            this.birthDay = json.BirthDay ? new Date(json.BirthDay) : null;
            this.gender = json.Gender;
            this.firstName = json.FirstName;
            this.lastName = json.LastName;
        }
        return Person;
    })();
    LightNode.Person = Person;
    var City = (function () {
        function City(json) {
            this.name = json.Name;
            this.people = (json.People || []).map(function (x) { return x ? new Person(x) : null; });
        }
        return City;
    })();
    LightNode.City = City;
    var LightNodeClient = (function (_super) {
        __extends(LightNodeClient, _super);
        function LightNodeClient($q, rootEndPoint, innerHandler) {
            _super.call(this, $q, rootEndPoint, innerHandler);
        }
        Object.defineProperty(LightNodeClient.prototype, "member", {
            get: function () {
                if (!this._member) {
                    this._member = {
                        random: this.memberRandom.bind(this),
                        city: this.memberCity.bind(this),
                        echo: this.memberEcho.bind(this)
                    };
                }
                return this._member;
            },
            enumerable: true,
            configurable: true
        });
        LightNodeClient.prototype.memberRandom = function (seed, cancellationToken) {
            var _this = this;
            var data = {
                "seed": seed
            };
            return this.post("/Member/Random", data, cancellationToken, function (data, headersGetter, status) {
                if (!_this.validStatusCode(status)) {
                    return data;
                }
                var json = _this.parseJSON(data);
                return json ? new Person(json) : null;
            });
        };
        LightNodeClient.prototype.memberCity = function (isBoolTest, cancellationToken) {
            var _this = this;
            var data = {
                "isBoolTest": !!isBoolTest
            };
            return this.post("/Member/City", data, cancellationToken, function (data, headersGetter, status) {
                if (!_this.validStatusCode(status)) {
                    return data;
                }
                var json = _this.parseJSON(data);
                return json ? new City(json) : null;
            });
        };
        LightNodeClient.prototype.memberEcho = function (test, cancellationToken) {
            var _this = this;
            var data = {
                "test": test
            };
            return this.post("/Member/Echo", data, cancellationToken, function (data, headersGetter, status) {
                return _this.parseJSON(data);
            });
        };
        return LightNodeClient;
    })(LightNodeClientBase);
    LightNode.LightNodeClient = LightNodeClient;
})(LightNode || (LightNode = {}));
/* tslint:enable */
//# sourceMappingURL=light-node-client.js.map