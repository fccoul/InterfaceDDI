using InterfaceServices.DTO;
using InterfaceServices.SAPHIRCOMDataAccess;
using InterfaceServices.SAPHIRCOMDataAccess.SAPHIRCOM;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACEVISION.Common;

namespace InterfaceServices.BusinessRules
{
    public class ReceptionRules
    {
        public bool InsertDesIndexDansLeNoeud(IList<ReceptionDTO> LesIndex, ref int NbreInsert, ref int NbreErreur, ref string pErrMsg)
        {
            bool Resultat = true;
            pErrMsg = string.Empty;
            NbreInsert = 0;
            NbreErreur = 0;
            StringBuilder stb = new StringBuilder();
            if (LesIndex != null && LesIndex.Count > 0)
            {
                // MiseAJourHistorique(LesIndex.FirstOrDefault().CodeExploitation, LesIndex.FirstOrDefault().PeriodeFacturation, ref pErrMsg);
                //new DAL<mvt_releve_emission>().DeleteRow(p => (p.CodeExploitation == LesEmissions[0].CodeExploitation) && (p.CodeSite == LesEmissions[0].CodeSite));
                //SqlConnectionStringBuilder ConnexionBuilder = GetSqlConnexionStringBuilder();
                foreach (ReceptionDTO ind in LesIndex)
                {
                    try
                    {
                        //----cryptages des données...
                        mvt_releve_reception lePRE = new mvt_releve_reception()
                        {
                            codexp = ind.CodeExploitation,
                            codsite = ind.CodeSite,
                            datcpte = ind.DateCpte,
                            datereleve = ind.DateReleve,
                            datrecept = ind.DateRecept,
                            idabon = ind.IdentifiantAbonne.Cryptage(),
                            idreception = ind.IDReception,
                            index_cons_monop1 = ind.IndexConsoMonop1.Cryptage(),
                            index_cons_monop2 = ind.IndexConsoMonop2.Cryptage(),
                            index_cons_monop3 = ind.IndexConsoMonop3.Cryptage(),
                            index_horaire = ind.IndexHoraire.Cryptage(),
                            index_ima1 = ind.IndexIma1.Cryptage(),
                            index_ima2 = ind.IndexIma2.Cryptage(),
                            index_ima3 = ind.IndexIma3.Cryptage(),
                            index_jour = ind.IndexJour.Cryptage(),
                            index_nuit = ind.IndexNuit.Cryptage(),
                            index_pointe = ind.IndexPointe.Cryptage(),
                            index_reactif1 = ind.IndexReactif1.Cryptage(),
                            index_reactif2 = ind.IndexReactif2.Cryptage(),
                            index_reactif3 = ind.IndexReactif3.Cryptage(),
                            modrecept = ind.ModeReception,
                            numctr = ind.NumeroCompteur.Cryptage(),
                            perfact = ind.PeriodeFacturation,
                            refraccord = ind.ReferenceRaccordement.Cryptage()
                            //datextract=ind.DateExtract,
                            //topextract=ind.TopExtract
                        };

                        //new DAL<mvt_releve_reception>(ConnexionBuilder.ConnectionString).InserRow(lePRE);
                        new DAL<mvt_releve_reception>().InserRow(lePRE);
                        NbreInsert++;
                    }
                    catch (Exception ex)
                    {
                        NbreErreur++;
                        Resultat = false;
                        stb.Append(pErrMsg);
                        stb.AppendLine();
                        stb.Append(ex.Message);

                        throw;
                    }

                }

            }
            else
            {

                pErrMsg = "Impossible de charger l'objet emission s'il est null";
            }

            //pErrMsg = stb.ToString();
            return Resultat;
        }

        //-FCO
        public bool InsertIndexDansLeNoeud(ReceptionDTO _Index, ref string pErrMsg)
        {
            bool Resultat = true;
            pErrMsg = string.Empty;

           if(_Index!=null)
           {
               try{
                        mvt_releve_reception lePRE = new mvt_releve_reception()
                        {
                            codexp = _Index.CodeExploitation,
                            codsite = _Index.CodeSite,
                            datcpte = _Index.DateCpte,
                            datereleve = _Index.DateReleve,
                            //datrecept = _Index.DateRecept,
                            idabon = _Index.IdentifiantAbonne.Cryptage(),
                            idreception = _Index.IDReception,
                            index_cons_monop1 = _Index.IndexConsoMonop1.Cryptage(),
                            index_cons_monop2 = _Index.IndexConsoMonop2.Cryptage(),
                            index_cons_monop3 = _Index.IndexConsoMonop3.Cryptage(),
                            index_horaire = _Index.IndexHoraire.Cryptage(),
                            index_ima1 = _Index.IndexIma1.Cryptage(),
                            index_ima2 = _Index.IndexIma2.Cryptage(),
                            index_ima3 = _Index.IndexIma3.Cryptage(),
                            index_jour = _Index.IndexJour.Cryptage(),
                            index_nuit = _Index.IndexNuit.Cryptage(),
                            index_pointe = _Index.IndexPointe.Cryptage(),
                            index_reactif1 = _Index.IndexReactif1.Cryptage(),
                            index_reactif2 = _Index.IndexReactif2.Cryptage(),
                            index_reactif3 = _Index.IndexReactif3.Cryptage(),
                            modrecept = _Index.ModeReception,
                            numctr = _Index.NumeroCompteur.Cryptage(),
                            perfact = _Index.PeriodeFacturation,
                            refraccord = _Index.ReferenceRaccordement.Cryptage()
                            //datextract=ind.DateExtract,
                            //topextract=ind.TopExtract
                        };
                         
                        new DAL<mvt_releve_reception>().InserRow(lePRE);
                         
                    }
                    catch (Exception ex)
                    {
                         
                        Resultat = false;
                        pErrMsg = ex.Message;
                        throw;
                    }

                }
            else
            {

                pErrMsg = "Impossible de charger l'objet emission s'il est null";
            }

            //pErrMsg = stb.ToString();
            return Resultat;
        }
        //-----------


        private SqlConnectionStringBuilder GetSqlConnexionStringBuilder()
        {
            //- Récupération des informations de connexion
            SqlConnectionStringBuilder Builder = new SqlConnectionStringBuilder();
            Builder.DataSource = ".\\GSWE0431";
            Builder.InitialCatalog = "ACE_NODE";
            Builder.IntegratedSecurity = false;
            Builder.PersistSecurityInfo = true;
            if (!Builder.IntegratedSecurity)
            {
                Builder.UserID = "sa";
                Builder.Password = "P@ssw0rd";
            }

            return Builder;
        }

    }
}
