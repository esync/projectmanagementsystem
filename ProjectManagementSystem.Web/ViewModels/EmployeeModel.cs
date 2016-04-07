using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class EmployeeModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Department")]
        public string Department { get; set; }

        public string UserId { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}