namespace ProjectManagementSystem.Web.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Employee
    {
        public Employee()
        {
            Projects = new HashSet<Project>();
            Tasks = new HashSet<Task>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeName { get; set; }

        [Required]
        [StringLength(50)]
        public string Department { get; set; }

        [StringLength(128)]
        public string UserId { get; set; }

        public virtual ICollection<Project> Projects { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }

        public virtual AspNetUser User { get; set; }
    }
}
