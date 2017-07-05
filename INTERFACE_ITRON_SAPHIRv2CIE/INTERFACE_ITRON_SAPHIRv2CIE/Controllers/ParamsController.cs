using ACEVISION.Common;
using ACEVISION.ProcessUI;
using INTERFACE_ITRON_SAPHIRv2CIE.Common;
using INTERFACE_ITRON_SAPHIRv2CIE.Models;
using InterfaceServices.DTO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Configuration;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Controllers
{
    //[Authorize(Roles = Constante.roleSuperAdmin + "," + Constante.roleAdmin)]
    [DefaultAuthorizeAttribute(Roles = Constante.roleSuperAdmin + "," + Constante.roleAdmin)]
    //[Authorize]
    public class ParamsController : Controller
    {

        //Variables
        ConnectionDB _connectionDB =new ConnectionDB();
        DataReferencePresenter _DataReferencePresenter = new DataReferencePresenter();
        static List<CTRHTDTO> _lstCTRHTDTO = new List<CTRHTDTO>();
        MeterPresenter _meterPresenter = new MeterPresenter();
        static string _mailAdminExcel = string.Empty;
        //
        // GET: /Params/
        //[Authorize(Roles = Constante.roleSuperAdmin + "," + Constante.roleAdmin + "," + Constante.roleAdminFileExcel)]
        [OverrideAuthorizeAttribute()]
        public ActionResult Index()
        {
            
            List<SelectListItem> items_Bdd= new List<SelectListItem>();
            //------
            string connStr = ConfigurationManager.ConnectionStrings[Constantes.SAPHIRCOMConnexionStringName].ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStr);
            ViewBag.ActualServer = builder.DataSource;
            //---------
            ViewBag.listBDDs = items_Bdd;
            ViewBag.urlWebSce = getAddressEndpoint_SceWeb();
            return View();
        }

        [OverrideAuthorizeAttribute()]
        [HttpPost]
          public ActionResult AddParams(FormCollection collection)//20102014
        {
         
            short typeParams = 1;
            string msgErr = "KO";

            bool b=Request.IsAjaxRequest();
            //if (Request.IsAjaxRequest()) // Appel fait via AJAX...
            //{

            //var xx = Request["paramServSce"];

            short paramServSce;
            short ModeAuthenticate;
            string NameServer = string.Empty; 
            string NameUser = string.Empty; 
            string Pwd = string.Empty; 
            string BDDName = string.Empty; 
            string AdrServeur = string.Empty; 
            string Port = string.Empty;

            string msgErreur=string.Empty;

            if (collection != null)
            {
              paramServSce= short.Parse(collection["paramServSce"].ToString());
              ModeAuthenticate = short.Parse(collection["ModeAuthenticate"].ToString());
              
            
                
                if (ModelState.IsValid)
                {
                    #region old
                    /*
                    //traitement(
                   // if (paramServSce == (short)Pamaters.Params_ServeurBDD) //cas du serveur de BDD
                    //{
                        NameServer = collection["txtNameServer"];                    
                        BDDName = collection["listBDDs"];
                        bool AuthenticateSQL = false;
                        if (ModeAuthenticate==(short)AuthenticateMode.SQL_Server)
                        {
                            NameUser = collection["txtNameUser"];
                            Pwd = collection["txtPwd"];
                            AuthenticateSQL = true;
                        }
                   // }
                    else //sinon le service WEB
                    {
                        //Uri urlWeb=
                        AdrServeur = collection["txtAdrServeur"];
                        Port = collection["textPort"];
                    }
                    */
                    #endregion

                    #region Get vaues form
                            NameServer = collection["NameServer"];
                            BDDName = collection["BDDName"];
                        
                                

                                bool AuthenticateSQL = false;
                                if (ModeAuthenticate == (short)AuthenticateMode.SQL_Server)
                                {
                                    NameUser = collection["NameUser"];
                                    Pwd = collection["Pwd"];
                                    AuthenticateSQL = true;
                                }
                                AdrServeur = collection["AdrServeur"];
                                Port = collection["Port"];
                    #endregion

                                #region Update file Config connexionString

                                ConnectionDB cb = new ConnectionDB() { DataSource = NameServer, InitialCatalog = BDDName, UserID = NameUser, Password = Pwd, IntegratedSecurity = !AuthenticateSQL };
                                   changeConnexionString(cb,out msgErreur);
                                   CryptConnexionString();
                                   //-------------
                                   //SaveMonitoring();
                    //------------------------------
                                   bool result = false;
                                      if(!string.IsNullOrEmpty(AdrServeur) && !string.IsNullOrEmpty(Port))
                                            result = changeAddressEndpoint_SceWeb(new Uri(string.Format("http://{0}:{1}/",AdrServeur,Port)));
                                #endregion

                                string jsonData = "Enregistrement réussi avec succès !";
                    //return Json(jsonData);
                    //return Content(jsonData);
                    return RedirectToAction("Index","Home");
                }
                else
                    return Content(msgErr);
            //}
            //else
              //  return Content(Constante.MsgErr_CallAjax);
        }
            return null;
    }

       

        /// <summary>
        /// Configuration de l'environnement
        /// </summary>
        /// <returns></returns>
        public ActionResult GetServerInfo()
        {
            return View();
        }

        [OverrideAuthorizeAttribute()]
        public ActionResult CheckConnexion(string NameServer,string _UserName,string  _Password,short itemParam,short ModeConnexion)
        {
            
                string msg=string.Empty;
                string iTemTested=string.Empty;

                //if(ModeConnexion==(short)AuthenticateMode.SQL_Server)
                   iTemTested="au serveur de base de données SAPHIR...";

               // if(ModeConnexion==(int)AuthenticateMode.Windows)
                 //  iTemTested="au service WEB ITRON...";

                if (ConnexionValide(itemParam, NameServer,_UserName, _Password, ModeConnexion, out msg))
                 return Content(string.Format(Constante.Msg_Test_connexion,iTemTested));
                else
                  return Content(string.Format(Constante.Msg_Test_connexion_failed,iTemTested,msg) + " | KO");
             
        }

        //verfie ke on a acces au serveur de BDD /service WEB
        //<add name="FIAH_DDI_NANConnectionString" connectionString="Data Source=GSW0183\GSW0183_R2;Initial Catalog=FIAH_29052015;Persist Security Info=True;User ID=sa;Password=P@ssw0rd" providerName="System.Data.SqlClient"/>
        public bool ConnexionValide(short itemParam,string nameServer,string _UserName,string _Password, short? ModeConnexion, out string msg)
        {
            msg = string.Empty;
            bool result=false;

                if (itemParam == (short)Pamaters.Params_ServeurBDD) //cas du Server BDD 
                {
                    msg = string.Empty;
                    //- Test des informations de connexion
                    if (string.IsNullOrEmpty(nameServer))
                    {
                        msg = Constante.MsgErr_SQLServer_Empty;
                        return false;
                    }


                    //if (string.IsNullOrEmpty(_connectionDB.DataSource))
                    //{
                    //    msg = Constante.MsgErr_NameDataBase_Empty;
                    //    return false;
                    //}
                    _connectionDB.DataSource = nameServer;
                    _connectionDB.InitialCatalog = "master";
                    if (ModeConnexion == (short)AuthenticateMode.SQL_Server)
                    {
                        _connectionDB.UserID = _UserName;
                        _connectionDB.Password = _Password;
                    }
                    else
                        _connectionDB.IntegratedSecurity = true;

                     result=isValid_ConnexionString(GetSqlConnexionStringBuilder(_connectionDB).ConnectionString, out msg);
                
                     //stock memoire Params BDD
                     if (result)
                     {
                         ParamsBDD.DataSource = _connectionDB.DataSource;
                         ParamsBDD.InitialCatalog = _connectionDB.InitialCatalog;
                         ParamsBDD.UserID = _connectionDB.UserID;
                         ParamsBDD.Password = _connectionDB.Password;
                         ParamsBDD.IntegratedSecurity = _connectionDB.IntegratedSecurity;
                     }
                //-------------------------
                    return result;
                }
                else //cas du service WEB
                {
                    return false;
                }


            }

  
        private bool isValid_ConnexionString(string sqlConnectionStringBuilder, out string msg)
        {
             msg = string.Empty;
            try
            {
                //test de la connexion
                SqlConnection Connexion = new SqlConnection(sqlConnectionStringBuilder);
                //-Tentative d'ouverture de la connexion
                Connexion.Open();
                //- Message réussite
                //- Test ok. Fermeture de la connexion


           
                Connexion.Close();

                return true;
            }
            catch (SqlException sqlex)
            {
                msg = sqlex.Message.ToString();
                return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
                //return true;
                return false;
            }
        }

        void GetDataBase_FromServer(string NameServer)
        {
            ConnectionDB cDB = new ConnectionDB();


            SqlConnectionStringBuilder ConnexionBuilder = GetSqlConnexionStringBuilder(cDB);
        }

        private SqlConnectionStringBuilder GetSqlConnexionStringBuilder(ConnectionDB _connDB)
        {
            //- Récupération des informations de connexion
            SqlConnectionStringBuilder Builder = new SqlConnectionStringBuilder();
            Builder.DataSource = _connDB.DataSource;
            Builder.InitialCatalog = _connDB.InitialCatalog;
            Builder.IntegratedSecurity = _connDB.IntegratedSecurity;
            Builder.PersistSecurityInfo = true;

            if (!Builder.IntegratedSecurity)
            {
                Builder.UserID = _connDB.UserID;
                Builder.Password = _connDB.Password;
            }

            return Builder;
        }

        private string GetSqlConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            return builder.ConnectionString;
        }

        [OverrideAuthorizeAttribute()]
        public JsonResult getAllDatabase_ofServer()
        {
            ConnectionDB conn = new ConnectionDB { DataSource = ParamsBDD.DataSource, InitialCatalog = ParamsBDD.InitialCatalog, UserID = ParamsBDD.UserID, Password = ParamsBDD.Password };
            //return Json("OK", JsonRequestBehavior.AllowGet);
            //-pour test
            //conn.DataSource = "sodw4357";
            //conn.InitialCatalog = "master";
            //conn.UserID = "sa";
            //conn.Password = "P@ssw0rd";
            try
            {
                var tab = getAllDatabases(conn);
                return Json(tab.Select(s => s.Name), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                //return null;
                return Json(ex.Message);
            }
             
        }

        //----Debug
        /*
        public JsonResult getAllDatabase_ofServer()
        {
            ConnectionDB conn = new ConnectionDB { DataSource = ParamsBDD.DataSource, InitialCatalog = ParamsBDD.InitialCatalog, UserID = ParamsBDD.UserID, Password = ParamsBDD.Password };
            //return Json("OK", JsonRequestBehavior.AllowGet);
            //-pour test
            //conn.DataSource = "sodw4357";
            //conn.InitialCatalog = "master";
            //conn.UserID = "sa";
            //conn.Password = "P@ssw0rd";
            string msgErr=string.Empty;
            try
            {
                //var tab = getAllDatabases(conn);
                var tab = getAllDatabases(conn,out msgErr );
                //return Json(tab.Select(s => s.Name), JsonRequestBehavior.AllowGet);
                //return Json(conn, JsonRequestBehavior.AllowGet);
               if(tab!=null)
                   return Json(tab.Length +" :|:* "+msgErr, JsonRequestBehavior.AllowGet);
                else
                    return Json("le tab est null :|:* "+msgErr, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //return Content(ex.Message);
                return Json(ex.Message);
                //return null;
            }

        }
        */
        [OverrideAuthorizeAttribute()]
        public Database[] getAllDatabases(ConnectionDB _connDB)
        {
            List<Database> lstDataBase = new List<Database>();
            try
            {
                _connDB.IntegratedSecurity = ParamsBDD.IntegratedSecurity;
                SqlConnectionStringBuilder ConnexionBuilder = GetSqlConnexionStringBuilder(_connDB);
                ConnexionBuilder.InitialCatalog = "master";

                string Error = string.Empty;
                if (isValid_ConnexionString(ConnexionBuilder.ConnectionString, out Error))
                {

                    ServerConnection sc = null;
                    if (ConnexionBuilder.IntegratedSecurity)
                        sc = new ServerConnection(ConnexionBuilder.DataSource);
                    else
                        sc = new ServerConnection(ConnexionBuilder.DataSource, ConnexionBuilder.UserID, ConnexionBuilder.Password);

                    Server server = new Server(sc);
                    Database[] tabDB = new Database[server.Databases.Count];
                    int i=0;
                    foreach (Database db in server.Databases)
                    {
                        //lstDataBase.Add(db);
                        tabDB[i++] = db;
                    }
                    return tabDB;
                }
                return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        //-DEbug
        /*
        public Database[] getAllDatabases(ConnectionDB _connDB,out string msgErr)
        {
            msgErr = string.Empty;

            List<Database> lstDataBase = new List<Database>();
            try
            {
                _connDB.IntegratedSecurity = ParamsBDD.IntegratedSecurity;
                SqlConnectionStringBuilder ConnexionBuilder = GetSqlConnexionStringBuilder(_connDB);
                ConnexionBuilder.InitialCatalog = "master";

                string Error = string.Empty;
                if (isValid_ConnexionString(ConnexionBuilder.ConnectionString, out Error))
                {

                    ServerConnection sc = null;
                    if (ConnexionBuilder.IntegratedSecurity)
                        sc = new ServerConnection(ConnexionBuilder.DataSource);
                    else
                        sc = new ServerConnection(ConnexionBuilder.DataSource, ConnexionBuilder.UserID, ConnexionBuilder.Password);

                    Server server = new Server(sc);
                    Database[] tabDB = new Database[server.Databases.Count];
                    int i = 0;
                    foreach (Database db in server.Databases)
                    {
                        //lstDataBase.Add(db);
                        tabDB[i++] = db;
                    }
                    return tabDB;
                }
                else
                    msgErr = "Bleme database SQL";
                return null;
            }
            catch (Exception ex)
            {
                msgErr = ex.Message;
                return null;
            }
        }
        */
        //-------------
      //-------------
        public string TestWebSce_Access()
        {
            string pTargetAddress = "http://10.109.240.43:92/"; //A change later
            // msgErr = string.Empty;
            WebRequest request;
            WebResponse response;

            try
            {
                request = WebRequest.Create(pTargetAddress);
                request.Timeout = 5000;
                response = request.GetResponse();
                request.Abort();
                return true.ToString();
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }


        public bool CryptConnexionString()
        {
            string msgErr = string.Empty;
            bool b=EncryptionSection(Constantes.sectionConnexionConfig, Constantes.ProviderCryptDecrypt,out msgErr);

            return b;
        }

        public bool DeCryptConnexionString()
        {
            string msgErr = string.Empty;
            bool b = DecryptionSection(Constantes.sectionConnexionConfig, out msgErr);

            return b;
        }

        #region methodes Privates
                private bool EncryptionSection(string sectionName, string provider, out string msgErr)
                {
                   msgErr=string.Empty;
                   try
                   {
                       System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                       ConfigurationSection section = config.GetSection(sectionName);
                       if (section != null && !section.SectionInformation.IsProtected)
                       {
                           section.SectionInformation.ProtectSection(provider);
                           config.Save();
                           return true;
                       }
                       else
                           return false;
                   }
                   catch (Exception ex)
                   {

                       msgErr = ex.Message;
                   }
                   return false;
                }

                private bool DecryptionSection(string sectionName,out string msgErr)
                {
                    msgErr = string.Empty;
                    try
                    {
                        System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                        ConfigurationSection section = config.GetSection(sectionName);
                        if (section != null && section.SectionInformation.IsProtected)
                        {
                            section.SectionInformation.UnprotectSection();
                            config.Save();
                            return true;
                        }
                        else
                            return false;
                    }
                    catch (Exception ex)
                    {

                        msgErr = ex.Message;
                    }
                    return false;

                }

                private bool changeConnexionString(ConnectionDB _ConnectionDB, out string msgErr)
                {
                    msgErr = string.Empty;
                    bool b = false;
                    try
                    {
                         SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                            builder.DataSource = _ConnectionDB.DataSource;
                            builder.InitialCatalog = _ConnectionDB.InitialCatalog;
                            builder.PersistSecurityInfo = true;

                            string _connexionString = string.Empty;

                        System.Configuration.Configuration configWeb = WebConfigurationManager.OpenWebConfiguration("~");
                        ConnectionStringsSection connectionStringSection = (ConnectionStringsSection)configWeb.GetSection(Constantes.sectionConnexionConfig);
                       
                        if (!string.IsNullOrEmpty(_ConnectionDB.UserID ))
                        {
                            builder.UserID = _ConnectionDB.UserID;
                            builder.Password = _ConnectionDB.Password;
                             
                        }
                        else
                        {
                            builder.IntegratedSecurity = _ConnectionDB.IntegratedSecurity;                         
                            
                        }
                        _connexionString = builder.ConnectionString;
                         connectionStringSection.ConnectionStrings[Constantes.SAPHIRCOMConnexionStringName].ConnectionString = _connexionString;
                         configWeb.Save(ConfigurationSaveMode.Modified); 
                        ConfigurationManager.RefreshSection(Constantes.sectionConnexionConfig);
                        
                        return b = true;
                    }
                    catch (Exception ex)
                    {

                        msgErr = ex.Message;
                    }
                    //pour le contenu de la connection string in ffile.
                    //string connStr = ConfigurationManager.ConnectionStrings["nameMyconnectionString"].ConnectionString;

                    return b;
                }

                private void SaveMonitoring()
                {
                    
                    //string _user = User.Identity.Name;
                    string _user = Environment.GetEnvironmentVariable("USERNAME");
                    string _hostname = Request.UserHostName + "-" + Request.UserHostAddress; ;
                   
                    
                    System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                    //ConfigurationSection section = config.GetSection(Constantes.sectionMonitoringConfig);
                    //var MonitoringSection = (MonitoringConfig)config.GetSection(Constantes.sectionMonitoringConfig);
                    AppSettingsSection MonitoringSection = (AppSettingsSection)config.GetSection(Constantes.sectionMonitoringConfig);
                    string resultkeyUser = MonitoringSection.Settings["UserConnected"].Value;
                    string resultkeyEdit = MonitoringSection.Settings["Edited"].Value;

                    MonitoringSection.Settings["UserConnected"].Value = _user;
                    MonitoringSection.Settings["Edited"].Value = _hostname + " : " + DateTime.Now;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(Constantes.sectionMonitoringConfig);
                    //if (section != null)
                    //{
                    //   // System.Configuration.ConfigurationManager..AppSettings["LoginWebSce"];
                    //}
                    
                }

                private Uri  getAddressEndpoint_SceWeb()
                {
                    Uri uriWceWeb=null;
                    try
                    {
                        ClientSection clientSection = (WebConfigurationManager.GetSection(Constantes.SectionEndpointClient) as ClientSection);

                        if (clientSection != null)
                        {
                            foreach (ChannelEndpointElement cee in clientSection.Endpoints)
                            {
                                if (cee.Name == Constantes.NameChannelEndpoint)
                                {
                                    uriWceWeb = cee.Address;
                                    break;
                                }
                            }
                            return uriWceWeb;
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    return uriWceWeb;
               
                }

                private bool changeAddressEndpoint_SceWeb(Uri newEndPoint)
                {
                    bool res = false;
                    try
                    {
                        System.Configuration.Configuration configWeb = WebConfigurationManager.OpenWebConfiguration("~");
                        ClientSection clientSection = configWeb.GetSection(Constantes.SectionEndpointClient) as ClientSection;

                        if (clientSection != null)
                        {
                            foreach (ChannelEndpointElement cee in clientSection.Endpoints)
                            {
                                if (cee.Name == Constantes.NameChannelEndpoint)
                                {
                                    cee.Address = newEndPoint;
                                    configWeb.Save();
                                    res = true;
                                    break;
                                }
                            }
                            return res;
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    return res;
                }
        #endregion

                [OverrideAuthorizeAttribute(Roles = Constante.roleSuperAdmin + "," + Constante.roleAdmin + "," + Constante.roleAdminFileExcel)]
                [HttpPost]
                public ActionResult Upload(string _pathFile, string Password, string _IDParamsCurrent)
                {
                    string FullPAth = Path.Combine(_pathFile.Trim(), Constante.NameFileExcel);
                    string msgErr=string.Empty;
                    //-store
                    if (string.IsNullOrEmpty(Password))//---aera admin
                    {
                         Password="default";
                       ParamsFileDTO _paramsFileDTO=new ParamsFileDTO();
                        _paramsFileDTO.PathFileExcel=FullPAth;
                         _paramsFileDTO.PasswordFileExcel = Password;
                         _paramsFileDTO.DateUpdatedFile = DateTime.Now;
                         //_paramsFileDTO.DateUpdatedPassword = DateTime.Now;

                        _paramsFileDTO.AccountUpdatedPath = User.Identity.Name;
                       // _paramsFileDTO.AccountUpdatetedPwd = User.Identity.Name;

                        string pathBDD = string.Empty;
                      ParamsFileDTO _pdto= _DataReferencePresenter.get_ParamsFile(ref msgErr);
                       // string pathBDD = _DataReferencePresenter.getPathFileExcel(ref msgErr);
                        if(_pdto!=null)
                             pathBDD = _pdto.PathFileExcel;
                        bool result=false;
                        if (string.IsNullOrEmpty(pathBDD))
                            result = _DataReferencePresenter.InsertData_FileExcel(_paramsFileDTO, ref msgErr);
                        else
                        {
                            _paramsFileDTO.ID = _pdto.ID;
                            result = _DataReferencePresenter.UpdateData_FileExcel(_paramsFileDTO, ref msgErr, false);
                        }
                       
                        if(result)
                        return Content("succes");
                        else
                            return Content(msgErr);
                    }
                    else
                    {//-update :  area admin excel
                        ParamsFileDTO _paramsFileDTO = new ParamsFileDTO();
                        _paramsFileDTO.PathFileExcel = FullPAth;
                        _paramsFileDTO.PasswordFileExcel = Password;
                       // _paramsFileDTO.DateUpdatedFile = DateTime.Now;
                        _paramsFileDTO.DateUpdatedPassword = DateTime.Now;

                       // _paramsFileDTO.AccountUpdatedPath = User.Identity.Name;
                        _paramsFileDTO.AccountUpdatetedPwd = User.Identity.Name;
                        if (!string.IsNullOrEmpty(_IDParamsCurrent))
                            _paramsFileDTO.ID = Guid.Parse(_IDParamsCurrent);
                        bool result = _DataReferencePresenter.UpdateData_FileExcel(_paramsFileDTO, ref msgErr, true);

                        if (result)
                        {
                            
                            //---------Send mail notification
                            string subject = "Interface SAPHIR / ACE VISION - Notification Mot de Passe ! ";
                             string body = "<b>Le Mot de passe du fichier Excel contenant les compteurs HT a été configuré </b><br/>";
                              body += "<p> Date : "+DateTime.Now+"</p> ";
                              body += "<p>Ceci est un mail automatique , veuillez ne pas répondre !</p>";

                              UserProfile _user;
                             int IDCurrentUser=WebSecurity.GetUserId(User.Identity.Name);
                             using (UsersContext db = new UsersContext())
                               {
                                  _user = db.UserProfiles.Where(w => w.UserId == IDCurrentUser).FirstOrDefault();
                             }

                             string toeMAil = _user.EmailID;
                             string msgErrReturn = string.Empty;
                             new Utility().SendEMail(toeMAil, subject, body, ref msgErrReturn);

                            return Content("succes");
                        }
                        else
                            return Content(msgErr);
                    }
                    //return Content("OK");
                }

               // [Authorize(Roles = Constante.roleSuperAdmin + "," + Constante.roleAdmin + "," + Constante.roleAdminFileExcel)]
                /*
                 * \author:FCO
                 * \date 21042017
                 * \deprecated
                 */ 
                [OverrideAuthorizeAttribute(Roles = Constante.roleSuperAdmin + "," + Constante.roleAdmin + "," + Constante.roleAdminFileExcel)]
                public ActionResult IndexFile()
                {
                    int IDCurrentUser=WebSecurity.GetUserId(User.Identity.Name);
                   string []tabRole_CurrentUser= Roles.GetRolesForUser(User.Identity.Name);
                   //TempData["role"] = tabRole_CurrentUser[0];
                   ViewBag.Role = tabRole_CurrentUser[0];
                    string msgErr=string.Empty;

                    ParamsFileDTO _pf = _DataReferencePresenter.get_ParamsFile(ref msgErr);
                    if (_pf != null)
                    {
                        ViewBag.Path = _pf.PathFileExcel;
                        ViewBag.IDParams = _pf.ID.ToString();
                    }
                    else
                    {
                        ViewBag.Path = string.Empty;
                        ViewBag.IDParams = string.Empty;
                    }

                   return View();
                }

                
                [OverrideAuthorizeAttribute(Roles = Constante.roleAdminFileExcel)]
                public ActionResult ManageFile()
                {
                    //TempData.Keep();
                    return View();
                }

                [OverrideAuthorizeAttribute(Roles = Constante.roleAdminFileExcel)]            
                public ActionResult ManageFileReload(string reload)
                {
                    if (!string.IsNullOrEmpty(reload))
                        TempData["MsgEndTransaction"] = null;

                    return View("ManageFile");
                }

                 /*
                 * \author:FCO
                 * \date 21042017
                **/
                [OverrideAuthorizeAttribute(Roles = Constante.roleAdminFileExcel)]
                [HttpPost]
                public ActionResult SaveContent(HttpPostedFileBase file, string valPwd)
                //public ActionResult SaveContent(FormCollection collection)
                {
                    //---pr test
                    //HttpPostedFileBase file = Request.Files[0]; ;
                    #region initialiasition Log
                    //---------------------
                    string jsonData = string.Empty;
                    //string msgErr = string.Empty;
                    //List<string> LstCTR_alreadyIntegratedNode = new List<string>();
                    string valueMeter = string.Empty;

                    PresenterLog.setValues_ofData();
                    PresenterLog.SetParametersLog();
                    //PresenterLog.TotalRead_fromNode = cptTotalLinesOKFile;
                    PresenterLog.TotalRead_fromNode = 0;
                    PresenterLog.TotalRead_fromAce = 0;
                    PresenterLog.TotalInserted_toAce = 0;
                    PresenterLog.TotalUpdated_toAce = 0;

                    PresenterLog.ExecutionMode = Constantes.ExecutionMode_Manuel;
                    PresenterLog.TypeTraitement = Constantes.TraitementIntegrationCtr;
                    PresenterLog.setValues_ofData();
                    Log.MonitoringLogger.Info(DateTime.Now + " -  Initialialisation du traitement !");

                    PresenterLog.setValues_ofData();
                    int NbreInsert = 0;
                    int NbreErreur = 0;
                    //string Errmess = string.Empty;


                    //--------------------
                    #endregion

                    string msgErr = string.Empty;                 
                    _lstCTRHTDTO.Clear();
                    //bool result = getInfosFileExcel(out msgErr, file);
                    bool result = getInfosFileExcel(out msgErr, file,valPwd);
                    StringBuilder SB = new StringBuilder();
                    int cptInserted = 0;
                    int cptUpdated = 0;
                    int cptFailed = 0;
                    int cptTotalLinesOKFile = _lstCTRHTDTO.Count();
                    string msg = string.Empty;

                    #region mail
                        UserProfile _UserExcel = Session["AccountFileExcel"] as UserProfile;
                        if (_UserExcel != null)
                            _mailAdminExcel = _UserExcel.EmailID;
                        else
                        {
                            UserProfile _userAccountExcel;
                            using (UsersContext db = new UsersContext())
                            {
                                Role _role = db.Roles.Where(r => r.RoleName == Constante.roleAdminFileExcel).FirstOrDefault();
                                webpages_UsersInRoles wp = db.webpages_UsersInRole.Where(w => w.RoleId == _role.RoleId).FirstOrDefault();
                                _userAccountExcel = db.UserProfiles.Where(u => u.UserId == wp.UserId).FirstOrDefault();

                                _mailAdminExcel = _userAccountExcel.EmailID;
                            }
                        }
                    #endregion

                        PresenterLog.TotalRead_fromNode = cptTotalLinesOKFile;
                        PresenterLog.setValues_ofData();
                    //msgErr = "aucun compteur";
                    string headerMsg = "Mise à Jour Des compteurs ";
                    if (string.IsNullOrEmpty(msgErr))
                    {
                        if (_lstCTRHTDTO.Count() > 0)
                        {
                            foreach (var item in _lstCTRHTDTO)
                            {
                                bool op = false;
                                CTRHTDTO ctrHT = _meterPresenter.getCTRHT_ofSerialNumber(item.SERIALNUMBER, out msgErr);
                                if (ctrHT != null)
                                {
                                    op = _meterPresenter.updateMetertoBdd(item, out msgErr);
                                    if (op)
                                        cptUpdated++;
                                    //---log
                                    //Log.MonitoringLogger.Info(string.Format("Mise à jour Infos du compteur {0}", item.SERIALNUMBER));
                                    SB.Append(item.SERIALNUMBER + " | ");
                                }
                                else
                                {
                                    op = _meterPresenter.insertMetertoBdd(item, out msgErr);
                                    if (op)
                                        cptInserted++;
                                }

                                //----pr test
                                //msgErr = "echec";
                                if (!string.IsNullOrEmpty(msgErr))
                                {
                                    cptFailed++;
                                    SB.Append(string.Format("N°Compteur : {0} - Message : {1} | ", item.SERIALNUMBER, msgErr));
                                }
                            }

                            PresenterLog.TotalInserted_toAce = cptInserted; //----nouvel ajout
                            PresenterLog.TotalUpdated_toAce = cptUpdated;//----modif ctr
                            PresenterLog.setValues_ofData();

                            if (SB.Length > 0)
                            {
                                if (cptFailed > 0)
                                    Log.MonitoringLogger.Info("Echec Operation - " + SB.ToString().Remove(SB.Length - 2));
                                else
                                    Log.MonitoringLogger.Info(headerMsg + SB.ToString().Remove(SB.Length - 2));

                                //---send notif
                                string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                                string msgEmailReturn = string.Empty;
                                string msgBody = "<h3>Résultat du traitement : Intégration de compteurs HT ...</h3>"
                                                 + "<p><b>Date Execution :</b> " + DateTime.Now + "</p>"
                                                 + "<p><b>Nombre total de compteurs ajoutés : </b>" + cptInserted + "</p>"
                                                 + "<p><b>Nombre total de compteurs modifiés  : </b>" + cptUpdated + "</p>"
                                                 + "<p><i>" + SB.ToString().Remove(SB.Length - 2) + "</i></p>"
                                                 + "<hr/>"
                                                 + "<p style='font-weight:bold;font-style:italic'>Nombre total de compteurs HT en base : " + _meterPresenter.getAllCTRHT(out msgEmailReturn).Count() + "</p>"

                                               + footer;
                                new Utility().SendEMail(_mailAdminExcel, Constantes.ObjectNotification, msgBody, ref msgEmailReturn);
                            }

                            if (cptInserted > 0 && cptFailed == 0 && cptUpdated == 0)
                            {
                                //---send notif
                                string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                                string msgEmailReturn = string.Empty;
                                string msgBody = "<h3>Résultat du traitement : Intégration de compteurs HT ...</h3>"
                                                 + "<p><b>Date Execution :</b> " + DateTime.Now + "</p>"
                                                 + "<p><b>Nombre total de compteurs ajoutés : </b>" + cptInserted + "</p>"
                                                 + "<hr/>"
                                                 + "<p style='font-weight:bold;font-style:italic'>Nombre total de compteurs HT en base : " + _meterPresenter.getAllCTRHT(out msgEmailReturn).Count() + "</p>"

                                               + footer;
                                new Utility().SendEMail(_mailAdminExcel, Constantes.ObjectNotification, msgBody, ref msgEmailReturn);
                            }

                            Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");
                        }
                    }
                    else
                    {
                        //file ctr empty
                        Log.MonitoringLogger.Info("ECHEC FICHIER EXCEL - "+msgErr);
                        //---send notif
                        string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                        string msgEmailReturn = string.Empty;
                        string msgBody = "<h3>Résultat du traitement : Intégration de compteurs HT ...</h3>"
                                         + "<p><b>Date Execution :</b> " + DateTime.Now + "</p>"                                  
                                         + "<p><i> ECHEC FICHIER EXCEL - "+msgErr+ "</i></p>"                                         
                                       + footer;
                        new Utility().SendEMail(_mailAdminExcel, Constantes.ObjectNotification, msgBody, ref msgEmailReturn);
                    }
                    
                    
                    if (cptInserted == 0 && cptUpdated == 0 && (cptFailed > 0 || cptFailed==0))
                        msg = "Echec du traitement de l'opération!";
                    else
                        if(cptFailed>0 && (cptInserted > 0 && cptUpdated > 0))
                           msg=string.Format("Echec -Traitement terminé avec {0} erreur(s) ",cptFailed);
                        else
                            if(cptFailed==0 && (cptInserted==0 || cptUpdated>0))
                                msg = string.Format("Traitement terminé avec succès ! Cependant {0} compteurs(s) mis à jour en base ", cptUpdated);
                            else
                                msg = "Traitement terminé avec succès !";

                    /*
                    if(cptUpdated>0)
                        msg=string.Format("Traitement terminé avec succès ! Cependant {0} compteurs(s) mis à jour en base ",cptUpdated);
                    else
                        msg="Traitement terminé avec succès !";
                    */

                    TempData["MsgEndTransaction"] = msg;
                    //ViewBag.MsgEndTransaction = msg;

                    return View("ManageFile");
                }

                [OverrideAuthorizeAttribute(Roles = Constante.roleAdminFileExcel)]
                [HttpPost]
                public JsonResult DisplayContent(HttpPostedFileBase file)
                //public JsonResult DisplayContent(string file)
                {
                    //._lstCTRHTDTO
                    string msgErr=string.Empty;

                    //bool result = false;
                    _lstCTRHTDTO.Clear();
                    //---pr test a reviser later 
                    var Pass = "123456";
                    bool result = getInfosFileExcel(out msgErr, file,Pass);
                    dynamic msgJSon = "OK";
               
                     if (!result)
                    {
                        MessageFile msgFile = new MessageFile()
                        {
                            detailsError = msgErr,
                            detailsResultat=null,
                            Resultat = "KO"
                        };
                        msgJSon = msgFile;
                        /*
                        msgJSon = "{"
                            + "Message : [{msgErr:" + msgErr
                                + " , result: KO }"
                                + "]}";
                        */
                    }
                    else
                    {
                        MessageFile msgFile = new MessageFile()
                        {
                            detailsError = msgErr,
                            detailsResultat = _lstCTRHTDTO,
                            Resultat="OK"
                        };
                        msgJSon = msgFile;
                        //msgJSon = _lstCTRHTDTO;
                    }
                    return Json(msgJSon, JsonRequestBehavior.AllowGet);
                }

                public JsonResult GetPathFile()
                {
                    string msgErr = string.Empty;                    
                    return Json(_DataReferencePresenter.getPathFileExcel(ref msgErr),JsonRequestBehavior.AllowGet);
                }


                //bool getInfosFileExcel(out string msgErr, HttpPostedFileBase file)
                bool getInfosFileExcel(out string msgErr, HttpPostedFileBase file,string Pwd)
                {
                    msgErr = string.Empty;
                    string strNewPath = string.Empty;
                    string nameFile = string.Empty;
                    var WFile = new Microsoft.Office.Interop.Excel.Application();
                    Microsoft.Office.Interop.Excel.Range range;
                    Microsoft.Office.Interop.Excel.Workbook Wbook=null;
                    try
                    {
                        string strFileType = System.IO.Path.GetExtension(file.FileName).ToString().ToLower();

                        //string nameFile = file.FileName;
                        string path = Path.Combine(Server.MapPath("~/Views/Account/UploadedExcel"),
                                                   Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        nameFile = new FileInfo(file.FileName).Name;
                        strNewPath = Server.MapPath("~/Views/Account/UploadedExcel/" + nameFile);

                        //Microsoft.Office.Interop.Excel.Workbook Wbook = WFile.Workbooks.Open(strNewPath, ReadOnly: true, Password: PwdFileExcel);
                       //Wbook = WFile.Workbooks.Open(strNewPath, ReadOnly: true);
                        Wbook = WFile.Workbooks.Open(strNewPath, ReadOnly: true, Password: Pwd);
                   
                        
                        //not open file pour user
                        //Wbook.Close();

                        //GET fisrt worksheet
                        Microsoft.Office.Interop.Excel.Worksheet sheet = (Microsoft.Office.Interop.Excel.Worksheet)Wbook.Sheets[1];
                        range = sheet.UsedRange;
                        //range = sheet.di;
                        object[,] cellValues = (object[,])range.Value2;
                        int cols = range.Columns.Count;
                        int rows = range.Rows.Count;
                        StringBuilder SBDoublons=new StringBuilder();

                        for (int i = 2; i <= rows; i++)
                        {
                            CTRHTDTO _ctrHT = new CTRHTDTO();
                            try
                            {
                                if (cellValues[i, 1] != null)//---manage lines emty !
                                {
                                    _ctrHT.SERIALNUMBER = cellValues[i, 1].ToString();
                                    _ctrHT.PASSWORD_READER = cellValues[i, 2].ToString();
                                    _ctrHT.PASSWORD_LABO = cellValues[i, 3] != null ? cellValues[i, 3].ToString() : string.Empty;
                                    _ctrHT.TYPEMETER = cellValues[i, 4].ToString();
                                    
                                    //_ctrHT.FIRMWARE = cellValues[i, 5].ToString();
                                    double versionFirmWare;
                                    bool bFirm = double.TryParse(cellValues[i, 5].ToString(),out versionFirmWare);
                                    if (bFirm)
                                        _ctrHT.FIRMWARE = versionFirmWare.ToString();
                                    else
                                    {
                                        msgErr = "Format Colonne FirmWare incorrect";
                                        return false;
                                    }

                                    //_ctrHT.versionFirmWare = (double)(cellValues[i, 4]);
                                    //--pr test -  adelete
                                    //_ctrHT.versionFirmWare = (double)(cellValues[i, 8]);

                                    //doublons
                                    //les doublons(numserie) d'une meme feuille sont ignorés
                                    CTRHTDTO ctrExist = _lstCTRHTDTO.Find(f => f.SERIALNUMBER == _ctrHT.SERIALNUMBER);
                                    if (ctrExist == null)
                                        _lstCTRHTDTO.Add(_ctrHT);
                                    else
                                        SBDoublons.Append("Ligne N °"+i+" - N°Compteur : "+_ctrHT.SERIALNUMBER + " | ");
                                }
                                else
                                    continue;
                            }
                            catch (Exception ex)
                            {
                                msgErr = ex.Message;
                                //-----------send email account Password
                                // _util.SendMail_Notification(_lstMailFileExcel,true,Constantes.ObjectNotification,ex.Message,out msErr);
                                //14122016
                                string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";

                                //new Utility().SendEMail(_lstMailFileExcel, Constantes.ObjectNotification, msgErr + footer, ref msgErrReturn);
                                //---------------------------------------
                                return false; ;
                            }

                        }

                        if (SBDoublons.Length > 0)
                        {
                            Log.MonitoringLogger.Info("Fichier Excel DOUBLONS IGNORES - " + SBDoublons.ToString().Remove(SBDoublons.Length - 2));
                            //---send notif
                            string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                            string msgEmailReturn = string.Empty;
                            string msgBody = "<h3>Résultat du traitement : Intégration de compteurs HT ...</h3>"
                                             + "<p><b>Date Execution :</b> " + DateTime.Now + "</p>"

                                             + "<p><b>Fichier Excel DOUBLONS IGNORES </b></p>"
                                             + "<p><i>" + SBDoublons.ToString().Remove(SBDoublons.Length - 2) + "</i></p>"
                                             + "<hr/>"
                                            

                                           + footer;
                            new Utility().SendEMail(_mailAdminExcel, Constantes.ObjectNotification, msgBody, ref msgEmailReturn);

                        }

                        if (_lstCTRHTDTO.Count() == 0)
                        {
                            msgErr = "Aucun compteur dans ce fichier !";
                            return false;
                        }
                        /*
                         *\todo dont forget gestion ligne vides file excel
                         */
                        return true;
                    }
                    catch (System.Runtime.InteropServices.COMException cex)
                    {
                        msgErr = cex.Message;
                        //-----------send email account Password
                        Log.MonitoringLogger.Info("ECHEC -Fichier Excel : " +msgErr);
                        //---send notif
                        string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                        string msgEmailReturn = string.Empty;
                        string msgBody = "<h3>Résultat du traitement : Intégration de compteurs HT ...</h3>"
                                         + "<p><b>Date Execution :</b> " + DateTime.Now + "</p>"

                                         + "<p><b>ECHEC -Fichier Excel </b></p>"
                                         + "<p><i>" + msgErr + "</i></p>"
                                         + "<hr/>"


                                       + footer;
                        new Utility().SendEMail(_mailAdminExcel, Constantes.ObjectNotification, msgBody, ref msgEmailReturn);

                        //---------------------------------------
                    }
                    catch (Exception ex)
                    {
                        msgErr = ex.Message;
                        //-----------send email account Password
                        Log.MonitoringLogger.Info("ECHEC -Fichier Excel : " + msgErr);
                        //---send notif
                        string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                        string msgEmailReturn = string.Empty;
                        string msgBody = "<h3>Résultat du traitement : Intégration de compteurs HT ...</h3>"
                                         + "<p><b>Date Execution :</b> " + DateTime.Now + "</p>"

                                         + "<p><b>ECHEC -Fichier Excel </b></p>"
                                         + "<p><i>" + msgErr + "</i></p>"
                                         + "<hr/>"


                                       + footer;
                        new Utility().SendEMail(_mailAdminExcel, Constantes.ObjectNotification, msgBody, ref msgEmailReturn);

                        //---------------------------------------
                    }
                    finally
                    {
                        if (System.IO.File.Exists(strNewPath))
                            System.IO.File.Delete(strNewPath);

                        if(Wbook!=null)
                        Wbook.Close();
                    }

                    return false;
                }
    }
}
