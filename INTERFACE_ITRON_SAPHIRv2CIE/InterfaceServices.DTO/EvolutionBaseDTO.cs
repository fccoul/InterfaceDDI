using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterfaceServices.DTO
{
    public class EvolutionBaseDTO
    {
        public Guid ID { get;set; }
        public int Version { get; set; }
        public string Full_Version { get; set; }
        public DateTime DateUpated { get; set; }
    }
}
