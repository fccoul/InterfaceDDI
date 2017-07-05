using InterfaceServices.BusinessRules;
using InterfaceServices.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACEVISION.ProcessUI
{
    public class MeterPresenter
    {
        /// <summary>
        /// methode pour injecter infos ctr en bdd
        /// </summary>
        /// <param name="infoCtr"></param>
        /// <param name="msgTransaction"></param>
        /// <returns></returns>
        public bool insertMetertoBdd(CTRHTDTO infoCtr,out string msgTransaction)         
        {
            return new CompteurRules().insertMetertoBdd(infoCtr, out msgTransaction);
        }

        public bool updateMetertoBdd(CTRHTDTO infoCtr, out string msgTransaction)
        {
            return new CompteurRules().updateMetertoBdd(infoCtr, out msgTransaction);
        }

        public List<CTRHTDTO> getAllCTRHT(out string  msgTransaction)
        {
            return new CompteurRules().getAllCTRHT(out msgTransaction);
        }

        public CTRHTDTO getCTRHT_ofSerialNumber(string SerialNumber, out string msgTransaction)
        {
            return new CompteurRules().getCTRHT_ofSerialNumber(SerialNumber, out msgTransaction);
        }
    }
}
