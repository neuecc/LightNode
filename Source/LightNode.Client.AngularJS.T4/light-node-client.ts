
/* tslint:disable */
namespace LightNode {
    "use strict";

    //#region LightNode

    angular.module("lightNode", [])
        .provider("lightNodeClientHandler", [function () {

            this.defaultRequestHeaders = true;

            this.lightNodeClientHandlerFactory = function ($http: ng.IHttpService) {

                let handler = new LightNodeClientHandler($http);

                return handler;

            };

            this.$get = ["$http", this.lightNodeClientHandlerFactory];

        }])
        .provider("lightNodeClient", [function () {

            this.lightNodeClientFactory =
                function ($q: ng.IQService, rootEndPoint: string, lightNodeClientHandler: LightNodeClientHandler) {

                    let client = new LightNodeClient($q, rootEndPoint, lightNodeClientHandler);

                    client.timeout = this.timeout;

                    Object.keys(this.defaultRequestHeaders || {}).forEach(
                        (x: string) => {
                            client.defaultRequestHeaders[x] = this.defaultRequestHeaders[x];
                        });

                    return client;

                };

            this.$get = [
                "$q", "lightNodeClientHandler",
                function ($q: ng.IQService, lightNodeClientHandler: LightNodeClientHandler) {
                    return this.lightNodeClientFactory($q, this.rootEndPoint, lightNodeClientHandler);
                }];

        }]);

    export interface ILightNodeClientHandlerProvider {
        lightNodeClientHandlerFactory: ($http: ng.IHttpService) => LightNodeClientHandler;
    }

    export interface ILightNodeClientProvider {
        rootEndPoint: string;
        defaultRequestHeaders: { [key: string]: string };
        timeout: number;
        lightNodeClientFactory: ($q: ng.IQService, rootEndPoint: string, $http: ng.IHttpService) => LightNodeClientBase;
    }

    export interface IHttpResponseTransformer<T> {
        (data: any, headersGetter: ng.IHttpHeadersGetter, status: number): T;
    }

    export class ArgumentError extends Error {
        constructor(message: string = "Value does not fall within the expected range.", paramName?: string) {
            super();
            this.name = "ArgumentError";
            this.message = message + (paramName ? ": " + paramName : "");
        }
    }

    export class ArgumentNullError extends ArgumentError {
        constructor(paramName?: string, message: string = "Value cannot be null.") {
            super(message, paramName);
            this.name = "ArgumentNullError";
        }
    }

    export class ArgumentOutOfRangeError extends ArgumentError {
        constructor(paramName?: string, message: string = "Specified argument was out of the range of valid values.") {
            super(message, paramName);
            this.name = "ArgumentOutOfRangeError";
        }
    }

    export class LightNodeClientHandler {

        constructor(private $http: ng.IHttpService) {
            if (!$http) {
                throw new ArgumentNullError("$http");
            }
        }

        protected serializeToFormData(data: any): string {

            return Object.keys(data || {})
                .map((x: string) => {
                    let value = data[x];
                    if (value === void 0 || value === null) {
                        value = "";
                    }
                    return encodeURIComponent(x) + "=" + encodeURIComponent(value);
                })
                .join("&");

        }

        public post<T>(
            url: string,
            data?: any,
            timeout?: number | ng.IPromise<any>,
            requestHeaders?: ng.IHttpRequestConfigHeaders,
            transformResponse?: IHttpResponseTransformer<T>): ng.IPromise<T> {

            if (!url) {
                throw new ArgumentNullError("url");
            }

            let config = <ng.IRequestShortcutConfig>{
                timeout: timeout,
                headers: requestHeaders,
                responseType: "json",
                transformResponse: transformResponse
            };

            return this.$http.post(url, this.serializeToFormData(data), config);

        }
    }

    export class CancellationTokenSource {

        constructor(private deferred: ng.IDeferred<any>) {
            if (!deferred) {
                throw new ArgumentNullError("deferred");
            }
        }

        public get token(): ng.IPromise<any> {
            return this.deferred.promise;
        }

        public cancel(): void {
            this.deferred.resolve();
        }

    }

    export class LightNodeClientBase {

        constructor(private $q: ng.IQService, private rootEndPoint: string, private innerHandler: LightNodeClientHandler) {

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

        private _timeout: number;
        public get timeout(): number {
            return this._timeout;
        }
        public set timeout(value: number) {
            if (value <= 0) {
                throw new ArgumentOutOfRangeError();
            }
            this._timeout = value;
        }

        private _defaultRequestHeaders: { [key: string]: string };
        public get defaultRequestHeaders(): { [key: string]: string } {
            return this._defaultRequestHeaders;
        };

        protected post<T>(
            method: string,
            data?: any,
            cancellationToken?: ng.IPromise<any>,
            transformResponse?: IHttpResponseTransformer<T>): ng.IPromise<T> {

            if (!method) {
                throw new ArgumentNullError("method");
            }

            return this.innerHandler.post(
                this.rootEndPoint + method,
                data,
                cancellationToken || this.timeout,
                this.defaultRequestHeaders,
                transformResponse)
                .then((x: any) => x.data);

        }

        public createCancellationTokenSource(): CancellationTokenSource {

            return new CancellationTokenSource(this.$q.defer<any>());

        };

        protected validStatusCode(status: number): boolean {

            return 200 <= status && status <= 299;

        }

        protected parseJSON(json: any): any {
            return typeof json === "string"
                ? JSON.parse(json)
                : json;
        }

    }

