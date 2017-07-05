using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Common
{
    public class Administration
    {
        [Required( ErrorMessage=Constante.ServeurBDDRequired)]
        public string NameServer{get;set;}
        [Required(ErrorMessage = Constante.BDDRequired)]
        public string BDDName { get; set; } 

        public string NameUser {get;set;} 
        public string Pwd  {get;set;}

        [Required(ErrorMessage = Constante.UrlSceWebRequired)]
        public string AdrServeur {get;set;}

        [Required(ErrorMessage = Constante.PortSceWebRRequired)]
        [StringLength(4,MinimumLength=2,ErrorMessage=Constante.LengthPortSceWebRRequired)]
        [RegularExpression(@"[1-9]*\.?[1-9]+",ErrorMessage=Constante.ZeroPort)]
        //[Range(1,Int16.MaxValue,ErrorMessage=Constante.TypeIntPort)]
        public int Port {get;set;} 
    }
}
