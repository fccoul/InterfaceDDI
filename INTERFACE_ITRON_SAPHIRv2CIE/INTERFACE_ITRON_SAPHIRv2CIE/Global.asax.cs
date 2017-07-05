using INTERFACE_ITRON_SAPHIRv2CIE.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using WebMatrix.WebData;

namespace INTERFACE_ITRON_SAPHIRv2CIE
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            

            //10122016
           AuthConfig.RegisterAuth();
            //WebSecurity.InitializeDatabaseConnection("DBConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            WebSecurity.InitializeDatabaseConnection("SAPHIRCOM_ConnectionString", "UserProfile", "UserId", "UserName", autoCreateTables: true);

            //-------------
            string log4NetPath = Server.MapPath("~/log4net2.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4NetPath));
            //---------------------
            
        }
    }
}