using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class AddProjectCommentViewModel
    {
        public int ProjectId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string CommentText { get; set; }
    }
}
