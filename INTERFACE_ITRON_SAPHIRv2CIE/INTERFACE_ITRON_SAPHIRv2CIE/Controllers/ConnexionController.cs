using INTERFACE_ITRON_SAPHIRv2CIE.Common;
using INTERFACE_ITRON_SAPHIRv2CIE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Controllers
{

    
    public class ConnexionController : Controller
    {
        //
        // GET: /Connexion/
        
        public ActionResult Connecter()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult Connecter(UserForm model)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.Login, model.Password, persistCookie: false))             
            {
                //FormsAuthentication.SetAuthCookie(model.Login, false);
                //---set element session
                //----------------------------
                UserProfile _userAccountExcel=new UserProfile();
                int IDCurrentUser = WebSecurity.GetUserId(User.Identity.Name);
                using (UsersContext db = new UsersContext())
                {
                   Role _role= db.Roles.Where(r => r.RoleName == Constante.roleAdminFileExcel).FirstOrDefault();
                  webpages_UsersInRoles wp= db.webpages_UsersInRole.Where(w => w.RoleId == _role.RoleId).FirstOrDefault();
                  if (wp != null)
                    _userAccountExcel = db.UserProfiles.Where(u => u.UserId == wp.UserId).FirstOrDefault();
               
                }
                Session["AccountFileExcel"] = _userAccountExcel;
                //----------------------
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if(ModelState.IsValid)
                ModelState.AddModelError("", "Le login ou Mot de passe est incorrect");
                return View(model);
            }
            
        }

    
        public ActionResult CheckAuthentication()
        {
            string jsonData = string.Empty;
            jsonData = "kpleus";
            //UserForm _user = new UserForm() { Login = "SuperAdmin", Password = "123456", Role = "SuperAdmin" };
            UserForm _user = new UserForm() { Login = "samy", Password = "123456", Role = "SuperAdmin" };
            //UserForm _user = new UserForm() { Login = "cool", Password = "123456", Role = "AccountExecutive" };
            //string userName="fhcoulibaly";
            FormsAuthentication.SetAuthCookie(_user.Login, true);
            Session["Username"] = _user.Login;
           //return RedirectToAction("Index", "Home");
            jsonData = "succès";
            return Content(jsonData);
            //return Content(jsonData);
            //if (1 == 1)
            //{
            //    //System.Web.HttpContext.Current.Session["UserName"] = "kpleus";
            //    //---si login et password OK
            //    //return this.RedirectToAction("Index", "Home");
            //       // return Content(jsonData);
            //}
            //else
            //{
            //    //jsonData = "Registration Failed";
            //   // return View("Connecter");
                
            //}
            ////return Content("KO KO");
        }


        public ActionResult Logoff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Connecter");
        }

    }
}
