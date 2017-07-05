using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterfaceServices.DTO
{
    public class LogReceptionDTO
    {
        public DateTime DateExecution { get; set; }
        public string Auteur { get; set; }
        public string Niveau { get; set; }
        public string Message { get; set; }
        public Int32 TotalRead_fromAce { get; set; }
        public Int32 TotalInserted_toNode { get; set; }
 
        public string ExecutionMode { get; set; }
        public string TypeTraitement { get; set; }
        public string PeriodeFacturation { get; set; }
        public string ExecutionID { get; set; }

        public Int16 CptExecution { get; set; } 
    }
}
