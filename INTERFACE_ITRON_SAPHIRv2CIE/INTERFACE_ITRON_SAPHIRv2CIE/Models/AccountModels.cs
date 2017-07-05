using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Models
{
    public class UsersContext : DbContext
    {
        //public UsersContext()
        //    : base("DefaultConnection")
        public UsersContext()
            : base("SAPHIRCOM_ConnectionString")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<INTERFACE_ITRON_SAPHIRv2CIE.Models.Role> Roles { get; set; }

        //public DbSet<RegisterModel> UserProfile { get; set; }

        public DbSet<webpages_UsersInRoles> webpages_UsersInRole { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
        //---------------
        public string EmailID { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
       
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        [Required]
        [Display(Name = "EmailID")]
        public string EmailID { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }


    public class AssignRoleVM
    {
        [Required(ErrorMessage = " Veuillez selectionner le profil")]
        public string RoleName { get; set; }
        [Required(ErrorMessage = "Veuillez selectionner le compte utilisateur")]
        public string UserID { get; set; }
        public List<System.Web.Mvc.SelectListItem> Userlist { get; set; }
        public List<System.Web.Mvc.SelectListItem> RolesList { get; set; }
    }

    [Table("webpages_UsersInRoles")]
    public class webpages_UsersInRoles
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public int RoleId { get; set; }
    }

    public class AllroleandUser
    {
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public IEnumerable<AllroleandUser> AllDetailsUserlist { get; set; }
    }

    //---------
    public class LocalSettingModel
    {
        [Required(ErrorMessage="Le Mot de passe actuel est obligatoire")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Le Nouveau de Mot de passe est obligatoire")]
        [StringLength(100, ErrorMessage = "le {0} doit contenir au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau Mot de passe")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmation Mot de passe")]
        [Compare("NewPassword", ErrorMessage = "la confirmation du mot de passe ne correspond pas")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "L'adresse electronique est obligatoire")]
        //[DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Adresse Electronique invalide")]
        [Display(Name = "Adresse Electronique")]
        public string Email { get; set; }
    }

    //12032017
    public class infosIntegrated
    {
        public string Client { get; set; }       
        public string IDABON { get; set; }
        public string Compteur { get; set; }
    }
}
