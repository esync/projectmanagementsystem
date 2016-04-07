using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class CustomerModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; }

        public string UserId { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}