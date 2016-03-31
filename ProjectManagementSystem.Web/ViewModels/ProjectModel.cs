using System;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class ProjectModel
    {
        public int Id { get; set; }

        public string ProjectName { get; set; }

        public int CustomerId { get; set; }

        public int EmployeeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Comments { get; set; }

        public byte[] Attachment { get; set; }

        public string CustomerName { get; set; }

        public string EmployeeName { get; set; }
    }
}