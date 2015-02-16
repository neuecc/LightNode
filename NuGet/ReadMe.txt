LightNode
https://github.com/neuecc/LightNode

Simply Configuration and sample contract

---
using LightNode.Server;
using Owin;

[assembly: Microsoft.Owin.OwinStartup(typeof(Startup))]

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.UseLightNode();
    }
}

public class My : LightNodeContract
{
    public string Echo(string x)
    {
        return x;
    }
}
---

Check the LightNode.Glimpse https://nuget.org/packages/Glimpse.LightNode/
Configuration Sample
---
public void Configuration(Owin.IAppBuilder app)
{
    app.EnableGlimpse(); // This is Glimpse.LightNode's helper for enable Glimpse
    app.MapWhen(x => !x.Request.Path.Value.StartsWith("/glimpse.axd", StringComparison.OrdinalIgnoreCase), x =>
    {
        x.UseLightNode(new LightNodeOptions()
        {
            // for Glimpse Profiling
            OperationCoordinatorFactory = new GlimpseProfilingOperationCoordinatorFactory()
        });
    });
}
---

Glimpse configuration guide
---
<!-- sometimes Glimpse rewrite response for display tab, but API no needs, set RuntimePolicy PersitResults -->
<glimpse defaultRuntimePolicy="PersistResults" endpointBaseUri="~/Glimpse.axd">
    <tabs>
        <ignoredTypes>
            <!-- no needs only Owin -->
            <add type="Glimpse.AspNet.Tab.Cache, Glimpse.AspNet" />
            <add type="Glimpse.AspNet.Tab.Routes, Glimpse.AspNet" />
            <add type="Glimpse.AspNet.Tab.Session, Glimpse.AspNet" />
        </ignoredTypes>
    </tabs>
    <runtimePolicies>
        <ignoredTypes>
            <!-- If API's client no use cookie, ignore control cookie -->
            <add type="Glimpse.Core.Policy.ControlCookiePolicy, Glimpse.Core" />
            <!-- for improvement LightNode debugging -->
            <add type="Glimpse.Core.Policy.StatusCodePolicy, Glimpse.Core" />
            <!-- If not Ajax -->
            <add type="Glimpse.Core.Policy.AjaxPolicy, Glimpse.Core" />
            <!-- If run on remote -->
            <add type="Glimpse.AspNet.Policy.LocalPolicy, Glimpse.AspNet" />
        </ignoredTypes>
    </runtimePolicies>
</glimpse>
---