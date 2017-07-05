using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterfaceServices.DTO
{
    public class LogEmissionDTO
    {
        public DateTime DateExecution { get; set; }
        public string Auteur { get; set; }
        public string Niveau { get; set; }
        public string Message { get; set; }
        public Int32 TotalRead_fromNode{ get; set; }
        public Int32 TotalInserted_toAce{ get; set; }
 
        public string ExecutionMode { get; set; }
        public string TypeTraitement { get; set; }
        public string PeriodeFacturation { get; set; }
        public string ExecutionID { get; set; }

        public Int16 CptExecution { get; set; } 
    }
    
}
