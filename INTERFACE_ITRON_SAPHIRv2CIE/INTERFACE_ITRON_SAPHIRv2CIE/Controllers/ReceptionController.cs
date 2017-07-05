using ACEVISION.Common;
using ACEVISION.ProcessUI;
using INTERFACE_ITRON_SAPHIRv2CIE.Common;
//using Console_ITRON_SAPHIRv2CIE;
using INTERFACE_ITRON_SAPHIRv2CIE.Models;
using InterfaceServices.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

 

namespace INTERFACE_ITRON_SAPHIRv2CIE.Controllers
{
   [Authorize(Roles = Constante.roleSuperAdmin + "," + Constante.roleExecutif)]
    public class ReceptionController : Controller
    {
        //
        // GET: /Reception/
        static List<ReceptionDTO> lstMvtReception = null;
        static DataTable _dtExport;
        static List<string> _lstHeaderColumns = new List<string>() { "COMPTEUR", "REF RACCOR", "ID ABON", "PERIODE FAC", "DATE RELEVE", "INDEX NUIT", "INDEX POINTE", "INDEX JOUR", "INDEX HORAIRE", "INDEX REACTIF 1", "INDEX IMA1", "INDEX IMA", "INDEX IMA3", "INDEX MONO1", "INDEX MONO2","INDEX MONO3","SITE", "EXPLOITATION" };

        public ActionResult Index()
        {
           

            //return View(lstReception.ToList());
            return View();
        }

        [HttpPost]
        public ActionResult DownloadDataFromACEVISION()
        {
            string jsonData = "Enregistrement réussi avec succès !";
             
            return Content(jsonData);
        }

#region old getDats_FromEOBilling_ACEVISIOn
       //// public List<ReceptionDTO> getDats_FromEOBilling_ACEVISIOn(DateTime dateDebut, DateTime dateFin)
        
       // public List<ReceptionDTO> getDats_FromEOBilling_ACEVISIOn(string dateDebut, string dateFin)
       // {
       //     dateDebut = "2015-09-01";
       //     dateFin = "2015-09-30";

       //      List<ReceptionDTO> lstReception = new List<ReceptionDTO>();

       //      string Errmess = string.Empty;
       //     //---------------
       //      ACEVISIONWebService.ContractsClient proxy = new ACEVISIONWebService.ContractsClient();
       //      //ServiceReference1.ContractsClient proxyAbidjan = new ServiceReference1.ContractsClient();
       //      Double PMax1 = 0;
       //      Double PMax2 = 0;
       //      Double PMax3 = 0;
       //      Double UTension = 10;
       //      int NbreInsert = 0;
       //      int NbreErreur = 0;
       //      ACEVISIONWebService.DTOValue[] LesValeursMax1 = null;
       //      ACEVISIONWebService.DTOValue[] LesValeursMax2 = null;
       //      ACEVISIONWebService.DTOValue[] LesValeursMax3 = null;

       //      try
       //      {
       //          #region Check Service
       //          try
       //          {
       //              proxy.checkService();
       //          }
       //          catch (Exception ex)
       //          {
       //              Errmess = ex.Message;
       //              Utilitaires.AfficherErreur("Erreur connexion avec ACEVISION", ex.Message);
       //              return null;
       //          }
       //          #endregion
       //          #region Init param Authentification DTO
       //          ACEVISIONWebService.DTOAuth authDTO = new ACEVISIONWebService.DTOAuth()
       //          {
       //              Login = "admin",
       //              Password = "admin",
       //              Authenticate = ACEVISIONWebService.DTOEnumAuthenticate.Logged,
       //              Authorization = ACEVISIONWebService.DTOEnumAuthorization.ReadAndWrite,
       //          };

       //          #endregion
       //          IList<ReceptionDTO> LesDonneesDeFacturation = new List<ReceptionDTO>();
       //          // - Obtention des compteurs de l'abonne :  faudra fournir l'IDAbon
       //          #region Get Billing DATA

       //          IList<ACEVISIONWebService.DTOMeterGroup> LesGroupeClient = proxy.getMeterGroups(ref authDTO);

       //          if (LesGroupeClient != null && LesGroupeClient.Count > 0)
       //          {
       //              foreach (ACEVISIONWebService.DTOMeterGroup grp in LesGroupeClient)
       //              {
       //                  IList<ACEVISIONWebService.DTOMeterIdentifier> LesCompteursDuGroupe = grp.MeterList;
       //                  if (LesCompteursDuGroupe != null && LesCompteursDuGroupe.Count > 0)
       //                  {
       //                      //foreach (ACEVISIONWebService.DTOMeterIdentifier cptgrp in LesCompteursDuGroupe)
       //                     foreach (ACEVISIONWebService.DTOMeterIdentifier cptgrp in LesCompteursDuGroupe.Take(2))
       //                      {
       //                          ACEVISIONWebService.DTOMeter LeCompteur = proxy.getMeterById(ref authDTO, cptgrp.MeterId);
       //                          if (LeCompteur != null)
       //                          {

       //                              /// - Pas tres utile pour le moment
       //                              ACEVISIONWebService.DTOCustomer LeClient = proxy.getCustomerById(ref authDTO, LeCompteur.CustomerId);

       //                              //   - Obtention des informations de facturation de la periode
       //                              DateTime pDateDebut=DateTime.Parse(dateDebut);
       //                              DateTime pDateFin=DateTime.Parse(dateFin);

       //                              ACEVISIONWebService.DTOEndOfBillingIdentifier LeEOBID = proxy.getEOBIDsByMeterId(ref authDTO, LeCompteur.MeterId, pDateDebut, pDateFin).FirstOrDefault();
       //                              if (LeEOBID != null)
       //                              {
       //                                  ACEVISIONWebService.DTOEndOfBilling LeEOBduCompteur = proxy.getEOBById(ref authDTO, LeEOBID.EndOfBillingId);
       //                                  if (LeEOBduCompteur != null)
       //                                  {
       //                                      ACEVISIONWebService.DTOValue[] LesValeurs = LeEOBduCompteur.BillingRateArray;
       //                                      ACEVISIONWebService.DTOValue[] LesValeursTotales = LeEOBduCompteur.BillingTotalArray;

