using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class CustomerTaskViewModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string AssignedEmployeeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TaskStatus { get; set; }
        public string Comments { get; set; } // Comments specific to the task
    }
}
