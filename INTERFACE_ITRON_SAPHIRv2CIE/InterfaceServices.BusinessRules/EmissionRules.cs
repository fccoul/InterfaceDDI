using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceServices.DASAPHIRCOM;
using InterfaceServices.DTO;
using INOVA.ISF.DATACESS;
using System.Data.Linq;
 
using System.IO;
using InterfaceServices.SAPHIRCOMDataAccess.SAPHIRCOM;
using InterfaceServices.SAPHIRCOMDataAccess;
using ACEVISION.Common;

namespace InterfaceServices.BusinessRules
{
    public class EmissionRules
    {
        public static bool MiseAJourHistorique(string CodeExploitation, string Periode, ref string errmess)
        {
            bool Result = false;
            try
            {
                historique histo = null;
                if (!string.IsNullOrEmpty(Periode) && !string.IsNullOrEmpty(CodeExploitation))
                    histo = new DAL<historique>().Find(p => p.codexp == CodeExploitation && p.perfact.Trim() == Periode.Trim()).FirstOrDefault();

                if (histo != null)
                {
                    histo.modextract = Convert.ToChar("1");
                    histo.datextract = DateTime.Now;
                    new DAL<historique>().UpdateRow(histo, p => p.codexp == histo.codexp.Trim() && p.perfact.Trim() == histo.perfact.Trim());
                    Result = true;
                }

            }
            catch (Exception ex)
            {
                errmess = ex.Message;
                Result = false;
            }
            return Result;
        }

 

        public static SDTO_Historique GetHistoriqueExploitation(string CodeExploitation, string Periode)
        {
            SDTO_Historique Result = null;
            historique histo = null;
            if (!string.IsNullOrEmpty(Periode) && !string.IsNullOrEmpty(CodeExploitation))
                histo = new DAL<historique>().Find(p => p.codexp == CodeExploitation && p.perfact.Trim() == Periode.Trim()).FirstOrDefault();

            if (histo != null)
            {
                Result = new SDTO_Historique()
                {
                    codexp = histo.codexp,
                    codsite = histo.codsite,
                    datecpte = histo.datecpte,
                    datextract = histo.datextract,
                    datoper = histo.datoper,
                    datrecept = histo.datrecept,
                    modextract = histo.modextract,
                    modrecept = histo.modrecept,
                    nbrmvt = histo.nbrmvt,
                    numoperec = histo.numoperec,
                    numoperex = histo.numoperex,
                    perfact = histo.perfact,

                };
            }
            return Result;
        }

