LightNode
=========
LightNode is Micro RPC/REST Framework built on OWIN provides both server and client. Server is lightweight and performant implementation. Client is code generation(T4) based auto generated RPC Client based on HttpClient, of course everything return Task. And client code generation for Unity3D(WWW base). We will provide for TypeScript. 

Installation
---
binary from NuGet, [LightNode.Server](https://nuget.org/packages/LightNode.Server/)

```
PM> Install-Package LightNode.Server
```

Client for Portable Client Library [LightNode.Client.PCL.T4](https://nuget.org/packages/LightNode.Client.PCL.T4/)

```
PM> Install-Package LightNode.Client.PCL.T4
```

Client for Unity3D [LightNode.Client.Unity.T4](https://nuget.org/packages/LightNode.Client.Unity.T4/)

```
PM> Install-Package LightNode.Client.Unity.T4
```

ContentFormatters(for JsonNet, ProtoBuf, MsgPack)
```
PM> Install-Package LightNode.Formatter.JsonNet
PM> Install-Package LightNode.Formatter.ProtoBuf
PM> Install-Package LightNode.Formatter.MsgPack
```

Lightweight Server
---
Server implementation is very easy, built up Owin and implements `LightNodeContract`.

```csharp
// Owin Code
public class Startup
{
    public void Configuration(Owin.IAppBuilder app)
    {
        // UseLightNode = AcceptVerbs.Get | Post, JavaScriptContentFormatter
        app.UseLightNode();
    
        // Details Option
        // global configuration, select your primary/secondary formatters(JsonNet/ProtoBuf/MsgPack/Xml/etc...)
        // app.UseLightNode(new LightNodeOptions(
        //     AcceptVerbs.Get | AcceptVerbs.Post, 
        //     new JsonNetContentFormatter()));
    }
}

// implement LightNodeContract, all public methods become API.
// You can access {ClassName}/{MethodName}
// Ex. http://localhost/My/Echo?x=test
public class My : LightNodeContract
{
    // return value is response body serialized by ContentTypeFormatter.    
    public string Echo(string x)
    {
        return x;
    }

    // support async! return type allows void, T, Task and Task<T>.
    // parameter supports array, nullable and optional parameter.
    public Task<int> Sum(int x, int? y, int z = 1000)
    {
        return Task.Run(() => x + y.Value + z);
    }
}
```
 
Server API rule is very simple.

> Parameter model bindings supports only basic pattern, can't use complex type. allow types are "string, DateTime, DateTimeOffset, Boolean, Decimal, Char, TimeSpan, Int16, Int32, Int64, UInt16, UInt32, UInt64, Single, Double, SByte, Byte and each Nullable types and array(except byte[]. If you want to use byte[], use Base64 string instead of byte[])

Return type allows all serializable(ContentFormatter support) type.

Filter
---
LightNode supports filter. The implementation is like middleware pipeline.

![lightnode_performance](https://f.cloud.github.com/assets/46207/1902207/3dbe3012-7c6f-11e3-8d39-7e442e92b970.jpg)

```csharp
public class SampleFilterAttribute : LightNodeFilterAttribute
{
    public override async Task Invoke(OperationContext operationContext, Func<Task> next)
    {
        try
        {
            // OnBeforeAction

            await next(); // next filter or operation handler

            // OnAfterAction
        }
        catch
        {
            // OnExeception
        }
        finally
        {
            // OnFinally
        }
    }
}
```

Filter can be attached contract(class), operation(method) and global. Execution pipeline is formed is sorted by Order all specified. Range is -int.MaxValue to int.MaxValue. Default Order of all filters is int.MaxValue.

Difference between Middleware and Filter is who knows operation context. Filter is located after the parameter binding. Therefore, it is possible check attributes(`operationContext.IsAttributeDefined`, `operationContext.GetAttributes`).

Control StatusCode
---
The default status code, can't find operation returns 404, failed operation returns 500, success and has value returns 200, success and no value returns 204. If returns arbitrary status code, throw ReturnStatusCodeException.

```csharp
throw new ReturnStatusCodeException(System.Net.HttpStatusCode.Unauthorized);
```

Authenticate, Routing, Session, etc...
---
You can use other owin middleware. For example, Auth:Microsoft.Owin.Security.*, Session:[Owin.RedisSession](https://github.com/neuecc/Owin.RedisSession/), Context:[OwinRequestScopeContext](https://github.com/neuecc/OwinRequestScopeContext), etc...

Routing and versioning example.
```csharp
// Conditional Use
app.MapWhen(x => x.Request.Path.Value.StartsWith("/v1/"), ap =>
{
   // Trim Version Path
   ap.Use((context, next) =>
   {
        context.Request.Path = new Microsoft.Owin.PathString(
            Regex.Replace(context.Request.Path.Value, @"^/v[1-9]/", "/"));
        return next();
   });
 
    // use v1 assembly
    ap.UseLightNode(new LightNodeOptions(AcceptVerbs.Post, new JsonNetContentFormatter()),
        typeof(v1Contract).Assembly);
});
 
app.MapWhen(x => x.Request.Path.Value.StartsWith("/v2/"), ap =>
{
   // copy and paste:)
   ap.Use((context, next) =>
   {
        context.Request.Path = new Microsoft.Owin.PathString(
            Regex.Replace(context.Request.Path.Value, @"^/v[1-9]/", "/"));
        return next();
   });
   
   // use v2 assembly
   ap.UseLightNode(new LightNodeOptions(AcceptVerbs.Post, new JsonNetContentFormatter()),
    typeof(v2Contract).Assembly);
});
```
Composability is owin's nice feature.

Lightweight Client
--- 
Implementation of the REST API is often painful. LightNode solves by T4 code generation.

```csharp
// Open .tt file and configure four steps.

<#@ assembly name="$(SolutionDir)\Performance\LightNode.Performance\bin\LightNode.Performance.dll" #>
<#
    // ------------- T4 Configuration ------------- //
    
    // 1. Set LightNodeContract assemblies(and all dependency) path to above #@ assembly name # directive

    // 2. Set Namespace & ClientName & Namespace
    var clientName = "LightNodeClient";
    var namespaceName = "LightNode.Client";

    // 3. Set DefaultContentFormatter Construct String
    var defaultContentFormatter = "new LightNode.Formatter.JsonNetContentFormatter()";

    // 4. Set Additional using Namespace
    var usingNamespaces = new [] {"System.Linq"};

    // 5. Set append "Async" suffix to method name(ex: CalcAsync or Calc)
    var addAsyncSuffix = true;

    // ----------End T4 Configuration ------------- //
```

```csharp
// generated code is like RPC Style.
// {ClassName}.{MethodName}({Parameters}) 

var client = new LightNodeClient("http://localhost");
await client.Me.EchoAsync("test");
var sum = await client.Me.SumAsync(1, 10, 100);
```

Client is very simple, too.

> Currently provides only for Portable Class Library. But we plan for Unity3D and TypeScript.

Language Interoperability
---
LightNode is like RPC but REST. Public API follows a simple rule. Address is `{ClassName}/{MethodName}`, and it's case insensitive. GET parameter use QueryString. POST parameter use x-www-form-urlencoded. Response type follows configured ContentFormatter. Receiver can select response type use url extension(.xml, .json etc...) or Accept header.

Performance
---
LightNode is fastest framework.

![lightnode_performance](https://f.cloud.github.com/assets/46207/1902439/a0a19c5c-7c72-11e3-9bea-244ac00dcd87.jpg)

Performance source code is in [LightNode/Performance](https://github.com/neuecc/LightNode/tree/master/Performance). Enviroment is "Windows 8.1/CPU Core i7-3770K(3.5GHz)/Memory 32GB" and disabled firewall and windows defender. Orange and Green bar is hosted on IIS(System.Web). LightNode(Green bar)'s performance is nearly raw handler. Gray bar is reference, LightNode on [Helios - Microsoft.Owin.Host.IIS](http://www.nuget.org/packages/Microsoft.Owin.Host.IIS/) gots extremely performance. 

Build/Test Status
---
[![Build status](https://ci.appveyor.com/api/projects/status/i7smkb51sr0ghy15)](https://ci.appveyor.com/project/neuecc/lightnode)

LightNode is using [AppVeyor](http://www.appveyor.com/) CI. You can check unit test status.

ReleaseNote
---
0.3.0 - 2014-05-12
* Add Unity T4 Template
* Some fixes for PCL.T4 Template
* Add default UseLightNode overload

0.2.0 - 2014-01-14
* Add Filter System
* Enum Binding Performance Improvement
* Strict parse for Enum
* Parameter String disallows null at default
* IContentFormatter needs Encoding
* IContentFormatter.Ext can add multiple ext by "|" separater
* Fixed T4 ClientCode generation
* Return 204 when operation is void or Task
* Return Arbitrary StatusCode that throws ReturnStatusCodeException
* Add IgnoreOperationAttribute

0.1.1 - 2013-12-23  
* First Release
