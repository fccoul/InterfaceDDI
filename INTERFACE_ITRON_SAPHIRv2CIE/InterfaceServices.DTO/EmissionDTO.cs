using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceServices.DTO
{
    [DataContract]
    public class EmissionDTO
    {
        [DataMember]
        public Guid IdEmission { set; get; }
        [DataMember]
        public string CodeSite { set; get; }
        [DataMember]
        public string CodeExploitation { set; get; }
        [DataMember]
        public string ReferenceRaccordement { set; get; }
        [DataMember]
        public string IdentifiantAbonne { set; get; }
        [DataMember]
        public string NomAbonne { set; get; }
        [DataMember]
        public string PrenomAbonne { set; get; }
        [DataMember]
        public string NumeroCompteur { set; get; }
        [DataMember]
        public string RueBoulevardAvenue { set; get; }
        [DataMember]
        public string LotIlot { set; get; }
        [DataMember]
        public string AggloQuartAutre { set; get; }
        [DataMember]
        public string PeriodeFacturation { set; get; }
        [DataMember]
        public DateTime? DateCreation { set; get; }
        [DataMember]
        public char? TopCalcul { set; get; }
        [DataMember]
        public char? TopExtract { set; get; }
        [DataMember]
        public DateTime? DateExtraction { set; get; }
        [DataMember]
        public char? ModeExtract { set; get; }
        [DataMember]
        public string TypeCompteur { set; get; }
        [DataMember]
        public string MotDePasse { set; get; }
        [DataMember]
        public string TypeConnexion { set; get; }
        [DataMember]
        public string NumeroTSP { set; get; }
        [DataMember]
        public string VersionFirmWare { set; get; }
    }
}
