using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Models
{
    //public class RoleModels
    //{
        [Table("webpages_Roles")]
        public class Role
        {
            [Required(ErrorMessage = "Veuillez saisir le profil")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            public string RoleName { get; set; }

            [Key]
            [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
            public int RoleId { get; set; }
        }
    //}
}
