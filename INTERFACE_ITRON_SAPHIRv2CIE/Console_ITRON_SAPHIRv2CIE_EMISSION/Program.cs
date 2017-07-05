using ACEVISION.Common;
using InterfaceServices.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Console_ITRON_SAPHIRv2CIE_EMISSION
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] tabParameter = new string[7];
            Dictionary<string, InfoSensitiveCTR> _dicoCTR = new Dictionary<string, InfoSensitiveCTR>();

            /*---21122015
             **/
            args = new string[7];

            args[0] = @"E:\Application\ITRON\Log";//;valueFile.Trim();
            //args[1] = ".";//"10.109.240.220";//valueServer.Trim();
           args[1] = ".";//"10.109.240.220";//valueServer.Trim();
            //args[1] = "10.108.28.188";//"10.109.240.220";//valueServer.Trim();
            args[2] = "ACE_NODE_TI_16112016Debug";//"ACE_NODE_TI_16112016";//valueBDD.Trim();
           // args[2] = "ACE_NODE";//"ACE_NODE_TI_16112016";//valueBDD.Trim();
            //args[2] = "ACE_NODE_Recette";//valueBDD.Trim();
            args[3] = "sa";//"GESADB";//"sa";//valueUserName.Trim();
            args[4] = "P@ssw0rd";//"GESA";//"P@ssw0rd";//valuePassword.Trim();
            args[5] = "fhcoulibaly@cie.ci";//mail notification File EXcel;
            //args[6] = "cyyao@cie.ci,fhcoulibaly@cie.ci";//valueTo.Trim();
            args[6] = "fhcoulibaly@cie.ci";//valueTo.Trim();


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
                    Presenter.TypeTraitement = Constantes.TypeTraitementEmi;
                    Presenter.ExecutionMode = Constantes.ExecutionMode_Auto;
                    Presenter.ExecutionID = Guid.NewGuid().ToString();
                    Presenter.TotalInserted_toAce = 0;
                    Presenter.setValues_ofData();
                    Log.MonitoringLogger.Info(DateTime.Now + " -  Initialialisation du traitement !");
                    DateTime dateExecution = DateTime.Now;
                    //---get dicoCtrExcel
                    string msgErrFile = string.Empty;
                    //string pathFile = PresenterService.getPathFileExcel(ref msgErrFile);
                    /*
                     *\author FCO
                     *\date 23042017
                     *\deprecated depuisla RG 14/04/2017
                     **/
                    //ParamsFileDTO _pfd = PresenterService.get_ParamsFile(ref msgErrFile);

                    //if (!string.IsNullOrEmpty(msgErrFile))
                    //{
                    //    Console.WriteLine("echec de traitement : " + msgErrFile);
                    //    Log.ExceptionLogger.Error(DateTime.Now + " - bug signalé ! \n " + msgErrFile);
                      
                    //}
                    //else
                    //{
                        /*-obsolète depuis le 14/04/2017
                        //string pathFile = _pfd.PathFileExcel;
                        //string PwdFileExcel = _pfd.PasswordFileExcel;
                        //_dicoCTR = PresenterService.lstInfosCTr_fileXls(pathFile, PwdFileExcel, tabParameter[5], ref msgErrFile);
                        */
                        _dicoCTR = PresenterService._lstInfosCTrHT(out msgErrFile, tabParameter[5]);
                     //13062017---debug to delete
                    /*
                        InfoSensitiveCTR _inf1 = new InfoSensitiveCTR();
                        InfoSensitiveCTR _inf2 = new InfoSensitiveCTR();
                        _dicoCTR.TryGetValue("66005534", out _inf1);
                        _dicoCTR.TryGetValue("66005658", out _inf2);
                    */
                    //--------13062017

                        if (!string.IsNullOrEmpty(msgErrFile))
                        {
                            LogDTO UnLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                DescriptionErreur = string.Format("ERREUR RECUPERATION INFOS des COMPTEURS  : {0}  ", msgErrFile),
                                Objet = "ERREUR DE TRAITEMENT - INFOS COMPTEURS",
                                // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                            };
                            string msgDisplayErr = UnLog.DescriptionErreur;
                            //Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                            Log.ExceptionLogger.Error(msgDisplayErr);
                        }
                        else
                        {
                            List<ActivationAbonneDTO> lstDemandes_pending = PresenterService.GetAllDemandesFromSAPHIR();
                            Presenter.TotalRead_fromNode = lstDemandes_pending.Count();
                            Presenter.setValues_ofData();

                            if (lstDemandes_pending.Count()>0)
                            {
                                bool b = PresenterService.SaveToAceVision(_dicoCTR, lstDemandes_pending, tabParameter[5]);
                                if (!b)
                                {
                                    Console.WriteLine("Traitement terminé avec des erreurs, \n Veuillez consulter les Logs ");
                                }
                                else
                                {
                                    Console.WriteLine("Traitement terminé avec succès !");
                                }
                            }
                            else
                            {
                                Log.ExceptionLogger.Error("Traitement Annulé , aucune donnée disponible !");
                                Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");
                                Console.WriteLine("Traitement Annulé , aucune donnée disponible !");
                            }

                        }
                   // }
                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine("echec de traitement : " + ex.Message);
                    Log.ExceptionLogger.Error(DateTime.Now + " - bug signalé ! \n " + ex.Message);
                    
                    Console.WriteLine(DateTime.Now + " - Fin du traitement ");
                    //throw;
                }
                Console.WriteLine(DateTime.Now + " - Fin du traitement ");
            }
            else
            {
                Log.ExceptionLogger.Error(DateTime.Now + " traitement annulé , aucun paramétrage effectué");
                Console.WriteLine("traitement annulé , aucun paramétrage effectué ");
                Console.WriteLine(DateTime.Now + " - Fin du traitement ");
            }
            Console.ReadLine();
        }
    }
}