       //                                      // -  GESTION DES I Max 1
       //                                      if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() >= 7)
       //                                          LesValeursMax1 = LeEOBduCompteur.BillingMaxArray.Take(7).ToArray();
       //                                      else
       //                                          LesValeursMax1 = LeEOBduCompteur.BillingMaxArray;
       //                                      PMax1 = LesValeursMax1 != null && LesValeursMax1.Count() > 0 ? LesValeursMax1.Skip(1).Take(5).Max(p => p.Value) * UTension : 0;

       //                                      // -  GESTION DES I Max 2 exceptionnellement pour les Compteurs de type SPX
       //                                      if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() > 7)
       //                                          LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(7).Take(7).ToArray();
       //                                      else
       //                                          LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(7).ToArray();
       //                                      PMax2 = LesValeursMax2 != null && LesValeursMax2.Count() > 0 ? LesValeursMax2.Skip(1).Take(5).Max(p => p.Value) * UTension : 0;


       //                                      // -  GESTION DES I Max 3 exceptionnellement pour les Compteurs de type SPX
       //                                      if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() > 14)
       //                                          LesValeursMax3 = LeEOBduCompteur.BillingMaxArray.Skip(14).Take(7).ToArray();
       //                                      else
       //                                          LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(13).ToArray();
       //                                      PMax3 = LesValeursMax3 != null && LesValeursMax3.Count() > 0 ? LesValeursMax3.Skip(1).Take(5).Max(p => p.Value) * UTension : 0;




       //                                      int laprofondeur = LesValeurs != null ? LesValeurs.Count() : 0;

       //                                      ReceptionDTO UneRec = new ReceptionDTO()
       //                                      {
       //                                          // vu la composition du groupe (CodeSite [3 position] + CodeExploitation [3 position]
       //                                          //CodeExploitation =!string .IsNullOrEmpty(grp.MeterGroupName)? grp.MeterGroupName.Substring(3,3) : "XXX",
       //                                          //CodeSite = !string .IsNullOrEmpty(grp.MeterGroupName)? grp.MeterGroupName.Substring(0,3) : "XXX",

       //                                          CodeExploitation = !string.IsNullOrEmpty(grp.MeterGroupName) ? grp.MeterGroupName.Length >= 6 ? grp.MeterGroupName.Substring(3, 3) : "XXX" : "N/A",
       //                                          CodeSite = !string.IsNullOrEmpty(grp.MeterGroupName) ? grp.MeterGroupName.Length >= 6 ? grp.MeterGroupName.Substring(0, 3) : "YYY" : "N/A",
       //                                          DateRecept = DateTime.Now,
       //                                          DateReleve = LeEOBduCompteur.ReadDate,
       //                                          IdentifiantAbonne = LeCompteur.Location,
       //                                          IDReception = Guid.NewGuid(),
       //                                          ReferenceRaccordement = !string.IsNullOrEmpty(LeCompteur.Address) ? LeCompteur.Address.Substring(-(8 - LeCompteur.Address.Length)) : "N/A",
       //                                          DateCpte = null,
       //                                          DateExtract = null,
       //                                          NumeroCompteur = LeCompteur.SerialNumber,
       //                                          // - Recuperation des index
       //                                          IndexConsoMonop1 = LesValeursTotales != null && LesValeursTotales[0] != null ? LesValeursTotales[0].Value.ToString() : "0",
       //                                          IndexConsoMonop2 = LesValeursTotales != null && LesValeursTotales[1] != null ? LesValeursTotales[1].Value.ToString() : "0",
       //                                          IndexConsoMonop3 = LesValeursTotales != null && LesValeursTotales[2] != null ? LesValeursTotales[2].Value.ToString() : "0",
       //                                          IndexIma1 = PMax1.ToString(), // Puissance Max tarif 1
       //                                          IndexIma2 = PMax2.ToString(), // Puissance Max tarif 1
       //                                          IndexIma3 = PMax3.ToString(), // Puissance Max tarif 1

       //                                          IndexNuit = LesValeurs != null && LesValeurs[0] != null ? LesValeurs[0].Value.ToString() : "0",
       //                                          IndexPointe = LesValeurs != null && LesValeurs[1] != null ? LesValeurs[1].Value.ToString() : "0",
       //                                          IndexJour = LesValeurs != null && LesValeurs[2] != null ? LesValeurs[2].Value.ToString() : "0",
       //                                          IndexReactif1 = LesValeurs != null && LesValeurs[6] != null ? LesValeurs[6].Value.ToString() : "0",
       //                                          IndexReactif2 = LesValeurs != null && laprofondeur > 8 && LesValeurs[7] != null != null ? LesValeurs[7].Value.ToString() : "0",
       //                                          IndexReactif3 = LesValeurs != null && laprofondeur > 9 && LesValeurs[8] != null != null ? LesValeurs[8].Value.ToString() : "0",
       //                                          IndexHoraire = LeEOBduCompteur.TotalOperatingTime.ToString()
       //                                      };
       //                                      LesDonneesDeFacturation.Add(UneRec);
       //                                  }

       //                              }

       //                          }
       //                      }
       //                  }
       //              }
       //          }

       //          //23092015
       //          lstMvtReception = LesDonneesDeFacturation.ToList();
       //          //............
       //          /*
       //          #region Save Data in Node

       //          new ReceptionIndexPresenter().InsertDesIndexDansLeNoeud(LesDonneesDeFacturation, ref NbreInsert, ref NbreErreur, ref Errmess);
       //          #endregion
       //           * */

       //          return LesDonneesDeFacturation.ToList();
 
       //          #endregion

               
                
