using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InterfaceServices.DTO
{
     [DataContract]
    public class CTRHTDTO
    {
        [DataMember]
         public Guid IDLINEXCEL { set; get; }
        [DataMember]
        public string SERIALNUMBER { set; get; }
        [DataMember]
        public string PASSWORD_READER { set; get; }
        [DataMember]
        public string PASSWORD_LABO { set; get; }
        [DataMember]
        public string TYPEMETER { set; get; }
        [DataMember]
        public string FIRMWARE { set; get; }
    }
}
