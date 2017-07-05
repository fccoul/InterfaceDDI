using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Common
{
    public class UserForm
    {
         [Required(ErrorMessage = "Le Login est obligatoire")]
        public string Login { get; set; }

         [Required(ErrorMessage = "Le Mot de passe est obligatoire")]
        public string Password { get; set; }

        public string Role { get; set; }
    }
}