        public static List<ActivationAbonneDTO> GetAllDemandesActivesFromSAPHIR()
        {
            List<ActivationAbonneDTO> Resultat = new List<ActivationAbonneDTO>();
            //IList<ACTIVATION_ABO_ACEVISION> Bord = new DAL<ACTIVATION_ABO_ACEVISION>().Find(p => (p.DATEXTRACT.HasValue ? p.DATEXTRACT == null : true));
            try
            {
                List<ACTIVATION_ABO_ACEVISION> Bord = new DAL<ACTIVATION_ABO_ACEVISION>().Find(p => p.DATEXTRACT == null).ToList();
                if (Bord != null && Bord.Count > 0)
                {
                    foreach (ACTIVATION_ABO_ACEVISION item in Bord)
                    {
                        /****for debug test...***/
                        /*
                        ActivationAbonneDTO em = new ActivationAbonneDTO()
                        {
                            CodeExploitation = item.CODEXP,
                            CodeSite = item.CODSITE,
                            DateCreation = item.DATCREAT,
                            DateExtraction = item.DATEXTRACT,
                            IdEmission = item.idemission,
                            IdentifiantAbonne = item.IDABON,//.DeCryptage(),
                            LotIlot = item.LOT_ILOT,//.DeCryptage(),
                            ModeExtract = item.MODEXTRACT,
                            NomAbonne = item.NOM_ABON,//.DeCryptage(),
                            NumeroCompteur = item.NUMCTR,//.DeCryptage(),
                            OLD_IdentifiantAbonne = item.IDABON_ANC,//.DeCryptage() != string.Empty ? item.IDABON_ANC.DeCryptage() : "-",
                            OLD_NumeroCompteur = item.NUMCTR_ANC,//.DeCryptage() != string.Empty ? item.NUMCTR_ANC.DeCryptage() : "-",
                            OLD_ReferenceRaccordement = item.REFRACCORD_ANC,//.DeCryptage() != string.Empty ? item.REFRACCORD_ANC.DeCryptage() : "-",
                            PrenomAbonne = item.PRENOM_ABON,//.DeCryptage(),
                            ReferenceRaccordement = item.REFRACCORD,//.DeCryptage(),
                            RueBoulevardAvenue = item.RUE_BVD_AVENUE,//.DeCryptage(),
                            TypeDemande = item.TYPDI,//.DeCryptage(),
                           
                            Agglomeration = item.AGGLO_QUAT_AUTR//.DeCryptage()

                            ,
                            //Client = item.NOM_ABON.DeCryptage() + " " + item.PRENOM_ABON.DeCryptage()
                            //Client = item.NOM_ABON + " " + item.PRENOM_ABON //---a verifeir later pr bleme d'espace
                            //09112016
                            Client = item.NOM_ABON.Trim() + " " + item.PRENOM_ABON //---a verifeir later pr bleme d'espace
                            ,
                            //Address = item.AGGLO_QUAT_AUTR.DeCryptage() + " ," + item.RUE_BVD_AVENUE.DeCryptage() + " ," + item.LOT_ILOT.DeCryptage()
                            Address = item.AGGLO_QUAT_AUTR.Trim()+ " ," + item.RUE_BVD_AVENUE.Trim() + " ," + item.LOT_ILOT.Trim()
                           ,
                           //LibelleTypeDemande = Enum.GetName(typeof (ACEVISION.Common.ExtensionMethod.enumTypeDI), int.Parse(item.TYPDI.DeCryptage()))
                           LibelleTypeDemande = Enum.GetName(typeof (ACEVISION.Common.ExtensionMethod.enumTypeDI), int.Parse(item.TYPDI))
                        
                        };
                        */
                        /**---For deploy*/
                         
                        ActivationAbonneDTO em = new ActivationAbonneDTO();
                        em.CodeExploitation = item.CODEXP;
                        em.CodeSite = item.CODSITE;
                        em.DateCreation = item.DATCREAT;
                        em.DateExtraction = item.DATEXTRACT;
                        em.IdEmission = item.idemission;
                        em.IdentifiantAbonne = item.IDABON.DeCryptage();
                        em.LotIlot = item.LOT_ILOT.DeCryptage();
                        em.ModeExtract = item.MODEXTRACT;
                        em.NomAbonne = item.NOM_ABON.DeCryptage();
                        em.NumeroCompteur = item.NUMCTR.DeCryptage();
                        em.OLD_IdentifiantAbonne = item.IDABON_ANC.DeCryptage() != string.Empty ? item.IDABON_ANC.DeCryptage() : "-";
                        em.OLD_NumeroCompteur = item.NUMCTR_ANC.DeCryptage() != string.Empty ? item.NUMCTR_ANC.DeCryptage() : "-";
                        em.OLD_ReferenceRaccordement = item.REFRACCORD_ANC.DeCryptage() != string.Empty ? item.REFRACCORD_ANC.DeCryptage() : "-";
                        em.PrenomAbonne = item.PRENOM_ABON.DeCryptage();
                        em.ReferenceRaccordement = item.REFRACCORD.DeCryptage();
                        em.RueBoulevardAvenue = item.RUE_BVD_AVENUE.DeCryptage();
                        em.TypeDemande = item.TYPDI.DeCryptage();
                        em.Agglomeration = item.AGGLO_QUAT_AUTR.DeCryptage();
                        //Client = item.NOM_ABON.DeCryptage() + " " + item.PRENOM_ABON.DeCryptage()
                        //Client = item.NOM_ABON + " " + item.PRENOM_ABON //---a verifeir later pr bleme d'espace
                        //09112016
                        em.Client = item.NOM_ABON.DeCryptage().Trim() + " " + item.PRENOM_ABON.DeCryptage(); //---a verifeir later pr bleme d'espace
                        //Address = item.AGGLO_QUAT_AUTR.DeCryptage() + " ," + item.RUE_BVD_AVENUE.DeCryptage() + " ," + item.LOT_ILOT.DeCryptage()
                        em.Address = item.AGGLO_QUAT_AUTR.DeCryptage().Trim() + " ," + item.RUE_BVD_AVENUE.DeCryptage().Trim() + " ," + item.LOT_ILOT.DeCryptage().Trim();
                        //LibelleTypeDemande = Enum.GetName(typeof (ACEVISION.Common.ExtensionMethod.enumTypeDI), int.Parse(item.TYPDI.DeCryptage()))
                        em.LibelleTypeDemande = Enum.GetName(typeof(ACEVISION.Common.ExtensionMethod.enumTypeDI), int.Parse(item.TYPDI.DeCryptage()));
                        
                        Resultat.Add(em);
                    }
                }
            }
            catch (Exception ex)
            {
                //--Logger les message Erreur later
                throw;
            }
            return Resultat;
        }

