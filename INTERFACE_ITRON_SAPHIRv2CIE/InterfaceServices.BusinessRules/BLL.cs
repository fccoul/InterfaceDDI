using ACEVISION.Common;
using InterfaceServices.DTO;
using InterfaceServices.SAPHIRCOMDataAccess;
using InterfaceServices.SAPHIRCOMDataAccess.SAPHIRCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceServices.BusinessRules
{
    public class BLL
    {
        public IList<ExploitationDTO> GetAllExploitationAccessibles(string CodeExploitation, ref string ErrMess)
        {
            try
            {
                IList<ExploitationDTO> Result = new List<ExploitationDTO>();
                IList<RefExploitation> LesExploitation = new DAL<RefExploitation>().Find(p => string.IsNullOrEmpty(CodeExploitation) ? p.CodeExploitation==p.SiteGesabel : p.CodeExploitation == CodeExploitation);
                if (LesExploitation != null && LesExploitation.Count > 0)
                {
                    foreach (RefExploitation item in LesExploitation)
                    {
                        ExploitationDTO UneExploitation = new ExploitationDTO()
                        {
                            CodeExploitation = item.CodeExploitation,
                            DirectionRegionale = item.DirectionRegionale,
                            Exploitation_libelle = item.Explotation_libelle,
                            SiteGesabel = item.SiteGesabel
                        };
                        Result.Add(UneExploitation);
                    }
                }
                return Result.OrderBy(o=>o.Exploitation_libelle).ToList();
            }
            catch (Exception ex)
            {

                ErrMess = ex.Message;
                return null;
            }

        }

        /// <summary>
        /// Get exploitation du site paframéterer
        /// </summary>
        /// <param name="CodeExploitation"></param>
        /// <param name="ErrMess"></param>
        /// <returns></returns>
        public IList<ExploitationDTO> GetAllExploitationAccessibles_ofSIte(string CodeExploitation, ref string ErrMess)
        {
            try
            {
                IList<ExploitationDTO> Result = new List<ExploitationDTO>();
                IList<RefExploitation> LesExploitation = new DAL<RefExploitation>().Find(p => p.SiteGesabel == CodeExploitation);
                if (LesExploitation != null && LesExploitation.Count > 0)
                {
                    foreach (RefExploitation item in LesExploitation)
                    {
                        ExploitationDTO UneExploitation = new ExploitationDTO()
                        {
                            CodeExploitation = item.CodeExploitation,
                            DirectionRegionale = item.DirectionRegionale,
                            Exploitation_libelle = item.Explotation_libelle,
                            SiteGesabel = item.SiteGesabel
                        };
                        Result.Add(UneExploitation);
                    }
                }
                return Result;
            }
            catch (Exception ex)
            {

                ErrMess = ex.Message;
                return null;
            }

        }

        /// <summary>
        /// recupere les log de la transaction Reception...
        /// </summary>
        /// <returns></returns>
        public IList<LogReceptionDTO> GetAllLog_Reception(out string msgErr)
        {
            msgErr = string.Empty;
            List<LogReceptionDTO> lstLog_Reception = new List<LogReceptionDTO>();
            List<LogBDD> lst = new DAL<LogBDD>().Find(l => l.TypeTraitement == Constantes.TypeTraitementRec).OrderByDescending(o => o.DateExecution).ToList();
            if (lst.Count() > 0)
            {
                try
                {
                    foreach (var item in lst)
                    {
                        LogReceptionDTO _log = new LogReceptionDTO()
                        {
                            Auteur=item.Auteur,
                            DateExecution=item.DateExecution.Value,
                            ExecutionID=item.ExecutionID,
                            ExecutionMode=item.ExecutionMode,
                            Message=item.Message,
                            Niveau=item.Niveau,
                            PeriodeFacturation=item.PeriodeFacturation,
                            TotalRead_fromAce = item.TotalRead_fromAce.Value, 
                            TotalInserted_toNode=item.TotalInserted_toNode.Value,
                            TypeTraitement=item.TypeTraitement,
                            CptExecution=(short)item.CptExecution.Value

                        };
                        lstLog_Reception.Add(_log);
                    }
                }
                catch (Exception ex)
                {

                    msgErr = ex.Message;
                }
            }
            return lstLog_Reception;
        }

        /// <summary>
        /// recupere les log de la transaction Emission...
        /// </summary>
        /// <param name="msgErr"></param>
        /// <returns></returns>
        public IList<LogEmissionDTO> GetAllLog_Emission(out string msgErr)
        {
            msgErr = string.Empty;
            List<LogEmissionDTO> lstLog_Emission = new List<LogEmissionDTO>();
            List<LogBDD> lst = new DAL<LogBDD>().Find(l => l.TypeTraitement == Constantes.TypeTraitementEmi).OrderByDescending(o => o.DateExecution).ToList();
            if (lst.Count() > 0)
            {
                try
                {
                    foreach (var item in lst)
                    {
                        LogEmissionDTO _log = new LogEmissionDTO()
                        {
                            Auteur = item.Auteur,
                            DateExecution = item.DateExecution.Value,
                            ExecutionID = item.ExecutionID,
                            ExecutionMode = item.ExecutionMode,
                            Message = item.Message,
                            Niveau = item.Niveau,
                            PeriodeFacturation = item.PeriodeFacturation,
                            TotalRead_fromNode=item.TotalRead_fromNode.Value,
                            TotalInserted_toAce=item.TotalInserted_toAce.Value,

                            //TotalRead_fromAce = item.TotalRead_fromAce.Value,
                            //TotalInserted_toNode = item.TotalInserted_toNode.Value,

                            TypeTraitement = item.TypeTraitement,
                            CptExecution = (short)item.CptExecution.Value

                        };
                        lstLog_Emission.Add(_log);
                    }
                }
                catch (Exception ex)
                {

                    msgErr = ex.Message;
                }
            }
            return lstLog_Emission;
        }

        /// <summary>
        /// recuperer les Log de a transaction Inetgration
        /// </summary>
        /// <param name="msgErr"></param>
        /// <returns></returns>
        public IList<LogEmissionDTO> GetAllLog_Integration(out string msgErr)
        {
            msgErr = string.Empty;
            List<LogEmissionDTO> lstLog_Emission = new List<LogEmissionDTO>();
            List<LogBDD> lst = new DAL<LogBDD>().Find(l => l.TypeTraitement.Contains(Constantes.TraitementIntegration)).OrderByDescending(o => o.DateExecution).ToList();
            if (lst.Count() > 0)
            {
                try
                {
                    foreach (var item in lst)
                    {
                        LogEmissionDTO _log = new LogEmissionDTO()
                        {
                            Auteur = item.Auteur,
                            DateExecution = item.DateExecution.Value,
                            ExecutionID = item.ExecutionID,
                            ExecutionMode = item.ExecutionMode,
                            Message = item.Message,
                            Niveau = item.Niveau,
                            PeriodeFacturation = item.PeriodeFacturation,
                            TotalRead_fromNode = item.TotalRead_fromNode.Value,
                            TotalInserted_toAce = item.TotalInserted_toAce.Value,

                            //TotalRead_fromAce = item.TotalRead_fromAce.Value,
                            //TotalInserted_toNode = item.TotalInserted_toNode.Value,

                            TypeTraitement = item.TypeTraitement,
                            CptExecution = (short)item.CptExecution.Value

                        };
                        lstLog_Emission.Add(_log);
                    }
                }
                catch (Exception ex)
                {

                    msgErr = ex.Message;
                }
            }
            return lstLog_Emission;
        }


        public IList<LogIntegrationCTRDTO> GetAllLog_IntegrationCtr(out string msgErr)
        {
            msgErr = string.Empty;
            List<LogIntegrationCTRDTO> _logIntegrationCTRDTO = new List<LogIntegrationCTRDTO>();
            List<LogBDD> lst = new DAL<LogBDD>().Find(l => l.TypeTraitement.Contains(Constantes.TraitementIntegrationCtr)).OrderByDescending(o => o.DateExecution).ToList();
            if (lst.Count() > 0)
            {
                try
                {
                    foreach (var item in lst)
                    {
                        LogIntegrationCTRDTO _log = new LogIntegrationCTRDTO()
                        {
                            Auteur = item.Auteur,
                            DateExecution = item.DateExecution.Value,
                            ExecutionID = item.ExecutionID,
                            ExecutionMode = item.ExecutionMode,
                            Message = item.Message,
                            Niveau = item.Niveau,
                             
                            TotalRead = item.TotalRead_fromNode.Value,
                            TotalInserted = item.TotalInserted_toAce.Value,
                            TotalUpdateted=item.TotalUpdated_toAce.Value,

                            //TotalRead_fromAce = item.TotalRead_fromAce.Value,
                            //TotalInserted_toNode = item.TotalInserted_toNode.Value,

                            TypeTraitement = item.TypeTraitement,
                            CptExecution = (short)item.CptExecution.Value

                        };
                        _logIntegrationCTRDTO.Add(_log);
                    }
                }
                catch (Exception ex)
                {

                    msgErr = ex.Message;
                }
            }
            return _logIntegrationCTRDTO;
        }

        public List<ReceptionDTO> GetAllMvtReceptionBDD(string PeriodFacturation,out string msgErr)
        {
            msgErr = string.Empty;
            List<ReceptionDTO> lstResult = new List<ReceptionDTO>();
            List<mvt_releve_reception> lstMvtRec= new DAL<mvt_releve_reception>().Find(p => p.perfact == PeriodFacturation).ToList();
            if (lstMvtRec.Count() > 0)
            {
                try
                {
                    foreach (var item in lstMvtRec)
                    {
                        ReceptionDTO _receptionDTO = new ReceptionDTO();
                        _receptionDTO.CodeExploitation = item.codexp;
                        _receptionDTO.CodeSite = item.codsite;
                        _receptionDTO.DateCpte = item.datcpte;
                        _receptionDTO.DateReleve = item.datereleve;
                        _receptionDTO.DateRecept = item.datrecept;
                        _receptionDTO.IdentifiantAbonne = item.idabon;
                        _receptionDTO.IDReception = item.idreception;
                        _receptionDTO.IndexConsoMonop1 = item.index_cons_monop1;
                        _receptionDTO.IndexConsoMonop2 = item.index_cons_monop2;
                        _receptionDTO.IndexConsoMonop3 = item.index_cons_monop3;
                        _receptionDTO.IndexHoraire = item.index_horaire;
                        _receptionDTO.IndexIma1 = item.index_ima1;
                        _receptionDTO.IndexIma2 = item.index_ima2;
                        _receptionDTO.IndexIma3 = item.index_ima3;
                        _receptionDTO.IndexJour = item.index_jour;
                        _receptionDTO.IndexNuit = item.index_nuit;
                        _receptionDTO.IndexPointe = item.index_pointe;
                        _receptionDTO.IndexReactif1 = item.index_reactif1;
                        _receptionDTO.IndexReactif2 = item.index_reactif2;
                        _receptionDTO.IndexReactif3 = item.index_reactif3;
                        _receptionDTO.ModeReception = item.modrecept;
                        _receptionDTO.NumeroCompteur = item.numctr;
                        _receptionDTO.PeriodeFacturation = item.perfact;
                        _receptionDTO.ReferenceRaccordement = item.refraccord;
                        //........................................
                        _receptionDTO.TopExtract = '1';

                        lstResult.Add(_receptionDTO);
                    }

                }
                catch (Exception ex)
                {

                    msgErr = ex.Message;
                }
            }
            return lstResult;
        }

        public ReceptionDTO getReception_Item (string serialnumber, string periodeFacturation, out string msgErr)
        {
            msgErr = string.Empty;
            ReceptionDTO _receptionDTO = new ReceptionDTO();
            //-crypt
           // var mv=new DAL<mvt_releve_reception>().Find(m => m.perfact == periodeFacturation.Trim()).FirstOrDefault();
            //var xx=mv.numctr.DeCryptage();
            //mvt_releve_reception item = new DAL<mvt_releve_reception>().Find(m => m.numctr.Trim() == serialnumber.Trim() && m.perfact == periodeFacturation.Trim()).FirstOrDefault();
            mvt_releve_reception item = new DAL<mvt_releve_reception>().Find(m => m.numctr == serialnumber.Cryptage() && m.perfact == periodeFacturation.Trim()).FirstOrDefault();

            if (item != null)
            {
                _receptionDTO.CodeExploitation = item.codexp;
                _receptionDTO.CodeSite = item.codsite;
                _receptionDTO.DateCpte = item.datcpte;
                _receptionDTO.DateReleve = item.datereleve;
                _receptionDTO.DateRecept = item.datrecept;
                _receptionDTO.IdentifiantAbonne = item.idabon;
                _receptionDTO.IDReception = item.idreception;
                _receptionDTO.IndexConsoMonop1 = item.index_cons_monop1;
                _receptionDTO.IndexConsoMonop2 = item.index_cons_monop2;
                _receptionDTO.IndexConsoMonop3 = item.index_cons_monop3;
                _receptionDTO.IndexHoraire = item.index_horaire;
                _receptionDTO.IndexIma1 = item.index_ima1;
                _receptionDTO.IndexIma2 = item.index_ima2;
                _receptionDTO.IndexIma3 = item.index_ima3;
                _receptionDTO.IndexJour = item.index_jour;
                _receptionDTO.IndexNuit = item.index_nuit;
                _receptionDTO.IndexPointe = item.index_pointe;
                _receptionDTO.IndexReactif1 = item.index_reactif1;
                _receptionDTO.IndexReactif2 = item.index_reactif2;
                _receptionDTO.IndexReactif3 = item.index_reactif3;
                _receptionDTO.ModeReception = item.modrecept;
                _receptionDTO.NumeroCompteur = item.numctr;
                _receptionDTO.PeriodeFacturation = item.perfact;
                _receptionDTO.ReferenceRaccordement = item.refraccord;

                return _receptionDTO;
            }
            else
                return null;
            
        }

        /// <summary>
        /// return les log d'une periode de facturation (get la 1ère afin de determiner le nombre d'execution passée sur cette periode de facturation
        /// </summary>
        /// <param name="_PerFact"></param>
        /// <returns></returns>
        public LogReceptionDTO getLogBdd(string _PerFact)
        {
             LogReceptionDTO _LogReceptionDTO = new LogReceptionDTO();
            LogBDD logBdd=new DAL<LogBDD>().Find(f => f.PeriodeFacturation == _PerFact).FirstOrDefault();
            if (logBdd != null)
            {


                _LogReceptionDTO.Auteur = logBdd.Auteur;
                _LogReceptionDTO.CptExecution = (short)logBdd.CptExecution;
                _LogReceptionDTO.DateExecution = logBdd.DateExecution.Value;
                _LogReceptionDTO.ExecutionID = logBdd.ExecutionID;
                _LogReceptionDTO.ExecutionMode = logBdd.ExecutionMode;
                _LogReceptionDTO.Message = logBdd.Message;
                _LogReceptionDTO.Niveau = logBdd.Niveau;
                _LogReceptionDTO.PeriodeFacturation = logBdd.PeriodeFacturation;
                _LogReceptionDTO.TotalInserted_toNode = logBdd.TotalInserted_toNode.Value;
                _LogReceptionDTO.TotalRead_fromAce = logBdd.TotalRead_fromAce.Value;
                _LogReceptionDTO.TypeTraitement = logBdd.TypeTraitement;

                return _LogReceptionDTO;
            }
            else
                return null;
        }


        /// <summary>
        /// return les log d'une periode de facturation afin d'obtenir le max d'execution passée...
        /// </summary>
        /// <param name="periodeFacturation"></param>
        /// <returns></returns>
        public List<LogReceptionDTO> getAllLogBdd_ofPerFact(string periodeFacturation)
        {

            List<LogReceptionDTO> _lstLog = new List<LogReceptionDTO>();
            List<LogBDD> lstLgB= new DAL<LogBDD>().Find(l => l.PeriodeFacturation == periodeFacturation).ToList();
            foreach (var item in lstLgB)
            {
                LogReceptionDTO lgRec = new LogReceptionDTO()
                {
                    Auteur=item.Auteur,
                    CptExecution=(short)item.CptExecution,
                    DateExecution=item.DateExecution.Value,
                    ExecutionMode=item.ExecutionMode,
                    Message=item.Message,
                    Niveau=item.Niveau,
                    PeriodeFacturation=item.PeriodeFacturation,
                    TotalInserted_toNode=item.TotalInserted_toNode.Value,
                    TotalRead_fromAce=item.TotalRead_fromAce.Value,
                    TypeTraitement=item.TypeTraitement,
                   
                };

                _lstLog.Add(lgRec);
            }
            return _lstLog;
        }


        /// <summary>
        /// MAJ des ligne Init / Error des log de l'execution en cours...
        /// </summary>
        /// <param name="executionID"></param>
        /// <param name="_CptExecution"></param>
        /// <returns></returns>
        public bool UpdateLog_ofPerFactExecutionID(string executionID, int _CptExecution)
            {
                 List<LogBDD> _lstLg = new DAL<LogBDD>().Find(l => l.ExecutionID == executionID).ToList();

                 if (_lstLg.Count > 0)
                 {
                     foreach (var item in _lstLg)
                     {
                         item.CptExecution = _CptExecution;
                         new DAL<LogBDD>().UpdateRow(item, u => u.ID == item.ID);
                     }

                     return true;
                 }

               return false;
            }


        public bool InsertData_FileExcel(ParamsFileDTO _paramsFileDTO,ref string msgErr)
        {
            try
            {
                ParamsFile pf = new ParamsFile()
                  {
                      ID = Guid.NewGuid(),
                      PathFileExcel = _paramsFileDTO.PathFileExcel,
                      PasswordFileExcel = Crypteur.CrypterText(_paramsFileDTO.PasswordFileExcel),
                      DateUpdatedFile = _paramsFileDTO.DateUpdatedFile,
                      DateUpdatedPassword = _paramsFileDTO.DateUpdatedPassword,
                      AccountUpdatedPath = _paramsFileDTO.AccountUpdatedPath,
                      AccountUpdatetedPwd = _paramsFileDTO.AccountUpdatetedPwd
                  };
                new DAL<ParamsFile>().InserRow(pf);
                return true;
            }
            catch (Exception ex)
            {
                msgErr = ex.Message;
                return false;
            }
            
        }

        public bool UpdateData_FileExcel(ParamsFileDTO _paramsFileDTO, ref string msgErr,bool isAdminExcel)
        {
            try
            {
               ParamsFile _pf= new DAL<ParamsFile>().Find(f => f.ID == _paramsFileDTO.ID).FirstOrDefault();
               if (_pf != null)
               {
                   if (isAdminExcel)
                   {
                       _pf.PasswordFileExcel = Crypteur.CrypterText(_paramsFileDTO.PasswordFileExcel);
                       _pf.AccountUpdatetedPwd = _paramsFileDTO.AccountUpdatetedPwd;
                       _pf.DateUpdatedPassword = _paramsFileDTO.DateUpdatedPassword;
                   }
                   else
                   {
                       _pf.DateUpdatedFile = _paramsFileDTO.DateUpdatedFile;
                       _pf.AccountUpdatedPath = _paramsFileDTO.AccountUpdatedPath;
                       _pf.PathFileExcel = _paramsFileDTO.PathFileExcel;
                   }
                    
                    

                   new DAL<ParamsFile>().UpdateRow(_pf, p => p.ID == _paramsFileDTO.ID);
                   return true;
               }
               else
               {
                   msgErr = "Aucun paramétrage trouvé";
                   return false;
                  
               }
 
            }
            catch (Exception ex)
            {
                msgErr = ex.Message;
                return false;
            }

        }

        public string getPathFileExcel(ref string msgErr)
        {
            try
            {
                string FullPath = new DAL<ParamsFile>().FindAll().FirstOrDefault().PathFileExcel;
                return FullPath;
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
                return null;
            }
        }

        public ParamsFileDTO get_ParamsFile(ref string msgErr)
        {
            try
            {
                ParamsFileDTO _pf = new ParamsFileDTO();
                ParamsFile dt= new DAL<ParamsFile>().FindAll().FirstOrDefault();
                if (dt != null)
                {
                    _pf.PasswordFileExcel = Crypteur.DecrypterText(dt.PasswordFileExcel);
                    _pf.DateUpdatedFile = dt.DateUpdatedFile;
                    _pf.DateUpdatedPassword = dt.DateUpdatedPassword;
                    _pf.AccountUpdatedPath = dt.AccountUpdatedPath;
                    _pf.AccountUpdatetedPwd = dt.AccountUpdatetedPwd;
                    _pf.PathFileExcel = dt.PathFileExcel;
                    _pf.ID = dt.ID;

                    return _pf;
                }
                else
                {
                    msgErr = "Aucune configuration du fichier !";
                    return null;
                }
                
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
                return null;
            }
        }


        public EvolutionBaseDTO getVersionSoftWare(ref string msgErr)
        {
            try
            {
                EvolutionBase eb= new DAL<EvolutionBase>().FindAll().OrderByDescending(u => u.DateUpated).FirstOrDefault();
                if (eb != null)
                {
                    EvolutionBaseDTO ebDTO = new EvolutionBaseDTO
                    {
                        ID = eb.ID,
                        Version = eb.Version.Value,
                        Full_Version = eb.Full_Version,
                        DateUpated = eb.DateUpated.Value
                    };
                    return ebDTO;
                }
                return null;

            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
                return null;
            }
        }
    }
}
