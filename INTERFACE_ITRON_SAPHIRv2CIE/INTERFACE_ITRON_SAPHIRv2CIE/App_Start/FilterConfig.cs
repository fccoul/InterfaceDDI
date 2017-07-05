using System.Web;
using System.Web.Mvc;

namespace INTERFACE_ITRON_SAPHIRv2CIE
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}