        //27062017
        public static List<ActivationAbonneDTO> GetAllDemandesSuccesromSAPHIR()
        {
            List<ActivationAbonneDTO> Resultat = new List<ActivationAbonneDTO>();
            //IList<ACTIVATION_ABO_ACEVISION> Bord = new DAL<ACTIVATION_ABO_ACEVISION>().Find(p => (p.DATEXTRACT.HasValue ? p.DATEXTRACT == null : true));
            try
            {
                List<ACTIVATION_ABO_ACEVISION> Bord = new DAL<ACTIVATION_ABO_ACEVISION>().Find(p => p.DATEXTRACT != null).ToList();
                if (Bord != null && Bord.Count > 0)
                {
                    foreach (ACTIVATION_ABO_ACEVISION item in Bord)
                    {
                        /****for debug test...***/
                        /*
                        ActivationAbonneDTO em = new ActivationAbonneDTO()
                        {
                            CodeExploitation = item.CODEXP,
                            CodeSite = item.CODSITE,
                            DateCreation = item.DATCREAT,
                            DateExtraction = item.DATEXTRACT,
                            IdEmission = item.idemission,
                            IdentifiantAbonne = item.IDABON,//.DeCryptage(),
                            LotIlot = item.LOT_ILOT,//.DeCryptage(),
                            ModeExtract = item.MODEXTRACT,
                            NomAbonne = item.NOM_ABON,//.DeCryptage(),
                            NumeroCompteur = item.NUMCTR,//.DeCryptage(),
                            OLD_IdentifiantAbonne = item.IDABON_ANC,//.DeCryptage() != string.Empty ? item.IDABON_ANC.DeCryptage() : "-",
                            OLD_NumeroCompteur = item.NUMCTR_ANC,//.DeCryptage() != string.Empty ? item.NUMCTR_ANC.DeCryptage() : "-",
                            OLD_ReferenceRaccordement = item.REFRACCORD_ANC,//.DeCryptage() != string.Empty ? item.REFRACCORD_ANC.DeCryptage() : "-",
                            PrenomAbonne = item.PRENOM_ABON,//.DeCryptage(),
                            ReferenceRaccordement = item.REFRACCORD,//.DeCryptage(),
                            RueBoulevardAvenue = item.RUE_BVD_AVENUE,//.DeCryptage(),
                            TypeDemande = item.TYPDI,//.DeCryptage(),
                           
                            Agglomeration = item.AGGLO_QUAT_AUTR//.DeCryptage()

                            ,
                            //Client = item.NOM_ABON.DeCryptage() + " " + item.PRENOM_ABON.DeCryptage()
                            //Client = item.NOM_ABON + " " + item.PRENOM_ABON //---a verifeir later pr bleme d'espace
                            //09112016
                            Client = item.NOM_ABON.Trim() + " " + item.PRENOM_ABON //---a verifeir later pr bleme d'espace
                            ,
                            //Address = item.AGGLO_QUAT_AUTR.DeCryptage() + " ," + item.RUE_BVD_AVENUE.DeCryptage() + " ," + item.LOT_ILOT.DeCryptage()
                            Address = item.AGGLO_QUAT_AUTR.Trim()+ " ," + item.RUE_BVD_AVENUE.Trim() + " ," + item.LOT_ILOT.Trim()
                           ,
                           //LibelleTypeDemande = Enum.GetName(typeof (ACEVISION.Common.ExtensionMethod.enumTypeDI), int.Parse(item.TYPDI.DeCryptage()))
                           LibelleTypeDemande = Enum.GetName(typeof (ACEVISION.Common.ExtensionMethod.enumTypeDI), int.Parse(item.TYPDI))
                        
                        };
                        */
                        /**---For deploy*/

                        ActivationAbonneDTO em = new ActivationAbonneDTO();
                        em.CodeExploitation = item.CODEXP;
                        em.CodeSite = item.CODSITE;
                        em.DateCreation = item.DATCREAT;
                        em.DateExtraction = item.DATEXTRACT;
                        em.IdEmission = item.idemission;
                        em.IdentifiantAbonne = item.IDABON.DeCryptage();
                        em.LotIlot = item.LOT_ILOT.DeCryptage();
                        em.ModeExtract = item.MODEXTRACT;
                        em.NomAbonne = item.NOM_ABON.DeCryptage();
                        em.NumeroCompteur = item.NUMCTR.DeCryptage();
                        em.OLD_IdentifiantAbonne = item.IDABON_ANC.DeCryptage() != string.Empty ? item.IDABON_ANC.DeCryptage() : "-";
                        em.OLD_NumeroCompteur = item.NUMCTR_ANC.DeCryptage() != string.Empty ? item.NUMCTR_ANC.DeCryptage() : "-";
                        em.OLD_ReferenceRaccordement = item.REFRACCORD_ANC.DeCryptage() != string.Empty ? item.REFRACCORD_ANC.DeCryptage() : "-";
                        em.PrenomAbonne = item.PRENOM_ABON.DeCryptage();
                        em.ReferenceRaccordement = item.REFRACCORD.DeCryptage();
                        em.RueBoulevardAvenue = item.RUE_BVD_AVENUE.DeCryptage();
                        em.TypeDemande = item.TYPDI.DeCryptage();
                        em.Agglomeration = item.AGGLO_QUAT_AUTR.DeCryptage();
                        //Client = item.NOM_ABON.DeCryptage() + " " + item.PRENOM_ABON.DeCryptage()
                        //Client = item.NOM_ABON + " " + item.PRENOM_ABON //---a verifeir later pr bleme d'espace
                        //09112016
                        em.Client = item.NOM_ABON.DeCryptage().Trim() + " " + item.PRENOM_ABON.DeCryptage(); //---a verifeir later pr bleme d'espace
                        //Address = item.AGGLO_QUAT_AUTR.DeCryptage() + " ," + item.RUE_BVD_AVENUE.DeCryptage() + " ," + item.LOT_ILOT.DeCryptage()
                        em.Address = item.AGGLO_QUAT_AUTR.DeCryptage().Trim() + " ," + item.RUE_BVD_AVENUE.DeCryptage().Trim() + " ," + item.LOT_ILOT.DeCryptage().Trim();
                        //LibelleTypeDemande = Enum.GetName(typeof (ACEVISION.Common.ExtensionMethod.enumTypeDI), int.Parse(item.TYPDI.DeCryptage()))
                        em.LibelleTypeDemande = Enum.GetName(typeof(ACEVISION.Common.ExtensionMethod.enumTypeDI), int.Parse(item.TYPDI.DeCryptage()));

                        Resultat.Add(em);
                    }
                }
            }
            catch (Exception ex)
            {
                //--Logger les message Erreur later
                throw;
            }
            return Resultat;
        }

