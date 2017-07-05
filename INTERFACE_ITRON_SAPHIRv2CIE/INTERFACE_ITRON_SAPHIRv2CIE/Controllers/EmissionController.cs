using ACEVISION.Common;
using ACEVISION.ProcessUI;
using INTERFACE_ITRON_SAPHIRv2CIE.ACEVISIONWebService;
using INTERFACE_ITRON_SAPHIRv2CIE.Common;
using INTERFACE_ITRON_SAPHIRv2CIE.Models;
using InterfaceServices.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Controllers
{  
    [Authorize(Roles=Constante.roleSuperAdmin+","+Constante.roleExecutif)]
    public class EmissionController : Controller
    {
        //
        // GET: /Emission/
        static List<string> _lstHeaderColumns = new List<string>() {"CLIENT","ID ABON","REF RACCOR","COMPTEUR","TYPE DI","ANC RACCOR","ANC IDABON","ANC COMPTEUR","ADRESSE","SITE","EXPLOITATION"};
        static DataTable _dtExport;
        //-17112016
        Utilitaires _util = new Utilitaires();
        //static string _lstMailFileExcel = System.Configuration.ConfigurationManager.AppSettings["MailAccountPassword"];
        //--14122016         
        static string _lstMailFileExcel;// =string.Empty;
        static string PwdFileExcel;// = string.Empty;

        static List<ActivationAbonneDTO> lstDemandes_pending = null;
        static List<ActivationAbonneDTO> lstDemandes_Succes=null;

         DataReferencePresenter _DataReferencePresenter = new DataReferencePresenter();
         ReceptionIndexPresenter _ReceptionIndexPresenter = new ReceptionIndexPresenter();

         //string pathFile = @"C:\Users\FHCOULIBALY\Documents\Visual Studio 2012\Projects\WinForm_FileExcel\ListeCTR_ITRON.xls";
        //14122016
         static string pathFile = string.Empty;
         //string pathFile = System.Configuration.ConfigurationManager.AppSettings["PathFile"];

         static Dictionary<string, InfoSensitiveCTR> _dicoCTR = new Dictionary<string, InfoSensitiveCTR>();

         static Dictionary<int, DTOMeter> DicoAllCTR = new Dictionary<int, DTOMeter>();

         string DefaultPhoneNumber = Constantes.DefaultPhoneNumber;
         string DefaultPasseWord = Constantes.DefaultPasseWord;
         string DefaultEmail = Constantes.DefaultEmail;
         string DefaultComment = Constantes.DefaultComment;
         string LeGroup = Constantes.LeGroup;
        //08112016
         string DefaultIDABON = Constantes.DefaultIdAbon;
         string DefaultAddress = Constantes.DefaultAddress;

         ACEVISIONWebService.DTOMeterIdentifier[] lstMeterIdentifier_ofGrp;
        
         ACEVISIONWebService.ContractsClient proxy = new ACEVISIONWebService.ContractsClient();

        //---a lire depuis le appsettong du web config avant deploy...
         ACEVISIONWebService.DTOAuth authDTO = new ACEVISIONWebService.DTOAuth()
         {
             //Login = "admin",
             //Password = "admin",
             Login = System.Configuration.ConfigurationManager.AppSettings["LoginWebSce"],
             Password = System.Configuration.ConfigurationManager.AppSettings["PasswordWebSce"],

             Authenticate = ACEVISIONWebService.DTOEnumAuthenticate.Logged,
             Authorization = ACEVISIONWebService.DTOEnumAuthorization.ReadAndWrite,
         };

         MeterPresenter _meterPresenter = new MeterPresenter();

        public ActionResult Index()
        {
             
            
            string msgErr = string.Empty;

            //*-recuperation des demandes en attentes de traitement dans base ACE VISION...
            lstDemandes_pending = new ReceptionIndexPresenter().GetAllDemandesFromSAPHIR().ToList();

            #region old
            /*

            List<SelectListItem> items_Sites = new List<SelectListItem>();
            items_Sites.Add(new SelectListItem { Text = string.Empty, Value = string.Empty, Selected = true });
            try
            {
                foreach (var item in getAllSites(ref msgErr))
                {
                    SelectListItem st = new SelectListItem { Text = item.Exploitation_libelle, Value = item.CodeExploitation };
                    items_Sites.Add(st);
                }
                @ViewBag.ListSites = items_Sites;
            }
            catch (Exception ex)
            {

                @ViewBag.MsgErr = Constantes.MsgErrLoadingSite;
            }
            */
            #endregion

            UserProfile _UserExcel = Session["AccountFileExcel"] as UserProfile;
            if (_UserExcel != null)
                _lstMailFileExcel = _UserExcel.EmailID;
            else
            {
                UserProfile _userAccountExcel;                
                using (UsersContext db = new UsersContext())
                {
                    Role _role = db.Roles.Where(r => r.RoleName == Constante.roleAdminFileExcel).FirstOrDefault();
                    webpages_UsersInRoles wp = db.webpages_UsersInRole.Where(w => w.RoleId == _role.RoleId).FirstOrDefault();
                    _userAccountExcel = db.UserProfiles.Where(u => u.UserId == wp.UserId).FirstOrDefault();

                }
            }
            string mgErr = string.Empty;
            //pathFile = @"C:\Users\FHCOULIBALY\Documents\Visual Studio 2012\Projects\WinForm_FileExcel\ListeCTR_ITRON.xls";//_DataReferencePresenter.getPathFileExcel(ref mgErr);
            pathFile = _DataReferencePresenter.getPathFileExcel(ref mgErr);
            //string pwdCrypted = _DataReferencePresenter.get_ParamsFile(ref mgErr).PasswordFileExcel;
            //PwdFileExcel = _DataReferencePresenter.get_ParamsFile(ref mgErr).PasswordFileExcel;
            ParamsFileDTO _pfd = _DataReferencePresenter.get_ParamsFile(ref mgErr);
            string filEx=string.Empty;
            if (_pfd != null)
            {
                PwdFileExcel = _pfd.PasswordFileExcel;

                //------
                ViewBag.CptElt = lstDemandes_pending.Count().ToString();
                //----------
                return View(lstDemandes_pending);
            }
            else
            {
                filEx = "not configured";
                return RedirectToAction("ErrorDisplay", "Home", new { _params = filEx });
                //return RedirectToAction("ErrorDisplay", "Home");

                
            }
            //PwdFileExcel = Crypteur.DecrypterText(pwd); ;

            //return View();
           

        }

        public List<ExploitationDTO> getAllSites(ref string msgErr)
        {
            
            return _DataReferencePresenter.AllSite(ref msgErr);
         
        }

        public JsonResult getAllExploiatations_ofSite(string codeSite)
        {
            string msgErr = string.Empty;

           
            if(!string.IsNullOrEmpty(codeSite))
            {
                return Json(_DataReferencePresenter.AllExploiatations_ofSite(codeSite,ref msgErr),JsonRequestBehavior.AllowGet);
            }
            
            return null;
        }

        /*
        public ActionResult sendData_toACEVISION(string codeExploitation)
        {

            List<EmissionDTO> Bordereau= _ReceptionIndexPresenter.GetAllBrderauxDeLexploitation(codeExploitation).ToList();

            IList<LogDTO> LesLogs = new List<LogDTO>();
            string ErrMess = string.Empty;
            SendBorderau(Bordereau,ref LesLogs, ref ErrMess);

            string jsonData = "Envoi réussi avec succès !";
            return Content(jsonData);
        }

        */
        public ActionResult getData_FromSAPHIR()
        {
                  string jsonData=string.Empty;
            IList<Guid> LesDITraitees = new List<Guid>();
            string _ErrMess = string.Empty;
            IList<LogDTO> LesLogs = new List<LogDTO>();
            int cptErr=0;
            //IList<ActivationAbonneDTO> LesDemandes = new ReceptionIndexPresenter().GetAllDemandesFromSAPHIR();            
            bool result = SaveData_InACEVISION(lstDemandes_pending, ref LesDITraitees, ref LesLogs, ref _ErrMess, ref cptErr);
            //18112016
            int cptTotalDI = lstDemandes_pending.Count();

            int cptDITraitees=LesDITraitees.Count();
           // if (result && cptErr == 0)
           if (result && cptErr == 0 && LesLogs.Count==0)
            {
                
                //Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");

                jsonData = "Envoi réussi avec succès !";
            }
           else
           {
               if (_ErrMess.Contains("EXCEL"))
                   jsonData = string.Format("Traitement annulé avec {0} Erreur(s) :  {1}  ", cptErr, _ErrMess);
               else
               {
                   //jsonData = string.Format("Traitement terminé avec {0} Erreur(s) / {1} :  \n  {2}  ", cptErr, cptTotalDI, _ErrMess);
                   //----Afficher les succès
                   if (cptErr == cptTotalDI)
                       jsonData = string.Format("Traitement terminé avec {0} Erreur(s) / {1}  :  \n  {2}  ", cptErr, cptTotalDI, _ErrMess);
                   else
                   {
                             //21122016  
                       StringBuilder sb = new StringBuilder();
                               foreach (var item in LesDITraitees)
                               {
                                 ActivationAbonneDTO ab=  lstDemandes_pending.Find(f => f.IdEmission == item);
                                 string valSucces = string.Format("SiteExploitation : {0} -Type Demande : {1} - N° Compteur {2} ", ab.CodeSite+ab.CodeExploitation,ab.LibelleTypeDemande, ab.NumeroCompteur);
                                  sb.Append(valSucces);
                               }
                               _ErrMess += "#Succès : " + sb.ToString();
                       jsonData = string.Format("Traitement terminé avec {0} succès , {1} Erreur(s) / {2}  :  \n  {3}  ", cptDITraitees, cptErr, cptTotalDI, _ErrMess);
                   }
               }
           }
            return Content(jsonData);
        }

        public bool SaveData_InACEVISION(IList<ActivationAbonneDTO> LesClients, ref  IList<Guid> LesDITraitees, ref  IList<LogDTO> LesLogs, ref string ErrMess, ref int cptErr)
        {
           
            bool result = true;
            //int cptErr=0;
            //----reinit du Dico
            DicoAllCTR=new Dictionary<int,DTOMeter>();

            LesLogs = new List<LogDTO>();
            LesDITraitees = new List<Guid>();
            LogDTO LaLog = null;

            int ClientID = 0;
            int DefaultClientID = -1;
            int DefautGroupID = -1;
            int CompteurId = 0;
            int GroupID = 0;

            //string msgDisplayErr = string.Empty;

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

            //PresenterLog.TotalRead_fromAce = lstDemandes_pending.Count();
            PresenterLog.TotalRead_fromNode = lstDemandes_pending.Count();

            //PresenterLog.TotalRead_fromNode = 0;
            PresenterLog.TotalRead_fromAce = 0;
            PresenterLog.TotalInserted_toAce = 0;
            PresenterLog.TotalUpdated_toAce = 0;



            PresenterLog.ExecutionMode = Constantes.ExecutionMode_Manuel;
            PresenterLog.TypeTraitement = Constantes.TypeTraitementEmi;
            PresenterLog.setValues_ofData();
            Log.MonitoringLogger.Info(DateTime.Now + " -  Initialialisation du traitement !");

            PresenterLog.setValues_ofData();
            int NbreInsert = 0;
            int NbreErreur = 0;
            //string Errmess = string.Empty;


            //--------------------
            #endregion

            #region Check Service
            try
            {
                proxy.checkService();
            }
            catch (Exception ex)
            {
                ErrMess = ex.Message;

                //----
                string msgDisplayErr = string.Format("Message : Erreur connexion avec ACEVISION : {0} ", ex.Message);
                Log.ExceptionLogger.Error(msgDisplayErr);
                Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");
                //---------
                //Utilitaires.AfficherErreur("Erreur connexion avec ACEVISION", ex.Message);
                LogDTO UnLog = new LogDTO()
                {
                    DateLog = DateTime.Now.ToString(),
                    DescriptionErreur = ex.Message,
                    Objet = "SERVICE WEB ACEVISION",
                    ReferenceObjet = "Check Service"
                };
                LesLogs.Add(UnLog);
                cptErr++;
                ErrMess = "Erreur connexion avec ACEVISION";
                return false;
            }
            #endregion

            #region Init param Authentification DTO 
            //----------A lire depusi le stting web.config avant deploy
            ACEVISIONWebService.DTOAuth authDTO = new ACEVISIONWebService.DTOAuth()
            {
                //Login = "admin",
                //Password = "admin",

                Login = System.Configuration.ConfigurationManager.AppSettings["LoginWebSce"],
                Password = System.Configuration.ConfigurationManager.AppSettings["PasswordWebSce"],
                
                Authenticate = ACEVISIONWebService.DTOEnumAuthenticate.Logged,
                Authorization = ACEVISIONWebService.DTOEnumAuthorization.ReadAndWrite,
            };

            #endregion

            #region Init User CIE par Defaut pour les nouveaux compteurs et compteurs resiliés
            // Si client CIE existe OK sinon creation
            //ACEVISIONWebService.DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, "CIE", true).FirstOrDefault();
            //*pr test ACEVISIONWebService.DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, Constantes.DefaultCustomer + "test", true).FirstOrDefault();
            ACEVISIONWebService.DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, Constantes.DefaultCustomer, true).FirstOrDefault();
            //ACEVISIONWebService.DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, "CIE-FCO", true).FirstOrDefault(); //---pr test...
            if (LeClient == null)
            {
                DefaultClientID = CreateCustumerInACEVISON(ref LaLog, Constantes.DefaultCustomer, Constantes.DefaultIdAbon, DefaultPhoneNumber, Constantes.DefaultAddress, DefaultEmail, Constantes.DefaultCommentCreateCltDefault, ref ErrMess);
                //-pr test DefaultClientID = CreateCustumerInACEVISON(ref LaLog, Constantes.DefaultCustomer, null, DefaultPhoneNumber, Constantes.DefaultAddress, DefaultEmail, Constantes.DefaultCommentCreateCltDefault, ref ErrMess);
               //-pour test DefaultClientID = CreateCustumerInACEVISON(ref LaLog, "CIE-FCO", "CIE-FCO|00000000", DefaultPhoneNumber, "CIE-FCO COTE D'IVOIRE", DefaultEmail, "Client CIE-FCO par defaut pour les nouveaux compteurs et compteurs resiliés", ref ErrMess);
            }
            else
                DefaultClientID = LeClient.CustomerId;

            if (!string.IsNullOrEmpty(ErrMess) || DefaultClientID == -1)//-echec ala creation ou recuperation du ID du client
            {
                Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");
                cptErr++;
                return false;//on arrete le traitement

            }
            #endregion

            #region Init Group CIE par Defaut pour les nouveaux compteurs et compteurs resiliés
            ACEVISIONWebService.DTOMeterGroup LeNewGroup = new ACEVISIONWebService.DTOMeterGroup();
            // Si groupe existe OK sinon creation
            if (DefautGroupID == -1)
            {
                //ACEVISIONWebService.DTOMeterGroup LeNewGroup = proxy.getMeterGroupsByName(ref authDTO, LeGroup, true).FirstOrDefault();
              LeNewGroup = proxy.getMeterGroupsByName(ref authDTO, LeGroup, true).FirstOrDefault();
                if (LeNewGroup == null)
                    DefautGroupID = CreateGroupInACEVISON(ref LaLog, LeGroup);
                else
                    DefautGroupID = LeNewGroup.MeterGroupId;

            }
                 
            #endregion


            #region liste des compteurs du group par defaut (disponibles ,non affectés dans ACE VISION)
                    //---les ctrs du groupe par defaut
                 ACEVISIONWebService.DTOMeterIdentifier []lstCrtFree = proxy.getMeterGroupById(ref authDTO, DefautGroupID).MeterList;
                        
            #endregion

                 #region MAJ nomClient selon RG : NAME / REFRACCOR

            //---A reviser later
            /*
                 DTOCustomer[] lstALLCLient = proxy.getCustomers(ref authDTO);
                 bool verifNAmeCustomer = !lstALLCLient.Select(l => l.Name).Contains("/");//--si true il exxite des noms e respectant pas la regle gestion
                 if (verifNAmeCustomer)
                 {
                     DTOMeter[] lstMeters = proxy.getMeters(ref authDTO);
                     if (DicoAllCTR.Count == 0)
                     {
                         foreach (var item in lstMeters)
                         {
                           DTOCustomer _clt=  proxy.getCustomerById(ref authDTO, item.CustomerId);
                             //----------------MAJ Name
                             string[] valAdress = item.Address.Split('/');
                             if (valAdress.Count() == 2)
                                 _clt.Name = _clt.Name.Trim() + " / " + valAdress[1].Trim();

                             //-----MAJ du name
                             DTOEnumWebServiceReturnCode oReturnCode = proxy.updateCustomerName(ref authDTO, item.CustomerId, _clt.Name);
                             if ((int)oReturnCode != 0)  // Erreur lors de la modification
                             {

                                 string msgDisplayErr = string.Format("COMPTEUR {0} / CLIENT {1} ", item.SerialNumber, _clt.Name);
                                 Log.ExceptionLogger.Error("MISE A JOUR NOM CLIENT , " + msgDisplayErr);
                                 // cptErr++;
                                 msgErr += msgDisplayErr + " | ";
                                 //return false;
                                 continue;
                             }   
                             //---------------

                            // DicoAllCTR.Add(item.CustomerId + ":" + item.SerialNumber, item);
                             //--check si key existe
                            // DTOMeter _dt=new DTOMeter();
                             //if(!DicoAllCTR.TryGetValue(item.CustomerId,out _dt))
                            // DicoAllCTR.Add(item.CustomerId, item);
                         }

                     }

                     //-----UPADTE name
                     //string mg=string.Empty;
                     //bool resultCHangeName = ChangeNameAllClient(out mg);
               //return false;
                     //if (!resultCHangeName)
                     //{
                     //    fdfd
                     //}
                 }
         */
                   
            //--------------------

                 #endregion

                 #region file excel
                 string msgErrFile = string.Empty;
                 #region old
                 /* 20042017
                  
                 _dicoCTR = _lstInfosCTr_fileXls(pathFile, ref msgErrFile);
                 if (!string.IsNullOrEmpty(msgErrFile))
                 {
                     LogDTO UnLog = new LogDTO()
                     {
                         DateLog = DateTime.Now.ToString(),
                         DescriptionErreur = string.Format("ERREUR LECTURE FILE EXCEL  : {0}  ", msgErrFile),
                         Objet = "ERREUR DE TRAITEMENT - FILE EXCEL",
                         // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                     };


                     string msgDisplayErr = UnLog.DescriptionErreur;
                     Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                     ErrMess += msgDisplayErr + " | ";
                     result = false;
                     cptErr++;
                     return false;
                 }
                  * */
#endregion

                 _dicoCTR = _lstInfosCTrHT(out msgErrFile);
                 if (!string.IsNullOrEmpty(msgErrFile))
                 {
                     LogDTO UnLog = new LogDTO()
                     {
                         DateLog = DateTime.Now.ToString(),
                         DescriptionErreur = string.Format("ERREUR RECUPERATION DES INFORMATIONS COMPTEURS  : {0}  ", msgErrFile),
                         Objet = "ERREUR DE TRAITEMENT - COMPTEURS HT",
                         
                     };


                     string msgDisplayErr = UnLog.DescriptionErreur;
                     Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                     ErrMess += msgDisplayErr + " | ";
                     result = false;
                     cptErr++;
                     return false;
                 }
          
                 #endregion


                 #region traitement metier
                 ErrMess = string.Empty;
            ACEVISIONWebService.DTOEnumWebServiceReturnCode leCode;
            try
            {
              DTOMeterGroup DefaulGroup=  proxy.getMeterGroupById(ref authDTO,DefautGroupID);
              DTOCustomer DefaultClt = proxy.getCustomerById(ref authDTO, DefaultClientID);
                if(DefaulGroup!=null && DefaultClt!=null)
                {

                if (LesClients != null && LesClients.Count > 0)
                {

                    IList<ACEVISIONWebService.DTOPortableTerminal> LesTSP = proxy.getAllPortableTerminals(ref authDTO);

                    

                    foreach (ActivationAbonneDTO item in LesClients)
                    {
                        // Creation du Client
                        //string Address = item.RueBoulevardAvenue + " " + item.LotIlot + " |" + item.ReferenceRaccordement;
                        //-22122015 FCO
                        //string Address = item.RueBoulevardAvenue.Trim() + " " + item.LotIlot.Trim() + " |" + item.ReferenceRaccordement.Trim();
                        //18042016
                        //string Address = item.ReferenceRaccordement.Trim() + " |" + item.RueBoulevardAvenue.Trim() + " " + item.LotIlot.Trim();
                        string Address =  item.Agglomeration.Trim()+" "+ item.RueBoulevardAvenue.Trim() + " " + item.LotIlot.Trim() + " / "+item.ReferenceRaccordement.Trim() ;

                        //RG: senace du 03112016 : lecteur depusi file Excel sinon valeur par defaut
                        //--------Alire
                        ACEVISIONWebService.DTOEnumMeter TypeCompteur = ACEVISIONWebService.DTOEnumMeter.SL7000;
                        string VersionFirmWare = string.IsNullOrEmpty(item.VersionFirmWare) ? "7.5" : item.VersionFirmWare;


                        ACEVISIONWebService.DTOEnumMeterConnection TypeTerminalReleve = ACEVISIONWebService.DTOEnumMeterConnection.PortableTerminal;
                        //----sassurer que le code : 20 TSP est tjrs valide sinon ne pas crre de ctr
                        //RG : comm regle de gestion : il ya tjrs au moins un TSP en BDD
                        int NumeroTSP = LesTSP != null && LesTSP.Count > 0 ? LesTSP[0].PortableTerminalId : 20;//-20 : code TSP valide en BDD tjrs existant
                        ClientID = 0;
                        CompteurId = 0;
                        GroupID = 0;

                        //string NomClient = item.NomAbonne.Trim() + " " + item.PrenomAbonne;// A revoir deja concatener dans le classe :ActivationAbonneDTO
                        //RG 09112016 : 1clt ==== 1ctr ;nameclient= nomclient  / refraccordement
                         string NomClient = item.Client.Trim() +" / "+item.ReferenceRaccordement,
                        //string NomClient =item.Client.Trim();// adelete jsute apres test

                        LeGroup = item.CodeSite.Trim() + item.CodeExploitation.Trim();
                        int LeTypeDI = -1;
                        int.TryParse(item.TypeDemande, out LeTypeDI);
                        string LeCritere = Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI);// item.TypeDemande
                        //-------Foramttage du msg erreur
                        #region msgErreur
                        switch (LeTypeDI) //---rajout du grp
                        {
                            case (int)ExtensionMethod.enumTypeDI.Abonnement_Simple: msgErr =Constante.msgErrHeader_Abonnement_Simple +" SiteExploitation :"+LeGroup;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Augmentation_PS_AMC: msgErr = Constante.msgErrHeader_VariationPuissance + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Augmentation_PS_SMC: msgErr = Constante.msgErrHeader_VariationPuissance + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Diminution_PS_SMC: msgErr = Constante.msgErrHeader_VariationPuissance + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Modification_commerciale: msgErr = Constante.msgErrHeader_Modification_commerciale + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Mutation: msgErr = Constante.msgErrHeader_Mutation + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Raccordement_Abonnement: msgErr = Constante.msgErrHeader_Raccordement_Abonnement + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Ré_Abonnement: msgErr = Constante.msgErrHeader_Re_Abonnement + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Rempl_Ctr_SET: msgErr = Constante.msgErrHeader_Rempl_Ctr + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Rempl_Ctr_SVC: msgErr = Constante.msgErrHeader_Rempl_Ctr + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Résiliation_à_la_demande: msgErr = Constante.msgErrHeader_Resiliation + " SiteExploitation :" + LeGroup; ;
                                break;
                            case (int)ExtensionMethod.enumTypeDI.Variation_Puissance: msgErr = Constante.msgErrHeader_VariationPuissance +" SiteExploitation :"+LeGroup;;
                                break;
                                //22032017
                            case (int)ExtensionMethod.enumTypeDI.Dimunition_PS_AMC: msgErr = Constante.msgErrHeader_VariationPuissance + " SiteExploitation :" + LeGroup; ;
                                break;
                            default: msgErr = "Erreur sur Type Demande"; break;

                        }
                        #endregion
                        //////
                        int[] tabMeterIdAdd = new int[1];
                        string idAbon_old = item.OLD_IdentifiantAbonne;
                        string newCtrHT = item.OLD_NumeroCompteur;
                        bool checkReAbonnement = false;
                        bool checkCtrChanged = false;

                        if (!string.IsNullOrEmpty(newCtrHT) || newCtrHT.Trim() != "")
                            checkCtrChanged = true;//le ctr a changé , il faut donc resilier l'ancien avanat tout traitement
                     
                        if (!string.IsNullOrEmpty(idAbon_old) || idAbon_old.Trim() != "")
                            checkReAbonnement = true;



                        PresenterLog.TotalRead_fromNode = lstDemandes_pending.Count;
                        PresenterLog.PeriodeFacturation = "N/A";
                        PresenterLog.setValues_ofData();
                        ////////

                        ACEVISIONWebService.DTOMeterGroup GrpAct;
                        int[] tabMeterIdAdd_GrpAct = new int[1];
                        ACEVISIONWebService.DTOCustomer CustumerToUpDate;

                        bool verifCtr=false; //verification Ctr dans liste des CTR Dispo
                        DTOEnumWebServiceReturnCode oReturnCode = new DTOEnumWebServiceReturnCode();
                        ACEVISIONWebService.DTOMeter LeCompteur = null;
                        ACEVISIONWebService.DTOMeter[] LesCompteur;

                        DTOMeterGroup GrpInitial;
                        //////
                        //string connString = System.Configuration.ConfigurationManager.ConnectionStrings["SAPHIRCOM_ConnectionString"].ConnectionString;
                        //////
                        bool caseReabonnement = false;
                        bool resultUseCase=false;
                        switch (LeTypeDI)
                        {
                            #region Raccordement_Abonnement-Abonnement_Simple -Ré abonnement //seance du 03112016
                            case (int)ExtensionMethod.enumTypeDI.Raccordement_Abonnement:
                            case (int)ExtensionMethod.enumTypeDI.Abonnement_Simple:
                            case (int)ExtensionMethod.enumTypeDI.Ré_Abonnement:

                                 
                                //using (var conn = new System.Data.SqlClient.SqlConnection(connString))
                                //{
                                //    conn.Open();
                                    //using (TransactionScope scope = new TransactionScope())
                                   // using (System.Data.IDbTransaction scope = conn.BeginTransaction())
                                    //{
                                      //  SimpleTransactrion

                                            //RG 08112016
                                            //--CAs Reabonnement : typedemande 4 /* obsolète*/
                                               /*Dans la mejueyre partie des cas , le clt sera toujours inexistant, vu que c'est le raccord kon reabonn et non le clt*/
                                            //----------Modification obligatoire de IDABON <---> Localisation (METER)
                                            //-----------Modifcation facultative du NumCTR

                                     //23022017: obsolète, le raccord reabponner aura toujours un nouveau clt nomme -->nameClient / refraccor
                                     /*if (LeTypeDI == (int)ExtensionMethod.enumTypeDI.Ré_Abonnement)
                                                caseReabonnement = true;*/

                                        //RG:03112016 seance de travail
                                        // 1 : Si client existe OK : rejet log : type de demande errone
                                        //-sinon creation clt
                                        #region 1
                                        //-RG:03112016                               
                                        //LeClient = proxy.getCustomersByAccountNumber(ref authDTO, item.IdentifiantAbonne).FirstOrDefault();
                                         //------------///23022017
                                        //-------------//check si reabonnement (si ancien ctr p^resent dnas groupe par defaut)
                                //RG 23022017 : dans le cas d'un rebonnement , il peut s'averer qu'on repositionne l'ancien compteur qui était là            
                                //if (caseReabonnement && !string.IsNullOrEmpty(item.OLD_NumeroCompteur) && item.OLD_NumeroCompteur.Trim() != "") //---verification de l'existence de l'ancien CTR et de sa disponiblite dan le stock)
                                if (!string.IsNullOrEmpty(item.OLD_NumeroCompteur) && item.OLD_NumeroCompteur.Trim() != "") //---verification de l'existence de l'ancien CTR et de sa disponiblite dan le stock)
                                {
                                    #region - check old ctr

                                    DTOMeter[] LesCompteurOLD = proxy.getMetersBySerialNumberFilter(ref authDTO, item.OLD_NumeroCompteur.Trim(), true);
                                    if (LesCompteurOLD.Count() > 0)
                                    {
                                        bool verifCtrOld = lstCrtFree.Select(l => l.SerialNumber).Contains(LesCompteurOLD[0].SerialNumber);
                                        if (!verifCtrOld)
                                        {
                                            LogDTO UnLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur = string.Format("ANCIEN COMPTEUR {0} NON DISPONIBLE", item.OLD_NumeroCompteur),
                                                // Objet = "ERREUR LORS DE LA CREATION COMPTEUR",
                                                Objet = "ERREUR LORS DE LA RECHERCHE DE L\' ANCIEN COMPTEUR - CLIENT"
                                                //ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                                            };


                                            string msgDisplayErr = UnLog.DescriptionErreur;
                                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                            //ErrMess += msgDisplayErr + " | ";
                                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";

                                            result = false;
                                            cptErr++;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        LogDTO UnLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = string.Format("ANCIEN COMPTEUR {0} INEXISTANT", item.OLD_NumeroCompteur),
                                            // Objet = "ERREUR LORS DE LA CREATION COMPTEUR",
                                            Objet = "ERREUR LORS DE LA RECHERCHE DE L\' ANCIEN COMPTEUR - CLIENT"
                                            //ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                                        };


                                        string msgDisplayErr = UnLog.DescriptionErreur;
                                        Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                        //ErrMess += msgDisplayErr + " | ";
                                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                        result = false;
                                        cptErr++;
                                        break;
                                    }

                                    //23022017
                                    //--------Check si le ctr(new) n'est pas deja affecté à un client
                                    LesCompteur = proxy.getMetersBySerialNumberFilter(ref authDTO, item.NumeroCompteur.Trim(), true);
                                    if (LesCompteur.Count() > 0)
                                    {
                                        LeCompteur = LesCompteur[0];

                                    if (LeCompteur.CustomerId != DefaultClientID)
                                    {
                                        //ctr non dispo
                                    
                                        DTOCustomer cltActif_ofMeter = proxy.getCustomerById(ref authDTO, LeCompteur.CustomerId);
                                        LaLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = "COMPTEUR DEJA ASSOCIE A UN AUTRE CLIENT : " + cltActif_ofMeter.Name,
                                            Objet = "ERREUR LORS DE L\'AFFECTATION COMPTEUR",
                                            ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber
                                        };
                                        //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n ERREUR LORS DE LA MODIFICATION COMPTEUR , COMPTEUR  {1} , appartenant Déjà  au client {2} ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), LeCompteur.SerialNumber, LeClient.Name);
                                        string msgDisplayErr = string.Format("ERREUR LORS DE L'AFFECTATION COMPTEUR , COMPTEUR  {0} , appartenant Déjà  au client {1} ", LeCompteur.SerialNumber, cltActif_ofMeter.Name);
                                        Log.ExceptionLogger.Error(msgErr + " - " + LaLog.DescriptionErreur + " : " + msgDisplayErr);
                                        // ErrMess += msgDisplayErr + " | ";
                                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                        result = false;
                                        cptErr++;
                             
                                        break;
                                    }
                                }
                                    //---------------------end

                                    #endregion
                                }
                                        //-------------------end 23022017

                                        //----------Abonnement simple simple / Raccordement_Abonnement
                                        LeClient = proxy.getCustomersByNameFilter(ref authDTO, NomClient, true).FirstOrDefault();

                                        //---------------------------
                                        if (LeClient == null)
                                        {
                                            if (!caseReabonnement)
                                                ClientID = CreateCustumerInACEVISON(ref LaLog,NomClient, item.IdentifiantAbonne, DefaultPhoneNumber, Address, DefaultEmail, DefaultComment, ref ErrMess);
                                            else //---mauvais use case indexe
                                            {
                                                LogDTO UnLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = string.Format("CLIENT {0} INEXISTANT  ",NomClient),                                                  
                                                    Objet = "ERREUR DE TRAITEMENT - CLIENT",
                                                   // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                                                };


                                                string msgDisplayErr = "COMPTEUR "+ item.NumeroCompteur.Trim() +", ERREUR LORS DE LA RECHERCHE DU CLIENT , "+UnLog.DescriptionErreur;
                                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                                //ErrMess += msgDisplayErr + " | ";
                                                ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                result = false;
                                                cptErr++;
                                                break;
                                            }

                                            if (ClientID == -1)
                                            {
                                                LesLogs.Add(LaLog);
                                                result = false;
                                                cptErr++;
                                                break;
                                            }
                                        }
                                        else //RG 23022017 : Vu que le clt sera tjrs null , not in here, a refactoriser later
                                        {
                                            if (!caseReabonnement)
                                            {
                                                //RG :18112016 vu les TI et les acs echecs sur le ctr par luiste du traitement de l'action
                                                // et comme on ne peut supprimer le clt dans ace vision via websces...
                                                /*
                                                LogDTO UnLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = string.Format("CLIENT {0} DEJA EXISTANT  ", NomClient),
                                                    Objet = "ERREUR DE TRAITEMENT - CLIENT"                                                     
                                                };

                                                string msgDisplayErr = "COMPTEUR " + item.NumeroCompteur.Trim() + " , ERREUR LORS DE LA RECHERCHE DU CLIENT , " + UnLog.DescriptionErreur;
                                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                                ErrMess += msgDisplayErr + " | ";
                                                result = false;
                                                cptErr++;
                                                break;
                                                */
                                            }

                                            //----------------09112016
                                            //----------check si le clt est deja associe a un ctr ,du coup il fait kil soit dabord resilier
                                            //---check clt possede au moin un ctr <----->en gros si le clt est tjrs actif
                                            DTOMeter[] dtMeter = proxy.getMetersByCustomerId(ref authDTO, LeClient.CustomerId);
                                            if (dtMeter.Count() == 0)
                                            {
                                                ClientID = LeClient.CustomerId;
                                            }
                                            else
                                            {
                                                LogDTO UnLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = string.Format("CLIENT {0} POSSEDANT DEJA COMPTEUR {1}  ", NomClient, dtMeter[0].SerialNumber),
                                                    Objet = "CLIENT ACTIF"

                                                };


                                                string msgDisplayErr = UnLog.Objet + " - " + UnLog.DescriptionErreur;
                                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                                ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                result = false;
                                                cptErr++;
                                                break;
                                            }

                                        
 
                                        }
                                        //----------------------------
                                        #endregion
                                        //RG 03112016
                                        //2: Si compteur existe OK sinon Creation - cas abonement 
                                      
                                        #region 2
                                        //ACEVISIONWebService.DTOMeter LeCompteur = proxy.getMetersBySerialNumber(ref authDTO, item.NumeroCompteur).FirstOrDefault();

                                        //if (caseReabonnement) //---verification de l'existence de l'ancien CTR et de sa disponiblite dan le stock)
                            

                                        //--check new ctr...
                                        // ACEVISIONWebService.DTOMeter[] LesCompteur = proxy.getMetersBySerialNumber(ref authDTO, item.NumeroCompteur.Trim());
                                        LesCompteur = proxy.getMetersBySerialNumberFilter(ref authDTO, item.NumeroCompteur.Trim(), true);
                                        if (LesCompteur.Count() > 0)
                                            LeCompteur = LesCompteur[0];
                                        //---
                                        if (LeCompteur == null)
                                        {
                                            //08112016
                                            //--CAs Reabonnement : typedemande 4
                                            //----------Modification obligatoire de IDABON <---> Localisation (METER)
                                                          //-----deja pris en compte ici
                                            //-----------Modifcation facultative du NumCTR                                           
                                            
                                            
                                            //----lecteure later depuis file excel
                                            CompteurId = CreateMeterInACEVISION(ref LaLog, item.NumeroCompteur, ClientID, item.IdentifiantAbonne, TypeCompteur,
                                                Address, VersionFirmWare, DefaultPasseWord, TypeTerminalReleve, NumeroTSP, DefaultPhoneNumber);
                                            //-----09112016
                                            //CompteurId = CreateMeterInACEVISION(ref LaLog, item.NumeroCompteur.Trim(), ClientID, item.IdentifiantAbonne, TypeCompteur,
                                            //  Address, VersionFirmWare, DefaultPasseWord, TypeTerminalReleve, NumeroTSP, DefaultPhoneNumber);

                                            if (CompteurId == -1)
                                            {
                                                LesLogs.Add(LaLog);
                                                //------15112016                                               
                                                //ErrMess += LaLog.Objet+" : "+ LaLog.DescriptionErreur + " | ";
                                                ErrMess += msgErr + " : " + LaLog.Objet + " : " + LaLog.DescriptionErreur + " | ";
                                                cptErr++;
                                                //22122016
                                                result = false;
                                                //-----24022017
                                                Log.ExceptionLogger.Error(ErrMess.Remove(ErrMess.Length - 2));
                                                //--------------------
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            CompteurId = LeCompteur.MeterId;

                                            //-----check meter appartient au clientârdefaut "CIE"
                                            if (LeCompteur.CustomerId == DefaultClientID)
                                            {
                                                // Le compteur appartient au client CIE par defaut : Modification : association du client à ce ctr dispo ??
                                                //et apprtient au grp par defaut
                                                //------------------------
                                                verifCtr = lstCrtFree.Select(l => l.SerialNumber).Contains(LeCompteur.SerialNumber);
                                                if (verifCtr)
                                                {
                                                    //-------------------------------
                                                    ACEVISIONWebService.DTOEnumWebServiceReturnCode xCode = proxy.updateMeterCustomer(ref authDTO, CompteurId, ClientID);
                                                    if (xCode != null && (int)xCode != 0)
                                                    {
                                                        LogDTO UnLog = new LogDTO()
                                                        {
                                                            DateLog = DateTime.Now.ToString(),
                                                            DescriptionErreur = xCode.ToString(),
                                                            // Objet = "ERREUR LORS DE LA CREATION COMPTEUR",
                                                            Objet = "ERREUR LORS DE L'ASSOCIATION COMPTEUR - CLIENT",
                                                            ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                                                        };


                                                        string msgDisplayErr = string.Format("Message : Type de demande -{0} \n ERREUR LORS DE L\'AFFECTTION COMPTEUR : {1} ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), LeCompteur.SerialNumber);
                                                        Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                                        //ErrMess += msgDisplayErr + " | ";
                                                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                        result = false;
                                                        cptErr++;
                                                        break;
                                                    }

                                                    //08112016
                                                    //--CAs Reabonnement : typedemande 4
                                                    //----------Modification obligatoire de IDABON <---> Localisation (METER)
                                                    //if (caseReabonnement)
                                                    //{ 
                                                     //---le ctr est ds le group par defaut du coup tout ce qui est IDABON, adresse st celle du groupe par defaut
                                                        //-chnagement location du CTR
                                                        ACEVISIONWebService.DTOEnumWebServiceReturnCode LeCode = proxy.updateMeterLocation(ref authDTO, CompteurId, item.IdentifiantAbonne);
                                                        if ((int)LeCode != 0)  // Erreur lors de la modification
                                                        {
                                                            LaLog = new LogDTO()
                                                            {
                                                                DateLog = DateTime.Now.ToString(),
                                                                DescriptionErreur = LeCode.ToString(),
                                                                Objet = "MODIFICATION DE L'Identifiant CLIENT AU NIVEAU COMPTEUR",
                                                                ReferenceObjet = item.IdentifiantAbonne
                                                            };


                                                           // string msgDisplayErr = string.Format("COMPTEUR {0} / Client {1} ,Ancien Identifiant : {2} ,Nouvel Identifiant DU CLIENT : {3} ", item.NumeroCompteur, item.Client, item.OLD_IdentifiantAbonne, item.IdentifiantAbonne);
                                                            string msgDisplayErr = string.Format("COMPTEUR {0} / Client {1} ", item.NumeroCompteur,item.IdentifiantAbonne);
                                                            //Log.ExceptionLogger.Error(msgDisplayErr);
                                                            Log.ExceptionLogger.Error(msgErr + " - " + LaLog.Objet + " : " + LaLog.DescriptionErreur);
                                                            result = false;
                                                            cptErr++;
                                                            //ErrMess = msgDisplayErr;
                                                            //ErrMess += msgDisplayErr + " | ";
                                                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                            break;
                                                        }

                                                        //-adresse
                                                        //  Modification Adresse CTR(@adrsese geo /refraccodement)
                                                        LeCode = proxy.updateMeterAddress(ref authDTO, CompteurId, Address);

                                                        if ((int)LeCode != 0)  // Erreur lors de la modification
                                                        {
                                                            LaLog = new LogDTO()
                                                            {
                                                                DateLog = DateTime.Now.ToString(),
                                                                DescriptionErreur = LeCode.ToString(),
                                                                Objet = "MODIFICATION DE L'Adresse DU COMPTEUR - REFRACCORDEMENT",
                                                                ReferenceObjet = item.IdentifiantAbonne
                                                            };


                                                            //string msgDisplayErr = string.Format("COMPTEUR {0} ,Ancienne Adresse DU CLIENT : {0} ,Nouvelle Adresse DU CLIENT : {1} ", LeCompteur.SerialNumber, LeCompteur.Address, Address);
                                                            string msgDisplayErr = string.Format("COMPTEUR {0} ,Adresse DU CLIENT : {1} ", LeCompteur.SerialNumber,Address);
                                                            //Log.ExceptionLogger.Error(msgDisplayErr);
                                                            Log.ExceptionLogger.Error(msgErr + " - " + LaLog.Objet + " : " + LaLog.DescriptionErreur);
                                                            result = false;
                                                            cptErr++;                                                            
                                                            //ErrMess += msgDisplayErr + " | ";
                                                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                            break;
                                                        }
                                                    //}
                                                    //-----deja pris en compte ici
                                                    //-----------Modifcation facultative du NumCTR

                                                }
                                                else //le ctr existant ne figure pas dans le groupe par defaut 
                                                {
                                                    LaLog = new LogDTO()
                                                    {
                                                        DateLog = DateTime.Now.ToString(),
                                                        DescriptionErreur = "COMPTEUR EXISTANT NON DISPONIBLE : " + LeCompteur.SerialNumber,
                                                        Objet = "ERREUR LORS DE L\'AFFECTATION COMPTEUR",
                                                        ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber
                                                    };
                                                    //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n ERREUR LORS DE LA MODIFICATION COMPTEUR , COMPTEUR  {1} , appartenant Déjà  au client {2} ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), LeCompteur.SerialNumber, LeClient.Name);
                                                    string msgDisplayErr = string.Format("ERREUR LORS DE L'AFFECTATION COMPTEUR {0} ,non disponible appartenant à un autre groupe ", LeCompteur.SerialNumber);
                                                    Log.ExceptionLogger.Error(msgErr + " - " + LaLog.DescriptionErreur + " : " + msgDisplayErr);
                                                    //ErrMess += msgDisplayErr + " | ";
                                                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                    result = false;
                                                    cptErr++;
                                                    break;
                                                }

                                            }
                                            else
                                            {
                                                // Le compteur appartient a un client autre que le client par defaut
                                                //LogDTO LaLog = new LogDTO()
                                                //-check clt affecet à ce ctr
                                                DTOCustomer cltActif_ofMeter = proxy.getCustomerById(ref authDTO, LeCompteur.CustomerId);
                                                LaLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = "COMPTEUR DEJA ASSOCIE A UN AUTRE CLIENT : " + cltActif_ofMeter.Name,
                                                    Objet = "ERREUR LORS DE L\'AFFECTATION COMPTEUR",
                                                    ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber
                                                };
                                                //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n ERREUR LORS DE LA MODIFICATION COMPTEUR , COMPTEUR  {1} , appartenant Déjà  au client {2} ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), LeCompteur.SerialNumber, LeClient.Name);
                                                string msgDisplayErr = string.Format("ERREUR LORS DE L'AFFECTATION COMPTEUR , COMPTEUR  {0} , appartenant Déjà  au client {1} ", LeCompteur.SerialNumber, cltActif_ofMeter.Name);
                                                Log.ExceptionLogger.Error(msgErr + " - " + LaLog.DescriptionErreur + " : " + msgDisplayErr);
                                               // ErrMess += msgDisplayErr + " | ";
                                                ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                result = false;
                                                cptErr++;
                                                /////
                                               // scope.Dispose();
                                                //scope.Rollback();
                                                /////
                                                break;
                                            }
                                        }
                                        if (CompteurId == -1)
                                        {
                                            LesLogs.Add(LaLog);
                                            break;
                                        }
                                        //-----------------------------------------------------------------------
                                        #endregion

                                        //RG 03112016
                                        //3: Si groupe metier(site/Exploitation) existe OK sinon creation
                                        #region 3
                                        LeNewGroup = proxy.getMeterGroupsByName(ref authDTO, LeGroup, true).FirstOrDefault();
                                        if (LeNewGroup == null)
                                            GroupID = CreateGroupInACEVISON(ref LaLog, LeGroup);
                                        else
                                            GroupID = LeNewGroup.MeterGroupId;
                                        if (GroupID == -1)
                                        {
                                            LesLogs.Add(LaLog);
                                            result = false;
                                            cptErr++;
                                            break;
                                        }
                                        #endregion

                                        //RG 03112016
                                        //4: Association  Compteur - Groupe 
                                        #region 4
                                        //changeMeterGroup : clientName-->cient initial
                                        if (verifCtr)//---ctr dispo ds le grp par defaut
                                        {
                                            // DTOCustomer oDTOCustomer = proxy.getCustomerById(ref authDTO,LeCompteur.CustomerId);
                                            //oReturnCode = proxy.ChangeMeterGroup(ref authDTO, item.NumeroCompteur.Trim(), DefaultClt.Name, DefaulGroup.MeterGroupName, LeGroup);
                                            //oReturnCode = proxy.ChangeMeterGroup(ref authDTO, item.NumeroCompteur.Trim(), DefaultClt.Name.Trim(), DefaulGroup.MeterGroupName.Trim(), LeGroup.Trim());
                                            //Nomclient :cds ce acas celui à qui on veut que le ctr soit associé...
                                            //ok juka 07112016 - oReturnCode = proxy.ChangeMeterGroup(ref authDTO, item.NumeroCompteur.Trim(), NomClient, DefaulGroup.MeterGroupName.Trim(), LeGroup.Trim());
                                            if(!caseReabonnement)
                                               oReturnCode = proxy.ChangeMeterGroup(ref authDTO, item.NumeroCompteur.Trim(), NomClient.Trim(), DefaulGroup.MeterGroupName.Trim(), LeGroup.Trim());
                                            else
                                               oReturnCode = proxy.ChangeMeterGroup(ref authDTO, item.NumeroCompteur.Trim(), LeClient.Name, DefaulGroup.MeterGroupName.Trim(), LeGroup.Trim());

                                            
                                            if ((int)oReturnCode != 0)
                                            {
                                                LaLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = oReturnCode.ToString(),
                                                    // DescriptionErreur = "mon test echechec",--pr test
                                                    Objet = msgErr,
                                                    ReferenceObjet = item.OLD_IdentifiantAbonne
                                                };

                                                string msgDisplayEr = string.Format("Message : Type de demande -{0} \n ERREUR LORS DE L\'AFFECTATION COMPTEUR {1} au GROUPE : {2} ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), LeCompteur.SerialNumber, LeGroup);
                                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayEr);
                                               // ErrMess += msgDisplayEr + " | ";
                                                ErrMess += msgErr + " : " + msgDisplayEr + " | ";
                                                result = false;
                                                cptErr++;
                                                break;
                                            }


                                        }
                                        else //---A revoir...
                                        {

                                            bool resultAddCtr = AddCompteurToGroup(ref LaLog, CompteurId, item.NumeroCompteur, GroupID, LeGroup);
                                            if (!resultAddCtr)
                                            {
                                                LesLogs.Add(LaLog);
                                                result = false;
                                                cptErr++;
                                                break;
                                            }

                                        }
                                        #endregion

                                       // scope.Complete();
                                       // scope.Commit();
                                    //}

                               // }//using connString

                                if (LaLog == null)
                                    LesDITraitees.Add(item.IdEmission);
                                break;

                            #endregion

                            #region Résiliation_à_la_demande
                            case (int)ExtensionMethod.enumTypeDI.Résiliation_à_la_demande:
                                // Update Compteur : Suppression du client rattaché au compteur 
                                // et Changement statut du compteur (Mise à disposition CIE)                     

                                #region 0 : check serialnumber not null
                                        if (string.IsNullOrEmpty(item.NumeroCompteur.Trim()))
                                        {
                                            LaLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur = "N°COMPTEUR NULL",//"NUMERO COMPTEUR DU CLIENT NULL",
                                                Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                                ReferenceObjet = item.NumeroCompteur
                                            };
                                            result = false;
                                            cptErr++;
                                            //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n NUMERO COMPTEUR DU CLIENT NULL : {1},  IDENTIFIANT : {2} DU CLIENT NON TROUVE ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client, item.NumeroCompteur);
                                            string msgDisplayErr = string.Format("CLIENT-{0} ,{1} ", item.Client, LaLog.DescriptionErreur);
                                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                            //ErrMess += msgDisplayErr + " | ";
                                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                            break;
                                        }
                               #endregion

                                #region 1 :check client by name
                        
                                        LeClient = proxy.getCustomersByNameFilter(ref authDTO, NomClient, true).FirstOrDefault();

                                        if (LeClient == null)
                                        {
                                            
                                                LogDTO UnLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = string.Format("CLIENT {0} INEXISTANT  ", NomClient),
                                                    Objet = "ERREUR DE TRAITEMENT - CLIENT",
                                                    // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                                                };


                                                string msgDisplayErr = "COMPTEUR " + item.NumeroCompteur.Trim() + ", ERREUR LORS DE LA RECHERCHE DU CLIENT , " + UnLog.DescriptionErreur;
                                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                                //ErrMess += msgDisplayErr + " | ";
                                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                result = false;
                                                cptErr++;
                                                break;
  
                                        }
                                        else
                                        { 
                                            //---check clt possede au moin un ctr <----->en gros si le clt est tjrs actif
                                          DTOMeter[] dtMeter=  proxy.getMetersByCustomerId(ref authDTO,LeClient.CustomerId);
                                          if (dtMeter.Count()>0)
                                            {
                                              ClientID = LeClient.CustomerId;
                                            }
                                            else
                                            {
                                                LogDTO UnLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = string.Format("CLIENT {0} NE POSSEDANT AUCUN COMPTEUR  ", NomClient),
                                                    Objet = "CLIENT INACTIF"
                                       
                                                };


                                                string msgDisplayErr =UnLog.Objet+" - "+ UnLog.DescriptionErreur;
                                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                               // ErrMess += msgDisplayErr + " | ";
                                              ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                result = false;
                                                cptErr++;
                                                break;
                                            }

                                        }
                                   
                                        #endregion

                                #region 2: check ctr by serialnumber
                                    
                                        DTOMeter[]  LesCompteurReturn = proxy.getMetersBySerialNumberFilter(ref authDTO, item.NumeroCompteur.Trim(), true);
                                        if (LesCompteurReturn.Count() > 0)
                                          {
                                              bool verifCtrResilied = lstCrtFree.Select(l => l.SerialNumber).Contains(LesCompteurReturn[0].SerialNumber);
                                             if (verifCtrResilied)
                                              {
                                                  LogDTO UnLog = new LogDTO()
                                                  {
                                                      DateLog = DateTime.Now.ToString(),
                                                      DescriptionErreur = string.Format("COMPTEUR {0} DEJA RESILIE", item.NumeroCompteur),
                                                      // Objet = "ERREUR LORS DE LA CREATION COMPTEUR",
                                                      Objet = "ERREUR LORS DE LA RESILIATION DU COMPTEUR "                                        
                                                  };


                                                  string msgDisplayErr = UnLog.DescriptionErreur;
                                                  Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                                 // ErrMess += msgDisplayErr + " | ";
                                                 ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                  result = false;
                                                  cptErr++;
                                                  break;
                                              }
                                          }
                                          else
                                          {
                                              LogDTO UnLog = new LogDTO()
                                              {
                                                  DateLog = DateTime.Now.ToString(),
                                                  DescriptionErreur = string.Format("COMPTEUR {0} INEXISTANT",item.NumeroCompteur),                                         
                                                  Objet = "ERREUR LORS DE LA RECHERCHE DU COMPTEUR A RESILIER "
                             
                                              };


                                              string msgDisplayErr = UnLog.DescriptionErreur;
                                              Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                              ErrMess += msgErr + " : " +  msgDisplayErr + " | ";
                                              result = false;
                                              cptErr++;
                                              break;
                                          }

                                            #endregion

                                #region 3 : change meter to defaulgroup

                                        //recherche du groupe dans Ace vision
                                        ACEVISIONWebService.DTOMeterGroup GrpInitialSAPHIR = proxy.getMeterGroupsByName(ref authDTO, LeGroup.Trim(), true).FirstOrDefault();
                                        // ACEVISIONWebService.DTOEnumWebServiceReturnCode LeCode = 0;
                                        //check num CtR existe bel et bien dans le group au niveau de ACE VIsion
                                        string CTRResilied = item.NumeroCompteur.Trim();
                                        if (GrpInitialSAPHIR != null)
                                        {
                                            DTOMeterGroup _grpAppartenance=null;
                                                   #region check groupe appartenance du ctr
                                             //Chargement de tous les groupes
                                            /*
                                                    DTOMeterGroup[] vDTOMeterGroup = proxy.getMeterGroups(ref authDTO);             
                                                    if ((vDTOMeterGroup != null) && vDTOMeterGroup.Any())
                                                    {
                                                        // Parcours des groupes
                                                            foreach (DTOMeterGroup oDTOMeterGroup in vDTOMeterGroup.OrderBy(o => o.MeterGroupName))
                                                            {                        
                                                                // Parcour des compteurs pour ce groupe
                                                                foreach (var oDTOMeteridentifier in oDTOMeterGroup.MeterList)
                                                                {
                                                                    //recherche du compteur dans ce groupe
                                                                     if (oDTOMeteridentifier.SerialNumber ==CTRResilied)
                                                                       {
                                                                         _grpAppartenance =oDTOMeterGroup;
                                                                         break;
                                                                       }
                                                                }
                                                                continue;
                                                            }
                                                    }
                                            */
                                            DTOMeterGroup[] lstGrpResult = proxy.getMeterGroupsBySerialNumber(ref authDTO, CTRResilied);
                                            if ((lstGrpResult != null) && lstGrpResult.Any())
                                                _grpAppartenance = lstGrpResult[0];
                                                   #endregion
                                            if(_grpAppartenance!=null)
                                            {
                                                if (_grpAppartenance.MeterGroupId == GrpInitialSAPHIR.MeterGroupId)
                                                {
                                                    ACEVISIONWebService.DTOCustomer LeClt = proxy.getCustomerById(ref authDTO, LesCompteurReturn[0].CustomerId);
                                                    //oReturnCode = proxy.ChangeMeterGroup(ref authDTO, CTRResilied, LeClt.Name, GrpInitialSAPHIR.MeterGroupName, DefaulGroup.MeterGroupName);
                                                    oReturnCode = proxy.ChangeMeterGroup(ref authDTO, CTRResilied, LeClt.Name,_grpAppartenance.MeterGroupName, DefaulGroup.MeterGroupName);
                                                    if ((int)oReturnCode != 0)
                                                    {
                                                        LaLog = new LogDTO()
                                                        {
                                                            DateLog = DateTime.Now.ToString(),
                                                            DescriptionErreur = oReturnCode.ToString(),
                                                            Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                                            ReferenceObjet = CTRResilied
                                                        };

                                                        string msgDisplayErr = string.Format("CLIENT : {0}  , N° COMPTEUR : {1} ", item.Client, CTRResilied);
                                                        Log.ExceptionLogger.Error(msgErr + " - " + LaLog.DescriptionErreur + " : " + msgDisplayErr);
                                                        result = false;
                                                        cptErr++;
                                                       // ErrMess += msgDisplayErr + " | ";
                                                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    LaLog = new LogDTO()
                                                    {
                                                        DateLog = DateTime.Now.ToString(),
                                                        DescriptionErreur = oReturnCode.ToString(),
                                                        Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                                        ReferenceObjet = CTRResilied
                                                    };

                                                    string msgDisplayErr = string.Format("CLIENT : {0}  , N° COMPTEUR : {1} appartenant deja au GROUPE {2}", item.Client, CTRResilied, _grpAppartenance.MeterGroupName);
                                                    Log.ExceptionLogger.Error(msgErr + " - " + LaLog.DescriptionErreur + " : " + msgDisplayErr);
                                                    result = false;
                                                    cptErr++;
                                                   // ErrMess += msgDisplayErr + " | ";
                                                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                 LaLog = new LogDTO()
                                                 {
                                                     DateLog = DateTime.Now.ToString(),
                                                     DescriptionErreur = oReturnCode.ToString(),
                                                     Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                                     ReferenceObjet = CTRResilied
                                                 };
                              
                                                 string msgDisplayErr = string.Format("AUCUN GROUPE TROUVE POUR CE COMPTEUR {0}", CTRResilied);
                                                 Log.ExceptionLogger.Error(msgErr + " - " + LaLog.DescriptionErreur + " : " + msgDisplayErr);
                                                 result = false;
                                                 cptErr++;                                                 
                                                // ErrMess += msgDisplayErr + " | ";
                                                   ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                 break;
                                            }


                                        }
                                        else
                                        {
                                            LogDTO UnLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur = string.Format("GROUPE {0} du COMPTEUR {1} INEXISTANT",LeGroup, item.NumeroCompteur),
                                                // Objet = "ERREUR LORS DE LA CREATION COMPTEUR",
                                                Objet = "ERREUR LORS DE LA RESILIATION DU COMPTEUR "
                                            };


                                            string msgDisplayErr = UnLog.DescriptionErreur;
                                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                            //ErrMess += msgDisplayErr + " | ";
                                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                            result = false;
                                            cptErr++;
                                            break;
                                        }
                                #endregion

                                #region 4 : change clt par defaulclt
                                         
                                        leCode = proxy.updateMeterCustomer(ref authDTO,LesCompteurReturn[0].MeterId, DefaultClientID);
                                        
                                        if (leCode != null && (int)leCode != 0)
                                        {
                                            LaLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur = leCode.ToString(),
                                                Objet = "ERREUR LORS DE LA MISE à JOUR CLient Par Defaut COMPTEUR "+CTRResilied,
                                                ReferenceObjet = "Numero Compteur : " + CTRResilied
                                            };

                                            string msgDisplayErr = LaLog.Objet;
                                            //Log.ExceptionLogger.Error(msgDisplayErr);
                                            Log.ExceptionLogger.Error(msgErr + " - " + LaLog.Objet + " : " + LaLog.DescriptionErreur );
                                            result = false;
                                            cptErr++;
                                            //ErrMess = msgDisplayErr;
                                            //ErrMess += msgDisplayErr + " | ";
                                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                            break;
                                        }
                                 
                                 #endregion

                                #region 5 : changement IDABON pr locationdefault
                                        ACEVISIONWebService.DTOEnumWebServiceReturnCode LeCod = proxy.updateMeterLocation(ref authDTO, LesCompteurReturn[0].MeterId, DefaultIDABON);
                                                    if ((int)LeCod != 0)  // Erreur lors de la modification
                                                    {
                                                        LaLog = new LogDTO()
                                                        {
                                                            DateLog = DateTime.Now.ToString(),
                                                            DescriptionErreur = LeCod.ToString(),
                                                            Objet = "MODIFICATION DE L'Identifiant CLIENT AU NIVEAU COMPTEUR " + CTRResilied,
                                                            ReferenceObjet = item.IdentifiantAbonne
                                                        };

                                                        //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n MODIFICATION DE L'Adresse DU CLIENT : {1} ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client);
                                                        string msgDisplayErr = LaLog.Objet;
                                                        //Log.ExceptionLogger.Error(msgDisplayErr);
                                                        Log.ExceptionLogger.Error(msgErr + " - " + LaLog.Objet + " : " + LaLog.DescriptionErreur);
                                                        result = false;
                                                        cptErr++;
                                                        //ErrMess = msgDisplayErr;
                                                        //ErrMess += msgDisplayErr + " | ";
                                                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                        break;
                                                    }
                                #endregion

                                #region 6 : changement adresse pr defaulAdress
                                                    LeCod = proxy.updateMeterAddress(ref authDTO, LesCompteurReturn[0].MeterId, DefaultAddress);

                                                    if ((int)LeCod != 0)  // Erreur lors de la modification
                                                    {
                                                        LaLog = new LogDTO()
                                                        {
                                                            DateLog = DateTime.Now.ToString(),
                                                            DescriptionErreur = LeCod.ToString(),
                                                            Objet = "MODIFICATION DE L'Adresse  AU NIVEAU COMPTEUR",
                                                            ReferenceObjet = item.IdentifiantAbonne
                                                        };

                                                        
                                                        string msgDisplayErr = LaLog.Objet;
                                                        
                                                        Log.ExceptionLogger.Error(msgErr + " - " + LaLog.Objet + " : " + LaLog.DescriptionErreur);
                                                        result = false;
                                                        cptErr++;                                                        
                                                        //ErrMess += msgDisplayErr + " | ";
                                                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                        break;
                                                    }
                                #endregion                              
                               //18112016
                                /*
                                if (LaLog != null)
                                {
                                    LesLogs.Add(LaLog);
                                    break;
                                }
                                */
                                LesDITraitees.Add(item.IdEmission);
                                break;
                            #endregion
 
                            #region Mutation-Augmentation_PS_SMC-Diminution_PS_SMC-Augmentation_PS_AMC-Variation_Puissance
                            case (int)ExtensionMethod.enumTypeDI.Mutation:
                            case (int)ExtensionMethod.enumTypeDI.Augmentation_PS_SMC:
                            case (int)ExtensionMethod.enumTypeDI.Diminution_PS_SMC:
                            case (int)ExtensionMethod.enumTypeDI.Augmentation_PS_AMC:
                            case (int)ExtensionMethod.enumTypeDI.Variation_Puissance:
                                //23022017--ajout de cette variationd e puissance
                            case (int)ExtensionMethod.enumTypeDI.Dimunition_PS_AMC:
                                // Update Client : Mise à jour de l'identifant du client
                                bool isMutation= LeTypeDI == (int)ExtensionMethod.enumTypeDI.Mutation?true:false;

                                //--------                                
                                //resultUseCase = useCaseVariationPuissance(isMutation, item, ref  LaLog, ref  cptErr, ref  ErrMess, msgErr);
                                //23022017
                                LogDTO _laLog = null;
                                resultUseCase = useCaseVariationPuissance(isMutation, item, lstCrtFree, DefaultClt
                                                                         , DefaulGroup, DefaultClientID, LesLogs, NumeroTSP
                                                                          ,ref _laLog, ref cptErr, ref ErrMess, msgErr);

                                if (!resultUseCase)
                                {
                                    result = false;
                                    break;
                                }

                                //--cas d'une mutation -Mise à jour du nom client...
                                 if (isMutation)
                                {
                                    resultUseCase = useCase_ModificationComAfterVariation(item, ref  LaLog, ref  cptErr, ref  ErrMess, msgErr);
                                    if (!resultUseCase)
                                    {
                                        result = false;
                                        break;
                                    }
                                }

                                /*
                                if (LaLog != null)
                                    LesLogs.Add(LaLog);
                                else //FCO
                                    LesDITraitees.Add(item.IdEmission);
                                */
                              
                                     LesDITraitees.Add(item.IdEmission);
                                break;
                            #endregion

                            #region Modification_commerciale
                            case (int)ExtensionMethod.enumTypeDI.Modification_commerciale:

                                resultUseCase = useCase_ModificationCommerciale(item,lstCrtFree, ref  LaLog, ref  cptErr, ref  ErrMess, msgErr);

                                if (!resultUseCase)
                                {
                                    result = false;
                                    break;
                                }
                                /*
                                if (LaLog != null)
                                    LesLogs.Add(LaLog);
                                else //24122015
                                    LesDITraitees.Add(item.IdEmission);
                                */
                               LesDITraitees.Add(item.IdEmission);


                                break;
                            #endregion

                            #region Rempl_Ctr_SET - Rempl_Ctr_SVC
                            case (int)ExtensionMethod.enumTypeDI.Rempl_Ctr_SET:
                            case (int)ExtensionMethod.enumTypeDI.Rempl_Ctr_SVC:
                                // Si client existe et possede un ctr OK sinon echec
                                // Si compteur existe alors Modification du Client associé sinon Creation en associant le client
                                // Si groupe existe OK sinon creation
                                // Association Compteur Groupe
                                // suppression de tous les groupes de l'ancien compteur
                                // Affectation de l'ancien compteur au groupe de compteur par defaut
                              
                                  #region Remplacement ctr - resiliation

                                    resultUseCase = useCase_Resiliation(true, item, lstCrtFree, DefaultClt, DefaulGroup, DefaultClientID, ref LaLog, ref cptErr, ref ErrMess, msgErr);
                                    if (!resultUseCase)
                                    {
                                        result = false;
                                        break;
                                    }
                                  #endregion

                                  #region Remplacement ctr - Reabonnement
                                        resultUseCase = useCase_ReAbonnementAfterResiliation(true, LesLogs,NumeroTSP, item, lstCrtFree, DefaultClt, DefaulGroup, DefaultClientID, ref LaLog, ref cptErr, ref ErrMess, msgErr);
                                        if (!resultUseCase)
                                        {
                                            result = false;
                                            break;
                                        }
                                  #endregion
                             
                                //----------------------------------------------------
                                if (LaLog == null)//24122015
                                    LesDITraitees.Add(item.IdEmission);

                                break;
                          
                                #endregion
                            default:
                                break;
                        }
                    }

                }
                }
                else
                {
                    //---message erreur
                    //echech get group par defaut
                }
                
            }
            catch (Exception ex)
            {
                
                //----
                //string msgDisplayErr = string.Format("Message : Erreur connexion avec ACEVISION : {0} ", ex.Message);
                //Log.ExceptionLogger.Error(msgDisplayErr);
                LaLog = new LogDTO()
                {
                    DateLog = DateTime.Now.ToString(),
                    DescriptionErreur = ex.Message,
                    Objet = "Erreur System",
                    ReferenceObjet = ex.Source
                };
                Log.ExceptionLogger.Error(msgErr + " - " + LaLog.Objet + " : " + LaLog.DescriptionErreur);
                cptErr++;
                result = false;
                ErrMess =LaLog.Objet+" - "+ ex.Message;
                //Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");
                //---------
            }
            finally
            {
                if(!string.IsNullOrEmpty(ErrMess) && ErrMess.Contains("|"))
                    ErrMess = ErrMess.Remove(ErrMess.Length - 2);//ErrMess = msgErr + " - " + ErrMess.Remove(ErrMess.Length - 2);

          
                //--------------------
                //-need methode ACE VISION pour lires les items inseted ds leur BDD
                PresenterLog.TotalInserted_toAce = LesDITraitees.Count;
                //-faire la difference later des items added des items updated later ???

                //-MAJ DateExtract,et autre infos après succes de traitement dans table "ACE_NODE_IN"
                //------
                new ReceptionIndexPresenter().UpDateMouvementsTraites(LesDITraitees, ref ErrMess);
                //--End MAJ

                //-----------------------------
                PresenterLog.setValues_ofData();
                //-----end
                Log.MonitoringLogger.Info(DateTime.Now + " - Fin de traitement ");

            }

            #endregion
            //----Fin traitement traitement
            //#endregion
            //Utilitaires.AfficherInformation("ACEVISION", "Fin de l'operation !");
            return result;
        }

        /// <summary>
        /// Affichages des demandes traitées
        /// </summary>
        /// <returns></returns>
        public ActionResult DisplayRequestSucces()
        {
          lstDemandes_Succes= new ReceptionIndexPresenter().GetAllDemandesSuccesFromSAPHIR().ToList();
           return View(lstDemandes_Succes);
        }


        #region methodes Privates metier

       
     
        private int CreateCustumerInACEVISON(ref LogDTO UnLog, string NomClient, string IdAbon, string PhoneNumber, string Address, string Email, string Comment, ref string msgDisplayErr)
        {
            int ClientID = -1;
            try
            {
                if (string.IsNullOrEmpty(IdAbon))//--A logger later...
                {
                    //**LOG
                    msgDisplayErr = string.Format("Message : Identifiant Abonne inexistant pour le client {0} ", NomClient);
                    Log.ExceptionLogger.Error(msgDisplayErr);
                    //---

                    UnLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = "L'identifant de l'abonné ne peut être null",
                        Objet = "CREATION D'UN CLIENT",
                        ReferenceObjet = IdAbon
                    };
                    return -1;
                }
               // ACEVISIONWebService.DTOObjectCreationReturn LeCode = proxy.addCustomer(ref authDTO, NomClient, IdAbon, PhoneNumber, Address, Email, Comment);
                //25072016
                //ACEVISIONWebService.DTOObjectCreationReturn LeCode = proxy.addCustomer(ref authDTO, NomClient, IdAbon, PhoneNumber, Address, Email, Comment, string.Empty, string.Empty, string.Empty);
                
                //-RG seance du 03112016
                //---on ne renseigne que le name du client,IDABON figurera au niveau du CTR
                ACEVISIONWebService.DTOObjectCreationReturn LeCode = proxy.addCustomer(ref authDTO, NomClient, string.Empty, string.Empty, string.Empty, string.Empty, Comment, string.Empty, string.Empty, string.Empty);
                if (LeCode != null)
                {
                    //if ((int)LeCode.ReturnCode == 0)
                    //22122015 
                        if ((int)LeCode.ReturnCode == 0 && LeCode.NewElementId > 0)
                        ClientID = LeCode.NewElementId;
                   else //-on s'assure de bel et bien recuperer le bon Id du client qui vient d'être créé
                    {
                        ACEVISIONWebService.DTOCustomer LeClient = proxy.getCustomersByAccountNumber(ref authDTO, IdAbon).FirstOrDefault();
                        if (LeClient != null)
                        {
                            ClientID = LeClient.CustomerId;
                        }
                        else
                        {
                            //**LOG
                            msgDisplayErr = string.Format("Message : Echec creation/recherche du client ,{0}  {1}", NomClient,LeCode.ReturnCode);
                            Log.ExceptionLogger.Error(msgDisplayErr);
                            //---

                            UnLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                DescriptionErreur = LeCode.ReturnCode.ToString(),
                                Objet = "CREATION D'UN CLIENT",
                                ReferenceObjet = IdAbon
                            };
                        }
                    }
                }
                else
                {
                    //**LOG
                    msgDisplayErr = string.Format("Message : Echec creation du client {0} ", NomClient);
                    Log.ExceptionLogger.Error(msgDisplayErr);
                    //---
                    Utilitaires.AfficherAvertissement("", "Une erreur est survenue lors de la création du client");
                    UnLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = LeCode.ReturnCode.ToString(),
                        Objet = "CREATION D'UN CLIENT",
                        ReferenceObjet = IdAbon
                    };

                }
            }
            catch (Exception ex)
            {
                UnLog = new LogDTO()
                {
                    DateLog = DateTime.Now.ToString(),
                    DescriptionErreur = ex.Message,
                    Objet = "CREATION D'UN CLIENT",
                    ReferenceObjet = IdAbon
                };
                ClientID = -1;
            }

            return ClientID;
        }

        //
            private int CreateMeterInACEVISION(ref LogDTO UnLog, string NumeroCompteur, int ClientID, string IdAbon, ACEVISIONWebService.DTOEnumMeter TypeCompteur, string Address, string VersionFirmWare, string DefaultPassWord,
               ACEVISIONWebService.DTOEnumMeterConnection TypeTerminalReleve, int NumeroTSP, string DefaultPhoneNumber)
            {
                int MeterID = -1;
                try
                {
                    if (string.IsNullOrEmpty(NumeroCompteur))
                    {
                        UnLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = "Le numero du compteur ne peut être null",
                            Objet = "CREATION D'UN COMPTEUR",
                            ReferenceObjet = NumeroCompteur
                        };
                        return -1;
                    }
                    ////////////////////14112016
                    string _TypeCompteur=string.Empty;
                    double _VersionFirmWare;
                    //---RG: onconserve le type de releve par TSP comme default value...
                   // string _TypeTerminalReleve=string.Empty;

                    //----A ùodifier later...
                    string _PassWord = string.Empty;

                    //string pathFile = @"C:\Users\FHCOULIBALY\Documents\Visual Studio 2012\Projects\WinForm_FileExcel\ListeCTR_ITRON.xls";
                    //Dictionary<string, InfoSensitiveCTR> _dicoCTR = _lstInfosCTr_fileXls(pathFile);
                    //if (_dicoCTR != null)
                        if (_dicoCTR.Count >0)//---contient au moins un ctr
                    {                   
                       

                        InfoSensitiveCTR _ctrInfoSensitive = new InfoSensitiveCTR();
                        if (_dicoCTR.TryGetValue(NumeroCompteur.Trim(), out _ctrInfoSensitive))
                        {
                            if (!string.IsNullOrEmpty(_ctrInfoSensitive.PasswordReader))
                                _PassWord = _ctrInfoSensitive.PasswordReader;
                            else
                                throw new Exception("Mot de Passe non fourni");

                            _VersionFirmWare = _ctrInfoSensitive.versionFirmWare;
                            /*
                            if (!string.IsNullOrEmpty(_ctrInfoSensitive.versionFirmWare))
                                _VersionFirmWare = _ctrInfoSensitive.versionFirmWare;
                            else
                                throw new Exception("Version Firmware non fournie");
                           */


                            ACEVISIONWebService.DTOEnumMeter _enumTypeCtr = (DTOEnumMeter)Enum.Parse(typeof(DTOEnumMeter), _ctrInfoSensitive.TypeMeter);

                            //----creation du ctr
                            //ACEVISIONWebService.DTOObjectCreationReturn LeCode = proxy.addMeter(ref authDTO, NumeroCompteur, ClientID, Address, IdAbon, TypeCompteur, VersionFirmWare,
                           //DefaultPassWord, TypeTerminalReleve, NumeroTSP, "10.0.0.1", 0, 0, 0, 0, DefaultPhoneNumber, DefaultPhoneNumber);
                          ACEVISIONWebService.DTOObjectCreationReturn LeCode = proxy.addMeter(ref authDTO, NumeroCompteur, ClientID, Address, IdAbon, _enumTypeCtr, _VersionFirmWare.ToString(),
                          _PassWord, TypeTerminalReleve, NumeroTSP, "10.0.0.1", 0, 0, 0, 0, DefaultPhoneNumber, DefaultPhoneNumber);

                            if (LeCode != null)
                            {
                                if ((int)LeCode.ReturnCode == 0)
                                {
                                    MeterID = LeCode.NewElementId;
                                    //14122016
                                    string msgBody = string.Format("{0} : nouveau compteur créé dans ACE VISION", NumeroCompteur);
                                    string footer = "<p><i>Ceci est un mail automatique , veuillez ne pas répondre !</i></p>";
                                    string msgErrReturn = string.Empty;
                                    new Utility().SendEMail(_lstMailFileExcel, Constantes.ObjectNotification, msgBody + footer, ref msgErrReturn);
                                }
                                else
                                {
                                    ACEVISIONWebService.DTOMeter LeCompteur = proxy.getMetersBySerialNumber(ref authDTO, NumeroCompteur).FirstOrDefault();
                                    if (LeCompteur != null)
                                    {
                                        MeterID = LeCompteur.MeterId;
                                    }
                                    else
                                    {
                                        UnLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur = LeCode.ReturnCode.ToString(),
                                            Objet = "CREATION D'UN COMPTEUR",
                                            ReferenceObjet = IdAbon
                                        };
                                    }
                                }
                            }
                            else
                            {
                                Utilitaires.AfficherAvertissement("", "Une erreur est survenue lors de la création du Compteur");
                                UnLog = new LogDTO()
                                {
                                    DateLog = DateTime.Now.ToString(),
                                    DescriptionErreur = LeCode.ReturnCode.ToString(),
                                    Objet = "CREATION DU COMPTEUR : " + NumeroCompteur,
                                    ReferenceObjet = IdAbon
                                };

                            }
                            //--------------------------------
                        }
                        else
                        {
                            UnLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                DescriptionErreur = "ECHEC DE RECUPERATION DES INFOS DU COMPTEUR",
                                Objet = "CREATION DU COMPTEUR : " + NumeroCompteur,
                                ReferenceObjet = IdAbon
                            };
                        }

                    }
                    else
                    {
                        UnLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = "ECHEC DE RECUPERATION DES INFOS DU COMPTEUR",
                            Objet = "CREATION DU COMPTEUR : " + NumeroCompteur,
                            ReferenceObjet = IdAbon
                        };
                      
                            //-TODO
                            //send mail au compte configuré pr mot de passe pr l'alerte 
                        
                    }
                    /////////////////////
                   
                }
                catch (Exception ex)
                {
                    UnLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = ex.Message,
                        Objet = "CREATION DU COMPTEUR : " + NumeroCompteur,
                        ReferenceObjet = IdAbon
                    };
                    MeterID = -1;
                }

                return MeterID;
            }

            private bool AddCompteurToGroupInACEVISION(ref LogDTO UnLog, int CompteurID, string sNumeroCompteur, int GroupID, string sGroup)
            {
                bool Result = false;
                try
                {
                    int[] MetersToUpDate = new int[1];
                    MetersToUpDate[0] = CompteurID;
                    ACEVISIONWebService.DTOEnumWebServiceReturnCode LeCode = proxy.addMeterToMeterGroup(ref authDTO, CompteurID, GroupID);
                    if ((int)LeCode == 0)
                    {
                        Result = true;
                        proxy.updateMeterToMeterGroup(ref authDTO, MetersToUpDate, GroupID, false);//-true pour reset (suppression du lien du ctr avec les autres groupe,afin de s'assurer kil appartient seulement a grp auquel il vient d'etrre rattaché...)
                    }
                    else
                    {
                        UnLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = LeCode.ToString(),
                            Objet = "ASSOCIATION DU COMPTEUR A UN GROUPE",
                            ReferenceObjet = "ID Compteur " + sNumeroCompteur + " ID Group " + sGroup
                        };
                        Result = false;
                    }
                }
                catch (Exception Ex)
                {
                    UnLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = Ex.Message,
                        Objet = "ASSOCIATION DU COMPTEUR A UN GROUPE",
                        ReferenceObjet = Ex.Message
                    };
                    Result = false;
                }
                return Result;
            }

        //04112016
            private bool AddCompteurToGroup(ref LogDTO UnLog, int CompteurID, string sNumeroCompteur, int GroupID, string sGroup)
            {
                bool Result = false;
                try
                {
                   
                    ACEVISIONWebService.DTOEnumWebServiceReturnCode LeCode = proxy.addMeterToMeterGroup(ref authDTO, CompteurID, GroupID);
                    if ((int)LeCode == 0)
                    {
                        Result = true;
                       
                    }
                    else
                    {
                        UnLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = LeCode.ToString(),
                            Objet = "ASSOCIATION DU COMPTEUR A UN GROUPE",
                            ReferenceObjet = "ID Compteur " + sNumeroCompteur + " ID Group " + sGroup
                        };
                        Result = false;
                    }
                }
                catch (Exception Ex)
                {
                    UnLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = Ex.Message,
                        Objet = "ASSOCIATION DU COMPTEUR A UN GROUPE",
                        ReferenceObjet = Ex.Message
                    };
                    Result = false;
                }
                return Result;
            }


            private int CreateGroupInACEVISON(ref LogDTO UnLog, string GroupeName)
            {
                int GroupID = -1;
                try
                {
                    if (string.IsNullOrEmpty(GroupeName))
                    {
                        UnLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = "Le nom du groupe ne peut être null",
                            Objet = "CREATION D'UN GROUPE",
                            ReferenceObjet = GroupeName
                        };
                        return -1;
                    }
                    //----code a reviser
                    //ACEVISIONWebService.DTOObjectCreationReturn LeCode = proxy.addMeterGroup(ref authDTO, GroupeName, ACEVISIONWebService.DTOEnumMeter.SL7000, 1);
                    //05112016
                    ACEVISIONWebService.DTOObjectCreationReturn LeCode = proxy.addMeterGroup(ref authDTO, GroupeName, ACEVISIONWebService.DTOEnumMeter.GroupMeterTypeAll, 1);
                    if (LeCode != null)
                    {
                        if ((int)LeCode.ReturnCode == 0)
                            GroupID = LeCode.NewElementId;
                        else//-on s'assure de bien recuperer le Id du groupe qui vient d'etre créé
                        {
                            ACEVISIONWebService.DTOMeterGroup LeGroupe = proxy.getMeterGroupsByName(ref authDTO, GroupeName, true).FirstOrDefault();
                            if (LeGroupe != null)
                            {
                                GroupID = LeGroupe.MeterGroupId;
                            }
                            else
                            {
                                UnLog = new LogDTO()
                                {
                                    DateLog = DateTime.Now.ToString(),
                                    DescriptionErreur = LeCode.ReturnCode.ToString(),
                                    Objet = "CREATION D'UN GROUPE",
                                    ReferenceObjet = GroupeName
                                };
                            }
                        }
                    }
                    else
                    {
                        Utilitaires.AfficherAvertissement("", "Une erreur est survenue lors de la création du Groupe");
                        UnLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = LeCode.ReturnCode.ToString(),
                            Objet = "CREATION D'UN GROUPE",
                            ReferenceObjet = GroupeName
                        };

                    }
                }
                catch (Exception ex)
                {
                    UnLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = ex.Message,
                        Objet = "CREATION D'UN GROUPE",
                        ReferenceObjet = GroupeName
                    };
                    GroupID = -1;
                }

                return GroupID;
            }


            public ActionResult getDemandes_FromType_SAPHIR_Detail(string NumCTR)
            {
                try
                {
                    List<ActivationAbonneDTO> lstInfo_ofDmde = new List<ActivationAbonneDTO>();
                    List<DetailsDemande> lstInfos = new List<DetailsDemande>();

                    if (!string.IsNullOrEmpty(NumCTR))
                    {
                        if (lstDemandes_pending != null & lstDemandes_pending.Count > 0)
                        {
                            lstInfo_ofDmde = lstDemandes_pending.Where(c => c.NumeroCompteur.Trim() == NumCTR.Trim()).ToList();

                            DetailsDemande _ddmde = new DetailsDemande();
                            foreach (var item in lstInfo_ofDmde)
                            {
                                _ddmde.OLD_IdentifiantAbonne = item.OLD_IdentifiantAbonne;
                                _ddmde.OLD_ReferenceRaccordement=item.OLD_ReferenceRaccordement;
                                _ddmde.OLD_NumeroCompteur=item.OLD_NumeroCompteur;
                                _ddmde.Address = item.Address;

                                lstInfos.Add(_ddmde);
                            }
                            return View(lstInfos);
                        }
                    }
                }
                catch (Exception ex)
                {

                    string errMsg = ex.Message;
                }
                return View();
            }

            public ActionResult getDemandesOK_FromType_SAPHIR_Detail(string NumCTR)
            {
                try
                {
                    List<ActivationAbonneDTO> lstInfo_ofDmde = new List<ActivationAbonneDTO>();
                    List<DetailsDemande> lstInfos = new List<DetailsDemande>();

                    if (!string.IsNullOrEmpty(NumCTR))
                    {
                        if (lstDemandes_Succes != null & lstDemandes_Succes.Count > 0)
                        {
                            lstInfo_ofDmde = lstDemandes_Succes.Where(c => c.NumeroCompteur.Trim() == NumCTR.Trim()).ToList();

                            DetailsDemande _ddmde = new DetailsDemande();
                            foreach (var item in lstInfo_ofDmde)
                            {
                                _ddmde.OLD_IdentifiantAbonne = item.OLD_IdentifiantAbonne;
                                _ddmde.OLD_ReferenceRaccordement = item.OLD_ReferenceRaccordement;
                                _ddmde.OLD_NumeroCompteur = item.OLD_NumeroCompteur;
                                _ddmde.Address = item.Address;

                                lstInfos.Add(_ddmde);
                            }
                            //return View(lstInfos);
                            return View("getDemandes_FromType_SAPHIR_Detail", lstInfos);
                        }
                    }
                }
                catch (Exception ex)
                {

                    string errMsg = ex.Message;
                }
                return View();
            }

           /**
            * \deprecated
            * \date : 14042017
            * \author : FCO
            */
           //14112016
            [Obsolete("cette methode est caduque depuis le comite projet du 14042017, les données sont maintenant stockées en bdd")]
            public Dictionary<string, InfoSensitiveCTR> _lstInfosCTr_fileXls(string strFileName,ref string msgErr)
            {
                Microsoft.Office.Interop.Excel.Range range;
                int rCnt = 0;
                int cCnt = 0;
                string msErr=string.Empty;
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
                    for (rCnt = 2; rCnt <= range.Rows.Count; rCnt++)
                    {
                        InfoSensitiveCTR _ctrHT = new InfoSensitiveCTR();
                        try
                        {
                            _ctrHT.SerialNumber = Convert.ToString((range.Cells[rCnt, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
                            _ctrHT.PasswordReader = Convert.ToString((range.Cells[rCnt, 2] as Microsoft.Office.Interop.Excel.Range).Value2);
                            //-09032017 : COPIL retirer PWD LABO
                            //_ctrHT.PasswordLabo = Convert.ToString((range.Cells[rCnt, 3] as Microsoft.Office.Interop.Excel.Range).Value2);
                            _ctrHT.TypeMeter = Convert.ToString((range.Cells[rCnt, 3] as Microsoft.Office.Interop.Excel.Range).Value2);
                            _ctrHT.versionFirmWare = (range.Cells[rCnt, 4] as Microsoft.Office.Interop.Excel.Range).Value2;
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

        /// <summary>
        /// recuperation des informations compteurs HT presents en BDD
        /// </summary>
        /// <returns>un dictionnaire de compteurs HT</returns>
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

        #region methode UseCase
            //-13112016

            //-si OK return true
        //CltActif : true pour indiquer sil sagit d'une resiliation si actif ,sinon d'un reabonnement
            private bool CheckClient(ActivationAbonneDTO adto,out DTOCustomer _customer,bool CltActif,ref LogDTO _laLog, ref int cptErr,ref string ErrMess, string msgErr)
            {
                _customer = null;

                string NomClient = adto.Client.Trim() + " / " + adto.ReferenceRaccordement;
                DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, NomClient, true).FirstOrDefault();
                int ClientID = 0;

                if (LeClient == null)
                {

                    _laLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = string.Format("CLIENT {0} INEXISTANT  ", NomClient),
                        Objet = "ERREUR DE TRAITEMENT - CLIENT",
                        // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                    };


                    string msgDisplayErr = "COMPTEUR " + adto.NumeroCompteur.Trim() + " , ERREUR LORS DE LA RECHERCHE DU CLIENT , " + _laLog.DescriptionErreur;
                    Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                    //ErrMess += msgDisplayErr + " | ";
                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                    //result = false;
                    cptErr++;
                    return false;

                }
                else
                {
                    //---check clt possede au moin un ctr <----->en gros si le clt est tjrs actif
                    DTOMeter[] dtMeter = proxy.getMetersByCustomerId(ref authDTO, LeClient.CustomerId);
                    _customer = LeClient;

                    if (dtMeter.Count() > 0)
                    {                    
                        if(!CltActif)//--cas du reabonnement,ce clt a deja un ctr
                        {
                             LogDTO UnLog = new LogDTO()
                                                {
                                                    DateLog = DateTime.Now.ToString(),
                                                    DescriptionErreur = string.Format("CLIENT {0} POSSEDANT DEJA COMPTEUR {1}  ", NomClient, dtMeter[0].SerialNumber),
                                                    Objet = "CLIENT ACTIF"

                                                };


                                                string msgDisplayErr = UnLog.Objet + " - " + UnLog.DescriptionErreur;
                                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                               // ErrMess += msgDisplayErr + " | ";
                                                ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                                //result = false;
                                                cptErr++;
                                                return false;
                        }
                        else
                        ClientID = LeClient.CustomerId;
                    }
                    else
                    {
                        if(!CltActif)//--le clt ne possede aucun ,est inactif , on peut le reabonner
                             ClientID = LeClient.CustomerId;
                        else
                        {
                            _laLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                DescriptionErreur = string.Format("CLIENT {0} NE POSSEDANT AUCUN COMPTEUR  ", NomClient),
                                Objet = "CLIENT INACTIF"

                            };


                            string msgDisplayErr = _laLog.Objet + " - " + _laLog.DescriptionErreur;
                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                            //ErrMess += msgDisplayErr + " | ";
                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                           // result = false;
                            cptErr++;
                            return false;
                        }
                    }

                }
                return true;
            }

            //-si OK return true
            private bool CheckCtr(ActivationAbonneDTO adto,bool CtrActif, bool remplacementCtr, DTOMeterIdentifier[] lstCrtFree, int DefaultClientID,out DTOMeter _oldMeter,out DTOMeter _newMeter, out DTOMeterGroup _grpAppartenance,ref LogDTO _laLog, ref int cptErr, ref string ErrMess, string msgErr)
            {
                 _grpAppartenance=null;//--value out
                 _oldMeter = null;//---value out
                 _newMeter = null;//---value out

              
                //-if (string.IsNullOrEmpty(adto.NumeroCompteur.Trim()))
                if (string.IsNullOrEmpty(adto.NumeroCompteur.Trim()) ||adto.NumeroCompteur.Trim()=="")
                {
                    _laLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = "N°COMPTEUR NULL",//"NUMERO COMPTEUR DU CLIENT NULL",
                        Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                        ReferenceObjet = adto.NumeroCompteur
                    };
                    //result = false;
                    cptErr++; // a revoir later...pr le count

                    //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n NUMERO COMPTEUR DU CLIENT NULL : {1},  IDENTIFIANT : {2} DU CLIENT NON TROUVE ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client, item.NumeroCompteur);
                    string msgDisplayErr = string.Format("CLIENT-{0} ,{1} ", adto.Client, _laLog.DescriptionErreur);
                    Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                    //ErrMess += msgDisplayErr + " | ";
                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                    return false;
                }
                else
                {
                     DTOMeter[] LesCompteurReturn = proxy.getMetersBySerialNumberFilter(ref authDTO, adto.NumeroCompteur.Trim(), true);
                     
                     if (LesCompteurReturn.Count() > 0)
                     {
                         _newMeter = LesCompteurReturn[0];
                             //---check si new ctr existe deja et si pas deja affcete a un client

                             if (LesCompteurReturn[0].CustomerId != DefaultClientID)
                             {
                                 // Le compteur appartient a un client autre que le client par defaut
                                 //LogDTO LaLog = new LogDTO()
                                 //-check clt affecet à ce ctr
                                 DTOCustomer cltActif_ofMeter = proxy.getCustomerById(ref authDTO, LesCompteurReturn[0].CustomerId);
                                 _laLog = new LogDTO()
                                 {
                                     DateLog = DateTime.Now.ToString(),
                                     DescriptionErreur = "COMPTEUR DEJA ASSOCIE A UN AUTRE CLIENT : " + cltActif_ofMeter.Name,
                                     Objet = "ERREUR LORS DE L\'AFFECTATION COMPTEUR",
                                     ReferenceObjet = "Numero Compteur : " + LesCompteurReturn[0].SerialNumber
                                 };

                                 string msgDisplayErr = string.Format("ERREUR LORS DE L'AFFECTATION COMPTEUR , COMPTEUR  {0} , appartenant Déjà  au client {1} ", LesCompteurReturn[0].SerialNumber, cltActif_ofMeter.Name);
                                 Log.ExceptionLogger.Error(msgErr + " - " + _laLog.DescriptionErreur + " : " + msgDisplayErr);
                                // ErrMess += msgDisplayErr + " | ";
                                 ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                 // result = false;
                                 cptErr++;

                                 return false;
                             }
                         
                     }
                     else//value ctr inexistant dans  ace vision
                    {
                       // if (CtrActif)//----cas resiliation simple
                       if (CtrActif && !remplacementCtr)//----ce control sur la value du newctr ,ne s'applique pas à une resiliation dans un replacement ctr
                        {
                            _laLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                //DescriptionErreur = "COMPTEUR DEJA ASSOCIE A UN AUTRE CLIENT : " + cltActif_ofMeter.Name,
                                Objet = "COMPTEUR INEXISTANT",
                                ReferenceObjet = "Numero Compteur : " + LesCompteurReturn[0].SerialNumber
                            };

                            string msgDisplayErr = string.Format("ABSENCE DE COMPTEUR  {0} ", LesCompteurReturn[0].SerialNumber);
                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                           // ErrMess += msgDisplayErr + " | ";
                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                            // result = false;
                            cptErr++;

                            return false;
                        }
                       else //--cas d'un remplacment ctr avec newCTR et cltActif //-22122016
                       {
                           //--check si newCTR in fileExcel
                           if (_dicoCTR.Count > 0)//---contient au moins un ctr
                           {


                               InfoSensitiveCTR _ctrInfoSensitive = new InfoSensitiveCTR();
                               if (!_dicoCTR.TryGetValue(adto.NumeroCompteur.Trim(), out _ctrInfoSensitive))
                               {

                                   _laLog = new LogDTO()
                                   {
                                       DateLog = DateTime.Now.ToString(),
                                       DescriptionErreur = "ECHEC DE RECUPERATION DES INFOS DU COMPTEUR",
                                       Objet = "CREATION DU COMPTEUR : " + adto.NumeroCompteur.Trim(),
                                       ReferenceObjet = adto.IdentifiantAbonne
                                   };
                                   string msgDisplayErr = string.Format("ABSENCE DE COMPTEUR  {0} - {1}", adto.NumeroCompteur.Trim(), _laLog.DescriptionErreur);
                                   Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                   // ErrMess += msgDisplayErr + " | ";
                                   ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                   // result = false;
                                   cptErr++;

                                   return false;
                               }
                           }
                           else
                           {
                               _laLog = new LogDTO()
                               {
                                   DateLog = DateTime.Now.ToString(),
                                   DescriptionErreur = "ECHEC DE RECUPERATION DES INFOS DU COMPTEUR",
                                   Objet = "CREATION DU COMPTEUR : " + adto.NumeroCompteur.Trim(),
                                   ReferenceObjet = adto.IdentifiantAbonne
                               };
                               string msgDisplayErr = string.Format("ABSENCE DE COMPTEUR  {0} - {1}", adto.NumeroCompteur.Trim(), _laLog.DescriptionErreur);
                               Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                               // ErrMess += msgDisplayErr + " | ";
                               ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                               // result = false;
                               cptErr++;

                               return false;
                           }
                           //------------
                       }
                    }

                }
               
                if (remplacementCtr)//---check oci value ancien ctr
                {
                    
                     // if (string.IsNullOrEmpty(adto.OLD_NumeroCompteur.Trim()))
                    if (string.IsNullOrEmpty(adto.OLD_NumeroCompteur.Trim()) || adto.OLD_NumeroCompteur.Trim() == "")
                        {
                            _laLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                DescriptionErreur = "N°ANCIEN COMPTEUR NULL",
                                Objet = "REMPLACEMENT ANCIEN CTR DU CLIENT",
                                ReferenceObjet = adto.NumeroCompteur
                            };
                            //result = false;
                            cptErr++;  

                            //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n NUMERO COMPTEUR DU CLIENT NULL : {1},  IDENTIFIANT : {2} DU CLIENT NON TROUVE ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client, item.NumeroCompteur);
                            string msgDisplayErr = string.Format("CLIENT-{0} ,{1} ", adto.Client, _laLog.DescriptionErreur);
                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                            //ErrMess += msgDisplayErr + " | ";
                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                            return false;
                       }         
                        else//---check ctr à remplacer by serialnumber
                        {
                            DTOMeter[] LesCompteurReturn = proxy.getMetersBySerialNumberFilter(ref authDTO, adto.OLD_NumeroCompteur.Trim(), true);
                            if (LesCompteurReturn.Count() > 0)
                            {
                                _oldMeter = LesCompteurReturn[0];
                                //---check si old meter deja dispo
                                bool verifCtrResilied = lstCrtFree.Select(l => l.SerialNumber).Contains(LesCompteurReturn[0].SerialNumber);
                               // if (verifCtrResilied)
                                if (verifCtrResilied && CtrActif)//---cas d'une resiliation, ne s'applique au moment ou l'on vient à nouveau faire le reabonnement
                                {
                                    _laLog = new LogDTO()
                                    {
                                        DateLog = DateTime.Now.ToString(),
                                        DescriptionErreur = string.Format("COMPTEUR {0} DEJA RESILIE", adto.OLD_NumeroCompteur),
                                        // Objet = "ERREUR LORS DE LA CREATION COMPTEUR",
                                        Objet = "ERREUR LORS DU REMPLACEMENT COMPTEUR "
                                    };


                                    string msgDisplayErr = _laLog.DescriptionErreur;
                                    Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                    //ErrMess += msgDisplayErr + " | ";
                                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                    // result = false;
                                    cptErr++;
                                    return false;
                                }
                                else
                                {//normalment déj géré au niveu du checkclient 
                                }

                               
                                 DTOMeterGroup[] lstGrpResult = proxy.getMeterGroupsBySerialNumber(ref authDTO, LesCompteurReturn[0].SerialNumber);
                                            if ((lstGrpResult != null) && lstGrpResult.Any())
                                                _grpAppartenance = lstGrpResult[0];

                                            if (_grpAppartenance == null)
                                            {
                                                _laLog = new LogDTO()
                                                 {
                                                     DateLog = DateTime.Now.ToString(),
                                                     DescriptionErreur = string.Format("AUCUN GROUPE TROUVE POUR CE COMPTEUR {0}", LesCompteurReturn[0].SerialNumber),
                                                     //Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                                     ReferenceObjet = LesCompteurReturn[0].SerialNumber
                                                 };

                                                string msgDisplayErr = string.Format("AUCUN GROUPE TROUVE POUR CE COMPTEUR {0}", LesCompteurReturn[0].SerialNumber);
                                                Log.ExceptionLogger.Error(msgErr + " - " + msgDisplayErr);
                                                //result = false;
                                                cptErr++;
                                               // ErrMess += msgDisplayErr + " | ";
                                                ErrMess += msgErr + " : " + msgDisplayErr + " | ";

                                                return false;
                                            }
                                            else//--check pour verifeir si old meter appartientbel au bien groupe donné par saphir
                                            {
                                               // ACEVISIONWebService.DTOMeterGroup GrpInitialSAPHIR = proxy.getMeterGroupsByName(ref authDTO, LeGroup.Trim(), true).FirstOrDefault();
                                                string _grp=adto.CodeSite.Trim() + adto.CodeExploitation.Trim();
                                                //if (_grpAppartenance.MeterGroupName != _grp)
                                                if (_grpAppartenance.MeterGroupName != _grp && CtrActif)
                                                {
                                                    _laLog = new LogDTO()
                                                    {
                                                        DateLog = DateTime.Now.ToString(),
                                                       // DescriptionErreur = oReturnCode.ToString(),
                                                        Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                                        ReferenceObjet = LesCompteurReturn[0].SerialNumber
                                                    };

                                                    string msgDisplayErr = string.Format("N° ANCIEN COMPTEUR : {0} appartenant deja au GROUPE {1}", LesCompteurReturn[0].SerialNumber, _grpAppartenance.MeterGroupName);
                                                    Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                                   // result = false;
                                                    cptErr++;
                                                   // ErrMess += msgDisplayErr + " | ";
                                                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";

                                                    return false;
                                                }
                                                //else//grp saphir inexistant
                                                //{
                                                   
                                                //}
                                            }
                                            
                            }
                            else
                            {
                                    _laLog = new LogDTO()
                                    {
                                        DateLog = DateTime.Now.ToString(),
                                        DescriptionErreur = string.Format("ANCIEN COMPTEUR {0} INEXISTANT", adto.OLD_NumeroCompteur),
                                        Objet = "ERREUR LORS DE LA RECHERCHE DU COMPTEUR A RESILIER "

                                    };


                                    string msgDisplayErr = _laLog.DescriptionErreur;
                                    Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                   // ErrMess += msgDisplayErr + " | ";
                                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                   // result = false;
                                    cptErr++;
                                    return false;
                             }
                         }
                    }                

                return true;
            }

            private bool useCase_Resiliation(bool remplacementCtr,ActivationAbonneDTO adto,DTOMeterIdentifier[] lstCrtFree, DTOCustomer DefaultClt, DTOMeterGroup DefaulGroup, int DefaultClientID, ref LogDTO _laLog, ref int cptErr, ref string ErrMess, string msgErr)
            {

                bool result = true;
                int ClientID = 0;
                int CompteurId = 0;
                int GroupID = 0;
                DTOEnumWebServiceReturnCode oReturnCode = new DTOEnumWebServiceReturnCode();
                string NomClient = adto.Client.Trim() + " / " + adto.ReferenceRaccordement;

                DTOMeterGroup _grpAppartenance=null;
                DTOMeter _dtMeter = null;
                DTOMeter _newMeter = null;
                 DTOCustomer _customer = null;

                #region 0 : pre -requis
                 bool resultRequired = CheckClient(adto, out _customer, true, ref _laLog, ref cptErr, ref ErrMess, msgErr);
                if (!resultRequired)
                    return false;
                else
                    resultRequired = CheckCtr(adto,true,remplacementCtr,lstCrtFree,DefaultClientID,out _dtMeter,out _newMeter,out _grpAppartenance,ref _laLog,ref cptErr,ref ErrMess,msgErr);
                if (!resultRequired)
                    return false;
                #endregion

                #region 1 : change meter to defaulgroup

                ACEVISIONWebService.DTOCustomer LeClt = proxy.getCustomerById(ref authDTO, _dtMeter.CustomerId);

                //--------
                if(remplacementCtr)
                    oReturnCode = proxy.ChangeMeterGroup(ref authDTO,adto.OLD_NumeroCompteur.Trim(), LeClt.Name, _grpAppartenance.MeterGroupName, DefaulGroup.MeterGroupName);
                else //-use case : resiliation simple
                    oReturnCode = proxy.ChangeMeterGroup(ref authDTO, _dtMeter.SerialNumber, LeClt.Name, _grpAppartenance.MeterGroupName, DefaulGroup.MeterGroupName);
                if ((int)oReturnCode != 0)
                {
                    _laLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = oReturnCode.ToString(),
                        Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                        ReferenceObjet = _dtMeter.SerialNumber
                    };

                    string msgDisplayErr = string.Format("CLIENT : {0}  , N° COMPTEUR : {1} ", adto.Client, _dtMeter.SerialNumber);
                    Log.ExceptionLogger.Error(msgErr + " - " + _laLog.DescriptionErreur + " : " + msgDisplayErr);
                    result = false;
                    cptErr++;
                    //ErrMess += msgDisplayErr + " | ";
                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";

                    return false;
                }
 
                #endregion

                #region 2 : change clt par defaulclt

                oReturnCode = proxy.updateMeterCustomer(ref authDTO, _dtMeter.MeterId, DefaultClientID);

                if (oReturnCode != null && (int)oReturnCode != 0)
                {
                    _laLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = oReturnCode.ToString(),
                        Objet = "ERREUR LORS DE LA MISE à JOUR CLient Par Defaut COMPTEUR " + _dtMeter.SerialNumber,
                        ReferenceObjet = "Numero Compteur : " + _dtMeter.SerialNumber
                    };

                    string msgDisplayErr = _laLog.Objet;
                    //Log.ExceptionLogger.Error(msgDisplayErr);
                    Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);
                    result = false;
                    cptErr++;
                    //ErrMess = msgDisplayErr;
                    //ErrMess += msgDisplayErr + " | ";
                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                    return false;
                }

                #endregion

                #region 3 : changement IDABON pr locationdefault
                oReturnCode = proxy.updateMeterLocation(ref authDTO, _dtMeter.MeterId, DefaultIDABON);
                if ((int)oReturnCode != 0)  // Erreur lors de la modification
                {
                    _laLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = oReturnCode.ToString(),
                        Objet = "MODIFICATION DE L'Identifiant CLIENT AU NIVEAU COMPTEUR " + _dtMeter.SerialNumber,
                        ReferenceObjet = adto.IdentifiantAbonne
                    };

                    //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n MODIFICATION DE L'Adresse DU CLIENT : {1} ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client);
                    string msgDisplayErr = _laLog.Objet;
                    //Log.ExceptionLogger.Error(msgDisplayErr);
                    Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);
                    result = false;
                    cptErr++;
                    //ErrMess = msgDisplayErr;
                    //ErrMess += msgDisplayErr + " | ";
                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                    return false;
                }
                #endregion

                #region 4 : changement adresse pr defaulAdress
                oReturnCode = proxy.updateMeterAddress(ref authDTO, _dtMeter.MeterId, DefaultAddress);

                if ((int)oReturnCode != 0)  // Erreur lors de la modification
                {
                    _laLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = oReturnCode.ToString(),
                        Objet = "MODIFICATION DE L'Adresse  AU NIVEAU COMPTEUR",
                        ReferenceObjet = adto.IdentifiantAbonne
                    };


                    string msgDisplayErr = _laLog.Objet;

                    Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);
                    result = false;
                    cptErr++;
                    //ErrMess += msgDisplayErr + " | ";
                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                    return false;
                }
                #endregion

                return true;
            }

            private bool useCase_ReAbonnementAfterResiliation(bool remplacementCtr, IList<LogDTO> LesLogs,int NumeroTSP, ActivationAbonneDTO adto, DTOMeterIdentifier[] lstCrtFree, DTOCustomer DefaultClt, DTOMeterGroup DefaulGroup, int DefaultClientID,ref LogDTO _laLog, ref int cptErr, ref string ErrMess, string msgErr)
            {

                #region a lire depuis file excel later
                     string Address =  adto.Agglomeration.Trim()+" "+ adto.RueBoulevardAvenue.Trim() + " " + adto.LotIlot.Trim() + " / "+adto.ReferenceRaccordement.Trim() ;
                        //RG: senace du 03112016 : lecteur depusi file Excel
                        //--------Alire
                        ACEVISIONWebService.DTOEnumMeter TypeCompteur = ACEVISIONWebService.DTOEnumMeter.SL7000;
                        string VersionFirmWare = string.IsNullOrEmpty(adto.VersionFirmWare) ? "7.5" : adto.VersionFirmWare;


                        ACEVISIONWebService.DTOEnumMeterConnection TypeTerminalReleve = ACEVISIONWebService.DTOEnumMeterConnection.PortableTerminal;
                      
                   #endregion

                bool result = true;
                int ClientID = 0;
                int CompteurId = 0;
                int GroupID = 0;
                DTOMeterGroup _grpAppartenance = null;
                DTOMeter _newMter = null;
                DTOMeter _oldMeter = null;
                DTOCustomer _customer = null;

                DTOEnumWebServiceReturnCode oReturnCode = new DTOEnumWebServiceReturnCode();
                string NomClient = adto.Client.Trim() + " / " + adto.ReferenceRaccordement;
                string LeGroup = adto.CodeSite.Trim() + adto.CodeExploitation.Trim();

                #region 0 : pre -requis
                bool resultRequired = CheckClient(adto,out _customer,false, ref _laLog, ref cptErr, ref ErrMess, msgErr);
                if (!resultRequired)
                    return false;
                else
                    resultRequired = CheckCtr(adto, false, remplacementCtr, lstCrtFree, DefaultClientID, out _oldMeter,out _newMter, out _grpAppartenance, ref _laLog, ref cptErr, ref ErrMess, msgErr);
                if (!resultRequired)
                    return false;
                #endregion


                #region 1 : creation du ctr si inexistant ou get ID
                bool _affectedGrp = false;
                    if (_newMter == null)
                                        {
                                            //08112016
                                            //--CAs Reabonnement : typedemande 4
                                            //----------Modification obligatoire de IDABON <---> Localisation (METER)
                                                          //-----deja pris en compte ici
                                            //-----------Modifcation facultative du NumCTR                                           
                                            
                                            
                                            //----lecteure later depuis file excel
                                            //CompteurId = CreateMeterInACEVISION(ref LaLog, item.NumeroCompteur, ClientID, item.IdentifiantAbonne, TypeCompteur,
                                              //  Address, VersionFirmWare, DefaultPasseWord, TypeTerminalReleve, NumeroTSP, DefaultPhoneNumber);
                                            //-----09112016
                                            CompteurId = CreateMeterInACEVISION(ref _laLog, adto.NumeroCompteur.Trim(), _customer.CustomerId, adto.IdentifiantAbonne, TypeCompteur,
                                              Address, VersionFirmWare, DefaultPasseWord, TypeTerminalReleve, NumeroTSP, DefaultPhoneNumber);

                                            if (CompteurId == -1)
                                            {
                                                LesLogs.Add(_laLog);
                                                return false;
                                            }
                                            else
                                            {                                                
                                                 _newMter = proxy.getMeterById(ref authDTO, CompteurId);
                                                  DTOMeterGroup [] tabGroup=    proxy.getMeterGroupsByName(ref authDTO,LeGroup,true);
                                                //------affectation du ctr au grp
                                                  bool resultAddCtr = AddCompteurToGroup(ref _laLog, CompteurId, _newMter.SerialNumber, tabGroup[0].MeterGroupId, LeGroup);
                                                 if (!resultAddCtr)
                                                 {
                                                     LesLogs.Add(_laLog);
                                                     result = false;
                                                     cptErr++;
                                                     return false;
                                                 }
                                                 else
                                                     _affectedGrp = true;

                                            }
                                        }
                                        else                                        
                                            CompteurId = _newMter.MeterId;


                #endregion

                #region 2 :  MAJ clt sur ce ctr

                    ACEVISIONWebService.DTOEnumWebServiceReturnCode xCode = proxy.updateMeterCustomer(ref authDTO, CompteurId, _customer.CustomerId);
                    if (xCode != null && (int)xCode != 0)
                    {
                        LogDTO UnLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = xCode.ToString(),
                            // Objet = "ERREUR LORS DE LA CREATION COMPTEUR",
                            Objet = "ERREUR LORS DE L'ASSOCIATION COMPTEUR - CLIENT",
                            ReferenceObjet = "Numero Compteur : " + _newMter.SerialNumber + " Identifiant abonné : " + adto.IdentifiantAbonne
                        };


                        string msgDisplayErr = string.Format("Message : ERREUR LORS DE L\'AFFECTATION COMPTEUR : {0} AU CLIENT {1}",_newMter.SerialNumber,NomClient);
                        Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                        //ErrMess += msgDisplayErr + " | ";
                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                        result = false;
                        cptErr++;
                        return false;
                    }

                    #endregion

                #region 3 : MAJ de l'IDABON au niveau du ctr

                    //-chnagement location du CTR
                    ACEVISIONWebService.DTOEnumWebServiceReturnCode LeCode = proxy.updateMeterLocation(ref authDTO, CompteurId, adto.IdentifiantAbonne);
                    if ((int)LeCode != 0)  // Erreur lors de la modification
                    {
                        _laLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = LeCode.ToString(),
                            Objet = "MODIFICATION DE L'Identifiant CLIENT AU NIVEAU COMPTEUR",
                            ReferenceObjet = adto.IdentifiantAbonne
                        };


                        // string msgDisplayErr = string.Format("COMPTEUR {0} / Client {1} ,Ancien Identifiant : {2} ,Nouvel Identifiant DU CLIENT : {3} ", item.NumeroCompteur, item.Client, item.OLD_IdentifiantAbonne, item.IdentifiantAbonne);
                        string msgDisplayErr = string.Format("COMPTEUR {0} / Client {1} ", adto.NumeroCompteur, adto.IdentifiantAbonne);
                        //Log.ExceptionLogger.Error(msgDisplayErr);
                        Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);
                        result = false;
                        cptErr++;
                        //ErrMess = msgDisplayErr;
                        //ErrMess += msgDisplayErr + " | ";
                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                        return false;
                    }

                    

                #endregion

                #region 4 : MAJ de l'adresse au niveau du CTR

                    //-adresse
                    //  Modification Adresse CTR(@adrsese geo /refraccodement)
                    LeCode = proxy.updateMeterAddress(ref authDTO, CompteurId, Address);

                    if ((int)LeCode != 0)  // Erreur lors de la modification
                    {
                        _laLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = LeCode.ToString(),
                            Objet = "MODIFICATION DE L'Adresse DU COMPTEUR - REFRACCORDEMENT",
                            ReferenceObjet = adto.IdentifiantAbonne
                        };


                        //string msgDisplayErr = string.Format("COMPTEUR {0} ,Ancienne Adresse DU CLIENT : {0} ,Nouvelle Adresse DU CLIENT : {1} ", LeCompteur.SerialNumber, LeCompteur.Address, Address);
                        string msgDisplayErr = string.Format("COMPTEUR {0} ,Adresse DU CLIENT : {1} ", _newMter.SerialNumber, Address);
                        //Log.ExceptionLogger.Error(msgDisplayErr);
                        Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);
                        result = false;
                        cptErr++;
                        //ErrMess += msgDisplayErr + " | ";
                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                        return false;
                    }

                #endregion
                 
                #region 5
                DTOMeterGroup LeNewGroup = proxy.getMeterGroupsByName(ref authDTO, LeGroup, true).FirstOrDefault();
                if (LeNewGroup == null)
                    GroupID = CreateGroupInACEVISON(ref _laLog, LeGroup);
                else
                    GroupID = LeNewGroup.MeterGroupId;
                if (GroupID == -1)
                {
                    LesLogs.Add(_laLog);
                   // result = false;
                    cptErr++;
                    return false;
                }
                #endregion
               
                #region 6
                //changeMeterGroup : clientName-->cient initial   
                if (!_affectedGrp)
                {
                    oReturnCode = proxy.ChangeMeterGroup(ref authDTO, _newMter.SerialNumber, _customer.Name, DefaulGroup.MeterGroupName.Trim(), LeGroup.Trim());

                    if ((int)oReturnCode != 0)
                    {
                        _laLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = oReturnCode.ToString(),
                            // DescriptionErreur = "mon test echechec",--pr test
                            Objet = msgErr,
                            //  ReferenceObjet = item.OLD_IdentifiantAbonne
                        };

                        string msgDisplayEr = string.Format("Message : ERREUR LORS DE L\'AFFECTATION COMPTEUR {0} au GROUPE : {1} ", _newMter.SerialNumber, LeGroup);
                        Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayEr);
                        //ErrMess += msgDisplayEr + " | ";
                        ErrMess += msgErr + " : " + msgDisplayEr + " | ";
                        // result = false;
                        cptErr++;
                        return false;
                    }
                }

                #endregion
                return true;
            }
             
            //-RG 03112016: vu que l'adresse sera celle presente sur le point de livarison (au niveau fiche ctr)
            //-------------on ne modifie que le nom /raison sociale du client
            private bool useCase_ModificationCommerciale(ActivationAbonneDTO adto, DTOMeterIdentifier[] lstCrtFree, ref LogDTO _laLog, ref int cptErr, ref string ErrMess, string msgErr)
            {
                DTOEnumWebServiceReturnCode oReturnCode = new DTOEnumWebServiceReturnCode();
                string NomClient = adto.Client.Trim() + " / " + adto.ReferenceRaccordement;
                

                /////
                   #region 0 : recherche client by name deja existant en BDD
                         DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, NomClient, true).FirstOrDefault();
                        
                        if (LeClient != null)
                        {

                            _laLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                DescriptionErreur = string.Format("CLIENT {0} EXISTANT AYANT LE MEME NOM  ", NomClient),
                                Objet = "ERREUR DE TRAITEMENT - CLIENT",
                                // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                            };

                            string msgDisplayErr = "ERREUR LORS DE L\'OPERATION , " + _laLog.DescriptionErreur;
                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                            ErrMess += msgDisplayErr + " | ";
                            //result = false;
                            cptErr++;
                            return false;

                        }                       
                      #endregion

                #region 1 : recherche du client dt le nom doit être modfié

                   if (string.IsNullOrEmpty(adto.NumeroCompteur.Trim()) ||adto.NumeroCompteur.Trim()=="")
                        {
                            _laLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                DescriptionErreur = "N°COMPTEUR NULL",//"NUMERO COMPTEUR DU CLIENT NULL",
                                Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                ReferenceObjet = adto.NumeroCompteur
                            };
                            //result = false;
                            cptErr++; // a revoir later...pr le count

                            //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n NUMERO COMPTEUR DU CLIENT NULL : {1},  IDENTIFIANT : {2} DU CLIENT NON TROUVE ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client, item.NumeroCompteur);
                            string msgDisplayErr = string.Format("CLIENT-{0} ,{1} ", adto.Client, _laLog.DescriptionErreur);
                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                            //ErrMess += msgDisplayErr + " | ";
                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                            return false;
                        }
                   else
                   {
                        DTOMeter[] LesCompteurReturn = proxy.getMetersBySerialNumberFilter(ref authDTO, adto.NumeroCompteur.Trim(), true);                     
                         if (LesCompteurReturn.Count() > 0)
                         {
                             //16112016 : check si ctr est actif(n'esta ds grp par default
                             bool verifCtr = lstCrtFree.Select(l => l.SerialNumber).Contains(LesCompteurReturn[0].SerialNumber);
                             if(!verifCtr)
                             {
                             //---get client of meter
                               DTOCustomer _custom= proxy.getCustomerById(ref authDTO,LesCompteurReturn[0].CustomerId);
                               //---------Modification du nom
                                oReturnCode= proxy.updateCustomerName(ref authDTO,_custom.CustomerId,NomClient);
                                 if ((int)oReturnCode != 0)  // Erreur lors de la modification
                                {
                                    _laLog = new LogDTO()
                                    {
                                        DateLog = DateTime.Now.ToString(),
                                        DescriptionErreur = oReturnCode.ToString(),
                                        Objet = "MODIFICATION DU NOM DU CLIENT",
                                        ReferenceObjet = adto.IdentifiantAbonne
                                    };
                    
                                    string msgDisplayErr = string.Format("COMPTEUR {0} / CLIENT {1} ", adto.NumeroCompteur, _custom.Name,NomClient);                  
                                    Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);                   
                                    cptErr++;                    
                                    //ErrMess += msgDisplayErr + " | ";
                                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                    return false;
                                }   
                         }
                         else
                             {
                                  _laLog = new LogDTO()
                                        {
                                            DateLog = DateTime.Now.ToString(),
                                            DescriptionErreur ="COMPTEUR DEJA RESILIE",
                                            Objet = "MODIFICATION DU NOM DU CLIENT",
                                            ReferenceObjet = adto.IdentifiantAbonne
                                        };
                    
                                        string msgDisplayErr = string.Format("COMPTEUR {0} DEJA RESILIE / CLIENT {1} ", adto.NumeroCompteur,NomClient);                  
                                        Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);                   
                                        cptErr++;                    
                                        //ErrMess += msgDisplayErr + " | ";
                                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                        return false;
                             }
                         }
                         else
                         {
                            _laLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                //DescriptionErreur = "COMPTEUR DEJA ASSOCIE A UN AUTRE CLIENT : " + cltActif_ofMeter.Name,
                                Objet = "COMPTEUR INEXISTANT",
                                ReferenceObjet = "Numero Compteur : " + adto.NumeroCompteur
                            };

                            string msgDisplayErr = string.Format("ABSENCE DE COMPTEUR  {0} ", adto.NumeroCompteur);
                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                            //ErrMess += msgDisplayErr + " | ";
                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                            // result = false;
                            cptErr++;

                            return false;
                         }
                   }
                #endregion
                /////
                 
                
                return true;
            }

            //private bool useCaseVariationPuissance(bool isMutation, ActivationAbonneDTO adto, ref LogDTO _laLog, ref int cptErr, ref string ErrMess, string msgErr)
        //23022017
            private bool useCaseVariationPuissance(bool isMutation, ActivationAbonneDTO adto, DTOMeterIdentifier[] lstCrtFree, DTOCustomer DefaultClt
                                                    , DTOMeterGroup DefaulGroup, int DefaultClientID, IList<LogDTO> LesLogs, int NumeroTSP
                                                    ,ref LogDTO _laLog, ref int cptErr, ref string ErrMess, string msgErr)
            {
                 DTOEnumWebServiceReturnCode oReturnCode = new DTOEnumWebServiceReturnCode();
                string NomClient = adto.Client.Trim() + " / " + adto.ReferenceRaccordement;
                //--------
                #region 0 : check client

                if (!isMutation)
                {
                    DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, NomClient, true).FirstOrDefault();


                    if (LeClient == null)
                    {

                        _laLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = string.Format("CLIENT {0} INEXISTANT  ", NomClient),
                            Objet = "ERREUR DE TRAITEMENT - CLIENT",
                            // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                        };


                        string msgDisplayErr = "COMPTEUR " + adto.NumeroCompteur.Trim() + " , ERREUR LORS DE LA RECHERCHE DU CLIENT , " + _laLog.DescriptionErreur;
                        Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                        //ErrMess += msgDisplayErr + " | ";
                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                        //result = false;
                        cptErr++;
                        return false;

                    }
                    else
                    {
                        //---check clt possede au moin un ctr <----->en gros si le clt est tjrs actif
                        DTOMeter[] dtMeter = proxy.getMetersByCustomerId(ref authDTO, LeClient.CustomerId);
                        if (dtMeter.Count() == 0)
                        {
                            _laLog = new LogDTO()
                            {
                                DateLog = DateTime.Now.ToString(),
                                DescriptionErreur = string.Format("CLIENT {0} NE POSSEDANT AUCUN COMPTEUR  ", NomClient),
                                Objet = "CLIENT INACTIF"

                            };
                            string msgDisplayErr = _laLog.Objet + " - " + _laLog.DescriptionErreur;
                            Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                           // ErrMess += msgDisplayErr + " | ";
                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                            // result = false;
                            cptErr++;
                            return false;
                        }


                    }

                }
                #endregion

                #region 1:check value oldIDABON & value IDABON
                //-----check IDABON ancien 
                            if (string.IsNullOrEmpty(adto.OLD_IdentifiantAbonne.Trim()) || adto.OLD_IdentifiantAbonne.Trim() == "")
                            {
                                _laLog = new LogDTO()
                                {
                                    DateLog = DateTime.Now.ToString(),
                                    DescriptionErreur = "ANCIEN IDABON NULL",//"NUMERO COMPTEUR DU CLIENT NULL",
                                   // Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                    ReferenceObjet = adto.NumeroCompteur
                                };
                                //result = false;
                                cptErr++; // a revoir later...pr le count

                                //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n NUMERO COMPTEUR DU CLIENT NULL : {1},  IDENTIFIANT : {2} DU CLIENT NON TROUVE ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client, item.NumeroCompteur);
                                string msgDisplayErr = string.Format("CLIENT-{0} ,{1} ", adto.Client, _laLog.DescriptionErreur);
                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                //ErrMess += msgDisplayErr + " | ";
                                ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                return false;
                            }
                            else
                            {//---check si new IDABON is not null
                                if (string.IsNullOrEmpty(adto.IdentifiantAbonne.Trim()) || adto.IdentifiantAbonne.Trim() == "")
                                {
                                    _laLog = new LogDTO()
                                    {
                                        DateLog = DateTime.Now.ToString(),
                                        DescriptionErreur = "NOUVEL IDABON NULL",//"NUMERO COMPTEUR DU CLIENT NULL",
                                        // Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                        ReferenceObjet = adto.NumeroCompteur
                                    };
                                    //result = false;
                                    cptErr++; // a revoir later...pr le count

                                    //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n NUMERO COMPTEUR DU CLIENT NULL : {1},  IDENTIFIANT : {2} DU CLIENT NON TROUVE ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client, item.NumeroCompteur);
                                    string msgDisplayErr = string.Format("CLIENT-{0} ,{1} ", adto.Client, _laLog.DescriptionErreur);
                                    Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                                    //ErrMess += msgDisplayErr + " | ";
                                    ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                    return false;
                                }                                

                            }
                #endregion

               #region 2 : check existence ctr 

                            int meterID = -1;
                 //-if (string.IsNullOrEmpty(adto.NumeroCompteur.Trim()))
                            if (string.IsNullOrEmpty(adto.NumeroCompteur.Trim()) || adto.NumeroCompteur.Trim() == "")
                            {
                                _laLog = new LogDTO()
                                {
                                    DateLog = DateTime.Now.ToString(),
                                    DescriptionErreur = "N°COMPTEUR NULL",//"NUMERO COMPTEUR DU CLIENT NULL",
                                   // Objet = "RESILIATION DE L'ABONNEMENT DU CLIENT",
                                    ReferenceObjet = adto.NumeroCompteur
                                };
                                //result = false;
                                cptErr++; // a revoir later...pr le count

                                //string msgDisplayErr = string.Format("Message : Type de demande -{0} \n NUMERO COMPTEUR DU CLIENT NULL : {1},  IDENTIFIANT : {2} DU CLIENT NON TROUVE ", Enum.GetName(typeof(ExtensionMethod.enumTypeDI), LeTypeDI), item.Client, item.NumeroCompteur);
                                string msgDisplayErr = string.Format("CLIENT-{0} ,{1} ", adto.Client, _laLog.DescriptionErreur);
                                Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                               // ErrMess += msgDisplayErr + " | ";
                                ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                return false;
                            }
                            else
                            {
                                DTOMeter[] LesCompteurReturn = proxy.getMetersBySerialNumberFilter(ref authDTO, adto.NumeroCompteur.Trim(), true);

                                //if (LesCompteurReturn.Count() > 0)
                                if (LesCompteurReturn.Count() > 0)//---24022017
                                {//--RG : si ctr rempalcant(new) est dispo dans ace vision , ne pas verifier sa valeur idabon vu que elle aura comma valeur :CIE|00000000 celle ds ctr du default group
                                    bool verifNewCtr = lstCrtFree.Select(l => l.SerialNumber).Contains(LesCompteurReturn[0].SerialNumber);
                                    if (!verifNewCtr)
                                    {
                                        meterID = LesCompteurReturn[0].MeterId;
                                        //---check si old idabon saphir <==> id abon ace vision
                                        if (LesCompteurReturn[0].Location.Trim() != adto.OLD_IdentifiantAbonne.Trim())
                                        {                                        
                                      
                                            _laLog = new LogDTO()
                                            {
                                                DateLog = DateTime.Now.ToString(),
                                                DescriptionErreur = string.Format("COMPTEUR {0} ,ANCIEN IDABON {1} ne CORRESPONDANT PAS A LA VALEUR ACE VISION {2} ",adto.NumeroCompteur.Trim(), adto.OLD_IdentifiantAbonne, LesCompteurReturn[0].Location),
                                                Objet = "ERREUR LORS DE DU TRAITEMENT",
                                                ReferenceObjet = "Numero Compteur : " + LesCompteurReturn[0].SerialNumber
                                            };

                                            //string msgDisplayErr = string.Format("ERREUR LORS DE L'AFFECTATION COMPTEUR , COMPTEUR  {0} , appartenant Déjà  au client {1} ", LesCompteurReturn[0].SerialNumber, cltActif_ofMeter.Name);
                                            string msgDisplayErr = _laLog.DescriptionErreur;
                                            Log.ExceptionLogger.Error(msgErr + " - "+ msgDisplayErr);
                                            //ErrMess += msgDisplayErr + " | ";
                                            ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                            // result = false;
                                            cptErr++;

                                            return false;
                                        }
                                    }

                                }
                            }

               #endregion

               //23022017
               #region 3 : MAJ du Comptage
                            int _TYPDI = int.Parse(adto.TypeDemande);
                            bool checkAMC = false;
                            if (_TYPDI == (int)ExtensionMethod.enumTypeDI.Dimunition_PS_AMC || _TYPDI == (int)ExtensionMethod.enumTypeDI.Augmentation_PS_AMC)
                            {
                                checkAMC = true;
                                //---on fait un remplacement de ctr
                                #region Remplacement ctr - resiliation

                                bool resultUseCase = useCase_Resiliation(true, adto, lstCrtFree, DefaultClt, DefaulGroup, DefaultClientID, ref _laLog, ref cptErr, ref ErrMess, msgErr);
                                //bool resultUseCase = useCase_Resiliation(true, item, lstCrtFree, DefaultClt, DefaulGroup, DefaultClientID, ref LaLog, ref cptErr, ref ErrMess, msgErr);
                                if (!resultUseCase)
                                {
                                    //result = false;
                                    //break;
                                    return false;
                                }
                                #endregion

                                #region Remplacement ctr - Reabonnement
                                //resultUseCase = useCase_ReAbonnementAfterResiliation(true, LesLogs, NumeroTSP, item, lstCrtFree, DefaultClt, DefaulGroup, DefaultClientID, ref LaLog, ref cptErr, ref ErrMess, msgErr);
                                //23022017
                                resultUseCase = useCase_ReAbonnementAfterResiliation(true, LesLogs, NumeroTSP, adto, lstCrtFree, DefaultClt, DefaulGroup, DefaultClientID, ref _laLog, ref cptErr, ref ErrMess, msgErr);
                                if (!resultUseCase)
                                {
                                    return false;
                                }
                                #endregion
                            }
                            #endregion

               #region 4 : MAJ ID ABON
                //23022017
               if (!checkAMC)
                {
                oReturnCode = proxy.updateMeterLocation(ref authDTO, meterID, adto.IdentifiantAbonne);
                  if ((int)oReturnCode != 0)  // Erreur lors de la modification
                            {
                                _laLog = new LogDTO()
                                {
                                    DateLog = DateTime.Now.ToString(),
                                    DescriptionErreur = oReturnCode.ToString(),
                                    Objet = "MODIFICATION DE L'Identifiant CLIENT AU NIVEAU COMPTEUR",
                                    ReferenceObjet = adto.IdentifiantAbonne
                                };
                            
                  string msgDisplayErr = string.Format("COMPTEUR {0} / Client {1} ", adto.NumeroCompteur, adto.IdentifiantAbonne);
                                //Log.ExceptionLogger.Error(msgDisplayErr);
                  Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);
                              //  result = false;
                                cptErr++;                      
                                //ErrMess += msgDisplayErr + " | ";
                                ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                                return false;
                            }
                }
               #endregion

              

                  //end 23022016
             return true;
            }

              

            private bool useCase_ModificationComAfterVariation(ActivationAbonneDTO adto, ref LogDTO _laLog, ref int cptErr, ref string ErrMess, string msgErr)
            {
                DTOEnumWebServiceReturnCode oReturnCode = new DTOEnumWebServiceReturnCode();
                string NomClient = adto.Client.Trim() + " / " + adto.ReferenceRaccordement;


                /////
                #region 0 : recherche client by name deja existant en BDD
                /* RG du 14112016 par M.SEKONGO : ne rien faire pour le nom s'il existe deja...metter jour que son ID ABON
                DTOCustomer LeClient = proxy.getCustomersByNameFilter(ref authDTO, NomClient, true).FirstOrDefault();

                if (LeClient != null)
                {


                    _laLog = new LogDTO()
                    {
                        DateLog = DateTime.Now.ToString(),
                        DescriptionErreur = string.Format("CLIENT {0} EXISTANT AYANT LE MEME NOM  ", NomClient),
                        Objet = "ERREUR DE TRAITEMENT - CLIENT",
                        // ReferenceObjet = "Numero Compteur : " + LeCompteur.SerialNumber + " Identifiant abonné : " + item.IdentifiantAbonne
                    };

                    string msgDisplayErr = "ERREUR LORS DE L\'OPERATION , " + _laLog.DescriptionErreur;
                    Log.ExceptionLogger.Error(msgErr + " : " + msgDisplayErr);
                    ErrMess += msgDisplayErr + " | ";
                    //result = false;
                    cptErr++;
                    return false;

                }*/
                #endregion

                #region MAJ Name Client

                DTOMeter[] LesCompteurReturn = proxy.getMetersBySerialNumberFilter(ref authDTO, adto.NumeroCompteur.Trim(), true);
                if (LesCompteurReturn.Count() > 0)
                {
                    //---get client of meter
                    DTOCustomer _custom = proxy.getCustomerById(ref authDTO, LesCompteurReturn[0].CustomerId);
                    //---------Modification du nom
                    oReturnCode = proxy.updateCustomerName(ref authDTO, _custom.CustomerId, NomClient);
                    if ((int)oReturnCode != 0)  // Erreur lors de la modification
                    {
                        _laLog = new LogDTO()
                        {
                            DateLog = DateTime.Now.ToString(),
                            DescriptionErreur = oReturnCode.ToString(),
                            Objet = "MODIFICATION DU NOM DU CLIENT",
                            ReferenceObjet = adto.IdentifiantAbonne
                        };

                        string msgDisplayErr = string.Format("COMPTEUR {0} / ANCIEN NOM {1} - NOUVEAU {2} ", adto.NumeroCompteur, _custom.Name, NomClient);
                        Log.ExceptionLogger.Error(msgErr + " - " + _laLog.Objet + " : " + _laLog.DescriptionErreur);
                        cptErr++;
                       // ErrMess += msgDisplayErr + " | ";
                        ErrMess += msgErr + " : " + msgDisplayErr + " | ";
                        return false;
                    }
                }

                #endregion

                return true;
            }
        
        #endregion

        #region Assainissement BDD

        private bool ChangeNameAllClient(out string msgErr)
        {
            msgErr = string.Empty;
            DTOCustomer[] lstCustomers = proxy.getCustomers(ref authDTO);


             try
             {
                 
                 //----MAJ Name Client
                 foreach (var item in lstCustomers)
                 {
                     DTOMeter _dtoMeter=new DTOMeter();
                     if (DicoAllCTR.TryGetValue(item.CustomerId, out _dtoMeter))
                        {
                            if (item.CustomerId == 1840)
                            {

                            }
                          string []valAdress=_dtoMeter.Address.Split('/');
                           if(valAdress.Count()==2)
                              item.Name = item.Name.Trim() + " / " + valAdress[1].Trim();
                             
                          //-----MAJ du name
                           DTOEnumWebServiceReturnCode oReturnCode = proxy.updateCustomerName(ref authDTO, item.CustomerId, item.Name);
                           if ((int)oReturnCode != 0)  // Erreur lors de la modification
                           {
                               
                               string msgDisplayErr = string.Format("COMPTEUR {0} / CLIENT {1} ",_dtoMeter.SerialNumber, item.Name);
                               Log.ExceptionLogger.Error("MISE A JOUR NOM CLIENT , "+msgDisplayErr);
                              // cptErr++;
                               msgErr += msgDisplayErr + " | ";
                               //return false;
                               continue;
                           }   

                         }                   

                 }

                 if (!string.IsNullOrEmpty(msgErr))
                     return false;
             }
             catch (Exception ex)
             {
                 msgErr = ex.Message;
                 return false;
             }
                return true;
         }


        #endregion

        #region methode private
        public FileResult ExportToExcel()
        //public FileResult ExportToExcel(int page, string orderBy,string filter)
        {
            //DataTable _dtExport = new Utility().FormatList_ExportToExcel(_lstHeaderColumns, lstDemandes_pending);
            if (_dtExport == null) 
             _dtExport = new Utility().FormatList_ExportToExcel(_lstHeaderColumns, lstDemandes_pending);
           // Creat the NpoiExport object
           using (var exporter = new NpoiExport())
           {
               exporter.ExportDataTableToWorkbook(_dtExport, "InfosSAPHIR");

               string saveAsFileName = string.Format("Liste DI SAPHIR-{0:d}.xls", DateTime.Now);
                
               //Response.ContentType = "application/vnd.ms-excel";

               NPOI.SS.UserModel.Workbook _workbook= exporter.Workbook;
               //Write the Workbook to a memory stream
               MemoryStream output = new MemoryStream();
               _workbook.Write(output);
               return File(output.ToArray(), "application/vnd.ms-excel", saveAsFileName);

               ////string saveAsFileName = string.Format("ListeCompteurs_KO-{0:d}.xlsx", DateTime.Now);
               ////Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

               //Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", saveAsFileName));
               //Response.Clear();
               //Response.BinaryWrite(exporter.GetBytes());
               //Response.End();
           }

           //return Content("ok kpleus");
        }

        //---A refactoriser later -manque de temps (eviter la regression...)
        public FileResult ExportToExcel_DIOK()
        {
             
            if (_dtExport == null)
                _dtExport = new Utility().FormatList_ExportToExcel(_lstHeaderColumns, lstDemandes_Succes);
            // Creat the NpoiExport object
            using (var exporter = new NpoiExport())
            {
                exporter.ExportDataTableToWorkbook(_dtExport, "InfosSAPHIR");

                string saveAsFileName = string.Format("Liste DI Succes SAPHIR-{0:d}.xls", DateTime.Now);

                //Response.ContentType = "application/vnd.ms-excel";

                NPOI.SS.UserModel.Workbook _workbook = exporter.Workbook;
                //Write the Workbook to a memory stream
                MemoryStream output = new MemoryStream();
                _workbook.Write(output);
                return File(output.ToArray(), "application/vnd.ms-excel", saveAsFileName);

                
            }
        }

        public JsonResult PrintTable()
        {
            
            return Json(lstDemandes_pending, JsonRequestBehavior.AllowGet);
        }

         public JsonResult PrintTable_DIOK()
        {
            
            return Json(lstDemandes_Succes, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}

