using ACEVISION.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Console_ITRON_SAPHIRv2CIE
{
    public static class Presenter
    {
        public static int TotalRead_fromAce=0;
        public static int TotalInserted_toNode = 0;//-1;
        public static int TotalRead_fromNode = 0;//-1;
        public static int TotalInserted_toAce = 0;
        public static int TotalUpdated_toAce = 0;
        public static string ExecutionMode=string.Empty;
        public static string TypeTraitement=string.Empty;
        public static string PeriodeFacturation = string.Empty;
        //public static string ExecutionID = Guid.NewGuid().ToString();
        //23122016
        public static string ExecutionID = string.Empty;

        public static int CptExecution = 1;


        public static string FooterMail = "Ceci est un mail automatique , veuillez ne pas répondre !";

        public static void SetParametersLog(string[] tabParameter)
        {
            #region variables
                string path = string.Empty;
                string server = string.Empty;
                string user = string.Empty;
                string pwd = string.Empty;
                string serverSmtp = string.Empty;
                string MailDestinataire = string.Empty;
                string Bdd = string.Empty;
                try
                {
                    path = tabParameter[0];
                    server = tabParameter[1];
                    Bdd = tabParameter[2];
                    user = tabParameter[3];
                    pwd = tabParameter[4];
                    serverSmtp = tabParameter[5];
                    MailDestinataire = tabParameter[6];


                    //-----------------change chaine de connexion du File
                    string msgErr = string.Empty;
                    bool b=changeConnexionString(server, Bdd, user, pwd,out msgErr);
                    if (b)
                    {
                        Console.WriteLine("Chaine de connection mise à jour !");

                        bool bCrypt = encryptConnexionString(out msgErr);
                        if (bCrypt)
                        {
                            Console.WriteLine("Chaine de connection cryptée !");
                            ResetConfigMechanism();
                        }
                        else
                            Console.WriteLine("Erreur : {0} , Echec Cryptage Chaine de connection !", msgErr);
                    }
                    else
                    {
                        Console.WriteLine("Erreur : {0} , Echec Mise à jour Chaine de connection !", msgErr);
                    }

                    
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }

           #endregion

              

                log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();

            var x = h.GetCurrentLoggers();

            var lstAppenders = h.GetAppenders().ToList();


            foreach (var item in lstAppenders)
            {
                #region File
                if (item is log4net.Appender.RollingFileAppender)
                {
                    if (item.Name.Equals(Constantes.RollingFileAppender))
                    {
                        try
                        {
                            log4net.Appender.RollingFileAppender fa = (log4net.Appender.RollingFileAppender)item;
                            string locationFileLog = path + @"\Interface_SAPHIR_ACEVISION_Log.log";
                            fa.File = locationFileLog;
                            fa.ActivateOptions();

                            Console.WriteLine("fullPath : " + locationFileLog);
                        }
                        catch (Exception ex)
                        {

                            ACEVISION.Common.Log.MonitoringLogger.Error(DateTime.Now + " - " + ex.Message);
                        }
                    }

                }
                #endregion

                #region BDD
                if (item is log4net.Appender.AdoNetAppender)
                {
                    if (item.Name.Equals(Constantes.AdoNetAppender))
                    {
                        try
                        {
                            log4net.Appender.AdoNetAppender fBDD = (log4net.Appender.AdoNetAppender)item;

                            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                            builder.DataSource = server;
                            builder.InitialCatalog = Bdd;
                            builder.PersistSecurityInfo = true;
                            builder.UserID = user;
                            builder.Password = pwd;
                            fBDD.ConnectionString = builder.ConnectionString;

                            //----------
                            setValues_ofData();
                            //-------------
                            fBDD.ActivateOptions();


                        }
                        catch (Exception ex)
                        {

                            ACEVISION.Common.Log.MonitoringLogger.Error(DateTime.Now + " - " + ex.Message);
                        }
                    }

                }
                #endregion


                #region Mail
                if (item is log4net.Appender.SmtpAppender)
                {
                    if (item.Name.Equals(Constantes.SmtpAppender))
                    {
                        try
                        {
                            log4net.Appender.SmtpAppender fMail = (log4net.Appender.SmtpAppender)item;

                            fMail.SmtpHost = serverSmtp;
                            fMail.To = MailDestinataire;

                            fMail.ActivateOptions();


                        }
                        catch (Exception ex)
                        {

                            ACEVISION.Common.Log.MonitoringLogger.Error(DateTime.Now + " - " + ex.Message);
                        }
                    }

                }
                #endregion

            }

   
        }

        public static void setValues_ofData()
        {

            try
            {
                log4net.GlobalContext.Properties["TotalRead_fromAce"] = Presenter.TotalRead_fromAce;
                log4net.GlobalContext.Properties["TotalInserted_toNode"] = Presenter.TotalInserted_toNode;
                log4net.GlobalContext.Properties["TotalRead_fromNode"] = Presenter.TotalRead_fromNode;
                log4net.GlobalContext.Properties["TotalInserted_toAce"] = Presenter.TotalInserted_toAce;
                log4net.GlobalContext.Properties["TotalUpdated_toAce"] = Presenter.TotalUpdated_toAce;
                log4net.GlobalContext.Properties["ExecutionMode"] = Presenter.ExecutionMode;
                log4net.GlobalContext.Properties["TypeTraitement"] = Presenter.TypeTraitement;
                log4net.GlobalContext.Properties["PeriodeFacturation"] = Presenter.PeriodeFacturation;
                //log4net.GlobalContext.Properties["ExecutionID"] = ExecutionID;
                log4net.GlobalContext.Properties["ExecutionID"] = Presenter.ExecutionID;
                log4net.GlobalContext.Properties["FooterMail"] = Presenter.FooterMail;

                log4net.GlobalContext.Properties["CptExecution"] = Presenter.CptExecution;
                
                
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
             
        }

        /// <summary>
        /// methode servant à changer et crypter la chaine de connction du fichier de configuration
        /// </summary>
        /// <param name="server"></param>
        /// <param name="Bdd"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="msgErr"></param>
        /// <returns></returns>
        public static bool changeConnexionString(string server,string Bdd,string user,string pwd,out string msgErr)
        {
            msgErr = string.Empty;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = server;
                builder.InitialCatalog = Bdd;
                builder.PersistSecurityInfo = true;
                builder.UserID = user;
                builder.Password = pwd;
                string _connexionString = builder.ConnectionString;

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var connectionStringSection = (ConnectionStringsSection)config.GetSection(Constantes.sectionConnexionConfig);
                connectionStringSection.ConnectionStrings[Constantes.SAPHIRCOMConnexionStringName].ConnectionString = _connexionString;
                config.Save();
                ConfigurationManager.RefreshSection(Constantes.sectionConnexionConfig);


                return true;
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
            }

            return false;
        }

        public static bool encryptConnexionString(out string msgErr)
        {
             msgErr = string.Empty;
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConfigurationSection cnxSection = config.GetSection(Constantes.sectionConnexionConfig);
                if (cnxSection != null)
                {
                    //if (!cnxSection.SectionInformation.IsProtected) //Always encryter au k ou la chaine de connexion change...
                    //{
                        if (!cnxSection.IsReadOnly() && !cnxSection.SectionInformation.IsProtected)
                        {
                            cnxSection.SectionInformation.ProtectSection(Constantes.ProviderCryptDecrypt);
                            cnxSection.SectionInformation.ForceSave = true;
                            config.Save(ConfigurationSaveMode.Full);
                            //--------
                             
                            return true;
                        }
                    //}
                    //else
                      //  msgErr="Cryptage déjà effectué"!
                }
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
            }
            return false;
        }

        /// <summary>
        /// refresh file de config
        /// </summary>
        private static void ResetConfigMechanism()
        {
            typeof(ConfigurationManager)
                .GetField("s_initState", BindingFlags.NonPublic |
                                         BindingFlags.Static)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", BindingFlags.NonPublic |
                                            BindingFlags.Static)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName ==
                            "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", BindingFlags.NonPublic |
                                       BindingFlags.Static)
                .SetValue(null, null);
        }
    }
}
