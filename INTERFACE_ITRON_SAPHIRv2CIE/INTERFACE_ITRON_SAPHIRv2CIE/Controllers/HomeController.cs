using ACEVISION.ProcessUI;
using INTERFACE_ITRON_SAPHIRv2CIE.Models;
using InterfaceServices.DTO;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //[Authorize]
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "A Propos API SAPHIR / ACE VISION.";
        DataReferencePresenter dp=new DataReferencePresenter();
            string msgErr=string.Empty;
            EvolutionBaseDTO ebDTO=dp.getVersionSoftWare(ref msgErr);
            if (ebDTO != null)
                @ViewBag.Version = ebDTO.Full_Version;
            else
                @ViewBag.Version = "1.0";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult test2()
        {
            return View();
        }

        
        //public ActionResult Settings()
        public ActionResult Settings(ManageMessageId? message)
        {
            UserProfile _user;
            int IDCurrentUser = WebSecurity.GetUserId(User.Identity.Name);
            using (UsersContext db = new UsersContext())
            {
                _user = db.UserProfiles.Where(w => w.UserId == IDCurrentUser).FirstOrDefault();
            }

            //MembershipUser mship = Membership.GetUser(_user.UserName,false);
            //string pass = mship.GetPassword();

            ViewBag.Email = _user.EmailID;
            LocalSettingModel sett = new LocalSettingModel();
            sett.Email = _user.EmailID;

            ViewBag.StatusMessage =
               message == ManageMessageId.ChangePasswordSuccess ? "Votre mot de passe été modifié."
               : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
               : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
               : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");

            return View(sett);
        }

        [HttpPost]
        public ActionResult Settings(LocalSettingModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Settings");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                 
                    if (changePasswordSucceeded)
                    {

                        //-------------Email
                        UserProfile _user;
                        bool emailUpdated = false;
                        int IDCurrentUser = WebSecurity.GetUserId(User.Identity.Name);
                        using (UsersContext db = new UsersContext())
                        {
                            _user = db.UserProfiles.Where(w => w.UserId == IDCurrentUser).FirstOrDefault();
                            _user.EmailID = model.Email;

                            db.SaveChanges();
                            emailUpdated = true;
                        }
                        //------------------------
                        if (!emailUpdated)
                            ModelState.AddModelError("", "Echec Mise à jour de l'adresse electronique !");
                        else
                        return RedirectToAction("Settings", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {                    
                            ModelState.AddModelError("", "Le mot de passe actuel est incorrect ou le nouveau mot de passe n'est pas valide.");
                       // ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                       
                    }
                }
            }
           

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

       public ActionResult ErrorDisplay(string _params)         
        {
            if(_params!=string.Empty)
            ViewBag.FileExcel = _params;
            return View();
        }
    }
}