        public static bool MiseAJourDesClientsActives(IList<ActivationAbonneDTO> LesBordereauxInsert, ref string ErrMess)
        {
            bool result = true;
            StringBuilder stb = new StringBuilder();
            if (LesBordereauxInsert != null && LesBordereauxInsert.Count > 0)
            {
                try
                {
                    //MiseAJourHistorique(LesBordereauxInsert.FirstOrDefault().CodeExploitation, LesBordereauxInsert.FirstOrDefault().PeriodeFacturation, ref ErrMess);
                    foreach (ActivationAbonneDTO item in LesBordereauxInsert)
                    {
                        ACTIVATION_ABO_ACEVISION bor = new DAL<ACTIVATION_ABO_ACEVISION>().Find(p => p.idemission == item.IdEmission).FirstOrDefault();
                        if (bor != null)
                        {
                            bor.MODEXTRACT = item.ModeExtract;
                            bor.DATEXTRACT = item.DateExtraction;
                            bor.HEUREXTRACT = item.DateExtraction.HasValue ? item.DateExtraction.Value.ToShortTimeString() : DateTime.Now.ToShortTimeString();
                            new DAL<ACTIVATION_ABO_ACEVISION>().UpdateRow(bor, p => p.idemission == bor.idemission);
                        }
                    }
                }
                catch (Exception ex)
                {
                    stb.Append(ErrMess);
                    stb.AppendLine();
                    stb.Append(ex.Message);
                    result = false;
                }
            }
            ErrMess = stb.ToString();
            return result;
        }


        public static void UpDateMouvementsTraites(IList<Guid> LesMouvementsTraites, ref string ErrMessage)
        {
            if (LesMouvementsTraites != null && LesMouvementsTraites.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                //18112016
                //ErrMessage = string.Empty;
                foreach (Guid MvtID in LesMouvementsTraites)
                {
                    try
                    {
                        ACTIVATION_ABO_ACEVISION ToUpdate = new DAL<ACTIVATION_ABO_ACEVISION>().Find(p => MvtID == p.idemission).FirstOrDefault();
                        if (ToUpdate != null)
                        {
                            ToUpdate.DATEXTRACT = DateTime.Now;
                            ToUpdate.HEUREXTRACT = DateTime.Now.ToShortTimeString();
                            ToUpdate.MODEXTRACT = '1'; //signale que l'extraction via web Sces...
                            new DAL<ACTIVATION_ABO_ACEVISION>().UpdateRow(ToUpdate, p => p.idemission == ToUpdate.idemission);
                        }
                    }
                    catch (Exception Ex)
                    {
                        sb.AppendLine(Ex.Message);
                    }

                }

                //18112016
                // ErrMessage = sb.ToString();
                if(!string.IsNullOrEmpty(sb.ToString()))
                ErrMessage += sb.ToString();
            }
        }
            


    
    }


}
