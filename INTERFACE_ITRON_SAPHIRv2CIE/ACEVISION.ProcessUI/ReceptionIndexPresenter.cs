using InterfaceServices.BusinessRules;
using InterfaceServices.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACEVISION.ProcessUI
{
    //-Oresenter pour la reception et l'emission
    public class ReceptionIndexPresenter
    {
        public bool InsertDesIndexDansLeNoeud(IList<ReceptionDTO> LesIndex, ref int NbreInsert, ref int NbreErreur, ref string pErrMsg)
        {
            return new ReceptionRules().InsertDesIndexDansLeNoeud(LesIndex, ref NbreInsert, ref NbreErreur, ref pErrMsg);
        }

        //-FCO
        public bool InsertIndexDansLeNoeud(ReceptionDTO _Index, ref string pErrMsg)
        {
            return new ReceptionRules().InsertIndexDansLeNoeud(_Index,ref pErrMsg);
        }
        

        //public IList<EmissionDTO> GetAllBrderauxDeLexploitation(string CodeExploi)
        //{
        //    return EmissionRules.GetAllBordereauxDeLexploitation(CodeExploi);
        //    //return null;
        //}

        //---Get Log
        public List<LogReceptionDTO> GetLog_Reception(out string msgErr)
        {
          msgErr=string.Empty;
            List<LogReceptionDTO> _LstLogReceptionDTO = new List<LogReceptionDTO>();
            _LstLogReceptionDTO= new BLL().GetAllLog_Reception(out msgErr).ToList();
             
            return _LstLogReceptionDTO; 
        }

        public List<LogEmissionDTO> GetLog_Emisssion(out string msgErr)
        {
            msgErr = string.Empty;
            List<LogEmissionDTO> _LstLogEmissionDTO = new List<LogEmissionDTO>();
            _LstLogEmissionDTO = new BLL().GetAllLog_Emission(out msgErr).ToList();

            return _LstLogEmissionDTO;
        }

        public List<LogEmissionDTO> GetLog_Integration(out string msgErr)
        {
            msgErr = string.Empty;
            List<LogEmissionDTO> _LstLogIntegrationDTO = new List<LogEmissionDTO>();
            _LstLogIntegrationDTO = new BLL().GetAllLog_Integration(out msgErr).ToList();

            return _LstLogIntegrationDTO;
        }

        public List<LogIntegrationCTRDTO> GetLog_IntegrationCtr(out string msgErr)
        {
            msgErr = string.Empty;
            List<LogIntegrationCTRDTO> _LstLogIntegrationCtr = new List<LogIntegrationCTRDTO>();
            _LstLogIntegrationCtr = new BLL().GetAllLog_IntegrationCtr(out msgErr).ToList();

            return _LstLogIntegrationCtr;
        }

        /// <summary>
        /// obtenir la liste de tous les compteurs integrés en BDD SAPHIR pour une periode de facturation
        /// </summary>
        /// <param name="periodeFacturation"></param>
        /// <returns></returns>
        public List<ReceptionDTO> GetAllMvtReception(string periodeFacturation,out string msgErr)
        {
            msgErr=string.Empty;
            List<ReceptionDTO> lstResult=new List<ReceptionDTO>();
            lstResult=new BLL().GetAllMvtReceptionBDD(periodeFacturation, out msgErr);
            return lstResult;
        }


        public ReceptionDTO getReception_Item(string serialnumber, string periodeFacturation, out string msgErr)
        {
            msgErr = string.Empty;
            ReceptionDTO rec = new BLL().getReception_Item(serialnumber, periodeFacturation, out msgErr);
            return rec;
        }

        public LogReceptionDTO getLogBdd(string periodeFacturation)
        {
            return new BLL().getLogBdd(periodeFacturation);
        }

        public List<LogReceptionDTO> getAllLogBdd_ofPerFact(string periodeFacturation)
        {
            return new BLL().getAllLogBdd_ofPerFact(periodeFacturation);
        }

        public bool UpdateLog_ofPerFactExecutionID(string executionID, int _CptExecution)
        {
            return new BLL().UpdateLog_ofPerFactExecutionID(executionID, _CptExecution);
        }


        /// <summary>
        /// REcuperation de toutes les types de deamndes(Abonnement,Mutation,...) non traites de la table "ACTIVATION_ABO_ACEVISION"
        /// </summary>
        /// <returns></returns>
        public List<ActivationAbonneDTO> GetAllDemandesFromSAPHIR()
        {
            return EmissionRules.GetAllDemandesActivesFromSAPHIR();
        }

        public List<ActivationAbonneDTO> GetAllDemandesSuccesFromSAPHIR()
        {
            return EmissionRules.GetAllDemandesSuccesromSAPHIR();
        }


        /// <summary>
        /// MAJ de la table "ACTIVATION_ABO_ACEVISION" :champ dtaEXtract,HerueExtract,ModeExtract,afin de signaler au "sniffer (prg)" k
        /// ces données ont déjà été envoyées
        /// </summary>
        /// <param name="LesMouvementsTraites"></param>
        /// <param name="ErrMessage"></param>
        public void UpDateMouvementsTraites(IList<Guid> LesMouvementsTraites, ref string ErrMessage)
        {
            EmissionRules.UpDateMouvementsTraites(LesMouvementsTraites, ref ErrMessage);
        }

        
    }
}
