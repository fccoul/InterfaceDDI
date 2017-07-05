using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Common
{

    public class MonitoringConfig : ConfigurationSection
    {
        [ConfigurationProperty("UserConnected", IsRequired = true)]
        public string Title
        {
            get { return this["UserConnected"] as string; }
        }

        [ConfigurationProperty("ValueEdit", IsRequired = false, DefaultValue = "default")]
        public int PostsPerPage
        {
            get { return (int)this["ValueEdit"]; }
        }
 
    }
}
