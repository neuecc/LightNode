LightNode
=========
LightNode is Micro RPC/REST Framework built on OWIN provides both server and client. Server is lightweight and performant implementation. Client is code generation(T4) based auto generated RPC Client based on HttpClient, of course everything return Task. And client code generation will provide for Unity3D and TypeScript. 

Installation
---
binary from NuGet, [LightNode.Server](https://nuget.org/packages/LightNode.Server/)

```
PM> Install-Package LightNode.Server
```

and Client is [LightNode.Client.PCL.T4](https://nuget.org/packages/LightNode.Client.PCL.T4/)

```
PM> Install-Package LightNode.Client.PCL.T4
```
and ContentFormatters(for JsonNet, ProtoBuf, MsgPack)
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
        // global configuration, select your primary/secondary formatters(JsonNet/ProtoBuf/MsgPack/Xml/etc...)
        app.UseLightNode(new LightNodeOptions(
            AcceptVerbs.Get | AcceptVerbs.Post, 
            new JavaScriptContentTypeFormatter()));
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

Lightweight Client
--- 
Implementation of the REST API is often painful. LightNode solves by T4 code generation.


```csharp
// Open .tt file and configure four steps.

// ------------- T4 Configuration ------------- //

// 1. Set Namespace & ClientName & Namespace
var clientName = "LightNodeClient";
var namespaceName = "LightNode.Client";

// Note: You can take solutionDirectory path
var solutionDir = this.Host.ResolveAssemblyReference("$(SolutionDir)"); 

// 2. Set LightNodeContract Assemblies Path
// Note: Currentry this T4 locks assembly. If need release assembly, please restart Visual Studio.
var assemblyPaths = new string[] { solutionDir + @"Source\Sample\bin\Debug\Asm.dll"};

// 3. Set DefaultContentFormatter Construct String
var defaultContentFormatter = "new LightNode.Formatter.XmlContentTypeFormatter()";

// 4. Set Additional using Namespace
var usingNamespaces = new [] {"System.Linq"};

// ----------End T4 Configuration ------------- //
```

```csharp
// generated code is like RPC Style.
// {ClassName}.{MethodName}({Parameters}) 

var client = new LightNodeClient("http://localhost");
await client.Me.Echo("test");
var sum = await client.Me.Sum(1, 10, 100);
```

Client is very simple, too.

> Currently provides only for Portable Class Library. But we plan for Unity3D and TypeScript.

Language Interoperability
---
LightNode is like RPC but REST. Public API follows a simple rule. Address is `{ClassName}/{MethodName}`, and it's case insensitive. GET parameter use QueryString. POST parameter use x-www-form-urlencoded. Response type follows configured ContentTypeFormatter. Receiver can select response type use url extension(.xml, .json etc...) or Accept header.

Performance
---
LightNode is very fast, nearly raw HttpHandler.

![lightnode_performance](https://f.cloud.github.com/assets/46207/1799212/32d94e5e-6b8d-11e3-9e58-cd8f89c62131.jpg)

ASP.NET Web API, LightNode, app.Run are Hosted on OWIN and IIS(System.Web). HttpHandler is hosted on IIS(System.Web).

ReleaseNote
---
0.1.1 - 2013-12-23  
* First Release