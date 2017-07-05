using ACEVISION.Common;
using ACEVISION.ProcessUI;
using DotNetOpenAuth.AspNet;
using INTERFACE_ITRON_SAPHIRv2CIE.ACEVISIONWebService;
using INTERFACE_ITRON_SAPHIRv2CIE.Common;
using INTERFACE_ITRON_SAPHIRv2CIE.Filters;
using INTERFACE_ITRON_SAPHIRv2CIE.Models;
using InterfaceServices.DTO;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mail;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Controllers
{

   [Authorize(Roles = Constante.roleSuperAdmin + "," + Constante.roleAdmin)]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login
        #region variables
        static List<InfoSite_ToIntegrate> List_InfoSite_ToIntegrate = new List<InfoSite_ToIntegrate>();
        static List<infosIntegrated> _lstinfosIntegrated = new List<infosIntegrated>();
        static string _lstMailFileExcel;

        ACEVISIONWebService.ContractsClient proxy = new ACEVISIONWebService.ContractsClient();
        ACEVISIONWebService.DTOAuth authDTO = new ACEVISIONWebService.DTOAuth()
        {
            //Login = "admin",
            //Password = "admin",
            Login = System.Configuration.ConfigurationManager.AppSettings["LoginWebSce"],
            Password = System.Configuration.ConfigurationManager.AppSettings["PasswordWebSce"],

            Authenticate = ACEVISIONWebService.DTOEnumAuthenticate.Logged,
            Authorization = ACEVISIONWebService.DTOEnumAuthorization.ReadAndWrite,
        };
        string LeGroup = Constantes.LeGroup;//-default group
        string msgErrFile = string.Empty;
        static Dictionary<string, InfoSensitiveCTR> _dicoCTRFile = new Dictionary<string, InfoSensitiveCTR>();
        DataReferencePresenter _DataReferencePresenter = new DataReferencePresenter();

        MeterPresenter _meterPresenter = new MeterPresenter();
        #endregion

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

      
        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    //WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { EmailID =model.EmailID});
                    WebSecurity.Login(model.UserName, model.Password);
                    //-------------
                    /*
                    if(!Roles.RoleExists(Constante.roleExecutif))
                        Roles.CreateRole(Constante.roleExecutif);
                    if (!Roles.IsUserInRole(model.UserName, Constante.roleExecutif))
                        Roles.AddUserToRole(model.UserName, Constante.roleExecutif);
                    */
                    //--------------------
                    //return RedirectToAction("Index", "Home");
                    return RedirectToAction("DisplayAllUsers", "Account");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
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
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        #region Role
        [HttpGet]
        public ActionResult RoleCreate()
        {
            return View(new INTERFACE_ITRON_SAPHIRv2CIE.Models.Role());
        }

        [HttpPost]
        public ActionResult RoleCreate(INTERFACE_ITRON_SAPHIRv2CIE.Models.Role role)
        {
            if (ModelState.IsValid)
            {
                if (Roles.RoleExists(role.RoleName))
                {
                    ModelState.AddModelError("Error", "Ce rôle existe déjà");
                    return View(role);
                }
                else
                {
                    Roles.CreateRole(role.RoleName);
                    return RedirectToAction("DisplayAlluserroles", "Account");
                }
            }
            //else
            //{
            //    ModelState.AddModelError("Error", "Veuillez saisir le profil");
            //}
            return View(role);
        }

        [HttpGet]
        public ActionResult DisplayAllRoles()
        {
            IEnumerable<INTERFACE_ITRON_SAPHIRv2CIE.Models.Role> ListRoles;
            using (UsersContext db = new UsersContext())
            {
                ListRoles = db.Roles.ToList();
            }
            return View(ListRoles.Where(u => u.RoleName != Constante.roleSuperAdmin).ToList());
        }

        [HttpGet]
        public ActionResult RoleAddToUser()
        {
            AssignRoleVM objvm = new AssignRoleVM();
            objvm.RolesList = GetAll_Roles();
            objvm.Userlist = GetAll_Users();
            return View(objvm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleAddToUser(AssignRoleVM objvm)
        {
            if (objvm.RoleName == "0")
            {
                ModelState.AddModelError("RoleName", "Veuillez selectionner un profil");
            }

            if (objvm.UserID == "0")
            {
                ModelState.AddModelError("UserName", "Veuillez selectionner un compte utilisateur");
            }

            if (ModelState.IsValid)
            {
                if (Get_CheckUserRoles(Convert.ToInt32(objvm.UserID)) == true)
                {
                    ViewBag.ResultMessage = "Ce compte est déjà associé à un profil !";
                }
                else
                {
                    var UserName = GetUserName_BY_UserID(Convert.ToInt32(objvm.UserID));

                    Roles.AddUserToRole(UserName, objvm.RoleName);
                    ViewBag.ResultMessage = "Succes : Affection du profil au compte utilisateur !";
                }
                objvm.RolesList = GetAll_Roles();
                objvm.Userlist = GetAll_Users();
                return View(objvm);
            }
            else
            {
                objvm.RolesList = GetAll_Roles();
                objvm.Userlist = GetAll_Users();
            }
            return View(objvm);

        }

        [NonAction]
        public List<SelectListItem> GetAll_Roles()
        {
            List<SelectListItem> listrole = new List<SelectListItem>();
            listrole.Add(new SelectListItem { Text = "...", Value = "0" });
            using (UsersContext db = new UsersContext())
            {
                foreach (var item in db.Roles)
                {
                    listrole.Add(new SelectListItem { Text = item.RoleName, Value = item.RoleName });
                }
            }
            return listrole.Where(u => u.Text != Constante.roleSuperAdmin).ToList();
        }

        [NonAction]
        public List<SelectListItem> GetAll_Users()
        {
        List<SelectListItem> listuser = new List<SelectListItem>();
        listuser.Add(new SelectListItem { Text = "...", Value = "0" });
 
        using (UsersContext db = new UsersContext())
        {
        foreach (var item in db.UserProfiles)
        {
        listuser.Add(new SelectListItem { Text = item.UserName, Value = item.UserId.ToString() });
        }
        }
        return listuser.Where(u=>u.Text!=Constante.AccountSuperAdmin).ToList();
        }


        public bool Get_CheckUserRoles(int UserId)
        {
            using (UsersContext context = new UsersContext())
            {
                var data = (from WR in context.webpages_UsersInRole
                            join R in context.Roles on WR.RoleId equals R.RoleId
                            where WR.UserId == UserId
                            orderby R.RoleId
                            select new
                            {
                                WR.UserId
                            }).Count();

                if (data > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Get_CheckUserRoles_Removed(int UserId,string RoleName)
        {
            using (UsersContext context = new UsersContext())
            {
                var data = (from WR in context.webpages_UsersInRole
                            join R in context.Roles on WR.RoleId equals R.RoleId
                            where WR.UserId == UserId
                            where R.RoleName==RoleName
                            orderby R.RoleId
                            select new
                            {
                                WR.UserId
                            }).Count();

                if (data > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string GetUserName_BY_UserID(int UserId)
        {
            using (UsersContext context = new UsersContext())
            {
                var UserName = (from UP in context.UserProfiles
                                where UP.UserId == UserId
                                select UP.UserName).SingleOrDefault();
                return UserName;
            }
        }

        [HttpGet]
        public ActionResult DisplayAllUserroles()
        {
            AllroleandUser objru = new AllroleandUser();
            objru.AllDetailsUserlist = Get_Username_And_Rolename();
            return View(objru);
        }

        [NonAction]
        public List<AllroleandUser> Get_Username_And_Rolename()
        {
            using (UsersContext db = new UsersContext())
            {
                var Alldata = (from User in db.UserProfiles
                               join WU in db.webpages_UsersInRole on User.UserId equals WU.UserId
                               join WR in db.Roles on WU.RoleId equals WR.RoleId
                               //-----without SuperAdmin
                               where WR.RoleName!=Constante.roleSuperAdmin
                               select new AllroleandUser { UserName = User.UserName, RoleName = WR.RoleName }).ToList();

                return Alldata;
            }
        }

        [HttpGet]
        public ActionResult RemoveRoleAddedToUser()
        {
            AssignRoleVM objvm = new AssignRoleVM();
            objvm.RolesList = GetAll_Roles();
            objvm.Userlist = GetAll_Users();
            return View(objvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveRoleAddedToUser(AssignRoleVM objvm)
        {
            if (objvm.RoleName == "0")
            {
                ModelState.AddModelError("RoleName", "Veuillez selectionner le profil");
            }
            if (objvm.UserID == "0")
            {
                ModelState.AddModelError("UserName", "Veuillez selectionner le compte utilisateur");
            }
            if (ModelState.IsValid)
            {
                if (Get_CheckUserRoles_Removed(Convert.ToInt32(objvm.UserID),objvm.RoleName) == true)
                {
                    var UserName = GetUserName_BY_UserID(Convert.ToInt32(objvm.UserID));
                    Roles.RemoveUserFromRole(UserName, objvm.RoleName);
                    ViewBag.ResultMessage = "Succes : Profil retiré pour ce compte utilisateur !";
                }
                else
                {
                    ViewBag.ResultMessage = "Ce compte utilisateur n'est pas associé à ce profil";
                }
                objvm.RolesList = GetAll_Roles();
                objvm.Userlist = GetAll_Users();
            }
            else
            {
                objvm.RolesList = GetAll_Roles();
                objvm.Userlist = GetAll_Users();
            }
            return View(objvm);
        }

        [HttpGet]
        public ActionResult DisplayAllUsers()
        {
            IEnumerable<INTERFACE_ITRON_SAPHIRv2CIE.Models.UserProfile> ListRoles;
            using (UsersContext db = new UsersContext())
            {
                ListRoles = db.UserProfiles.ToList();
            }
            return View(ListRoles.Where(u => u.UserName != Constante.AccountSuperAdmin).ToList());
            //return View(ListRoles.ToList());
        }

        [HttpGet]
        public ActionResult DisplayInfosSite()
        {
            List_InfoSite_ToIntegrate = new List<InfoSite_ToIntegrate>(); 
             /////////////////
            /*
            Microsoft.Office.Interop.Excel.Range range;
            int rCnt = 0;
            int cCnt = 0;
            int cptIntegrate = 0;
            string msErr = string.Empty;
            string msgErrReturn = string.Empty;

            //GET fisrt worksheet
            string strFileName = @"E:\Application\FIAH\NEW_SITETO_INTEGRATE.xls";
            string strNewPath = strFileName;
            var WFile = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook Wbook = WFile.Workbooks.Open(strNewPath, ReadOnly: true);//, Password: PwdFileExcel);
            Microsoft.Office.Interop.Excel.Worksheet sheet = (Microsoft.Office.Interop.Excel.Worksheet)Wbook.Sheets[1];
            range = sheet.UsedRange;

            for (rCnt = 2; rCnt <= range.Rows.Count; rCnt++)
            {
                //InfoSensitiveCTR _ctrHT = new InfoSensitiveCTR();
                InfoSite_ToIntegrate infos = new InfoSite_ToIntegrate();
                try
                {
                    infos.Client = Convert.ToString((range.Cells[rCnt, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
                    infos.IDABON = Convert.ToString((range.Cells[rCnt, 2] as Microsoft.Office.Interop.Excel.Range).Value2);
                    infos.REFRACCORD = Convert.ToString((range.Cells[rCnt, 3] as Microsoft.Office.Interop.Excel.Range).Value2);
                    infos.ADRESSE = Convert.ToString((range.Cells[rCnt, 4] as Microsoft.Office.Interop.Excel.Range).Value2);
                    infos.NUMCTR = Convert.ToString((range.Cells[rCnt, 5] as Microsoft.Office.Interop.Excel.Range).Value2);
                    infos.SITEEXP = Convert.ToString((range.Cells[rCnt, 6] as Microsoft.Office.Interop.Excel.Range).Value2);

                    List_InfoSite_ToIntegrate.Add(infos);
                }
                catch (Exception ex)
                {
                }
            }
            ////////////////

            //return View(List_InfoSite_ToIntegrate);
            return View(List_InfoSite_ToIntegrate.Take(1));
             */
            return View(List_InfoSite_ToIntegrate);
        }


        [HttpPost]
        public ActionResult DisplayContent(HttpPostedFileBase file)
        {
            //System.Threading.Thread.Sleep(10000);
            //-----------------
            List_InfoSite_ToIntegrate = new List<InfoSite_ToIntegrate>();    
            Microsoft.Office.Interop.Excel.Range range;
             int rCnt = 0;
            int cCnt = 0;
            int cptIntegrate = 0;
           
            string msErr = string.Empty;
            string msgErrReturn = string.Empty;
            string strNewPath = string.Empty;

            PresenterLog.setValues_ofData();
            //update file config
            PresenterLog.SetParametersLog();

            PresenterLog.TotalRead_fromNode = List_InfoSite_ToIntegrate.Count();

            //PresenterLog.TotalRead_fromNode = 0; 
            PresenterLog.TotalRead_fromAce = 0;
            PresenterLog.TotalInserted_toAce = 0;
            PresenterLog.TotalUpdated_toAce = 0;
            PresenterLog.ExecutionMode = Constantes.ExecutionMode_Manuel;
            PresenterLog.TypeTraitement = string.Format(Constantes.TraitementIntegration);         
            Log.MonitoringLogger.Info(DateTime.Now + " -  Affichage du contenu du fichier!");
            PresenterLog.setValues_ofData();



            string nameFile = string.Empty;
            if (file != null && file.ContentLength > 0)
                try
                {
                    string strFileType = System.IO.Path.GetExtension(file.FileName).ToString().ToLower();

                    //string nameFile = file.FileName;
                    string path = Path.Combine(Server.MapPath("~/Views/Account/UploadedExcel"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);

                    nameFile = new FileInfo(file.FileName).Name;

                    strNewPath = Server.MapPath("~/Views/Account/UploadedExcel/" + nameFile);

                    if (strFileType.Trim() != ".xls")
                    {
                        ViewBag.Message = string.Format("ERREUR: fichier non supporté - {0} ! format Fichier '.xls' attendu", nameFile);
                        LogDTO UnLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = ViewBag.Message,
                            Objet = "ERREUR DE TRAITEMENT - FILE EXCEL",
                            // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                        };
                        string msgDisplayErr = UnLog.DescriptionErreur;
                        Log.ExceptionLogger.Error(msgDisplayErr);
                    }
                    else
                    {
                        var WFile = new Microsoft.Office.Interop.Excel.Application();
                        Microsoft.Office.Interop.Excel.Workbook Wbook = WFile.Workbooks.Open(strNewPath, ReadOnly: true);//, Password: PwdFileExcel);
                        Microsoft.Office.Interop.Excel.Worksheet sheet = (Microsoft.Office.Interop.Excel.Worksheet)Wbook.Sheets[1];
                        range = sheet.UsedRange;
                        //////////////////////////
                         object[,] cellValues = (object[,])range.Value2;
                          int cols = range.Columns.Count;
                        int rows = range.Rows.Count;
                        for (int i = 2; i <= rows; i++)
                        {
                            //InfoSensitiveCTR _ctrHT = new InfoSensitiveCTR();
                            InfoSite_ToIntegrate infos = new InfoSite_ToIntegrate();
                            try
                            {
                                if (cellValues[i, 1] == null)
                                    continue;
                                infos.Client = cellValues[i, 1].ToString();
                                infos.IDABON = cellValues[i, 2].ToString();
                                infos.REFRACCORD = cellValues[i, 3].ToString();
                                infos.ADRESSE = cellValues[i, 4].ToString(); ;
                                infos.NUMCTR = cellValues[i, 5].ToString(); ;
                                infos.SITEEXP = cellValues[i, 6].ToString(); ;
                                /*
                                infos.Client = Convert.ToString((range.Cells[rCnt, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
                                infos.IDABON = Convert.ToString((range.Cells[rCnt, 2] as Microsoft.Office.Interop.Excel.Range).Value2);
                                infos.REFRACCORD = Convert.ToString((range.Cells[rCnt, 3] as Microsoft.Office.Interop.Excel.Range).Value2);
                                infos.ADRESSE = Convert.ToString((range.Cells[rCnt, 4] as Microsoft.Office.Interop.Excel.Range).Value2);
                                infos.NUMCTR = Convert.ToString((range.Cells[rCnt, 5] as Microsoft.Office.Interop.Excel.Range).Value2);
                                infos.SITEEXP = Convert.ToString((range.Cells[rCnt, 6] as Microsoft.Office.Interop.Excel.Range).Value2);
                                //--Pour Test
                                //infos.SITEEXP = Convert.ToDouble((range.Cells[rCnt, 9] as Microsoft.Office.Interop.Excel.Range).Value2);
                                 */

                                List_InfoSite_ToIntegrate.Add(infos);
                            }
                            catch (Exception ex)
                            {
                                LogDTO UnLog = new LogDTO()
                                {
                                    DateLog = DateTime.Now.ToString(),
                                    DescriptionErreur = string.Format("ERREUR LECTURE FILE EXCEL  {0} : {1}  ",nameFile, ex.Message),
                                    Objet = "ERREUR DE TRAITEMENT - FILE EXCEL"
                                    // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                                };
                                string msgDisplayErr = UnLog.DescriptionErreur;
                                Log.ExceptionLogger.Error("ECHEC : " + msgDisplayErr);
                                ViewBag.Message = UnLog.DescriptionErreur;
                                break;
                            }
                        }
                        //return View(List_InfoSite_ToIntegrate.Take(1));
                        return View("DisplayInfosSite", List_InfoSite_ToIntegrate);
                    }
                    //ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERREUR:" + ex.Message.ToString();
                    LogDTO UnLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = string.Format("ERREUR LECTURE FILE EXCEL  {0} : {1}  ", nameFile, ex.Message),
                        Objet = "ERREUR DE TRAITEMENT - FILE EXCEL",
                        // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                    };
                    string msgDisplayErr = UnLog.DescriptionErreur;
                    ViewBag.Message = UnLog.DescriptionErreur;
                    Log.ExceptionLogger.Error("ECHEC : " + msgDisplayErr);
                }
                    
                finally
                {
                    if (System.IO.File.Exists(strNewPath))                    
                        System.IO.File.Delete(strNewPath);
               
                    //-----end
                    Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement Affichage");
                }
            else
            {
                ViewBag.Message = "Aucun fichier n'a été selectionné";
            }
            //return View();
            return View("DisplayInfosSite",List_InfoSite_ToIntegrate); 
        }

        [HttpGet]
        public JsonResult Integrate()
        {
            _lstinfosIntegrated.Clear();
           // System.Threading.Thread.Sleep(10000);
            UserProfile _UserExcel = Session["AccountFileExcel"] as UserProfile;
            if (_UserExcel != null)
                _lstMailFileExcel = _UserExcel.EmailID;
            //-----------------
            #region initialiasition Log
            //---------------------
            string jsonData = string.Empty;
            string msgErr = string.Empty;
            List<string> LstCTR_alreadyIntegratedNode = new List<string>();
            string valueMeter = string.Empty;
            //------------LOg
            //-27102015
            //get values to save in Log....
            PresenterLog.setValues_ofData();
            //update file config
            PresenterLog.SetParametersLog();
     
            PresenterLog.TotalRead_fromNode =List_InfoSite_ToIntegrate.Count();

            //PresenterLog.TotalRead_fromNode = 0;
            PresenterLog.TotalRead_fromAce = 0;
            PresenterLog.TotalInserted_toAce = 0;
            PresenterLog.TotalUpdated_toAce = 0;



            PresenterLog.ExecutionMode = Constantes.ExecutionMode_Manuel;

            PresenterLog.TypeTraitement = string.Format(Constantes.TypeTraitementIntegration, List_InfoSite_ToIntegrate.FirstOrDefault().SITEEXP);
           // PresenterLog.TypeTraitement = string.Format(Constantes.TraitementIntegration);
            PresenterLog.setValues_ofData();
            Log.MonitoringLogger.Info(DateTime.Now + " -  Initialialisation du traitement !");

            PresenterLog.setValues_ofData();
            int NbreInsert = 0;
            int NbreErreur = 0;
            //string Errmess = string.Empty;


            //--------------------
            #endregion

                StringBuilder sbmsg = new StringBuilder();
                string msgReturn = string.Empty;
                int cptTotal = -1;
                int cptIntegrate=0;
                int cptError = 0;

                string headerMsg = "Traitement integration terminé avec ";

                try
                {

                    //List<InfoSite_ToIntegrate> lst = List_InfoSite_ToIntegrate.Take(1).ToList();
                    List<InfoSite_ToIntegrate> lst = List_InfoSite_ToIntegrate;
                   cptTotal=lst.Count();
                    LogDTO _laLog = null;
                    string mgErr = string.Empty;
                    //string pathFile = _DataReferencePresenter.getPathFileExcel(ref mgErr);
                    /*
                    ParamsFileDTO _pfd = _DataReferencePresenter.get_ParamsFile(ref mgErr);
                    string pathFile = _pfd != null ? _pfd.PathFileExcel : string.Empty;
                    */
                    //_dicoCTRFile = _lstInfosCTr_fileXls(pathFile, _pfd.PasswordFileExcel, ref mgErr);
                    _dicoCTRFile = _lstInfosCTrHT(out mgErr);
                    if (!string.IsNullOrEmpty(mgErr))
                    {           
                         LogDTO UnLog = new LogDTO()
                         {
                             DateLog = DateTime.Now.ToString(),
                             DescriptionErreur = string.Format("ERREUR LECTURE FILE EXCEL  : {0}  ", mgErr),
                             Objet = "ERREUR DE TRAITEMENT - FILE EXCEL",
                             // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                         };
                             string msgDisplayErr = UnLog.DescriptionErreur;
                             Log.ExceptionLogger.Error("ECHEC : " + msgDisplayErr);
                        return Json("KO#ECHEC : " + mgErr, JsonRequestBehavior.AllowGet);
                    }

                    int IDcustomerexist = -1;
                    string DefaultPhoneNumber = Constantes.DefaultPhoneNumber;
                    IList<ACEVISIONWebService.DTOPortableTerminal> LesTSP = proxy.getAllPortableTerminals(ref authDTO);
                    int NumeroTSP = LesTSP != null && LesTSP.Count > 0 ? LesTSP[0].PortableTerminalId : 20;//-20 : code TSP valide en BDD tjrs existant
                    ACEVISIONWebService.DTOEnumMeterConnection TypeTerminalReleve = ACEVISIONWebService.DTOEnumMeterConnection.PortableTerminal;

                    string DefaultEmail = Constantes.DefaultEmail;
                    string DefaultComment = Constantes.DefaultComment;

                    ACEVISIONWebService.DTOCustomer LeClientDefault = proxy.getCustomersByNameFilter(ref authDTO, Constantes.DefaultCustomer, true).FirstOrDefault();
                    ACEVISIONWebService.DTOMeterGroup LeGroupDefault = proxy.getMeterGroupsByName(ref authDTO, LeGroup, true).FirstOrDefault();

                    List<InfoSite_ToIntegrate> lstReejected = new List<InfoSite_ToIntegrate>();
                    InfoSensitiveCTR _ctrInfoSensitive = new InfoSensitiveCTR();
                    ACEVISIONWebService.DTOObjectCreationReturn LeCode = new DTOObjectCreationReturn();
                    int idMeter = -1;
                    int idGrp = -1;

                    //---les ctrs du groupe par defaut
                    DTOMeterGroup DefaultGroup = proxy.getMeterGroupsByName(ref authDTO, LeGroup, true).FirstOrDefault();
                    ACEVISIONWebService.DTOMeterIdentifier[] lstCrtFree = proxy.getMeterGroupById(ref authDTO, DefaultGroup.MeterGroupId).MeterList;
                    //if (!string.IsNullOrEmpty(mgErr))
                    //{
                    //    if (_dicoCTRFile==null || _dicoCTRFile.Count == 0)//---contient au moins un ctr
                    //    {
                    //        return Json(Constante.DicoEmpty, JsonRequestBehavior.AllowGet);
                    //    }

                    //}
                    //else
                    //{
                    //    return Json(Constante.DicoisNull+":"+mgErr, JsonRequestBehavior.AllowGet);
                    //}

                    //---------------------------------------------------------------------------            
                 
                        foreach (var item in lst)
                        {
                            if (!string.IsNullOrEmpty(item.Client) && !string.IsNullOrEmpty(item.IDABON) && !string.IsNullOrEmpty(item.REFRACCORD)
                                && !string.IsNullOrEmpty(item.ADRESSE) && !string.IsNullOrEmpty(item.NUMCTR) && !string.IsNullOrEmpty(item.SITEEXP))
                            {
                                if (item.Client.Contains('/'))
                                {
                                    var tab=item.Client.Split('/');
                                    if (string.IsNullOrEmpty(tab[0]) || string.IsNullOrEmpty(tab[1]) || string.IsNullOrEmpty(tab[0].Trim()) || string.IsNullOrEmpty(tab[1].Trim()))
                                    {
                                        sbmsg.Append(string.Format("SiteExp {0} - une des valeurs du nom du client {1} est vide  : nom Client / reference raccord", item.SITEEXP, item.Client));
                                        cptError++;
                                        _laLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = string.Format("SiteExp {0} - une des valeurs du nom du client {1} est vide  : nom Client / reference raccord", item.SITEEXP, item.Client),
                                            Objet = "Integration Site : " + item.SITEEXP,
                                            //ReferenceObjet = IdAbon
                                        };
                                        Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                        continue;
                                    }
                                    else
                                    {
                                   

                                        if (tab[1].Trim().ToString() != item.REFRACCORD.Trim().ToString())
                                        {
                                            string msErr = string.Format("SiteExp {0} - N°Compteur {1} - reference du raccord du nom client {2} différent de celle au niveau du champ raccord",item.SITEEXP, item.NUMCTR, item.Client);
                                            sbmsg.Append(msErr);
                                            cptError++;
                                            _laLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur = msErr,
                                                Objet = "Integration Site : " + item.SITEEXP,
                                                //ReferenceObjet = IdAbon
                                            };
                                            Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    sbmsg.Append(string.Format("SiteExp {0} -le nom du client {1} ne resptecte pas la règle de gestion : nom Client / reference raccord",item.SITEEXP,item.Client));
                                    cptError++;
                                    _laLog = new LogDTO()
                                    {
                                        DateLog = DateTime.Now.ToString(),
                                        DescriptionErreur = string.Format("SiteExp {0} -le nom du client {1} ne resptecte pas la règle de gestion : nom Client / reference raccord",item.SITEEXP, item.Client),
                                        Objet = "Integration Site : " + item.SITEEXP,
                                        //ReferenceObjet = IdAbon
                                    };
                                    Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                    continue;
                                }

                                //---------------------
                                string _addr = item.ADRESSE + " / " + item.REFRACCORD;
                                #region 1 : Pré-requis

                                #region 0 :check si ctr present File excel
                                if (_dicoCTRFile.Count > 0)
                                {

                                    if (!_dicoCTRFile.TryGetValue(item.NUMCTR.Trim(), out _ctrInfoSensitive))
                                    {
                                        _laLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = string.Format("Integration Site :  {0} - ECHEC DE RECUPERATION DES INFOS DU FICHIER EXCEL DE COMPTEUR {1}", item.SITEEXP, item.NUMCTR),
                                            Objet = "CREATION DU COMPTEUR : " + item.NUMCTR,
                                            //ReferenceObjet = IdAbon
                                        };
                                        cptError++;
                                        sbmsg.Append(_laLog.DescriptionErreur + ": " + item.NUMCTR + " |");
                                        Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                        continue;
                                    }
                                }
                                #endregion

                                #region 1 : check si cctr existe ds ace vision
                                DTOMeter[] LesCompteur = proxy.getMetersBySerialNumberFilter(ref authDTO, item.NUMCTR.Trim(), true);
                                bool isCreateMeter = false;
                                if (LesCompteur.Count() == 0)
                                    isCreateMeter = true;//ctr a crééer
                                else
                                {
                                    idMeter = LesCompteur[0].MeterId;
                                    bool verifCtr = lstCrtFree.Select(l => l.SerialNumber).Contains(item.NUMCTR.Trim());
                                    //----check si ctr dispo :defaultgroup
                                    if (!verifCtr)
                                    {
                                        //----check clt affecte au clt 
                                        DTOCustomer _clt = proxy.getCustomerById(ref authDTO, LesCompteur[0].CustomerId);
                                        if (_clt.Name.Trim() == item.Client.Trim())
                                        {
                                            _laLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur = string.Format("Integration Site :  {0} - ECHEC COMPTEUR {1} DEJA Affecté à ce client {2} ",item.SITEEXP,item.NUMCTR,item.Client),
                                                Objet = "Integration Site : " + item.SITEEXP,
                                                //ReferenceObjet = IdAbon
                                            };
                                            cptError++;
                                            sbmsg.Append(_laLog.DescriptionErreur + " |");
                                            Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                            continue;
                                        }
                                        else
                                        {
                                            _laLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur =string.Format("Integration Site :  {0} - ECHEC COMPTEUR {1} DEJA Affecté au client {2} dans Ace Vision ",item.SITEEXP,item.NUMCTR, _clt.Name),
                                                Objet = "Integration Site : "+item.SITEEXP,
                                                //ReferenceObjet = IdAbon
                                            };
                                            cptError++;
                                            Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                            sbmsg.Append(_laLog.DescriptionErreur + " |");

                                            continue;
                                        }

                                    }
                                    //---------------------------
                                }
                                #endregion

                                #region 2 : check si clt existe
                                //--- verifie que le client n'existe pas deja dans Ace Vision
                                DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, item.Client.Trim(), true).FirstOrDefault();
                                bool iscreateCustomer = false;

                                if (LeClient == null)
                                    iscreateCustomer = true;
                                else
                                {
                                    //au cas ou il possede un ctr
                                    DTOMeter[] _meterSelf = proxy.getMetersByCustomerId(ref authDTO, LeClient.CustomerId);
                                    if (_meterSelf.Count() > 0)
                                    {
                                        _laLog = new LogDTO()
                                         {
                                             DateLog = DateTime.Now.ToString(),
                                             DescriptionErreur = string.Format("Integration Site :  {0} - ECHEC le client {1} possède dejà un compteur {2}",item.SITEEXP, item.Client, _meterSelf[0].SerialNumber)
                                             //Objet = "Integration COMPTEUR : " + _meterSelf[0].SerialNumber,
                                             //ReferenceObjet = IdAbon
                                         };
                                        cptError++;
                                        sbmsg.Append(_laLog.DescriptionErreur + " |");
                                        Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                        continue;
                                    }
                                    else
                                    {
                                        //---recuperer son ID
                                        IDcustomerexist = LeClient.CustomerId;
                                    }
                                }
                                #endregion

                                #region 3 : check si grp  ctr - ace vision identique Saphir
                                /*---plus necessaire deja gerer au niveau du check ctr
                                if(!isCreateMeter)
                                {
                                //---check si grp ctr acevision(different du default group) correspond au group du file import
                                DTOMeterGroup[] lstGrpResult = proxy.getMeterGroupsBySerialNumber(ref authDTO, item.NUMCTR);
                                DTOMeterGroup _grpAppartenance = lstGrpResult[0];
                                if (_grpAppartenance.MeterGroupName != Constantes.LeGroup && _grpAppartenance.MeterGroupName != item.SITEEXP.Trim())
                                {
                                    _laLog = new LogDTO()
                                    {
                                        DateLog = DateTime.Now.ToString(),
                                        DescriptionErreur = string.Format("Integration Site :  {0} - ECHEC Ce compteur est déjà affecté à une autre exploitation Ace vision {1} : ",item.SITEEXP,_grpAppartenance.MeterGroupName),
                                        Objet = "Integration COMPTEUR : " + item.NUMCTR,
                                        //ReferenceObjet = IdAbon
                                    };
                                    sbmsg.Append(_laLog.DescriptionErreur + " |");
                                    cptError++;
                                    continue;
                                }
                                }
                                */
                                #endregion


                                #endregion

                                #region 2 : creation de la ligne infos (clt rattache au ctr et au grp)
                                //----integration in Ace Vision
                                #region 0 : creation client
                                if (iscreateCustomer)
                                {
                                    //item.Client = "TEMso / 14714856";//---pr test
                                    LeCode = proxy.addCustomer(ref authDTO, item.Client.Trim(), string.Empty, string.Empty, string.Empty, string.Empty, DefaultComment, string.Empty, string.Empty, string.Empty);
                                    if (LeCode != null)
                                    {
                                        if ((int)LeCode.ReturnCode == 0 && LeCode.NewElementId > 0)
                                            IDcustomerexist = LeCode.NewElementId;
                                        else
                                        {
                                            //**LOG
                                            //msgDisplayErr = string.Format("Message : Echec creation du client {0} ", NomClient);
                                            //Log.ExceptionLogger.Error(msgDisplayErr);
                                            //---
                                            //Utilitaires.AfficherAvertissement("", "Une erreur est survenue lors de la création du client");
                                            _laLog = new LogDTO()
                                             {
                                                 DateLog = DateTime.Now.ToString(),
                                                 DescriptionErreur = string.Format("Integration Site :  {0} - ECHEC CREATION DU CLIENT {1} - Erreur : {2} ", item.SITEEXP, item.Client, LeCode.ReturnCode.ToString()),
                                                 Objet = "CREATION D'UN CLIENT",
                                                 //ReferenceObjet = IdAbon
                                             };
                                            sbmsg.Append(_laLog.DescriptionErreur + " |");
                                            Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                            cptError++;
                                            continue;
                                        }
                                    }
                                }

                                #endregion

                                #region 0' : creation du ctr

                                if (isCreateMeter)
                                {

                                    double _VersionFirmWare = _ctrInfoSensitive.versionFirmWare;
                                    ACEVISIONWebService.DTOEnumMeter _enumTypeCtr = (DTOEnumMeter)Enum.Parse(typeof(DTOEnumMeter), _ctrInfoSensitive.TypeMeter);

                                    LeCode = proxy.addMeter(ref authDTO, item.NUMCTR.Trim(), IDcustomerexist, _addr, item.IDABON, _enumTypeCtr, _VersionFirmWare.ToString(),
                                  _ctrInfoSensitive.PasswordReader, TypeTerminalReleve, NumeroTSP, "10.0.0.1", 0, 0, 0, 0, DefaultPhoneNumber, DefaultPhoneNumber);
                                    
                                    if (LeCode != null)
                                    {
                                        if ((int)LeCode.ReturnCode == 0)
                                        {
                                            idMeter = LeCode.NewElementId;
                                            //14122016
                                            string msgBody = string.Format("{0} : nouveau compteur créé dans ACE VISION", item.NUMCTR);
                                            string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                                            string msgErrReturn = string.Empty;
                                             new Utility().SendEMail(_lstMailFileExcel, Constantes.ObjectNotification, msgBody + footer, ref msgErrReturn);
                                        }
                                    }
                                    else
                                    {
                                        //Utilitaires.AfficherAvertissement("", "Une erreur est survenue lors de la création du Compteur");
                                        _laLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = string.Format("Integration Site :  {0} - ECHEC CREATION DU COMPTEUR {1} - Erreur : {2} ", item.SITEEXP, item.NUMCTR, LeCode.ReturnCode.ToString()),
                                            Objet = "CREATION DU COMPTEUR : " + item.NUMCTR,
                                            //ReferenceObjet = IdAbon
                                        };
                                        sbmsg.Append(_laLog.DescriptionErreur + " |");
                                        Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                        cptError++;
                                        continue;
                                    }
                                }

                                #endregion

                                #region 1 : update meter customer
                                if (!isCreateMeter) //---ctr existant et dispo
                                {
                                    ACEVISIONWebService.DTOEnumWebServiceReturnCode xCode = proxy.updateMeterCustomer(ref authDTO, idMeter, IDcustomerexist);
                                    if (xCode != null && (int)xCode != 0)
                                    {
                                        _laLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = string.Format("Integration Site :  {0} - ERREUR LORS DE L'ASSOCIATION COMPTEUR / CLIENT {1} -Erreur {2}",item.SITEEXP,item.NUMCTR, xCode.ToString()),
                                            // Objet = "ERREUR LORS DE LA CREATION COMPTEUR",
                                            Objet = "ERREUR LORS DE L'ASSOCIATION COMPTEUR - CLIENT",
                                            //ReferenceObjet = "Numero Compteur : " + _newMter.SerialNumber + " Identifiant abonné : " + adto.IdentifiantAbonne
                                        };


                                        sbmsg.Append(_laLog.DescriptionErreur + " |");
                                        Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                        cptError++;
                                        continue;
                                    }
                                    else
                                    {
                                        //----upadte IDABON
                                        ACEVISIONWebService.DTOEnumWebServiceReturnCode _LeCode = proxy.updateMeterLocation(ref authDTO, idMeter, item.IDABON);
                                        if ((int)_LeCode != 0)  // Erreur lors de la modification
                                        {
                                            _laLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur = string.Format("Integration Site :  {0} - MODIFICATION DE L'Identifiant CLIENT AU NIVEAU COMPTEUR {1} - Erreur {2}", item.SITEEXP,item.NUMCTR, LeCode.ToString()),
                                                Objet = "MODIFICATION DE L'Identifiant CLIENT AU NIVEAU COMPTEUR",
                                               // ReferenceObjet = adto.IdentifiantAbonne
                                            };

                                            sbmsg.Append(_laLog.DescriptionErreur + " |");
                                            Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                            cptError++;
                                            continue;
                                        }
                                        else
                                        {
                                            //-----upadte Adresse
                                            DTOEnumWebServiceReturnCode _eCode = proxy.updateMeterAddress(ref authDTO, idMeter, _addr);

                                            if ((int)_eCode != 0)  // Erreur lors de la modification
                                            {
                                                _laLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = string.Format("Integration Site :  {0} - MODIFICATION DE L'Adresse DU COMPTEUR - REFRACCORDEMENT {1} - Erreur {2}",item.SITEEXP,item.NUMCTR, LeCode.ToString()),
                                                    Objet = "MODIFICATION DE L'Adresse DU COMPTEUR - REFRACCORDEMENT",
                                                    //ReferenceObjet = adto.IdentifiantAbonne
                                                };


                                                sbmsg.Append(_laLog.DescriptionErreur + " |");
                                                Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                                cptError++;
                                                continue;
                                            }
                                        }


                                    }
                                }
                                #endregion

                                #region 2 : creation du groupe
                                DTOMeterGroup LeNewGroup = proxy.getMeterGroupsByName(ref authDTO, item.SITEEXP, true).FirstOrDefault();
                                if (LeNewGroup == null)
                                {
                                    //-creation du groupe
                                    LeCode = proxy.addMeterGroup(ref authDTO, item.SITEEXP, ACEVISIONWebService.DTOEnumMeter.GroupMeterTypeAll, 1);
                                    if (LeCode != null)
                                    {
                                        if ((int)LeCode.ReturnCode == 0)
                                            idGrp = LeCode.NewElementId;
                                        else//-on s'assure de bien recuperer le Id du groupe qui vient d'etre créé
                                        {
                                            ACEVISIONWebService.DTOMeterGroup LeGroupe = proxy.getMeterGroupsByName(ref authDTO, item.SITEEXP, true).FirstOrDefault();
                                            if (LeGroupe != null)
                                            {
                                                idGrp = LeGroupe.MeterGroupId;
                                            }
                                            else
                                            {
                                                _laLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = string.Format("Integration Site :  {0} - CREATION D'UN GROUPE{1} - Erreur {2} ",item.SITEEXP,item.NUMCTR, LeCode.ReturnCode.ToString()),
                                                    Objet = "CREATION D'UN GROUPE",
                                                    //ReferenceObjet = GroupeName
                                                };
                                                sbmsg.Append(_laLog.DescriptionErreur + " |");
                                                Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                                cptError++;
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Utilitaires.AfficherAvertissement("", "Une erreur est survenue lors de la création du Groupe");
                                        _laLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = string.Format("Integration Site :  {0} - CREATION D'UN GROUPE{1} - Erreur {2} ", item.SITEEXP, item.NUMCTR, LeCode.ReturnCode.ToString()),
                                            Objet = "CREATION D'UN GROUPE",
                                            // ReferenceObjet = GroupeName
                                        };
                                        sbmsg.Append(_laLog.DescriptionErreur + " |");
                                        Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                        cptError++;
                                        continue;
                                    }
                                }
                                else
                                    idGrp = LeNewGroup.MeterGroupId;

                                #endregion

                                #region 3 : change meter group
                                if (!isCreateMeter) //---ctr existant et dispo
                                {
                                    DTOEnumWebServiceReturnCode oReturnCode = proxy.ChangeMeterGroup(ref authDTO, LesCompteur[0].SerialNumber, item.Client.Trim(), LeGroupDefault.MeterGroupName.Trim(), item.SITEEXP.Trim());

                                    if ((int)oReturnCode != 0)
                                    {
                                        _laLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = string.Format("Integration Site :  {0} - TRANSFERT DE COMPTEUR {1} - Erreur {2}", item.SITEEXP, item.NUMCTR, oReturnCode.ToString()),
                                            // DescriptionErreur = "mon test echechec",--pr test
                                            //Objet = msgErr,
                                            //  ReferenceObjet = item.OLD_IdentifiantAbonne
                                        };
                                        sbmsg.Append(_laLog.DescriptionErreur + " |");
                                        Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                        cptError++;
                                        continue;
                                    }
                                    else
                                    {
                                       // _lstinfosIntegrated.add
                                        _lstinfosIntegrated.Add(new infosIntegrated() { Client = item.Client, Compteur = item.NUMCTR, IDABON = item.IDABON });
                                        cptIntegrate++;
                                    }
                                    //el
                                    //    sbmsg.Append("OK");
                                }
                                else
                                {
                                    DTOEnumWebServiceReturnCode LCode = proxy.addMeterToMeterGroup(ref authDTO, idMeter, idGrp);
                                    if (LCode != null && (int)LCode != 0)
                                    {
                                        _laLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = string.Format("Integration Site :  {0} - ASSOCIATION DU COMPTEUR AU GROUPE {1} - Erreur {2}", item.SITEEXP, item.NUMCTR, LeCode.ToString()),
                                            Objet = "ASSOCIATION DU COMPTEUR A UN GROUPE",
                                            // ReferenceObjet = "ID Compteur " + sNumeroCompteur + " ID Group " + sGroup
                                        };
                                        // Result = false;
                                        sbmsg.Append(_laLog.DescriptionErreur + " |");
                                        Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                        cptError++;
                                        continue;
                                    }
                                    {
                                        _lstinfosIntegrated.Add(new infosIntegrated() {Client=item.Client,Compteur=item.NUMCTR,IDABON=item.IDABON });
                                        cptIntegrate++;
                                    }
                                    //else
                                    //      sbmsg.Append("OK");
                                }

                                #endregion

                                #endregion
                            }
                            else
                            {
                                //logger later
                                //string msgR = "une ligne contient des elements vides";
                                sbmsg.Append("une ligne contient des elements vides");
                                _laLog = new LogDTO()
                                {
                                    DateLog = DateTime.Now.ToString(),
                                    DescriptionErreur = "une ligne contient des elements vides",
                                    //Objet = "Integration Site : " + item.SITEEXP,
                                    //ReferenceObjet = IdAbon
                                };
                               // sbmsg.Append(_laLog.DescriptionErreur + " |");
                                cptError++;
                                Log.ExceptionLogger.Error(_laLog.DescriptionErreur);
                                continue;
                            }
                        }

                   
                }
                catch (Exception ex)
                {
                    sbmsg.Append(ex.Message);
                    Log.ExceptionLogger.Error(ex.Message);
                    //throw;
                }
                finally
                {
                    if (sbmsg.Length > 0)
                    {
                        if (cptIntegrate < cptTotal && cptIntegrate !=0)//-succès partial
                        {
                            msgReturn = string.Format("KO-succes# {0} {1} erreur(s) / {2} - Details : {3}", headerMsg, cptError, List_InfoSite_ToIntegrate.Count(), sbmsg.ToString());
                        }
                        else
                        {
                            // msgReturn = "KO|"+headerMsg +" "+ Message - ECHEC INTEGRATION : " + sbmsg.ToString();
                            msgReturn = string.Format("KO# {0} {1} erreur(s) / {2} - Details : {3}", headerMsg, cptError, List_InfoSite_ToIntegrate.Count(), sbmsg.ToString());
                        }
                        //msgReturn = string.Format("KO-succes| {0} {1} erreur(s) / {2} - Details : {3}", headerMsg, cptError, List_InfoSite_ToIntegrate.Count(), sbmsg.ToString());
                        PresenterLog.TotalInserted_toAce = cptIntegrate;
                    }
                    else
                    {
                        msgReturn = "OK#" + cptIntegrate + "/ " + cptTotal;
                        //sbmsg.Append("OK|" + cptIntegrate + "/ " + cptTotal);

                        PresenterLog.TotalInserted_toAce = cptIntegrate;
                    }
                    PresenterLog.setValues_ofData();
                    //if (!string.IsNullOrEmpty(msgReturn) && msgReturn.Contains("|"))
                    if (!string.IsNullOrEmpty(msgReturn) && (!msgReturn.Contains("OK")) && msgReturn.Contains("#"))
                    {
                        msgReturn = msgReturn.Remove(msgReturn.Length - 2);
                        var msgRet = msgReturn.Split('#');
                       // Log.ExceptionLogger.Error(msgRet[1]); //---LOG Groupé
                    }
                    Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");
                }
               
               // msgReturn = "OK|17 / 21 ";

            return Json(msgReturn,JsonRequestBehavior.AllowGet);
        }

       [HttpGet]
        public JsonResult DisplayItemsIntegrated()
        {
            //var result="ok";
           // _lstinfosIntegrated.Add(new infosIntegrated() { Client = "kpleus / 12354785",IDABON="10000001", Compteur = "7777999" });
            //_lstinfosIntegrated.Add(new infosIntegrated() { Client = "Luke / 47859632", IDABON = "10000002", Compteur = "11114444" });
            return Json(_lstinfosIntegrated, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public ActionResult EditAccount(int IDUser)
        {
            UserProfile _user = new UserProfile();
            RegisterModel _userModel = new RegisterModel();
            using (UsersContext db = new UsersContext())
            {
                _user = db.UserProfiles.Where(w => w.UserId == IDUser).FirstOrDefault();
            }
            _userModel.UserName = _user.UserName;
            _userModel.EmailID= _user.EmailID;
            ViewBag.IDUser = IDUser.ToString();
            
            return View(_userModel);
        }

        [HttpPost]
        public ActionResult EditAccount(FormCollection collection)
        {
            bool valSaisi=false;
            string login=string.Empty;
            //string email = string.Empty;

            if (string.IsNullOrEmpty(collection["UserName"]))
            {
                ModelState.AddModelError("UserName", "Veuillez saisir le login");
            }
            else
            {//---check si login dispo
                valSaisi=true;
                 login = collection["UserName"];
                using (UsersContext db = new UsersContext())
                {
                    UserProfile _user = new UserProfile();
                    _user = db.UserProfiles.Where(w => w.UserName == login).FirstOrDefault();
                    if(_user!=null)
                        ModelState.AddModelError("UserName", "Ce login est déjà utilisé !");
                }
            }

            if (string.IsNullOrEmpty(collection["EmailID"]))
            {
                ModelState.AddModelError("EmailID", "Veuillez saisir l'adresse electronique");
            }

            if (ModelState.IsValid)
            {
                int _userID = int.Parse(collection["idUser"]);
                using (UsersContext db = new UsersContext())
                {
                    UserProfile _user = new UserProfile();
                    _user = db.UserProfiles.Where(w => w.UserId == _userID).FirstOrDefault();
                    _user.UserName = login;
                    _user.EmailID = collection["EmailID"];
                    db.SaveChanges();

                    TempData["ValResult"] = true;
                 
                    //////
                    
                }
                    return RedirectToAction("DisplayAllUsers", "Account");
                
            }
            //else
            //{
            //    ModelState.AddModelError("Error", "Veuillez saisir le profil");
            //}
            ViewBag.IDUser = collection["idUser"].ToString();
            if (valSaisi)
                return View(new RegisterModel() { UserName = collection["UserName"].ToString() });
            else
                return View();
        }

         [HttpGet]
        public ActionResult ResetPassword(int IDUser)
        {
              UserProfile _user = new UserProfile();
              using (UsersContext db = new UsersContext())
              {
                  _user = db.UserProfiles.Where(w => w.UserId == IDUser).FirstOrDefault();


                  //MembershipUser mu=Membership.GetUser(_user.UserName);
                  //mu.ChangePassword(mu.ResetPassword(), Constante.PasswordDefaultAccount);


                  string token = WebSecurity.GeneratePasswordResetToken(_user.UserName, 1440);
                  bool res = WebSecurity.ResetPassword(token, Constante.PasswordDefaultAccount);
                  //WebSecurity.
                  //bool res = false;
                  //bool res = WebSecurity.ChangePassword(_user.UserName, token, Constante.PasswordDefaultAccount);
                  if (res)
                  {
                      TempData["ValResult"] = true;
                      /////////////////////

                      //get user emailid to send password
                      var emailid = _user.EmailID;
                      //send email
                      string subject = "Interface SAPHIR / ACE VISION - ReInitialisation de votre mot de passe";
                      //string body = "<b>Please find the New Password</b><br/>" + newpassword; //edit it
                      string body = "<b>Votre mot de passe a été rénitialisé </b><br/>" + Constante.PasswordDefaultAccount;
                      body += "<p> Veuillez le modifier une fois connecté</p> ";
                      body += "<p>Ceci est un mail automatique , veuillez ne pas répondre !</p>";
                      try
                      {
                          SendEMail(emailid, subject, body);
                          //TempData["Message"] = "Mail Sent.";
                          TempData["Message"] = "Succes! un mail contenant le nouveau mot de passe a été envoyé à cet adresse.";

                      }
                      catch (Exception ex)
                      {
                          TempData["Message"] = "Error occured while sending email." + ex.Message;
                          TempData["ValResult"] = false;
                          return RedirectToAction("DisplayAllUsers", "Account");
                      }

                      //display message
                      //TempData["Message"] = "Success! Check email we sent. Your New Password Is " + newpassword;
                      TempData["Message"] = "Succes! un mail contenant le nouveau mot de passe a été envoyé à cet adresse .\n Veuillez le modifier une fois connecté ."; //Your New Password Is " + Constante.PasswordDefaultAccount;

                      ////////////////////////////
                  }
                  else
                      TempData["ValResult"] = false;
                  return RedirectToAction("DisplayAllUsers", "Account");
              }
         }

             private void SendEMail(string emailid, string subject, string body)
                {
                    SmtpClient client = new SmtpClient();
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //client.EnableSsl = true;
                    client.EnableSsl = false;
                    client.Host = System.Configuration.ConfigurationManager.AppSettings["serverSMTP"];
                   // client.Port = 587;

                    //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("xxxxx", "yyyy");
                    //client.UseDefaultCredentials = false;
                    //client.Credentials = credentials;

                    System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                    msg.From = new MailAddress("adminDDI@cie.ci");
                    msg.To.Add(new MailAddress(emailid));

                    msg.Subject = subject;
                    msg.IsBodyHtml = true;
                    msg.Body = body;

                    client.Send(msg);
                }


             //14112016
       /*
        * \deprecated depuis le 14042017
        */ 
             public Dictionary<string, InfoSensitiveCTR> _lstInfosCTr_fileXls(string strFileName, string PwdFileExcel, ref string msgErr)
             {
                 Microsoft.Office.Interop.Excel.Range range;
                 int rCnt = 0;
                 int cCnt = 0;
                 string msErr = string.Empty;
                 string msgErrReturn = string.Empty;

                 //strNewPath = Server.MapPath("~/UploadedExcel/" + strFileName + strFileType);
                 string strNewPath = strFileName;
                 var WFile = new Microsoft.Office.Interop.Excel.Application();
                 try
                 {
                     //Microsoft.Office.Interop.Excel.Workbook Wbook = WFile.Workbooks.Open(strNewPath, ReadOnly: true, Password: System.Configuration.ConfigurationManager.AppSettings["FilePassword"]);
                     //14122016
                     Microsoft.Office.Interop.Excel.Workbook Wbook = WFile.Workbooks.Open(strNewPath, ReadOnly: true, Password: PwdFileExcel);

                     //GET fisrt worksheet
                     Microsoft.Office.Interop.Excel.Worksheet sheet = (Microsoft.Office.Interop.Excel.Worksheet)Wbook.Sheets[1];

                     range = sheet.UsedRange;

                     Dictionary<string, InfoSensitiveCTR> myDicoCTR = new Dictionary<string, InfoSensitiveCTR>();
                     // string str = string.Empty;
                     string test = string.Empty;

                     object[,] cellValues = (object[,])range.Value2;
                     int cols = range.Columns.Count;
                        int rows = range.Rows.Count;
                        for (int i = 2; i <= rows; i++)
                        {
                            InfoSensitiveCTR _ctrHT = new InfoSensitiveCTR();
                                try
                                {
                                    _ctrHT.SerialNumber = cellValues[i, 1].ToString();
                                    _ctrHT.PasswordReader = cellValues[i, 2].ToString();
                                    _ctrHT.TypeMeter = cellValues[i, 3].ToString();
                                    _ctrHT.versionFirmWare = (double)(cellValues[i, 4]);
                                    //--pr test -  adelete
                                   // _ctrHT.versionFirmWare = (double)(cellValues[i, 8]);
                                }
                                catch (Exception ex)
                                {
                                    //test = ex.Message;
                                    msgErr = ex.Message;
                                    //-----------send email account Password
                                    // _util.SendMail_Notification(_lstMailFileExcel,true,Constantes.ObjectNotification,ex.Message,out msErr);
                                    //14122016
                                    string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";

                                    new Utility().SendEMail(_lstMailFileExcel, Constantes.ObjectNotification, msgErr + footer, ref msgErrReturn);
                                    //---------------------------------------
                                    return null;
                                }
                                myDicoCTR.Add(_ctrHT.SerialNumber.Trim(), _ctrHT);

                        }//---end for

                        return myDicoCTR;
                 }
                 catch (System.Runtime.InteropServices.COMException cex)
                 {
                     msgErr = cex.Message;
                     //-----------send email account Password
                     string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                     new Utility().SendEMail(_lstMailFileExcel, Constantes.ObjectNotification, msgErr + footer, ref msgErrReturn);
                     //_util.SendMail_Notification(_lstMailFileExcel, true, Constantes.ObjectNotification, cex.Message, out msErr);
                     //---------------------------------------
                 }
                 catch (Exception ex)
                 {
                     msgErr = ex.Message;
                     string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                     //-----------send email account Password
                     new Utility().SendEMail(_lstMailFileExcel, Constantes.ObjectNotification, msgErr + footer, ref msgErrReturn);
                     //_util.SendMail_Notification(_lstMailFileExcel, true, Constantes.ObjectNotification, ex.Message, out msErr);
                     //---------------------------------------
                 }

                 return null;
             }

             /*
               * \date:20042017
               * \author : FCO
               */
             public Dictionary<string, InfoSensitiveCTR> _lstInfosCTrHT(out string msgReturn)
             {
                 msgReturn = string.Empty;
                 Dictionary<string, InfoSensitiveCTR> myDicoCTR = new Dictionary<string, InfoSensitiveCTR>();

                 List<CTRHTDTO> lstAllCtr = _meterPresenter.getAllCTRHT(out msgReturn);
                 if (lstAllCtr.Count() > 0)
                 {
                     foreach (var item in lstAllCtr)
                     {
                         InfoSensitiveCTR _ctrHT = new InfoSensitiveCTR();
                         try
                         {
                             _ctrHT.SerialNumber = item.SERIALNUMBER;
                             _ctrHT.PasswordReader = item.PASSWORD_READER;
                             _ctrHT.TypeMeter = item.TYPEMETER;
                             _ctrHT.versionFirmWare = double.Parse(item.FIRMWARE);

                             myDicoCTR.Add(_ctrHT.SerialNumber.Trim(), _ctrHT);

                         }
                         catch (Exception ex)
                         {
                             //mail admin excel
                             msgReturn = ex.Message;
                             string msgErrReturn = string.Empty;
                             string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                             new Utility().SendEMail(_lstMailFileExcel, Constantes.ObjectNotification, msgReturn + footer, ref msgErrReturn);
                         }

                     }
                 }

                 return myDicoCTR;
             }
        //-----------------------------
        #endregion

             
    }
}
