using InterfaceServices.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceServices.SAPHIRCOMDataAccess.SAPHIRCOM;
using InterfaceServices.SAPHIRCOMDataAccess;
using ACEVISION.Common;
namespace InterfaceServices.BusinessRules
{
    /// <summary>
    /// cette classe gère les actions sur les compteurs en provenance du file Excel
    /// </summary>
    /*
     * \author FCO
     */ 
    public class CompteurRules
    {
        /// <summary>
        /// ajoute la ligne du fichier excel en bdd en crytant les données e
        /// </summary>
        /// <param name="infoCtr"></param>
        /// <param name="msgTransaction"></param>
        /// <returns>message derreur genéré cas echeant</returns>
        public bool insertMetertoBdd(CTRHTDTO infoCtr,out string msgTransaction)
        {
           
            msgTransaction = string.Empty;
            try
            {
                CTRHT _ctrHT = new CTRHT();
                _ctrHT.IDLINEXCEL = Guid.NewGuid();
                _ctrHT.SERIALNUMBER = infoCtr.SERIALNUMBER.Cryptage();
                _ctrHT.PASSWORD_READER = infoCtr.PASSWORD_READER.Cryptage();
                //---pr tests
                //_ctrHT.SERIALNUMBER = infoCtr.SERIALNUMBER;
                //_ctrHT.PASSWORD_READER = infoCtr.PASSWORD_READER;

                _ctrHT.PASSWORD_LABO = !string.IsNullOrEmpty(infoCtr.PASSWORD_LABO) ? infoCtr.PASSWORD_LABO.Cryptage() : string.Empty;
                _ctrHT.TYPEMETER = infoCtr.TYPEMETER.Cryptage();
                _ctrHT.FIRMWARE = infoCtr.FIRMWARE.Cryptage();

                _ctrHT.UPDATED = false;

                _ctrHT.DATE_OPERATION = DateTime.Now;
                new DAL<CTRHT>().InserRow(_ctrHT);
                return true;
            }
            catch (Exception ex)
            {
               
                msgTransaction = ex.Message;
            }

            return false;
        }

        public bool updateMetertoBdd(CTRHTDTO infoCtr, out string msgTransaction)
        {
           
            msgTransaction = string.Empty;
            try
            {
                CTRHT _ctrHT = new CTRHT();                
                _ctrHT.SERIALNUMBER = infoCtr.SERIALNUMBER.Cryptage();
                _ctrHT.PASSWORD_READER = infoCtr.PASSWORD_READER.Cryptage();
                //_ctrHT.SERIALNUMBER = infoCtr.SERIALNUMBER;
               // _ctrHT.PASSWORD_READER = infoCtr.PASSWORD_READER;
                 _ctrHT.PASSWORD_LABO = !string.IsNullOrEmpty(infoCtr.PASSWORD_LABO) ? infoCtr.PASSWORD_LABO.Cryptage() : string.Empty;
                //_ctrHT.PASSWORD_LABO = !string.IsNullOrEmpty(infoCtr.PASSWORD_LABO) ? infoCtr.PASSWORD_LABO : string.Empty;
                _ctrHT.TYPEMETER = infoCtr.TYPEMETER.Cryptage();
                _ctrHT.FIRMWARE = infoCtr.FIRMWARE.Cryptage();

                _ctrHT.UPDATED = true;

                _ctrHT.DATE_OPERATION = DateTime.Now;
                new DAL<CTRHT>().UpdateRow(_ctrHT,c=>c.SERIALNUMBER==_ctrHT.SERIALNUMBER);
                return true;
                
            }
            catch (Exception ex)
            {
               
                msgTransaction = ex.Message;
            }

            return false;
        }
        

        /// <summary>
        /// recupère l'ensemble des compteurs enregistrées en bdd
        /// </summary>
        /// <param name="msgTransaction"></param>
        /// <returns></returns>
        public List<CTRHTDTO> getAllCTRHT(out string msgTransaction)
        {
            msgTransaction = string.Empty;
            List<CTRHTDTO> _lstCTR = new List<CTRHTDTO>();
            foreach (var infoCtr in new DAL<CTRHT>().FindAll())
            {
                try
                {

                    CTRHTDTO _ctrHTDTO = new CTRHTDTO();

                    _ctrHTDTO.SERIALNUMBER = infoCtr.SERIALNUMBER.DeCryptage();
                    _ctrHTDTO.PASSWORD_READER = infoCtr.PASSWORD_READER.DeCryptage();
                    _ctrHTDTO.PASSWORD_LABO = !string.IsNullOrEmpty(infoCtr.PASSWORD_LABO) ? infoCtr.PASSWORD_LABO.DeCryptage() : string.Empty;
                    _ctrHTDTO.TYPEMETER = infoCtr.TYPEMETER.DeCryptage();
                    _ctrHTDTO.FIRMWARE = infoCtr.FIRMWARE.DeCryptage();

                    _lstCTR.Add(_ctrHTDTO);
                    
                }
                catch (Exception ex)
                {                    
                    msgTransaction=ex.Message;
                    
                }
            }
            return _lstCTR;
           
        }


        /// <summary>
        /// recupère les infossur un compteur précis
        /// </summary>
        
        public CTRHTDTO getCTRHT_ofSerialNumber(string SerialNumber,out string msgTransaction)
        {
            msgTransaction = string.Empty;
            //CTRHT infoCtr = new DAL<CTRHT>().Find(f => f.SERIALNUMBER == SerialNumber).FirstOrDefault();
            CTRHT infoCtr = new DAL<CTRHT>().Find(f => f.SERIALNUMBER == SerialNumber.Cryptage()).FirstOrDefault();
            CTRHTDTO _ctrHTDTO = new CTRHTDTO();
            if (infoCtr != null)
            {
                try
                {
                    //_ctrHTDTO.SERIALNUMBER = infoCtr.SERIALNUMBER.DeCryptage();
                    //--ppr test
                    _ctrHTDTO.SERIALNUMBER = infoCtr.SERIALNUMBER;

                    _ctrHTDTO.PASSWORD_READER = infoCtr.PASSWORD_READER.DeCryptage();
                    _ctrHTDTO.PASSWORD_LABO = !string.IsNullOrEmpty(infoCtr.PASSWORD_LABO) ? infoCtr.PASSWORD_LABO.DeCryptage() : string.Empty;
                    _ctrHTDTO.TYPEMETER = infoCtr.TYPEMETER.DeCryptage();
                    _ctrHTDTO.FIRMWARE = infoCtr.FIRMWARE.DeCryptage();

                    return _ctrHTDTO;

                }
                catch (Exception ex)
                {
                    msgTransaction = ex.Message;

                }
            }
            else
            {
                msgTransaction = "Aucune correspondance de compteur trouvée !";
                
            }
            return null;

        }
    }
}
