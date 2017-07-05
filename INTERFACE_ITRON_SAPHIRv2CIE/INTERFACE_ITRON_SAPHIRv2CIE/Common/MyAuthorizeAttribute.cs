using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Common
{
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        public MyAuthorizeAttribute(params string[] roleKeys)
        {
            List<string> roles = new List<string>(roleKeys.Length);

            var allRoles=(NameValueCollection)ConfigurationManager.GetSection("roles");
            foreach (var roleKey in roleKeys)
	            {
		            roles.Add(allRoles[roleKey]);
                //---------@me
                    //switch (roleKey)
                    //{
                    //    case "AccountExecutive":
                    //                             break;
                    //}
	            }
            this.Roles = string.Join(",", roles);
            //----------------------------
            

        }
    }

    //---13122016
    public class DefaultAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var action = filterContext.ActionDescriptor;
            if (action.IsDefined(typeof(OverrideAuthorizeAttribute), true)) return;

            base.OnAuthorization(filterContext);
        }
    }
    public class OverrideAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
        }
    }
}
