
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
            transformResponse?: IHttpResponseTransformer<T>,
            responseType?: string): ng.IPromise<T> {

            if (!url) {
                throw new ArgumentNullError("url");
            }

            let config = <ng.IRequestShortcutConfig>{
                timeout: timeout,
                headers: requestHeaders,
                responseType: responseType || "json",
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
            transformResponse?: IHttpResponseTransformer<T>,
            responseType?: string): ng.IPromise<T> {

            if (!method) {
                throw new ArgumentNullError("method");
            }

            return this.innerHandler.post(
                this.rootEndPoint + method,
                data,
                cancellationToken || this.timeout,
                this.defaultRequestHeaders,
                transformResponse,
                responseType)
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

    export enum Gender {
        Male = 0,
        Female = 1
    }

    export enum Test {
        Foo = 0,
        Bar = 1
    }

    export class NoReferenceClass {

        constructor(json?: any) {
            this.foo = json.Foo;
        }

        public foo: number;
    }

    export class Person {

        constructor(json?: any) {
            this.age = json.Age;
            this.birthDay = json.BirthDay ? new Date(json.BirthDay) : null;
            this.gender = json.Gender;
            this.firstName = json.FirstName;
            this.lastName = json.LastName;
        }

        public age: number;
        public birthDay: Date;
        public gender: any;
        public firstName: string;
        public lastName: string;
    }

    export class City {

        constructor(json?: any) {
            this.name = json.Name;
            this.people = (<any[]>json.People || []).map((x: any) => x ? new Person(x) : null);
        }

        public name: string;
        public people: Person[];
    }

    export interface _IFoo {
        voidMethod(cancellationToken?: ng.IPromise<any>): ng.IPromise<void>;
        echoByte(bar: string, cancellationToken?: ng.IPromise<any>): ng.IPromise<ArrayBuffer>;
    }

    export interface _IMember {
        random(seed: number, cancellationToken?: ng.IPromise<any>): ng.IPromise<Person>;
        city(isBoolTest: Boolean, cancellationToken?: ng.IPromise<any>): ng.IPromise<City>;
        echo(test: Test, cancellationToken?: ng.IPromise<any>): ng.IPromise<string>;
    }

    export class LightNodeClient extends LightNodeClientBase {

        constructor($q: ng.IQService, rootEndPoint: string, innerHandler: LightNodeClientHandler) {
            super($q, rootEndPoint, innerHandler);
        }

        private _foo: _IFoo;
        public get foo(): _IFoo {
            if (!this._foo) {
                this._foo = {
                    voidMethod: this.fooVoidMethod.bind(this),
                    echoByte: this.fooEchoByte.bind(this)
                };
            }
            return this._foo;
        }

        private _member: _IMember;
        public get member(): _IMember {
            if (!this._member) {
                this._member = {
                    random: this.memberRandom.bind(this),
                    city: this.memberCity.bind(this),
                    echo: this.memberEcho.bind(this)
                };
            }
            return this._member;
        }

        protected fooVoidMethod(cancellationToken?: ng.IPromise<any>): ng.IPromise<void> {

            var data = {};

            return this.post<void>("/Foo/VoidMethod", data, cancellationToken);

        }

        protected fooEchoByte(bar: string, cancellationToken?: ng.IPromise<any>): ng.IPromise<ArrayBuffer> {

            var data = {
                "bar": bar
            };

            return this.post<ArrayBuffer>("/Foo/EchoByte", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return data;
                }, "arraybuffer");

        }

        protected memberRandom(seed: number, cancellationToken?: ng.IPromise<any>): ng.IPromise<Person> {

            var data = {
                "seed": seed
            };

            return this.post<Person>("/Member/Random", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    if (!this.validStatusCode(status)) {
                        return data;
                    }
                    let json = this.parseJSON(data);
                    return json ? new Person(json) : null;
                });

        }

        protected memberCity(isBoolTest: Boolean, cancellationToken?: ng.IPromise<any>): ng.IPromise<City> {

            var data = {
                "isBoolTest": !!isBoolTest
            };

            return this.post<City>("/Member/City", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    if (!this.validStatusCode(status)) {
                        return data;
                    }
                    let json = this.parseJSON(data);
                    return json ? new City(json) : null;
                });

        }

        protected memberEcho(test: Test, cancellationToken?: ng.IPromise<any>): ng.IPromise<string> {

            var data = {
                "test": test
            };

            return this.post<string>("/Member/Echo", data, cancellationToken,
                (data: any, headersGetter: ng.IHttpHeadersGetter, status: number) => {
                    return this.parseJSON(data);
                });

        }

    }

}
/* tslint:enable */
