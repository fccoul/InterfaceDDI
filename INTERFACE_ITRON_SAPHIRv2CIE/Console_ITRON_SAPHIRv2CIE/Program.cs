using ACEVISION.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Console_ITRON_SAPHIRv2CIE
{
    class Program
    {
        
        static void Main(string[] args)        
        {

            string[] tabParameter = new string[7];

            /*---21122015
             **/
            args = new string[7];
            
            args[0] = @"E:\Application\ITRON\Log";//;valueFile.Trim();
            args[1] = ".";//"10.109.240.220";//valueServer.Trim();
            args[2] = "ACE_NODE_TI_16112016";//valueBDD.Trim();
            args[3] = "sa";//valueUserName.Trim();
            args[4] = "P@ssw0rd";//valuePassword.Trim();
            args[5] = "mail.univers.ci";//valueSmtp.Trim();
            //args[6] = "cyyao@cie.ci,fhcoulibaly@cie.ci";//valueTo.Trim();
            args[6] = "fhcoulibaly@cie.ci";//valueTo.Trim();
             
            //args[0] = @"E:\Application\ITRON\Log";//;valueFile.Trim();
            //args[1] = "sodw4357";//"10.109.240.220";//valueServer.Trim();
            //args[2] = "ACE_NODE";//valueBDD.Trim();
            //args[3] = "GESADB";//valueUserName.Trim();
            //args[4] = "GESA";//valuePassword.Trim();
            //args[5] = "mail.univers.ci";//valueSmtp.Trim();
            //args[6] = "fhcoulibaly@cie.ci";//valueTo.Trim();


            if (args.Length > 0)
            {
                Console.WriteLine(DateTime.Now + " - Début du traitement...");
                Console.WriteLine(" - Initialisation des parametres...");

                for (int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine(args[i].ToString());
                }

                try
                {
                    tabParameter[0] = args[0].ToString().Trim();
                    tabParameter[1] = args[1].ToString().Trim();
                    tabParameter[2] = args[2].ToString().Trim();
                    tabParameter[3] = args[3].ToString().Trim();
                    tabParameter[4] = args[4].ToString().Trim();
                    tabParameter[5] = args[5].ToString().Trim();
                    tabParameter[6] = args[6].ToString().Trim();

                    Console.WriteLine("Traitement en cours d'execution...");

                    //get values to save in Log....
                    Presenter.setValues_ofData();
                    //update file config
                    Presenter.SetParametersLog(tabParameter);

                    //---------------
                    //----afin de logger le type de traitement à l'initialisation du traitement
                    Presenter.TypeTraitement = Constantes.TypeTraitementRec;
                    Presenter.ExecutionMode = Constantes.ExecutionMode_Auto;

                    //23122016
                    Presenter.ExecutionID = Guid.NewGuid().ToString();
                    Presenter.TotalInserted_toNode = 0;

                    Presenter.setValues_ofData();

                    Log.MonitoringLogger.Info(DateTime.Now + " -  Initialialisation du traitement !");
                    //*---------action
                    //A change later pour parameteer le mode reception/emission???
                    DateTime dateExecution = DateTime.Now;
                    DateTime  ?_DateDebut=null;
                    DateTime ?_DateFin=null;
                    bool bres=dateExecution.GetPeriodeRecherche(ref _DateDebut, ref _DateFin);
                    if (bres)
                        PresenterService.getDats_FromEOBilling_ACEVISIOn(_DateDebut.Value.ToString(), _DateFin.Value.ToString());
                    else
                    {
                        Console.WriteLine("echec de traitement : Periode de facturation invalide");
                        Log.ExceptionLogger.Error(DateTime.Now + " - bug signalé ! \n Periode de facturation invalide");
                    }
                    //Log.MonitoringLogger.Info(DateTime.Now + " -  Prg :Fin du traitement");
                    //--------------------
                    Console.WriteLine(DateTime.Now + " - Fin du traitement ");

                    //string[] tab = new string[] { "bleu", "rouge", "vert" };
                    //Console.Write(tab[4]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("echec de traitement : " + ex.Message);
                    Log.ExceptionLogger.Error(DateTime.Now + " - bug signalé ! \n " + ex.Message);
                    //23122016
                    Console.WriteLine(DateTime.Now + " - Fin du traitement ");
                    //throw;
                }
               // Log.MonitoringLogger.Info(DateTime.Now + " Fin du traitement de l'application ---logger file ExT");
            }
            else
            {
                Log.ExceptionLogger.Error(DateTime.Now + " traitement annulé , aucun paramétrage effectué");
                Console.WriteLine("traitement annulé , aucun paramétrage effectué ");
                Console.WriteLine(DateTime.Now + " - Fin du traitement ");

                //#region
                //try
                //{
                //    //------------------
                //    /*
                //    Console.WriteLine("Veuillez Fournir les paramètres de journalisation / notification :...");
                //    Console.Write("> repertoire du fichier Log... :");
                //    string valueFile = Console.ReadLine();
                //    Console.Write("> Serveur BDD... :");
                //    string valueServer = Console.ReadLine();
                //    Console.Write("> Base de Données... :");
                //    string valueBDD = Console.ReadLine();
                //    Console.Write("> Compte utilisateur... :");
                //    string valueUserName = Console.ReadLine();
                //    Console.Write("> Mot de passe... :");
                //    string valuePassword = Console.ReadLine();
                //    Console.Write("> Serveur de Messagerie... :");
                //    string valueSmtp = Console.ReadLine();
                //    Console.Write("> Adresse Mail Destinataire... :");
                //    string valueTo = Console.ReadLine();

                //    */
                //    Console.WriteLine(DateTime.Now + " - Début du traitement...");
                //    Console.WriteLine(" - Initialisation des parametres...");

                //    tabParameter[0] = @"E:\Application\ITRON\Log";//;valueFile.Trim();
                //    tabParameter[1] = "sodw4357";//"10.109.240.220";//valueServer.Trim();
                //    tabParameter[2] = "ACE_NODE";//valueBDD.Trim();
                //    tabParameter[3] = "GESADB";//valueUserName.Trim();
                //    tabParameter[4] = "GESA";//valuePassword.Trim();
                //    tabParameter[5] = "mail.univers.ci";//valueSmtp.Trim();
                //    tabParameter[6] = "cyyao@cie.ci";//valueTo.Trim();


                //    Console.WriteLine("Traitement en cours d'execution...");
                //    //get values to save in Log....
                //    Presenter.SetParametersLog(tabParameter);
                //    //------FCO
                //    //gestion emission,ill va fallir add un new paramter aukel on dira s'il s'agit de reception ou emission...
                //    //----------------
                //    //----afin de logger le type de traitement à l'initialisation du traitement
                //    Presenter.TypeTraitement = Constantes.TypeTraitementRec;
                //    Presenter.ExecutionMode = Constantes.ExecutionMode_Auto;
                //    Presenter.setValues_ofData();
                //    //---------------------
                //    Log.MonitoringLogger.Info(DateTime.Now + " -  Initialialisation du traitement !");
                //    //Presenter.setValues_ofData();
                //    //...........
                //    //Presenter.SetParametersLog(tabParameter);

                //    //*---------action
                //    //PresenterService.getDats_FromEOBilling_ACEVISIOn("2015-09-01", "2015-09-30");
                //    //A change later pour parameteer le mode reception/emission???
                //    DateTime dateExecution = DateTime.Now;
                //    DateTime? _DateDebut = null;
                //    DateTime? _DateFin = null;
                //    bool bres = dateExecution.GetPeriodeRecherche(ref _DateDebut, ref _DateFin);
                //    if (bres)
                //        PresenterService.getDats_FromEOBilling_ACEVISIOn(_DateDebut.Value.ToString(), _DateFin.Value.ToString());
                //    else
                //    {
                //        Console.WriteLine("echec de traitement : Periode de facturation invalide");
                //        Log.ExceptionLogger.Error(DateTime.Now + " - bug signalé ! \n Periode de facturation invalide");
                //    }
                //    //Log.MonitoringLogger.Info(DateTime.Now + " -  Prg :Fin du traitement");
                //    //--------------------
                //    Console.WriteLine(DateTime.Now + " - Fin du traitement ");

                //    //Log.ExceptionLogger.Error(DateTime.Now + " - bug signalé ");
                //    //Console.Write("vous avez saisi {0}",value);

                //    //Console.ReadLine();

                    
                //}
                //catch (Exception ex)
                //{
                    
                //    Console.WriteLine("echec de traitement : "+ ex.Message);
                //}
                //#endregion
            }
            //-----------------------------
            //int xx = Presenter.TotalRead_fromAce;
          Console.ReadLine();

        }
    }
}