       //      }
       //      catch (Exception ex)
       //      {
       //          Errmess = ex.Message;
       //          //Utilitaires.AfficherErreur("Recuperation des données depuis ACEVISION", ex.Message);
       //          return null;
       //      }
       //     //---------------
           
       //      #region test item list
       //      /*
       //      lstReception.Add(new ReceptionDTO
       //     {
       //         CodeExploitation="143",
       //         CodeSite="146",                
       //         DateExtract=DateTime.Now,                 
       //         DateReleve=DateTime.Now,
       //         IdentifiantAbonne="112555663",                 
       //         IndexConsoMonop1="15",
       //         IndexConsoMonop2="18",
       //         IndexConsoMonop3="22",
       //         IndexHoraire="2487",
       //         IndexIma1="78",
       //         IndexIma2="0",
       //         IndexIma3="0",
       //         IndexJour="147",
       //         IndexNuit="966",
       //         IndexPointe="10254",
       //         IndexReactif1="45",
       //         IndexReactif2="47",
       //         IndexReactif3="478",
                
       //         NumeroCompteur="7847845",
       //         PeriodeFacturation="741",
       //         ReferenceRaccordement="63"
                 
       //         });
       //     lstReception.Add(new ReceptionDTO
       //     {
       //         CodeExploitation = "143",
       //         CodeSite = "146",
       //         DateExtract = DateTime.Now,
       //         DateReleve = DateTime.Now,
       //         IdentifiantAbonne = "112555663",
       //         IndexConsoMonop1 = "15",
       //         IndexConsoMonop2 = "18",
       //         IndexConsoMonop3 = "22",
       //         IndexHoraire = "2487",
       //         IndexIma1 = "78",
       //         IndexIma2 = "0",
       //         IndexIma3 = "0",
       //         IndexJour = "147",
       //         IndexNuit = "966",
       //         IndexPointe = "10254",
       //         IndexReactif1 = "45",
       //         IndexReactif2 = "47",
       //         IndexReactif3 = "478",

       //         NumeroCompteur = "7847845",
       //         PeriodeFacturation = "741",
       //         ReferenceRaccordement = "63"

       //     });
       //       * */
       //      #endregion
          

       //     //return lstReception;
       // }
        
#endregion

        public ActionResult CTR_ACEVSISION(string dateDebut, string dateFin)
        {
            string msgErr = string.Empty;
           
            var myList = getDats_FromEOBilling_ACEVISIOn_Grp(dateDebut, dateFin,ref msgErr);
            //var myList = 2
            if (myList != null)
            {
                ViewBag.CptItems = myList.Count();
                TempData["_cptItems"] = myList.Count().ToString();
                return PartialView(myList);
            }
            else
            {

                return Content("Erreur de chargement de données ! Détails : "+msgErr);
            }
        }

