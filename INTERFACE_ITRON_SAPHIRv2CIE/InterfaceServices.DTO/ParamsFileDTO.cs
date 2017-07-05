using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InterfaceServices.DTO
{
     [DataContract]
    public class ParamsFileDTO
    {

        [DataMember]
        public Guid ID { set; get; }

        [DataMember]
        public string PathFileExcel { set; get; }
        
        [DataMember]
        public string PasswordFileExcel { set; get; }
        
         [DataMember]
        public DateTime ? DateUpdatedPassword { set; get; }

        [DataMember]
        public DateTime? DateUpdatedFile { set; get; }

         [DataMember]
        public string AccountUpdatedPath { set; get; }

         [DataMember]
        public string AccountUpdatetedPwd { set; get; }
         

    }
}
