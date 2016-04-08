using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Task Name")]
        public string TaskName { get; set; }

        [Display(Name = "Project Name")]
        public int ProjectId { get; set; }

        [Display(Name = "Employee Name")]
        public int EmployeeId { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Task Status")]
        public string TaskStatus { get; set; }

        [Display(Name = "Comments")]
        public string Comments { get; set; }

        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }
    }
}