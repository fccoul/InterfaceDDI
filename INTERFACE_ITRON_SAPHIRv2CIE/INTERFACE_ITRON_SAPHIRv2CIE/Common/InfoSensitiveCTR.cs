using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Common
{
    public class InfoSensitiveCTR
    {
        public string SerialNumber { get; set; }
        public string PasswordReader { get; set; }
        public string PasswordLabo { get; set; }
        public string TypeMeter { get; set; }
        public double versionFirmWare { get; set; }
    }

    //22042017
    public class MessageFile
    {
        public string detailsError {get;set;}
        public string Resultat { get; set; }
        public dynamic detailsResultat { get; set; }
    }
}
