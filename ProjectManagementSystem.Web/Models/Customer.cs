namespace ProjectManagementSystem.Web.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Customer
    {
        public Customer()
        {
            Projects = new HashSet<Project>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerName { get; set; }

        [StringLength(50)]
        public string ContactPerson { get; set; }

        [StringLength(50)]
        public string ContactPhone { get; set; }

        [StringLength(128)]
        public string UserId { get; set; }

        public virtual ICollection<Project> Projects { get; set; }

        public virtual AspNetUser User { get; set; }
    }
}
