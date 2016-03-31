using System;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class TaskModel
    {
        public int Id { get; set; }

        public string TaskName { get; set; }

        public int ProjectId { get; set; }

        public int EmployeeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string TaskStatus { get; set; }

        public string Comments { get; set; }

        public string ProjectName { get; set; }

        public string EmployeeName { get; set; }
    }
}