    //#endregion

    export enum MyEnum {
        A = 2,
        B = 3,
        C = 4
    }

    export enum MyEnum2 {
        A = 100,
        B = 3000,
        C = 50000
    }

    export class MyClass {

        constructor(json?: any) {
            this.name = json.Name;
            this.sum = json.Sum;
        }

        public name: string;
        public sum: number;
    }

    export interface _IPerf {
        echo(name: string, x: number, y: number, e: MyEnum, cancellationToken?: ng.IPromise<any>): ng.IPromise<MyClass>;
        test(a?: string, x?: number, z?: MyEnum2, cancellationToken?: ng.IPromise<any>): ng.IPromise<any>;
        te(cancellationToken?: ng.IPromise<any>): ng.IPromise<any>;
        testArray(array: string[], array2: number[], array3: MyEnum[], cancellationToken?: ng.IPromise<any>): ng.IPromise<any>;
        teVoid(cancellationToken?: ng.IPromise<any>): ng.IPromise<any>;
        te4(xs: string, cancellationToken?: ng.IPromise<any>): ng.IPromise<string>;
        postString(hoge: string, cancellationToken?: ng.IPromise<any>): ng.IPromise<string>;
    }

    export interface _IDebugOnlyTest {
        hoge(cancellationToken?: ng.IPromise<any>): ng.IPromise<any>;
    }

    export interface _IDebugOnlyMethodTest {
        hoge(cancellationToken?: ng.IPromise<any>): ng.IPromise<any>;
    }

    export class LightNodeClient extends LightNodeClientBase {

        constructor($q: ng.IQService, rootEndPoint: string, innerHandler: LightNodeClientHandler) {
            super($q, rootEndPoint, innerHandler);
        }

        private _perf: _IPerf;
        public get perf(): _IPerf {
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
        }

        private _debugOnlyTest: _IDebugOnlyTest;
        public get debugOnlyTest(): _IDebugOnlyTest {
            if (!this._debugOnlyTest) {
                this._debugOnlyTest = {
                    hoge: this.debugOnlyTestHoge.bind(this)
                };
            }
            return this._debugOnlyTest;
        }

        private _debugOnlyMethodTest: _IDebugOnlyMethodTest;
        public get debugOnlyMethodTest(): _IDebugOnlyMethodTest {
            if (!this._debugOnlyMethodTest) {
                this._debugOnlyMethodTest = {
                    hoge: this.debugOnlyMethodTestHoge.bind(this)
                };
            }
            return this._debugOnlyMethodTest;
        }

        protected perfEcho(name: string, x: number, y: number, e: MyEnum, cancellationToken?: ng.IPromise<any>): ng.IPromise<MyClass> {

            var data = {
                "name": name,
                "x": x,
                "y": y,
                "e": e
            };

            return this.post<MyClass>("/Perf/Echo", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    if (!this.validStatusCode(status)) {
                        return data;
                    }
                    let json = this.parseJSON(data);
                    return json ? new MyClass(json) : null;
                });

        }

        protected perfTest(a?: string, x?: number, z?: MyEnum2, cancellationToken?: ng.IPromise<any>): ng.IPromise<any> {

            var data = {
                "a": a,
                "x": x,
                "z": z
            };

            return this.post<any>("/Perf/Test", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

        protected perfTe(cancellationToken?: ng.IPromise<any>): ng.IPromise<any> {

            var data = {};

            return this.post<any>("/Perf/Te", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

        protected perfTestArray(array: string[], array2: number[], array3: MyEnum[], cancellationToken?: ng.IPromise<any>): ng.IPromise<any> {

            var data = {
                "array": array,
                "array2": array2,
                "array3": array3
            };

            return this.post<any>("/Perf/TestArray", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

        protected perfTeVoid(cancellationToken?: ng.IPromise<any>): ng.IPromise<any> {

            var data = {};

            return this.post<any>("/Perf/TeVoid", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

        protected perfTe4(xs: string, cancellationToken?: ng.IPromise<any>): ng.IPromise<string> {

            var data = {
                "xs": xs
            };

            return this.post<string>("/Perf/Te4", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

        protected perfPostString(hoge: string, cancellationToken?: ng.IPromise<any>): ng.IPromise<string> {

            var data = {
                "hoge": hoge
            };

            return this.post<string>("/Perf/PostString", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

        protected debugOnlyTestHoge(cancellationToken?: ng.IPromise<any>): ng.IPromise<any> {

            var data = {};

            return this.post<any>("/DebugOnlyTest/Hoge", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

        protected debugOnlyMethodTestHoge(cancellationToken?: ng.IPromise<any>): ng.IPromise<any> {

            var data = {};

            return this.post<any>("/DebugOnlyMethodTest/Hoge", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

    }

}
/* tslint:enable */