        private List<ReceptionDTO> getDats_FromEOBilling_ACEVISIOn_Grp(string dateDebut, string dateFin,ref string msgErr)
        {
            //dateDebut = "2015-09-01";
            //dateFin = "2015-09-30";

            List<ReceptionDTO> lstReception = new List<ReceptionDTO>();

            string Errmess = string.Empty;
            //---------------
            
            ACEVISIONWebService.ContractsClient proxy = new ACEVISIONWebService.ContractsClient();
            //ServiceReference1.ContractsClient proxyAbidjan = new ServiceReference1.ContractsClient();
            Double PMax1 = 0;
            Double PMax2 = 0;
            Double PMax3 = 0;
            Double UTension = 10;
            int NbreInsert = 0;
            int NbreErreur = 0;
            ACEVISIONWebService.DTOValue[] LesValeursMax1 = null;
            ACEVISIONWebService.DTOValue[] LesValeursMax2 = null;
            ACEVISIONWebService.DTOValue[] LesValeursMax3 = null;

            try
            {

                #region Get Perfact
                //   - Obtention des informations de facturation de la periode

                //-08122016 
                //DateTime pDeb = DateTime.Parse(dateDebut);
                //DateTime pDFin = DateTime.Parse(dateFin);

                DateTime ?pDeb=null;  
                DateTime ?pDFin=null; 
                //---pr test
                //dateDebut = "31/12/16";
                //dateDebut = "12/20/2016";
                //dateFin = "12/30/2016";//forat : MM/JJ/AAAA
                try
                {
                    pDeb=DateTime.Parse(dateDebut);
                    ///---------test
                    //pDeb = DateTime.Parse(dateDebut, new CultureInfo("en-US"));
                }
                catch(Exception ex){
                    //pDeb = DateTime.ParseExact(dateDebut, "g", new CultureInfo("fr-FR"));
                    //pDeb = DateTime.Parse(dateDebut,new CultureInfo("fr-FR"));
                    //pDFin = DateTime.Parse(dateFin, new CultureInfo("fr-FR"));

                    pDeb = DateTime.Parse(dateDebut, new CultureInfo("en-US"));
                    pDFin = DateTime.Parse(dateFin, new CultureInfo("en-US"));

                    msgErr = ex.Message;
                }

                if (pDFin == null)
                {
                    try
                    {
                        pDFin = DateTime.Parse(dateFin);
                    }
                    catch(Exception ex)
                    {
                        //pDFin = DateTime.ParseExact(dateFin, "g", new CultureInfo("fr-FR"));
                        //pDFin = DateTime.Parse(dateFin, new CultureInfo("fr-FR"));
                        //pDeb = DateTime.Parse(dateDebut, new CultureInfo("fr-FR"));

                        pDFin = DateTime.Parse(dateFin, new CultureInfo("en-US"));
                        pDeb = DateTime.Parse(dateDebut, new CultureInfo("en-US"));

                        msgErr = ex.Message;
                    }
                }

                DateTime? pDateDebut = null;
                DateTime? pDateFin = null;
                //-
                //bool resRech = GetPeriodeRecherche(pDeb, pDFin, ref pDateDebut, ref pDateFin);
                bool resRech = GetPeriodeRecherche(pDeb.Value, pDFin.Value, ref pDateDebut, ref pDateFin);
                //--
                if (!resRech)
                    return null;

                #endregion

              

                #region Check Service
                //------------LOg
                /*Log.MonitoringLogger.Info(DateTime.Now + " -  Initialialisation du traitement !");
                Presenter.ExecutionMode = Constantes.ExecutionMode_Manuel;
                Presenter.TypeTraitement = Constantes.TypeTraitementRec;
                 * */
                //--------------------
                try
                {
                    proxy.checkService();
                }
                catch (Exception ex)
                {
                    Errmess = ex.Message;
                    //--------------
                  /*
                    Presenter.setValues_ofData();
                    Log.ExceptionLogger.Error("Message : " + Errmess);
                   * */
                    //--------------------
                    Utilitaires.AfficherErreur("Erreur connexion avec ACEVISION", ex.Message);
                    return null;
                }
                #endregion
                #region Init param Authentification DTO
                ACEVISIONWebService.DTOAuth authDTO = new ACEVISIONWebService.DTOAuth()
                {
                    //Login = "admin",
                   // Password = "admin",
                   //16112016
                    Login = System.Configuration.ConfigurationManager.AppSettings["LoginWebSce"],
                    Password = System.Configuration.ConfigurationManager.AppSettings["PasswordWebSce"],

                    Authenticate = ACEVISIONWebService.DTOEnumAuthenticate.Logged,
                    Authorization = ACEVISIONWebService.DTOEnumAuthorization.ReadAndWrite,
                };

                #endregion

              
                //-19112015---la Log
                PresenterLog.SetParametersLog();
                //-----------------
                IList<ReceptionDTO> LesDonneesDeFacturation = new List<ReceptionDTO>();
                // - Obtention des compteurs de l'abonne :  faudra fournir l'IDAbon
                #region Get Billing DATA

                IList<ACEVISIONWebService.DTOMeterGroup> LesGroupeClient = proxy.getMeterGroups(ref authDTO);

                string ListGroupFailed = string.Empty;
                string ListRefRaccordFailed = string.Empty;

                if (LesGroupeClient != null && LesGroupeClient.Count > 0)
                {
                    foreach (ACEVISIONWebService.DTOMeterGroup grp in LesGroupeClient)
                   //foreach (ACEVISIONWebService.DTOMeterGroup grp in LesGroupeClient.Where(g=>g.MeterGroupName=="032032"))
                    {
                        #region check groupe
                                 //-A verfier qye le groupe correspond bel et bien à un code SiteExplotation
                        if (grp.MeterGroupName.Length == 6)//groupe==XXXYYY                             
                                {
                                    IList<ACEVISIONWebService.DTOMeterIdentifier> LesCompteursDuGroupe = grp.MeterList;
                                    if (LesCompteursDuGroupe != null && LesCompteursDuGroupe.Count > 0)
                                    {
                                        foreach (ACEVISIONWebService.DTOMeterIdentifier cptgrp in LesCompteursDuGroupe)//--OK
                                       // foreach (ACEVISIONWebService.DTOMeterIdentifier cptgrp in LesCompteursDuGroupe.Where(f => f.SerialNumber == "68025196"))//-pr test
                                       // foreach (ACEVISIONWebService.DTOMeterIdentifier cptgrp in LesCompteursDuGroupe.Take(100))//54 ok
                                           // foreach (ACEVISIONWebService.DTOMeterIdentifier cptgrp in LesCompteursDuGroupe.Skip(52).Take(3))//52 elt ok (bleme proxy)
                                        {
                                            #region check compteur
                                            ACEVISIONWebService.DTOMeter LeCompteur = proxy.getMeterById(ref authDTO, cptgrp.MeterId);
                                            if (LeCompteur != null)
                                            {

                                                /// - Pas tres utile pour le moment
                                                ACEVISIONWebService.DTOCustomer LeClient = proxy.getCustomerById(ref authDTO, LeCompteur.CustomerId);

                                               

                                                ACEVISIONWebService.DTOEndOfBillingIdentifier LeEOBID = proxy.getEOBIDsByMeterId(ref authDTO, LeCompteur.MeterId, pDateDebut.Value, pDateFin.Value).FirstOrDefault();
                                                if (LeEOBID != null)
                                                {
                                                    ACEVISIONWebService.DTOEndOfBilling LeEOBduCompteur = proxy.getEOBById(ref authDTO, LeEOBID.EndOfBillingId);
                                                    if (LeEOBduCompteur != null)
                                                    {
                                                                #region get Index
                                                        ACEVISIONWebService.DTOValue[] LesValeurs = LeEOBduCompteur.BillingRateArray;
                                                        ACEVISIONWebService.DTOValue[] LesValeursTotales = LeEOBduCompteur.BillingTotalArray;

                                                        // -  GESTION DES I Max 1
                                                        if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() >= 7)
                                                            LesValeursMax1 = LeEOBduCompteur.BillingMaxArray.Take(7).ToArray();
                                                        else
                                                            LesValeursMax1 = LeEOBduCompteur.BillingMaxArray;
                                                        //---get saclar 
                                                        UTension = Math.Pow(10, LesValeursMax1.Max(m => m.ValueScalar));
                                                        //------------------

                                                        //puis division par 1000 pour get value en Kw
                                                        PMax1 = LesValeursMax1 != null && LesValeursMax1.Count() > 0 ? LesValeursMax1.Skip(1).Take(5).Max(p => p.Value) * UTension /1000 : 0;

                                                        // -  GESTION DES I Max 2 exceptionnellement pour les Compteurs de type SPX
                                                        if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() > 7)
                                                            LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(7).Take(7).ToArray();
                                                        else
                                                            LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(7).ToArray();
                                                        PMax2 = LesValeursMax2 != null && LesValeursMax2.Count() > 0 ? LesValeursMax2.Skip(1).Take(5).Max(p => p.Value) * UTension /1000 : 0;


                                                        // -  GESTION DES I Max 3 exceptionnellement pour les Compteurs de type SPX
                                                        if (LeEOBduCompteur.BillingMaxArray != null && LeEOBduCompteur.BillingMaxArray.Count() > 14)
                                                            LesValeursMax3 = LeEOBduCompteur.BillingMaxArray.Skip(14).Take(7).ToArray();
                                                        else
                                                            LesValeursMax2 = LeEOBduCompteur.BillingMaxArray.Skip(13).ToArray();
                                                        PMax3 = LesValeursMax3 != null && LesValeursMax3.Count() > 0 ? LesValeursMax3.Skip(1).Take(5).Max(p => p.Value) * UTension /1000 : 0;

 
                                                        int laprofondeur = LesValeurs != null ? LesValeurs.Count() : 0;
            #endregion

                                                                #region reception Item
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
                                                            
                                                            //---RG suite à MAJ du File de dcorrespondance le 10/05/2016
                                                               //IndexReactif1 = LesValeurs != null && LesValeurs[6] != null ? LesValeurs[6].Value.ToString().Replace(",", ".") : "0",
                                                               //IndexReactif2 = LesValeurs != null && laprofondeur > 8 && LesValeurs[7] != null != null ? LesValeurs[7].Value.ToString().Replace(",", ".") : "0",
                                                               //IndexReactif3 = LesValeurs != null && laprofondeur > 9 && LesValeurs[8] != null != null ? LesValeurs[8].Value.ToString().Replace(",", ".") : "0",

                                                            //RG du 16112016 :Triphasé Réactif Import  dans Energie totale
                                                            //RG du 16112016 :IndexReactif2 et IndexReactif3 toujours =0                                                           
                                                            IndexReactif1 = LesValeursTotales != null && LesValeursTotales[11] != null ? LesValeursTotales[11].Value.ToString().Replace(",", ".") : "0",                                                            
                                                            IndexReactif2 = "0",
                                                            IndexReactif3 = "0",
                                                            
                                                            
                                                            IndexHoraire = LeEOBduCompteur.TotalOperatingTime.ToString().Replace(",", ".")
                                                        };

                                                        //-Adresses : adresses geo / refraccordement(8 position)
                                                        if (!string.IsNullOrEmpty(LeCompteur.Address) && LeCompteur.Address.Length > 8)
                                                        {
                                                            UneRec.ReferenceRaccordement = LeCompteur.Address.Substring(LeCompteur.Address.Length - 8);

                                                            UneRec.PeriodeFacturation = UneRec.DateReleve.Value.GetPeriodeFacturation();
                                                            LesDonneesDeFacturation.Add(UneRec);
                                                        }
                                                        else
                                                        {
                                                            ListRefRaccordFailed += LeCompteur.SerialNumber + ":" + LeCompteur.Address + " | ";
                                                        }

            #endregion  
                                                           //20102015
                                                           // UneRec.PeriodeFacturation = UneRec.DateReleve.Value.GetPeriodeFacturation();
                                                       // LesDonneesDeFacturation.Add(UneRec);
                                                    }

                                                }

                                            }
            #endregion
                                        }
                                    }
                                    else
                                    {
                                        ListGroupFailed += grp.MeterGroupName + " | ";
 
                                    }

                                }
                        #endregion

                        //---Error
                        if (!string.IsNullOrEmpty(ListGroupFailed))
                        {
                            ListGroupFailed = ListGroupFailed.Remove(ListGroupFailed.Length - 2);
                            Log.ExceptionLogger.Error("Message : Nom du groupe incorrect selon le format SAPHIR XXXYYY {SiteExploitation} \n Groupe (s) concerné(s) : " + ListGroupFailed);

                            //-sortir du traitement general
                        }
                      
                    }

                }

