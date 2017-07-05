using ACEVISION.Common;
using ACEVISION.ProcessUI;
using InterfaceServices.DTO;
using InterfaceServices.SAPHIRCOMDataAccess;
using InterfaceServices.SAPHIRCOMDataAccess.SAPHIRCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Console_ITRON_SAPHIRv2CIE
{
    public static class PresenterService
    {
        /*
                    public static int TotalRead_fromAce;
                    public static int TotalInserted_toNode;
                    public static int TotalRead_fromNode;
                    public static int TotalInserted_toAce;
                    public static int TotalUpdated_toAce;
                    public static string ExecutionMode;
                    public static string TypeTraitement;

        */

        public static List<ReceptionDTO> getDats_FromEOBilling_ACEVISIOn(string dateDebut, string dateFin)
        {
            //dateDebut = "2015-09-01";
            //dateFin = "2015-09-30";

            List<ReceptionDTO> lstReception = new List<ReceptionDTO>();

            string Errmess = string.Empty;
            //---------------
            SceReference_Itron.ContractsClient proxy = new SceReference_Itron.ContractsClient();
            //ServiceReference1.ContractsClient proxyAbidjan = new ServiceReference1.ContractsClient();
            Double PMax1 = 0;
            Double PMax2 = 0;
            Double PMax3 = 0;
            Double UTension = 10;
            int NbreInsert = 0;
            int NbreErreur = 0;
            SceReference_Itron.DTOValue[] LesValeursMax1 = null;
            SceReference_Itron.DTOValue[] LesValeursMax2 = null;
            SceReference_Itron.DTOValue[] LesValeursMax3 = null;

            //23122016
             IList<ReceptionDTO> LesDonneesDeFacturation = new List<ReceptionDTO>();
            try
            {
                #region Check Service
                try
                {
                    proxy.checkService();
                }
                catch (Exception ex)
                {
                    Errmess = ex.Message;
                    Utilitaires.AfficherErreur("Erreur connexion avec ACEVISION", ex.Message);
                    return null;
                }
                #endregion
                #region Init param Authentification DTO
                /*
                SceReference_Itron.DTOAuth authDTO = new SceReference_Itron.DTOAuth()
                {
                    Login = "admin",
                    Password = "admin",
                    Authenticate = SceReference_Itron.DTOEnumAuthenticate.Logged,
                    Authorization = SceReference_Itron.DTOEnumAuthorization.ReadAndWrite,
                };
                */
                //21122016
                SceReference_Itron.DTOAuth authDTO = new SceReference_Itron.DTOAuth()
                {

                    Login = System.Configuration.ConfigurationManager.AppSettings["LoginWebSce"],
                    Password = System.Configuration.ConfigurationManager.AppSettings["PasswordWebSce"],

                    Authenticate = SceReference_Itron.DTOEnumAuthenticate.Logged,
                    Authorization = SceReference_Itron.DTOEnumAuthorization.ReadAndWrite,
                };

                #endregion
                //IList<ReceptionDTO> LesDonneesDeFacturation = new List<ReceptionDTO>();
                // - Obtention des compteurs de l'abonne :  faudra fournir l'IDAbon
                #region Get Billing DATA

                IList<SceReference_Itron.DTOMeterGroup> LesGroupeClient = proxy.getMeterGroups(ref authDTO);

                string ListGroupFailed = string.Empty;
                string ListRefRaccordFailed = string.Empty;

                if (LesGroupeClient != null && LesGroupeClient.Count > 0)
                {
                    foreach (SceReference_Itron.DTOMeterGroup grp in LesGroupeClient)
                    {
                        #region Groupe
                        //-A verfier qye le groupe correspond bel et bien à un code SiteExplotation
                        if (grp.MeterGroupName.Length == 6)//groupe==XXXYYY
                        {
                            IList<SceReference_Itron.DTOMeterIdentifier> LesCompteursDuGroupe = grp.MeterList;

                            if (LesCompteursDuGroupe != null && LesCompteursDuGroupe.Count > 0)
                            {
                                foreach (SceReference_Itron.DTOMeterIdentifier cptgrp in LesCompteursDuGroupe)
                                //foreach (SceReference_Itron.DTOMeterIdentifier cptgrp in LesCompteursDuGroupe.Take(2))
                                {

                                    SceReference_Itron.DTOMeter LeCompteur = proxy.getMeterById(ref authDTO, cptgrp.MeterId);
                                    if (LeCompteur != null)
                                    {

                                        /// - Pas tres utile pour le moment
                                        SceReference_Itron.DTOCustomer LeClient = proxy.getCustomerById(ref authDTO, LeCompteur.CustomerId);

                                        //   - Obtention des informations de facturation de la periode
                                        //DateTime pDateDebut = DateTime.Parse(dateDebut);
                                        //DateTime pDateFin = DateTime.Parse(dateFin);

                                        //---pr test a delete later
                                        //DateTime pDateDebut = DateTime.Parse("2016/11/1");
                                        //DateTime pDateFin = DateTime.Parse("2016/11/30");
                                        //-----------------------

                                        //---retourn ts les index de ctr ayant date de releve comprise entre datedeb et datefin
                                        SceReference_Itron.DTOEndOfBillingIdentifier LeEOBID = proxy.getEOBIDsByMeterId(ref authDTO, LeCompteur.MeterId, pDateDebut, pDateFin).FirstOrDefault();
                                        if (LeEOBID != null)
                                        {
                                            SceReference_Itron.DTOEndOfBilling LeEOBduCompteur = proxy.getEOBById(ref authDTO, LeEOBID.EndOfBillingId);
                                            if (LeEOBduCompteur != null)
                                            {
                                                SceReference_Itron.DTOValue[] LesValeurs = LeEOBduCompteur.BillingRateArray;
                                                SceReference_Itron.DTOValue[] LesValeursTotales = LeEOBduCompteur.BillingTotalArray;

                                                // -  GESTION DES I Max 1
                                                if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() >= 7)
                                                    LesValeursMax1 = LeEOBduCompteur.BillingMaxArray.Take(7).ToArray();
                                                else
                                                    LesValeursMax1 = LeEOBduCompteur.BillingMaxArray;

                                                //---get saclar 
                                                UTension = Math.Pow(10, LesValeursMax1.Max(m => m.ValueScalar));
                                                //------------------

                                                PMax1 = LesValeursMax1 != null && LesValeursMax1.Count() > 0 ? LesValeursMax1.Skip(1).Take(5).Max(p => p.Value) * UTension / 1000 : 0;


                                                // -  GESTION DES I Max 2 exceptionnellement pour les Compteurs de type SPX
                                                if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() > 7)
                                                    LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(7).Take(7).ToArray();
                                                else
                                                    LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(7).ToArray();
                                                PMax2 = LesValeursMax2 != null && LesValeursMax2.Count() > 0 ? LesValeursMax2.Skip(1).Take(5).Max(p => p.Value) * UTension / 1000 : 0;


                                                // -  GESTION DES I Max 3 exceptionnellement pour les Compteurs de type SPX
                                                if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() > 14)
                                                    LesValeursMax3 = LeEOBduCompteur.BillingMaxArray.Skip(14).Take(7).ToArray();
                                                else
                                                    LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(13).ToArray();
                                                PMax3 = LesValeursMax3 != null && LesValeursMax3.Count() > 0 ? LesValeursMax3.Skip(1).Take(5).Max(p => p.Value) * UTension / 1000 : 0;




                                                int laprofondeur = LesValeurs != null ? LesValeurs.Count() : 0;

                                                ReceptionDTO UneRec = new ReceptionDTO()
                                                {
                                                    // vu la composition du groupe (CodeSite [3 position] + CodeExploitation [3 position]
                                                    //CodeExploitation =!string .IsNullOrEmpty(grp.MeterGroupName)? grp.MeterGroupName.Substring(3,3) : "XXX",
                                                    //CodeSite = !string .IsNullOrEmpty(grp.MeterGroupName)? grp.MeterGroupName.Substring(0,3) : "XXX",

                                                    CodeExploitation = !string.IsNullOrEmpty(grp.MeterGroupName) ? grp.MeterGroupName.Length >= 6 ? grp.MeterGroupName.Substring(3, 3) : "XXX" : "N/A",
                                                    CodeSite = !string.IsNullOrEmpty(grp.MeterGroupName) ? grp.MeterGroupName.Length >= 6 ? grp.MeterGroupName.Substring(0, 3) : "YYY" : "N/A",
                                                    DateRecept = DateTime.Now,
                                                    DateReleve = LeEOBduCompteur.ReadDate,
                                                    IdentifiantAbonne = LeCompteur.Location,
                                                    IDReception = Guid.NewGuid(),
                                                    //ReferenceRaccordement = !string.IsNullOrEmpty(LeCompteur.Address) ? LeCompteur.Address.Substring(-(8 - LeCompteur.Address.Length)) : "N/A",
                                                    DateCpte = null,
                                                    DateExtract = null,
                                                    NumeroCompteur = LeCompteur.SerialNumber,
                                                    // - Recuperation des index
                                                    IndexConsoMonop1 = LesValeursTotales != null && LesValeursTotales[0] != null ? LesValeursTotales[0].Value.ToString().Replace(",", ".") : "0",
                                                    IndexConsoMonop2 = LesValeursTotales != null && LesValeursTotales[1] != null ? LesValeursTotales[1].Value.ToString().Replace(",", ".") : "0",
                                                    IndexConsoMonop3 = LesValeursTotales != null && LesValeursTotales[2] != null ? LesValeursTotales[2].Value.ToString().Replace(",", ".") : "0",
                                                    IndexIma1 = PMax1.ToString().Replace(",", "."), // Puissance Max tarif 1
                                                    IndexIma2 = PMax2.ToString().Replace(",", "."), // Puissance Max tarif 1
                                                    IndexIma3 = PMax3.ToString().Replace(",", "."), // Puissance Max tarif 1

                                                    IndexNuit = LesValeurs != null && LesValeurs[0] != null ? LesValeurs[0].Value.ToString().Replace(",", ".") : "0",
                                                    IndexPointe = LesValeurs != null && LesValeurs[1] != null ? LesValeurs[1].Value.ToString().Replace(",", ".") : "0",
                                                    IndexJour = LesValeurs != null && LesValeurs[2] != null ? LesValeurs[2].Value.ToString().Replace(",", ".") : "0",

                                                    /*
                                                    IndexReactif1 = LesValeurs != null && LesValeurs[6] != null ? LesValeurs[6].Value.ToString().Replace(",", ".") : "0",
                                                    IndexReactif2 = LesValeurs != null && laprofondeur > 8 && LesValeurs[7] != null != null ? LesValeurs[7].Value.ToString().Replace(",", ".") : "0",
                                                    IndexReactif3 = LesValeurs != null && laprofondeur > 9 && LesValeurs[8] != null != null ? LesValeurs[8].Value.ToString().Replace(",", ".") : "0",
                                                    */
                                                    //RG du 16112016 :Triphasé Réactif Import  dans Energie totale
                                                    //RG du 16112016 :IndexReactif2 et IndexReactif3 toujours =0                                                           
                                                    IndexReactif1 = LesValeursTotales != null && LesValeursTotales[11] != null ? LesValeursTotales[11].Value.ToString().Replace(",", ".") : "0",
                                                    IndexReactif2 = "0",
                                                    IndexReactif3 = "0",

                                                    IndexHoraire = LeEOBduCompteur.TotalOperatingTime.ToString().Replace(",", ".")
                                                };
                                                //create methode periode facturation m-1 et YYYY-1 si mois janvier?
                                                // LesDonneesDeFacturation.Add(UneRec);
                                                //UneRec.PeriodeFacturation = UneRec.DateReleve.Value.Month.ToString().PadLeft(2, '0') + UneRec.DateReleve.Value.Year.ToString();

                                                //11122015
                                                // UneRec.PeriodeFacturation = UneRec.DateReleve.Value.GetPeriodeFacturation();                                         
                                                // LesDonneesDeFacturation.Add(UneRec);//test

                                                if (!string.IsNullOrEmpty(LeCompteur.Address) && LeCompteur.Address.Length > 8)//-A definir template (adresse geo | refaccord sur 8 position))
                                                {
                                                    UneRec.ReferenceRaccordement = LeCompteur.Address.Substring(LeCompteur.Address.Length - 8);

                                                    UneRec.PeriodeFacturation = UneRec.DateReleve.Value.GetPeriodeFacturation();
                                                    LesDonneesDeFacturation.Add(UneRec);//test
                                                }
                                                else
                                                {
                                                    //ListRefRaccordFailed += LeCompteur.SerialNumber + ":" + LeCompteur.Address + " | ";
                                                    //21122016
                                                    ListRefRaccordFailed += "SiteExploitation : " + UneRec.CodeSite + UneRec.CodeExploitation + "-" + LeCompteur.SerialNumber + ":" + LeCompteur.Address + " | ";
                                                }
                                            }

                                        }
                                    }


                                }
                            }
                        }
                        else
                        {
                            ListGroupFailed += grp.MeterGroupName + " | ";
                            //log
                            //Log.ExceptionLogger.Error("Message : Code Site/Exploitation nom respecté"  grp.MeterGroupName
                        }
                        #endregion
                    }

                    //---Error
                    if (!string.IsNullOrEmpty(ListGroupFailed))
                    {
                        ListGroupFailed = ListGroupFailed.Remove(ListGroupFailed.Length - 2);
                        Log.ExceptionLogger.Error("Message : Nom du groupe incorrect selon le format SAPHIR XXXYYY {SiteExploitation} \n Groupe (s) concerné(s) : " + ListGroupFailed);

                        //-sortir du traitement general
                    }


                }

                //11/12/2015
                if (!string.IsNullOrEmpty(ListRefRaccordFailed))
                {
                    ListRefRaccordFailed = ListRefRaccordFailed.Remove(ListRefRaccordFailed.Length - 2);

                    Presenter.ExecutionMode = Constantes.ExecutionMode_Auto;
                    Presenter.TypeTraitement = Constantes.TypeTraitementRec;
                    Presenter.PeriodeFacturation = LesDonneesDeFacturation.FirstOrDefault().PeriodeFacturation;//"N/A";
                    Presenter.setValues_ofData();
                    Log.ExceptionLogger.Error("Message : Reference de raccordement incorrect; format a respecter sur 8 positions \n Compteur(s) concerné(s) :" + ListRefRaccordFailed);
                }
                #endregion
                //FCO
                #region for Log & save Data in node BDD

                Presenter.TotalRead_fromAce = LesDonneesDeFacturation.Count();

                Presenter.TotalRead_fromNode = 0;
                Presenter.TotalInserted_toAce = 0;
                Presenter.TotalUpdated_toAce = 0;
                Presenter.ExecutionMode = Constantes.ExecutionMode_Auto;
                Presenter.TypeTraitement = Constantes.TypeTraitementRec;





                List<string> LstCTR_alreadyIntegratedNode = new List<string>();
                string valueMeter = string.Empty;
                StringBuilder sbvalueMeterNotIntegrated = new StringBuilder();
                string valueMeterNotIntegrated=string.Empty;
                try
                {
                    //---------
                    Presenter.setValues_ofData();

                    //-----------
                    int cpt = LesDonneesDeFacturation.Where(s => s.PeriodeFacturation == string.Empty || s.PeriodeFacturation == null).Count();
                    if (cpt == 0)//-check si data a periodefact renseignée...                            
                    {
                        string msgErr = string.Empty;
                        //string valueMeter = string.Empty;
                        foreach (var item in LesDonneesDeFacturation)
                        {
                            //-check si N°compteur et perfact déjà integré dans le noeud
                            if (!checkMeterNode(item.NumeroCompteur, item.PeriodeFacturation, out msgErr))
                            {
                                // new ReceptionIndexPresenter().InsertDesIndexDansLeNoeud(LesDonneesDeFacturation, ref NbreInsert, ref NbreErreur, ref Errmess);
                                new ReceptionIndexPresenter().InsertIndexDansLeNoeud(item, ref Errmess);
                                //23122016
                                //valueMeterNotIntegrated += valueMeterNotIntegrated += "SiteExploitation :" + item.CodeSite + item.CodeExploitation + "- Compteur :" + item.NumeroCompteur + " | ";
                                string _val="SiteExploitation :" + item.CodeSite + item.CodeExploitation + "- Compteur :" + item.NumeroCompteur ;
                                sbvalueMeterNotIntegrated.AppendLine(string.Join("|",_val));
                            }
                            else
                            {
                                string _PerFact = LesDonneesDeFacturation.FirstOrDefault().PeriodeFacturation;
                                if (!string.IsNullOrEmpty(_PerFact))
                                {
                                    Presenter.PeriodeFacturation = _PerFact;
                                    
                                }
                                else
                                    Presenter.PeriodeFacturation = string.Empty;
                                Presenter.setValues_ofData();

                                //-stock value meter
                                LstCTR_alreadyIntegratedNode.Add(item.NumeroCompteur);
                                valueMeter += "SiteExploitation :" + item.CodeSite + item.CodeExploitation + "- Compteur :" + item.NumeroCompteur + " | ";
                            }
                        }
                        //---Error
                        //if (!string.IsNullOrEmpty(valueMeter))
                        //{
                        //    valueMeter = valueMeter.Remove(valueMeter.Length - 2);
                        //    //23122016
                        //    if (LstCTR_alreadyIntegratedNode.Count() == Presenter.TotalInserted_toNode)
                        //        Log.ExceptionLogger.Error(string.Format("Message : Traitement annulé ,\n Ces {0} compteur (s) ont déjà intégré(s) pour cette periode de facturation ", LstCTR_alreadyIntegratedNode.Count()));
                        //    else
                        //        Log.ExceptionLogger.Error(string.Format("Message : Traitement terminé avec succès ,\n cependant Pour cette periode de facturation,  ce (s) {0} compteur (s) déjà intégré(s)  : {1} ", LstCTR_alreadyIntegratedNode.Count(), valueMeter));
                        //}


                    }
                    else
                        Log.ExceptionLogger.Error("Message : Données de facturation sans periode de facturation");

                }
                catch (Exception ex)
                {
                    //log
                    Log.ExceptionLogger.Error("Message : " + ex.Message);
                    Log.MonitoringLogger.Error("Message : " + ex.Message);
                    Console.WriteLine("Erreur lors du traitement : " + ex.Message);
                }
                finally
                {
                    if (LesDonneesDeFacturation.Count > 0)
                    {
                        /*- //ok si une seul perfact pour tous les compteurs
         *
                        string _PerFact=LesDonneesDeFacturation.FirstOrDefault().PeriodeFacturation;
                        if (!string.IsNullOrEmpty(_PerFact))
                        {
                            //---fer un contrôle ici pour dissoscier les compteur mis en ecart ?(style dejà intégré ?)
                           // Presenter.TotalInserted_toNode = new DAL<mvt_releve_reception>().Find(p => p.perfact == _PerFact).Count();

                            List < string > lstCTRBDD= new DAL<mvt_releve_reception>().Find(p => p.perfact == _PerFact).Select(s=>s.numctr).ToList();
                            int cptBdd = LstCTR_alreadyIntegratedNode.Except(lstCTRBDD).Count();                                
                            Presenter.TotalInserted_toNode = (new DAL<mvt_releve_reception>().Find(p => p.perfact == _PerFact).Count())-cptBdd;
                            //-------methode cyrille ?
                            Presenter.PeriodeFacturation = _PerFact;
                        }
                        else
                        {
                            Presenter.TotalInserted_toNode = 0;
                            Presenter.PeriodeFacturation = string.Empty;
                        }
                        */

                        //---12112015
                        List<string> listPerfact = LesDonneesDeFacturation.Select(p => p.PeriodeFacturation).Distinct().ToList();
                        if (listPerfact.Count == 1)
                            ConsThreadPerFact_One(LesDonneesDeFacturation, LstCTR_alreadyIntegratedNode);
                        else
                            ConsThreadPerFact_Full(LstCTR_alreadyIntegratedNode, listPerfact);


                        //23122016
                        Presenter.setValues_ofData();
                        if (!string.IsNullOrEmpty(valueMeter))
                        {
                            valueMeter = valueMeter.Remove(valueMeter.Length - 2);
                            if (sbvalueMeterNotIntegrated.Length!=0)
                                valueMeterNotIntegrated = sbvalueMeterNotIntegrated.ToString();
                           // valueMeterNotIntegrated = valueMeterNotIntegrated.Remove(valueMeterNotIntegrated.Length - 2);
                            //23122016
                            int diffIntegrated =LesDonneesDeFacturation.Count()- LstCTR_alreadyIntegratedNode.Count();
                            if (LstCTR_alreadyIntegratedNode.Count() == LesDonneesDeFacturation.Count)
                                Log.ExceptionLogger.Error(string.Format("Message : Traitement annulé ,\n Ce (s) {0} compteur (s) déjà intégré(s) pour cette periode de facturation,\n Veuillez consulter la page d'interface pour les details sur ce(s) compteur(s) ", LstCTR_alreadyIntegratedNode.Count()));
                            else
                               // Log.ExceptionLogger.Error(string.Format("Message : Traitement terminé avec succès ,\n cependant Pour cette periode de facturation,  ce (s) {0} compteur (s) déjà intégré(s)  : {1} ", LstCTR_alreadyIntegratedNode.Count(), valueMeter));
                                Log.ExceptionLogger.Error(string.Format("Message : Traitement terminé avec succès ,\n cependant Pour cette periode de facturation,  sur {0} compteurs, seul(s) ce (s) {1} compteur (s)  ont été intégré(s)  : {2} , les autres déjà pris en compte ", LesDonneesDeFacturation.Count(), diffIntegrated, valueMeterNotIntegrated));
                        }
                    }
                    //----execution counter
                    //List<LogBDD> _logb = new DAL<LogBDD>().Find(f => f.PeriodeFacturation == _PerFact).ToList();
                    DateTime db = DateTime.Parse(dateDebut);
                    List<LogBDD> _logb = new DAL<LogBDD>().Find(f => f.PeriodeFacturation == db.GetPeriodeFacturation()).ToList();

                    if (_logb.Count() > 0)// != null)
                    {
                        int _CptExecution = _logb.Max(m => m.CptExecution).Value + 1;
                        Presenter.CptExecution = _CptExecution;
                    }

                    //------------------------------
                    //---------
                    Presenter.setValues_ofData();
                    //-----------
                    //--+-------------
                    //23122016
                    //Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");

                    //-------Update cptExecution des Log de l'execution(debut traitee,t , Error si present...
                    List<LogBDD> _lstLg = new DAL<LogBDD>().Find(l => l.ExecutionID == Presenter.ExecutionID).ToList();
                    if (_lstLg.Count > 0)
                    {
                        foreach (var item in _lstLg)
                        {
                            item.CptExecution = Presenter.CptExecution;
                            new DAL<LogBDD>().UpdateRow(item, u => u.ID == item.ID);
                        }

                    }

                    //------------------------
                }
                #endregion

                return LesDonneesDeFacturation.ToList();




            }
            catch (Exception ex)
            {
                Errmess = ex.Message;
                //Utilitaires.AfficherErreur("Recuperation des données depuis ACEVISION", ex.Message);
                Log.ExceptionLogger.Error(DateTime.Now + " - " + ex.Message);
                return null;
            }
            finally
            {
                if(LesDonneesDeFacturation.Count==0)
                    Log.ExceptionLogger.Error(string.Format("Aucune donnée disponible  pour cette periode [{0}-{1}] de facturation", dateDebut, dateFin));
                //Log.MonitoringLogger.Info(string.Format("Aucune donnée disponible  pour cette periode [{0}-{1}] de facturation", dateDebut, dateFin));

                Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");

            }
            //---------------

        }

        static bool checkMeterNode(string serialnumber,string periodeFacturation,out string msgErr)
        {
            msgErr = string.Empty;
            //mvt_releve_reception _mvt_releve_reception = new DAL<mvt_releve_reception>().Find(m => m.numctr.Trim() == serialnumber.Trim() && m.perfact == periodeFacturation.Trim()).FirstOrDefault();
            mvt_releve_reception _mvt_releve_reception = new DAL<mvt_releve_reception>().Find(m => m.numctr == serialnumber.Cryptage() && m.perfact == periodeFacturation.Trim()).FirstOrDefault();
            if (_mvt_releve_reception != null)
                return true;
            return false;
        }

        static void ConsThreadPerFact_One(IList<ReceptionDTO> LesDonneesDeFacturation, List<string> LstCTR_alreadyIntegratedNode)
        {
            string msgErr = string.Empty;
            string _PerFact = LesDonneesDeFacturation.FirstOrDefault().PeriodeFacturation;
            if (!string.IsNullOrEmpty(_PerFact))
            {


                //---fer un contrôle ici pour dissoscier les compteur mis en ecart ?(style dejà intégré ?)
                // Presenter.TotalInserted_toNode = new DAL<mvt_releve_reception>().Find(p => p.perfact == _PerFact).Count();
                List<ReceptionDTO> lstReceptionDTO = new ReceptionIndexPresenter().GetAllMvtReception(_PerFact, out msgErr);
                List<string> lstCTRBDD = new List<string>();
                if (lstReceptionDTO.Count() > 0)
                {
                    lstCTRBDD = lstReceptionDTO.Select(s => s.NumeroCompteur).ToList();
                    int cptBdd = LstCTR_alreadyIntegratedNode.Except(lstCTRBDD).Count();

                    Presenter.TotalInserted_toNode = lstReceptionDTO.Count() - cptBdd;
                }

                Presenter.PeriodeFacturation = _PerFact;
 
            }
            else
            {
                Presenter.TotalInserted_toNode = 0;
                Presenter.PeriodeFacturation = string.Empty;
            }
        }

        static void ConsThreadPerFact_Full(List<string> LstCTR_alreadyIntegratedNode, List<string> listPerfact)
        {
            string msgErr = string.Empty;
            List<ReceptionDTO> lstReceptionDTO = new List<ReceptionDTO>();
            string perFactFull = string.Empty;
            foreach (var item in listPerfact)
            {
                List<ReceptionDTO> lstCTR_fact = new ReceptionIndexPresenter().GetAllMvtReception(item, out msgErr);
                foreach (var recDTO in lstCTR_fact)
                {
                    lstReceptionDTO.Add(recDTO);
                }

                perFactFull += item + " | ";
            }
            List<string> lstCTRBDD = new List<string>();
            if (lstReceptionDTO.Count() > 0)
            {
                lstCTRBDD = lstReceptionDTO.Select(s => s.NumeroCompteur).ToList();
                int cptBdd = LstCTR_alreadyIntegratedNode.Except(lstCTRBDD).Count();

                Presenter.TotalInserted_toNode = lstReceptionDTO.Count() - cptBdd;
                //---------A revoir
                perFactFull = perFactFull.Remove(perFactFull.Length - 2);
                Presenter.PeriodeFacturation = perFactFull;
            }


        }
    }
}
