using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class ProjectModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Customer Name")]
        public int CustomerId { get; set; }

        [Display(Name = "Employee Name")]
        public int EmployeeId { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Comments")]
        public string Comments { get; set; }

        [Display(Name = "Attachment")]
        public byte[] Attachment { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Project Manager")]
        public string EmployeeName { get; set; }

        public IList<TaskModel> Tasks { get; set; }
    }
}