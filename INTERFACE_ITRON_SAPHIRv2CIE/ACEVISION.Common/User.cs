using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACEVISION.Common
{
    [Serializable]
    public class User
    {
        public string Matricule { get; set; }
        public string DisplayedName { get; set; }
        public bool  EstAdmin { get; set; }
    }
}
