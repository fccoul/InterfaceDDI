using ACEVISION.Common;
using ACEVISION.ProcessUI;
using INTERFACE_ITRON_SAPHIRv2CIE.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

 
namespace INTERFACE_ITRON_SAPHIRv2CIE.Controllers
{
    //[Authorize(Roles = Constante.roleSuperAdmin + "," + Constante.roleExecutif+ "," +Constante.roleAdmin)]
    [DefaultAuthorizeAttribute(Roles = Constante.roleSuperAdmin + "," + Constante.roleExecutif+ "," +Constante.roleAdmin)]
    public class LogController : Controller
    {
        //
        // GET: /Log/
        ReceptionIndexPresenter _presenter = new ReceptionIndexPresenter();

        public ActionResult LogReception()
        {
            string msgErr=string.Empty;

            string connStr = ConfigurationManager.ConnectionStrings[Constantes.SAPHIRCOMConnexionStringName].ConnectionString;
            
            return View( _presenter.GetLog_Reception(out msgErr));
        }
        public ActionResult LogEmission()
        {
            string msgErr = string.Empty;

            return View(_presenter.GetLog_Emisssion(out msgErr));
        }
        //-----------------
        public ActionResult LogIntegration()
        {
            string msgErr = string.Empty;

            return View(_presenter.GetLog_Integration(out msgErr));
        }

        [OverrideAuthorizeAttribute(Roles = Constante.roleAdminFileExcel)]
        public ActionResult LogIntegrationCtr()
        {
            string msgErr = string.Empty;

            return View(_presenter.GetLog_IntegrationCtr(out msgErr));
        }

        public ActionResult Ecart()
        {
            return View();
        }

        public ActionResult TestLog()
        {
            return View();
        }


    }
}
