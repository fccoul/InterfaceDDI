using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceServices.DTO
{
    [DataContract]
    public class LogDTO
    {
        [DataMember]
        public string DescriptionErreur { set; get; }
        [DataMember]
        public string ReferenceObjet { set; get; }
        [DataMember]
        public string DateLog { set; get; }
        [DataMember]
        public string Objet { set; get; }
    }
}
