using ACEVISION.Common;
using INTERFACE_ITRON_SAPHIRv2CIE.Common;
using INTERFACE_ITRON_SAPHIRv2CIE.Models;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Controllers
{
     [AllowAnonymous]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
         ConnectionDB _connectionDB = new ConnectionDB();

        public ActionResult Index()
        {
            //string _user = User.Identity.Name;

            List<SelectListItem> items_Bdd = new List<SelectListItem>();
            //------
            string connStr = ConfigurationManager.ConnectionStrings[Constantes.SAPHIRCOMConnexionStringName].ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStr);
            ViewBag.ActualServer = builder.DataSource;
            //---------
            ViewBag.listBDDs = items_Bdd;
            ViewBag.urlWebSce = getAddressEndpoint_SceWeb();
             
            return View();
             
        }
        private Uri getAddressEndpoint_SceWeb()
        {
            Uri uriWceWeb = null;
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
       
        public bool DeCryptConnexionString()
        {
            string msgErr = string.Empty;
            bool b = DecryptionSection(Constantes.sectionConnexionConfig, out msgErr);

            return b;
        }

        private bool DecryptionSection(string sectionName, out string msgErr)
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

        public JsonResult getAllDatabase_ofServer()
        {
            //ConnectionDB conn = new ConnectionDB { DataSource = ParamsBDD.DataSource, InitialCatalog = ParamsBDD.InitialCatalog, UserID = ParamsBDD.UserID, Password = ParamsBDD.Password };
            ConnectionDB conn = new ConnectionDB { DataSource = ParamsBDD.DataSource, InitialCatalog = ParamsBDD.InitialCatalog, UserID = ParamsBDD.UserID, Password = ParamsBDD.Password };
 
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
                    int i = 0;
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

        public ActionResult CheckConnexion(string NameServer, string _UserName, string _Password, short itemParam, short ModeConnexion)
        {

            string msg = string.Empty;
            string iTemTested = string.Empty;

            //if(ModeConnexion==(short)AuthenticateMode.SQL_Server)
            iTemTested = "au serveur de base de données SAPHIR...";

            // if(ModeConnexion==(int)AuthenticateMode.Windows)
            //  iTemTested="au service WEB ITRON...";

            if (ConnexionValide(itemParam, NameServer, _UserName, _Password, ModeConnexion, out msg))
                return Content(string.Format(Constante.Msg_Test_connexion, iTemTested));
            else
                return Content(string.Format(Constante.Msg_Test_connexion_failed, iTemTested, msg) + " | KO");

        }

        public bool ConnexionValide(short itemParam, string nameServer, string _UserName, string _Password, short? ModeConnexion, out string msg)
        {
            msg = string.Empty;
            bool result = false;

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

                result = isValid_ConnexionString(GetSqlConnexionStringBuilder(_connectionDB).ConnectionString, out msg);

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



         //--------------------A moving after test
           
          [HttpPost]
          public ActionResult AddParams(FormCollection collection)//20102014
        {

            SaveMonitoring();

            short typeParams = 1;
            string msgErr = "KO";

            bool b=Request.IsAjaxRequest();
             
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
              //paramServSce= short.Parse(collection["paramServSce"].ToString());
              ModeAuthenticate = short.Parse(collection["ModeAuthenticate"].ToString());
              
            
                
                if (ModelState.IsValid)
                {
                     
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
                    //?????????????????????
                                       //------------------------------
                                   
                                #endregion

                                string jsonData = "Enregistrement réussi avec succès !";
                    //return Json(jsonData);
                    //return Content(jsonData);
                    return RedirectToAction("Connecter","Connexion");
                }
                else
                    return Content(msgErr);
            //}
            //else
              //  return Content(Constante.MsgErr_CallAjax);
        }
            return null;
    }
            //------------------------------
          public bool CryptConnexionString()
          {
              string msgErr = string.Empty;
              bool b = EncryptionSection(Constantes.sectionConnexionConfig, Constantes.ProviderCryptDecrypt, out msgErr);

              return b;
          }

          private bool EncryptionSection(string sectionName, string provider, out string msgErr)
          {
              msgErr = string.Empty;
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

                  if (!string.IsNullOrEmpty(_ConnectionDB.UserID))
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

              string _user =Environment.GetEnvironmentVariable("USERNAME");
              string _hostname = Request.UserHostName + ":"+Request.UserHostAddress;


              System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
              //ConfigurationSection section = config.GetSection(Constantes.sectionMonitoringConfig);
              //var MonitoringSection = (MonitoringConfig)config.GetSection(Constantes.sectionMonitoringConfig);
              AppSettingsSection MonitoringSection = (AppSettingsSection)config.GetSection(Constantes.sectionMonitoringConfig);
              string resultkeyUser = MonitoringSection.Settings["UserConnected"].Value;
              string resultkeyEdit = MonitoringSection.Settings["Edited"].Value;

              MonitoringSection.Settings["UserConnected"].Value = _user;
              MonitoringSection.Settings["Edited"].Value = _hostname + " - " + DateTime.Now;
              config.Save(ConfigurationSaveMode.Modified);
              ConfigurationManager.RefreshSection(Constantes.sectionMonitoringConfig);
              //if (section != null)
              //{
              //   // System.Configuration.ConfigurationManager..AppSettings["LoginWebSce"];
              //}

          }

          public string getValue_ofCryptageSaphir(string _value)
          {
              string valReturn=string.Empty;
              if (!string.IsNullOrEmpty(_value))
                  valReturn = _value.Cryptage();

              return valReturn;
          }

          public string getValue_ofDECryptageSaphir(string _value)
          {

              string valReturn = string.Empty;
              if (!string.IsNullOrEmpty(_value))
                  valReturn = _value.DeCryptage();

              return valReturn;
          }

          public string DisplayChaineConnexion()
          {
              string CurrentConnectionString = string.Empty;
              System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
              ConfigurationSection section = config.GetSection(Constantes.sectionConnexionConfig);
              ConnectionStringsSection connectionStringSection = (ConnectionStringsSection)config.GetSection(Constantes.sectionConnexionConfig);
                   
               ConnectionStringSettingsCollection collectConnectionString= connectionStringSection.ConnectionStrings;

              StringBuilder sb=new StringBuilder();
              foreach (var item in collectConnectionString)
	            {
		                sb.Append(item.ToString());
	            }
               //CurrentConnectionString = String.Join(" | ", collectConnectionString.);
               CurrentConnectionString = sb.ToString();
 
              return CurrentConnectionString;
          }
    }
}