                //11/12/2015
                if (!string.IsNullOrEmpty(ListRefRaccordFailed))
                {
                    ListRefRaccordFailed = ListRefRaccordFailed.Remove(ListRefRaccordFailed.Length - 2);

                    PresenterLog.ExecutionMode = Constantes.ExecutionMode_Manuel;
                    PresenterLog.TypeTraitement = Constantes.TypeTraitementRec;
                    PresenterLog.PeriodeFacturation = "N/A";
                    PresenterLog.setValues_ofData();
                    Log.ExceptionLogger.Error("Message : Reference de raccordement incorrect; format a respecter sur 8 positions \n Compteur(s) concerné(s) :" + ListRefRaccordFailed);
                }

    
                lstMvtReception = LesDonneesDeFacturation.ToList();   
             


                return LesDonneesDeFacturation.ToList();



                #endregion



            }
            catch (Exception ex)
            {
                Errmess = ex.Message;
                msgErr = ex.Message;
                //Utilitaires.AfficherErreur("Recuperation des données depuis ACEVISION", ex.Message);
                return null;
            }
        }

        public ActionResult getDats_FromEOBilling_ACEVISIOn_Detail(string NumCTR)
        {
            try
            {
                List<ReceptionDTO>lstInfo_ofCTR =new List<ReceptionDTO>();
                List<Index> lstIndex = new List<Index>();

                if(!string.IsNullOrEmpty(NumCTR))
                {
                    if (lstMvtReception != null & lstMvtReception.Count > 0)
                    {
                        lstInfo_ofCTR = lstMvtReception.Where(c => c.NumeroCompteur.Trim() == NumCTR.Trim()).ToList();

                        Index _index = new Index();
                        foreach (var item in lstInfo_ofCTR)
                        {
                            _index.IndexConsoMonop1 = item.IndexConsoMonop1;
                            _index.IndexConsoMonop2 = item.IndexConsoMonop2;
                            _index.IndexConsoMonop3 = item.IndexConsoMonop3;
                            _index.IndexHoraire = item.IndexHoraire;
                            _index.IndexIma1 = item.IndexIma1;
                            _index.IndexIma2 = item.IndexIma2;
                            _index.IndexIma3 = item.IndexIma3;
                            _index.IndexNuit = item.IndexNuit;
                            _index.IndexPointe = item.IndexPointe;
                            _index.IndexJour = item.IndexJour;
                            _index.IndexReactif1 = item.IndexReactif1;
                            _index.IndexReactif2 = item.IndexReactif2;
                            _index.IndexReactif3 = item.IndexReactif3;

                            lstIndex.Add(_index);
                        }
                        return View(lstIndex);
                    }
                }
            }
            catch (Exception ex)
            {

                string errMsg=ex.Message;
            }
            return View();
        }

        /// <summary>
        /// Save DAta in Node...
        /// </summary>
        /// <returns></returns>
        public ActionResult Upload_CTR_ACEVISION()
        {
            //-------------
            //System.Threading.Thread.Sleep(5000) //-give a effect de chargement .;;pending....
            //---------------------
            string jsonData = string.Empty;
            string msgErr=string.Empty;
            List<string> LstCTR_alreadyIntegratedNode = new List<string>();
            string valueMeter = string.Empty;
            //------------LOg
            //-27102015
            //get values to save in Log....
            PresenterLog.setValues_ofData();
            //update file config
            PresenterLog.SetParametersLog();

            PresenterLog.TotalRead_fromAce = lstMvtReception.Count();

            PresenterLog.TotalRead_fromNode = 0;
            PresenterLog.TotalInserted_toAce = 0;
            PresenterLog.TotalUpdated_toAce = 0;


            PresenterLog.ExecutionMode = Constantes.ExecutionMode_Manuel;
            PresenterLog.TypeTraitement = Constantes.TypeTraitementRec;
            //23122016
            PresenterLog.ExecutionID = Guid.NewGuid().ToString();
            PresenterLog.TotalInserted_toNode = 0;
            PresenterLog.setValues_ofData();
            Log.MonitoringLogger.Info(DateTime.Now + " -  Initialialisation du traitement !");
             
            //--------------------
            StringBuilder sbvalueMeterNotIntegrated = new StringBuilder();
            string valueMeterNotIntegrated = string.Empty;
          
            try
            {
                PresenterLog.setValues_ofData();
                //----------------------------------

                int NbreInsert = 0;
                int NbreErreur = 0;
                string Errmess = string.Empty;

                //---------------------------------------------------
                int cpt = lstMvtReception.Where(s => s.PeriodeFacturation == string.Empty || s.PeriodeFacturation == null).Count();
                
                if (cpt == 0)//-check si data a periodefact renseignée...                            
                {
                    foreach (var item in lstMvtReception)
                    {
                        //-check si N°compteur et perfact déjà integré dans le noeud
                        if (!checkMeterNode(item.NumeroCompteur, item.PeriodeFacturation, out msgErr))
                        {
                            //new ReceptionIndexPresenter().InsertDesIndexDansLeNoeud(lstMvtReception, ref NbreInsert, ref NbreErreur, ref Errmess);
                            new ReceptionIndexPresenter().InsertIndexDansLeNoeud(item, ref Errmess);

                            //23122016
                            string _val = "SiteExploitation :" + item.CodeSite + item.CodeExploitation + "- Compteur :" + item.NumeroCompteur;
                            sbvalueMeterNotIntegrated.AppendLine(string.Join("|", _val));

                            //if (NbreErreur == 0 && string.IsNullOrEmpty(Errmess))
                              //  jsonData = "Enregistrement réussi avec succès !";
                            //else
                            if(!string.IsNullOrEmpty(Errmess))
                                jsonData = string.Format("{0} Erreur(s) detectée(s) lors du telechargement ...\n Message : {1}", NbreErreur, Errmess);
                        }
                        else
                        {
                            string _PerFact = lstMvtReception.FirstOrDefault().PeriodeFacturation;
                            if (!string.IsNullOrEmpty(_PerFact))
                            {
                                PresenterLog.PeriodeFacturation = _PerFact;
                            }
                            else
                                PresenterLog.PeriodeFacturation = string.Empty;
                            PresenterLog.setValues_ofData();

                            //-stock value meter
                            LstCTR_alreadyIntegratedNode.Add(item.NumeroCompteur);
                            //valueMeter += item.NumeroCompteur + " | ";
                            //21122016
                            valueMeter +="SiteExploitation :"+item.CodeSite+item.CodeExploitation+"-Compteur :"+ item.NumeroCompteur + " | ";
                             
                        }
                    }

                    //23122016
                    ////---Error
                    //if (!string.IsNullOrEmpty(valueMeter))
                    //{
                    //    valueMeter = valueMeter.Remove(valueMeter.Length - 2);
                    //    //string msgDisplayErr = string.Format("Message : N° compteur (s) : {0} déjà intégré(s) pour cette periode de facturation ", valueMeter);
                    //    //21122016
                    //    //23122016
                    //    //var valInetgrated =string.Format("Message : {0} compteur (s) : {1} déjà intégré(s) pour cette periode de facturation ", LstCTR_alreadyIntegratedNode.Count(), valueMeter);
                    //    if (sbvalueMeterNotIntegrated.Length != 0)
                    //        valueMeterNotIntegrated = sbvalueMeterNotIntegrated.ToString();

                    //    //string msgDisplayErr = string.Format("Message : {0} compteur (s) : {1} déjà intégré(s) pour cette periode de facturation ", LstCTR_alreadyIntegratedNode.Count(),valueMeter);
                    //    //23122016
                    //    int diffIntegrated=lstMvtReception.Count- LstCTR_alreadyIntegratedNode.Count;
                    //    string msgDisplayErr = string.Format("Message : Traitement terminé avec succès ,\n cependant Pour cette periode de facturation,  sur {0} compteurs, seul(s) ce (s) {1} compteur (s)  ont été intégré(s)  : {2} , les autres déjà pris en compte ", lstMvtReception.Count, diffIntegrated, valueMeterNotIntegrated);
                    //    Log.ExceptionLogger.Error(msgDisplayErr);
                    //    jsonData = msgDisplayErr;
                    //}
                    //--------------------------------------------------
                }
                else
                    Log.ExceptionLogger.Error("Message : Données de facturation sans periode de facturation");
               
            }
            catch (Exception ex)
            {

                jsonData = ex.Message +" | KO";

                PresenterLog.setValues_ofData();
                Log.ExceptionLogger.Error("Message : " + jsonData);
            }
            finally
            {

               

                //---12112015
                 List<string> listPerfact=lstMvtReception.Select(p => p.PeriodeFacturation).Distinct().ToList();
                 if (listPerfact.Count == 1)
                     ThreadPerFact_One(LstCTR_alreadyIntegratedNode);
                 else
                     ThreadPerFact_Full(LstCTR_alreadyIntegratedNode,listPerfact);
                //---plusieurs perfact : mois date deb diffrent mois date fin
                //fer un contrôle ici pour dissoscier les compteur mis en ecart ?(style dejà intégré ?)

                //-------------------------------------------------------------
                //---end 12112015
                 //---Error
                 PresenterLog.setValues_ofData();
                 if (!string.IsNullOrEmpty(valueMeter))
                 {
                     valueMeter = valueMeter.Remove(valueMeter.Length - 2);
                     //string msgDisplayErr = string.Format("Message : N° compteur (s) : {0} déjà intégré(s) pour cette periode de facturation ", valueMeter);
                     //21122016
                     //23122016
                     //var valInetgrated =string.Format("Message : {0} compteur (s) : {1} déjà intégré(s) pour cette periode de facturation ", LstCTR_alreadyIntegratedNode.Count(), valueMeter);
                     if (sbvalueMeterNotIntegrated.Length != 0)
                         valueMeterNotIntegrated = sbvalueMeterNotIntegrated.ToString();

                     //string msgDisplayErr = string.Format("Message : {0} compteur (s) : {1} déjà intégré(s) pour cette periode de facturation ", LstCTR_alreadyIntegratedNode.Count(),valueMeter);
                     //23122016
                     if (lstMvtReception.Count > LstCTR_alreadyIntegratedNode.Count())
                     {
                         int diffIntegrated = lstMvtReception.Count - LstCTR_alreadyIntegratedNode.Count;
                         string msgDisplayErr = string.Format("Message : Traitement terminé ! ,\n cependant Pour cette periode de facturation,  sur {0} compteurs, seul(s) ce (s) {1} compteur (s)  ont été intégré(s)  : {2} , les autres déjà pris en compte ", lstMvtReception.Count, diffIntegrated, valueMeterNotIntegrated);
                         Log.ExceptionLogger.Error(msgDisplayErr);
                         jsonData = msgDisplayErr;
                     }
                 }


                //----execution counter
                //LogReceptionDTO _logb = new ReceptionIndexPresenter().getLogBdd(_PerFact);

                //-----------A revoir pour plusieur facturation ????
                 string _PerFact = lstMvtReception.FirstOrDefault().PeriodeFacturation;
                List<LogReceptionDTO> _lstLogb= new ReceptionIndexPresenter().getAllLogBdd_ofPerFact(_PerFact);

                if (_lstLogb.Count>0)
                {
                    int _CptExecution = _lstLogb.Max(m => m.CptExecution)+ 1;
                    PresenterLog.CptExecution = _CptExecution;
                }
                //------------------------------            
                PresenterLog.setValues_ofData();
              
                //Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");

                //-------Update cptExecution des Log de l'execution(debut traitee,t , Error si present...)
                bool b = new ReceptionIndexPresenter().UpdateLog_ofPerFactExecutionID(PresenterLog.ExecutionID, PresenterLog.CptExecution);
                 
                //------------------------
            }
            //-si certains CTR déjà intégrés...
            //23122016..deja gerer plus haut...
            //if (PresenterLog.TotalInserted_toNode > 0 && PresenterLog.TotalRead_fromAce > PresenterLog.TotalInserted_toNode)
            //{
            //    //jsonData = Constantes.MsgInfos_SucceedRecording + "\n Cependant " + jsonData;
            //    //jsonData = Constantes.MsgInfos_SucceedRecording + "\n Cependant ces compteur(s) ont déjà été intégrés : " + valueMeter;
            //    jsonData = Constantes.MsgInfos_SucceedRecording + "\n Cependant ces " + LstCTR_alreadyIntegratedNode.Count() + "compteur(s) ont déjà été intégrés : " + valueMeter;
            //    Log.ExceptionLogger.Error(jsonData);
            //}

            //21122016
            if (LstCTR_alreadyIntegratedNode.Count() == PresenterLog.TotalRead_fromAce)
            {
                jsonData = "Traitement Annulé Tous ces compteurs ont déjà été intégrés !";
                Log.ExceptionLogger.Error(jsonData);
            }

            //-21122016
            //if(LstCTR_alreadyIntegratedNode.Count()!=PresenterLog.TotalInserted_toNode && LstCTR_alreadyIntegratedNode.Count()!=PresenterLog.TotalRead_fromAce)
              //  jsonData = Constantes.MsgInfos_SucceedRecording + "\n Cependant ces "+LstCTR_alreadyIntegratedNode.Count()+" compteur(s) ont déjà été intégrés : " + valueMeter;

            if (string.IsNullOrEmpty(jsonData) && PresenterLog.TotalInserted_toNode == PresenterLog.TotalRead_fromAce)
                    jsonData = Constantes.MsgInfos_SucceedRecording;
            if (string.IsNullOrEmpty(jsonData) && PresenterLog.TotalInserted_toNode == 0)                    
                    jsonData = Constantes.MsgErrDataWithoutPerFact + " | KO ";

            Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");

            return Content(jsonData);
        }


        #region methodes private


        void ThreadPerFact_One(List<string> LstCTR_alreadyIntegratedNode)
        {
            string msgErr = string.Empty;
            string _PerFact = lstMvtReception.FirstOrDefault().PeriodeFacturation;
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

                    PresenterLog.TotalInserted_toNode = lstReceptionDTO.Count() - cptBdd;
                }

                PresenterLog.PeriodeFacturation = _PerFact;




            }
            else
            {
                PresenterLog.TotalInserted_toNode = 0;
                PresenterLog.PeriodeFacturation = string.Empty;
            }
        }

        void ThreadPerFact_Full(List<string> LstCTR_alreadyIntegratedNode, List<string> listPerfact)
        {
            string msgErr = string.Empty;
            List<ReceptionDTO> lstReceptionDTO = new List<ReceptionDTO>();
            string perFactFull=string.Empty;
            foreach (var item in listPerfact)
            {
                List <ReceptionDTO> lstCTR_fact= new ReceptionIndexPresenter().GetAllMvtReception(item, out msgErr);
                foreach (var recDTO in lstCTR_fact)
                {
                    lstReceptionDTO.Add(recDTO);
                }

                perFactFull+=item + " | ";
            }
            List<string> lstCTRBDD = new List<string>();
            if (lstReceptionDTO.Count() > 0)
            {
                lstCTRBDD = lstReceptionDTO.Select(s => s.NumeroCompteur).ToList();
                int cptBdd = LstCTR_alreadyIntegratedNode.Except(lstCTRBDD).Count();

                PresenterLog.TotalInserted_toNode = lstReceptionDTO.Count() - cptBdd;
                //---------A revoir
                perFactFull=perFactFull.Remove(perFactFull.Length - 2);
                PresenterLog.PeriodeFacturation = perFactFull;
            }

             
        }
        
        /// <summary>
        /// verifie si le compteur à intégrer n'exite dejà pas en BDD SAPHIR pour la meme periode
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <param name="periodeFacturation"></param>
        /// <param name="msgErr"></param>
        /// <returns></returns>
        bool checkMeterNode(string serialnumber, string periodeFacturation, out string msgErr)
        {
            msgErr = string.Empty;

            ReceptionDTO rec= new ReceptionIndexPresenter().getReception_Item(serialnumber, periodeFacturation, out msgErr);
             
            if (rec != null)
                return true;
            return false;
        }


        /// <summary>
        /// retourne laou les periode(s) de facturation
        /// RG1:si date debut et datefin sont du meme mois ,on retourne comme periode datedeb= (01/m+1/yyyy) m=12 alors (01/01/y+1),datefin=(31 ou 30 ou 28/29)
        /// RG2:si date debut(db) et datefin(df) sont de mois differents,periode==>datedeb=(01/m+1 de {db}/yyyy) ,dateinf(last day du mois/m+1{df}/yyyy)
        /// </summary>
        /// <param name="ladateDebut"></param>
        /// <param name="ladateFin"></param>
        /// <param name="DateDebut"></param>
        /// <param name="DateFin"></param>
        /// <returns></returns>
        bool GetPeriodeRecherche(DateTime ladateDebut, DateTime ladateFin, ref DateTime? DateDebut, ref DateTime? DateFin)
        {
            bool Result = false;
            try
            {
                DateTime temp = ladateDebut.AddMonths(1);
                DateTime temp2 = ladateFin.AddMonths(1);
                DateTime temp3 = new DateTime(temp2.Year, temp2.Month, 1, 23, 59, 59);
                DateDebut = new DateTime(temp.Year, temp.Month, 1);
                DateFin = temp3.AddMonths(1).AddDays(-1);
                Result = true;
            }
            catch (Exception ex)
            {
                Result = false;
                throw ex;
            }


            return Result;
        }

        /// <summary>
        /// Fonctionnalité d'export des données de releve pour une impression
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportDataReleve()
        {
            string jsonData = string.Empty;

            try
            {
                if (lstMvtReception!=null && lstMvtReception.Any())
                {
                    GridView gv = new GridView();
                    gv.DataSource = lstMvtReception;
                    gv.DataBind();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=ListeCompteurs.xls");//customize name file later...
                    Response.ContentType = "application/ms-excel";
                    Response.Charset = "";
                    //StringWriter sw = new StreamWriter(@"E:\Application\ITRON\Doc_Appli\ListeCompteurs.xls");
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter htw = new HtmlTextWriter(sw);
                    gv.RenderControl(htw);
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();

                    jsonData = "Export OK";
                }
                else
                    jsonData = "Aucun element à imprimer";
                return Content(jsonData);
            }
            catch (Exception ex)
            {
                
               return Content("Erreur de chargement de données !");
            }
        }


        public FileResult ExportToExcel()        
        {
            //DataTable _dtExport = new Utility().FormatList_ExportToExcel(_lstHeaderColumns, lstDemandes_pending);
            if (_dtExport == null)
                _dtExport = new Utility().FormatList_ExportToExcelREception(_lstHeaderColumns, lstMvtReception);
            // Creat the NpoiExport object
            using (var exporter = new NpoiExport())
            {
                exporter.ExportDataTableToWorkbook(_dtExport, "InfosACEVISION");

                string saveAsFileName = string.Format("Liste INDEX_RELEVES ACE VISION-{0:d}.xls", DateTime.Now);

                NPOI.SS.UserModel.Workbook _workbook = exporter.Workbook;
       
                MemoryStream output = new MemoryStream();
                _workbook.Write(output);
                return File(output.ToArray(), "application/vnd.ms-excel", saveAsFileName);

            }

        }
        public JsonResult PrintTable()
        {

            return Json(lstMvtReception, JsonRequestBehavior.AllowGet);
        }

        #endregion
        //........
        //public ActionResult CTR_ACEVSISIONTest(string dateDebut, string dateFin)
        //{

        //    var myList = getDats_FromEOBilling_ACEVISIOn(dateDebut, dateFin);

        //    return View(myList);
        //}
    }
}
