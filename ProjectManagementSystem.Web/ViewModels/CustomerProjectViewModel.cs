using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Web.ViewModels
{
    public class CustomerProjectViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProjectManagerName { get; set; }
        public string ProjectComments { get; set; } // To display existing comments

        public List<CustomerTaskViewModel> Tasks { get; set; }
        public List<CustomerProjectCommentViewModel> CommentsList { get; set; } // For a more structured display of comments

        public AddProjectCommentViewModel NewComment { get; set; }


        public CustomerProjectViewModel()
        {
            Tasks = new List<CustomerTaskViewModel>();
            CommentsList = new List<CustomerProjectCommentViewModel>();
            NewComment = new AddProjectCommentViewModel();
        }
    }
}
