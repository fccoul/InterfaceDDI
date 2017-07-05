using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;
using log4net.Core;
using log4net;
using System.IO;
using System.Globalization;
using System.IO.Compression;

using System.Net.Mail;
using System.Net;
 

namespace ACEVISION.Common
{
    public class Utilitaires
    {

        #region Traitements (logique métier)

        #region Message box

        public static void AfficherErreur(string pTitre, string pUserMessage)
        {
            AfficherErreur(pTitre, pUserMessage, pUserMessage);
        }

        public static void AfficherErreur(string pTitre, string pUserMessage, string pSystemMessage)
        {
            //AppLogger Log = LogManager.GetLogger("MyClass");

            //log4net.Config.XmlConfigurator.Configure();
            //Log.TraceError(pSystemMessage);

            ILog _log = log4net.LogManager.GetLogger(Assembly.GetCallingAssembly(), "MyClass");

            log4net.Config.XmlConfigurator.Configure();
            _log.Error(pUserMessage);

            //----------------FCO
           //Presenter
            //Log.ExceptionLogger.Error(pUserMessage);
            //--------------------
            ShowMessage(pTitre, pUserMessage, MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        public static void AfficherInformation(string pTitre, string pUserMessage)
        {
            AfficherInformation(pTitre, pUserMessage, pUserMessage);
        }
        public static void AfficherInformation(string pTitre, string pUserMessage, string pSystemMessage)
        {
            ShowMessage(pTitre, pUserMessage, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void AfficherAvertissement(string pTitre, string pUserMessage)
        {
            AfficherAvertissement(pTitre, pUserMessage, pUserMessage);
        }

        public static void AfficherAvertissement(string pTitre, string pUserMessage, string pSystemMessage)
        {
            ShowMessage(pTitre, pUserMessage, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


        public static void AfficherExclamation(string pTitre, string pUserMessage)
        {
            AfficherExclamation(pTitre, pUserMessage, pUserMessage);
        }

        public static void AfficherExclamation(string pTitre, string pUserMessage, string pSystemMessage)
        {
            ShowMessage(pTitre, pUserMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static bool AfficherQuestion(string pTitre, string pUserMessage)
        {
            return AfficherQuestion(pTitre, pUserMessage, pUserMessage);
        }

        public static bool AfficherQuestion(string pTitre, string pUserMessage, string pSystemMessage)
        {
            DialogResult resultat = ShowMessage(pTitre, pUserMessage, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (resultat == DialogResult.Yes)
                return true;
            return false;
        }

        private static DialogResult ShowMessage(string pTitre, string pUserMessage, MessageBoxButtons MessageButton,
            MessageBoxIcon MessageIcon)
        {
            return MessageBox.Show(pUserMessage, pTitre, MessageButton, MessageIcon);
        }

        //public static string HashString(string stringToHash)
        //{
        //    byte[] data = Encoding.UTF8.GetBytes(stringToHash);
        //    using (HashAlgorithm sha = new SHA256Managed())
        //    {
        //        byte[] encryptedBytes = sha.TransformFinalBlock(data, 0, data.Length);
        //        return Convert.ToBase64String(sha.Hash);
        //    }
        //}

        #endregion

        #region LOG4NET

        public static class LogFactory
        {
            private static ILog logger;

            public static void ConfigureLogger()
            {
                log4net.Config.XmlConfigurator.Configure();
                GetLogger();
            }

            public static void GetLogger(Assembly assembly, string name)
            {
                logger = LogManager.GetLogger(assembly, name);
            }
            public static void GetLogger()
            {
                logger = LogManager.GetLogger(typeof(LogFactory));
            }

            public static void GetLogger(Type type)
            {
                logger = LogManager.GetLogger(type);
            }

            public static void GetLogger(string name)
            {
                logger = LogManager.GetLogger(Assembly.GetCallingAssembly(), name);
            }
            public static void LogDebug(string message)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(message);
                }
            }
            public static void LogError(string message)
            {
                if (logger.IsErrorEnabled)
                {
                    logger.Error(message);
                }
            }

            public static void LogWarn(string message)
            {
                if (logger.IsWarnEnabled)
                {
                    logger.Warn(message);
                }
            }

            public static void LogInfo(string message)
            {
                if (logger.IsInfoEnabled)
                {
                    logger.Info(message);
                }
            }
        }

        #endregion

        #region Cryptage d'un mot de passe

        public static string HashString(string stringToHash)
        {
            byte[] data = Encoding.UTF8.GetBytes(stringToHash);
            using (HashAlgorithm sha = new SHA256Managed())
            {
                byte[] encryptedBytes = sha.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(sha.Hash);
            }
        }

        #endregion

        #region Calculs Synchro automatique

        private static DateTime? DateDeLancementAujourdhui(int pHeureLancement_Hour, int pHeureLancement_Minuts, DateTime pDateDebutValidite)
        {
            DateTime? Resultat = null;

            DateTime Today = DateTime.Now;
            DateTime laDateDeLancementToday = new DateTime(Today.Year, Today.Month, Today.Day, pHeureLancement_Hour, pHeureLancement_Minuts, 0);

            if (pDateDebutValidite < laDateDeLancementToday)
                Resultat = laDateDeLancementToday;
            else
                Resultat = new DateTime(pDateDebutValidite.Year, pDateDebutValidite.Month, pDateDebutValidite.Day, pHeureLancement_Hour, pHeureLancement_Minuts, 0);

            return Resultat;
        }
        public static DateTime? CalculerDateLancementSynchroSuivante(DateTime? pDateDerniereSynchro, EnumPeriodiciteSynchroAutomatique pPeriodicite,
                          int pHeureLancement_Hour, int pHeureLancement_Minuts, DateTime pDateDebutValidite)
        {
            DateTime? Resultat = null;

            if (pHeureLancement_Hour > 23 || pHeureLancement_Hour < 0) pHeureLancement_Hour = 6;
            if (pHeureLancement_Minuts > 59 || pHeureLancement_Minuts < 0) pHeureLancement_Minuts = 0;

            //- En fonction de la périodicité et de la date de dernière exécution
            if (pDateDerniereSynchro != null && pDateDerniereSynchro.HasValue)
            {
                DateTime laDate = DateTime.Now;
                switch (pPeriodicite)
                {
                    case EnumPeriodiciteSynchroAutomatique.Journalier:
                        laDate = pDateDerniereSynchro.Value.AddDays(1);
                        break;
                    case EnumPeriodiciteSynchroAutomatique.Hebdomadaire:
                        laDate = pDateDerniereSynchro.Value.AddDays(7);
                        break;
                    case EnumPeriodiciteSynchroAutomatique.Mensuel:
                        laDate = pDateDerniereSynchro.Value.AddMonths(1);
                        break;
                    default:
                        break;
                }

                if (laDate.Hour == pHeureLancement_Hour && laDate.Minute == pHeureLancement_Minuts)
                    Resultat = laDate;
                else
                    Resultat = new DateTime(laDate.Year, laDate.Month, laDate.Day, pHeureLancement_Hour, pHeureLancement_Minuts, 0);
            }
            else
            {
                Resultat = DateDeLancementAujourdhui(pHeureLancement_Hour, pHeureLancement_Minuts, pDateDebutValidite);
            }

            return Resultat;
        }

        #endregion

        #region Formatage Chiffres pour affichage

        public static string FormatNumberSeparateurEspace(double theNumber)
        {
            string Resultat = "0";

            if (theNumber != 0)
            {
                //NumberFormatInfo theProvider = CultureInfo.InvariantCulture.NumberFormat;
                NumberFormatInfo theProvider = new NumberFormatInfo()
                {
                    NumberGroupSeparator = " ",
                    NumberGroupSizes = new int[] { 3 }
                };

                Resultat = theNumber.ToString("#,#", theProvider);
            }

            return Resultat;
        }

        #endregion

        //17112016
        public void SendMail_Notification(string listMailCollaborateurs,bool isNotifError,string objetMsg, string description,out string msgErr)
        {
            try
            {
                msgErr = string.Empty;

                string MailAdmin = " adminDDI@cie.ci";
                MailMessage email = new MailMessage();


                #region message               

                string MSG_BROWSEFile = "<html>"
                               + "<body>"
                                      + "<p>L'erreur suivante a été détectée lors de la lecture du fichier Excel de compteurs </p>"                                          
                                             + "<p><i><u>Description de l'erreur :</u>" + description + "</i></p>"
                                       + "<p>Ceci est un Email automatique, veuillez ne pas repondre !<p>"
                                       + "<p>Cordialement.</p>"

                               + "</body>"
                           + "</html>";

                string MSG_CTRCreated = "<html>"
                              + "<body>"
                                     + "<p>NOUVEAU COMPTEUR CREE DANS ACE VISION</p>"
                                      + "<p><i>N° :" + description + "</i>  ,  Veuillez vous assurez de la pertinence,intégrité et conformité des informations sensibles rattachées</p>"
                                      + "<p>Ceci est un Email automatique, veuillez ne pas repondre !<p>"
                                      + "<p>Cordialement.</p>"

                              + "</body>"
                          + "</html>";
                #endregion

                if (!string.IsNullOrEmpty(listMailCollaborateurs))
                {
                    if (listMailCollaborateurs.Contains(";"))
                    {
                        string laListe = listMailCollaborateurs.Substring(0, listMailCollaborateurs.Length - 1);

                        string[] tabMail = laListe.Split(';');
                        for (int i = 0; i < tabMail.Length; i++)
                        {
                            email.To.Add(tabMail[i]);
                        }
                    }
                    else
                    {
                        //email.To.Add(new System.Net.Mail.MailAddress(laListe));
                        email.To.Add(new System.Net.Mail.MailAddress(listMailCollaborateurs));
                    }

                    
                    email.Subject = objetMsg;
                    if (isNotifError)
                        email.Body = MSG_BROWSEFile;
                    else
                        email.Body = MSG_CTRCreated;

                    email.IsBodyHtml = true;
                  
                    //email.BodyFormat = MailFormat.Html;

                    email.From = new System.Net.Mail.MailAddress(MailAdmin);
                    //email.From = MailAdmin;

                    email.Priority = MailPriority.High;

                   System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                   string serverSMTP = System.Configuration.ConfigurationManager.AppSettings["serverSMTP"];
                   smtp.Host = "smtp.gmail.com"; //mon server SMTP
                   //SmtpMail.SmtpServer = serverSMTP + ":465";
                    try
                    {
                        //---A delete later 
                        
                        smtp.Port = 587;
                         NetworkCredential NetworkCred =new System.Net.NetworkCredential("franckcool15@gmail.com", "xxxx");
                         smtp.UseDefaultCredentials = true;
                         smtp.Credentials = NetworkCred;
                        smtp.EnableSsl = true;
                        //------------------------
                        smtp.Send(email);
                    }
                    catch (Exception exp)
                    {
                        msgErr = exp.Message;
                        //Utilitaires.AfficherAvertissement("Alerte Mail", exp.Message + "\n  Veuillez contacter votre administrateur !");
                        //throw new Exception("SmtpServer must be specified"); 
                    }

                }
                else //---liste Mail dest vide ???
                {
                }
              
            }
            catch (Exception exp)
            {
                //Utilitaires.AfficherErreur("Alerte Mail", "Echec d'envoi de courrier : \n" + exp.Message);
                msgErr= exp.Message;
            }
        }

        #endregion


       

    }
}
