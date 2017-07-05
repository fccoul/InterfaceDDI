using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceServices.DTO
{
    [DataContract]
    public class ReceptionDTO
    {
        [DataMember]
        public Guid IDReception { set; get; }
        [DataMember]
        public string CodeSite { set; get; }
        [DataMember]
        public string CodeExploitation { set; get; }
        [DataMember]
        public string ReferenceRaccordement { set; get; }
        [DataMember]
        public string IdentifiantAbonne { set; get; }
        [DataMember]
        public string NumeroCompteur { set; get; }
        [DataMember]
        public string PeriodeFacturation { set; get; }
        [DataMember]
        public char? ModeReception { set; get; }
        [DataMember]
        public string IndexNuit { set; get; }
        [DataMember]
        public string IndexJour { set; get; }
        [DataMember]
        public string IndexPointe { set; get; }
        [DataMember]
        public string IndexHoraire { set; get; }
        [DataMember]
        public string IndexReactif1 { set; get; }
        [DataMember]
        public string IndexReactif2 { set; get; }
        [DataMember]
        public string IndexReactif3 { set; get; }
        [DataMember]
        public string IndexIma1 { set; get; }
        [DataMember]
        public string IndexIma2 { set; get; }
        [DataMember]
        public string IndexIma3 { set; get; }
        [DataMember]
        public string IndexConsoMonop1 { set; get; }
        [DataMember]
        public string IndexConsoMonop2 { set; get; }
        [DataMember]
        public string IndexConsoMonop3 { set; get; }
        [DataMember]
        public DateTime? DateReleve { set; get; }
        [DataMember]
        public DateTime? DateRecept { set; get; }
        [DataMember]
        public DateTime? DateCpte { set; get; }
        [DataMember]
        public DateTime? DateExtract { set; get; }
        [DataMember]
        public char? TopExtract { set; get; }

    }
}


