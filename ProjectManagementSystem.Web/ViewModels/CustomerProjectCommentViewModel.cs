using System;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class CustomerProjectCommentViewModel
    {
        public string AuthorName { get; set; } // "Customer" or "Project Manager"
        public DateTime CommentDate { get; set; }
        public string CommentText { get; set; }
    }
}
