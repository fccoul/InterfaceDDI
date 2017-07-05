using ACEVISION.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Common
{
    public static class PresenterLog
    {
        public static int TotalRead_fromAce = 0;
        public static int TotalInserted_toNode = 0;
        public static int TotalRead_fromNode = 0;
        public static int TotalInserted_toAce = 0;
        public static int TotalUpdated_toAce = 0;
        public static string ExecutionMode = string.Empty;
        public static string TypeTraitement = string.Empty;
        public static string PeriodeFacturation = string.Empty;
        // static string ExecutionID = Guid.NewGuid().ToString();
        public static string ExecutionID = string.Empty;

        public static int CptExecution = 1;
        public static string FooterMail = "Ceci est un mail automatique , veuillez ne pas répondre !";

        public static void SetParametersLog()
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
                //path = tabParameter[0];
            

                string connStr = ConfigurationManager.ConnectionStrings[Constantes.SAPHIRCOMConnexionStringName].ConnectionString;
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStr);
                server = builder.DataSource;
                Bdd = builder.InitialCatalog;
                if (!builder.IntegratedSecurity)
                {
                    user = builder.UserID;
                    pwd = builder.Password;
                }

                serverSmtp = WebConfigurationManager.AppSettings["AddressServerMailing"];
                MailDestinataire = WebConfigurationManager.AppSettings["AddressRecipient"];
 
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
               //-file ,we  take default...

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
                            if (!string.IsNullOrEmpty(user))
                            {
                                builder.UserID = user;
                                builder.Password = pwd;
                            }
                            else
                                builder.IntegratedSecurity = true;

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
                log4net.GlobalContext.Properties["TotalRead_fromAce"] = PresenterLog.TotalRead_fromAce;
                log4net.GlobalContext.Properties["TotalInserted_toNode"] = PresenterLog.TotalInserted_toNode;
                log4net.GlobalContext.Properties["TotalRead_fromNode"] = PresenterLog.TotalRead_fromNode;
                log4net.GlobalContext.Properties["TotalInserted_toAce"] = PresenterLog.TotalInserted_toAce;
                log4net.GlobalContext.Properties["TotalUpdated_toAce"] = PresenterLog.TotalUpdated_toAce;
                log4net.GlobalContext.Properties["ExecutionMode"] = PresenterLog.ExecutionMode;
                log4net.GlobalContext.Properties["TypeTraitement"] = PresenterLog.TypeTraitement;
                log4net.GlobalContext.Properties["PeriodeFacturation"] = PresenterLog.PeriodeFacturation;
                //log4net.GlobalContext.Properties["ExecutionID"] = ExecutionID;
                log4net.GlobalContext.Properties["ExecutionID"] = PresenterLog.ExecutionID;
                log4net.GlobalContext.Properties["FooterMail"] = PresenterLog.FooterMail;
                log4net.GlobalContext.Properties["CptExecution"] = PresenterLog.CptExecution;


            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }
    }
}
