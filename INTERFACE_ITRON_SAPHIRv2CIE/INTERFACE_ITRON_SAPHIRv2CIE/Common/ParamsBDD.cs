using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Common
{
    public static class ParamsBDD
    {
        public static string DataSource { get; set; }
        public static string InitialCatalog { get; set; }
        public static bool IntegratedSecurity { get; set; }
        public static string UserID { get; set; }
        public static string Password { get; set; }
    }
}
