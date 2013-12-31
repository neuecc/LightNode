using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace ServiceStack
{
    public class Global : HttpApplication
    {
        public class AppHost : AppHostBase
        {
            public AppHost() : base("Hello Web Services", typeof(HelloService).Assembly) { }

            public override void Configure(Funq.Container container)
            {

            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            new AppHost().Init();
        }
    }
}