using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InterfaceServices.DTO
{

    [DataContract]
    public class SDTO_Historique
    {
        [DataMember]
        public string codexp { set; get; }
        [DataMember]
        public string codsite { set; get; }
        [DataMember]
        public DateTime? datecpte { set; get; }
        [DataMember]
        public DateTime? datextract { set; get; }
        [DataMember]
        public DateTime? datoper { set; get; }
        [DataMember]
        public DateTime? datrecept { set; get; }
        [DataMember]
        public char? modextract { set; get; }
        [DataMember]
        public char? modrecept { set; get; }
        [DataMember]
        public int? nbrmvt { set; get; }
        [DataMember]
        public string numoperec { set; get; }
        [DataMember]
        public string numoperex { set; get; }
        [DataMember]
        public string perfact { set; get; }

    }

}
