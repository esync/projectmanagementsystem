namespace ProjectManagementSystem.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Project> Projects { get; set; }
    }
}
