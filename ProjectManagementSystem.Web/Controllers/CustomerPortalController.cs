using Microsoft.AspNet.Identity;
using ProjectManagementSystem.Web.Models;
using ProjectManagementSystem.Web.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Collections.Generic;

namespace ProjectManagementSystem.Web.Controllers
{
    [Authorize]
    public class CustomerPortalController : Controller
    {
        private PmSyncDbContext db = new PmSyncDbContext();

        // GET: CustomerPortal/Index
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "User not logged in.");
            }

            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
            {
                ViewBag.ErrorMessage = "No customer account is associated with your user profile. Please contact support.";
                return View("Error_CustomerAccess"); // Ensure this view is created later
            }

            var projectsViewModel = await db.Projects
                .Where(p => p.CustomerId == customer.Id)
                .Include(p => p.Employee) // To get Project Manager Name
                .Select(p => new CustomerProjectViewModel
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ProjectManagerName = p.Employee.EmployeeName,
                })
                .ToListAsync();

            ViewBag.CustomerName = customer.CustomerName;
            return View(projectsViewModel);
        }
        // ProjectDetails, AddComment, ParseProjectComments, and Dispose will be added next

        // GET: CustomerPortal/ProjectDetails/5
        public async Task<ActionResult> ProjectDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Project ID is required.");
            }

            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                 return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "User not logged in.");
            }

            var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
            {
                ViewBag.ErrorMessage = "No customer account is associated with your user profile. Please contact support.";
                return View("Error_CustomerAccess"); // Ensure this view is created later
            }

            var project = await db.Projects
                .Where(p => p.Id == id.Value && p.CustomerId == customer.Id)
                .Include(p => p.Employee) // For PM Name
                .Include(p => p.Tasks.Select(t => t.Employee)) // For Task Assignee Name
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return HttpNotFound("Project not found or you do not have permission to view it.");
            }

            var projectViewModel = new CustomerProjectViewModel
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ProjectManagerName = project.Employee.EmployeeName,
                ProjectComments = project.Comments,
                Tasks = project.Tasks.Select(t => new CustomerTaskViewModel
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    AssignedEmployeeName = t.Employee != null ? t.Employee.EmployeeName : "N/A",
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    TaskStatus = t.TaskStatus,
                    Comments = t.Comments
                }).ToList(),
                // CommentsList will be populated by ParseProjectComments
                // NewComment will be initialized for the form
            };

            projectViewModel.CommentsList = ParseProjectComments(project.Comments, customer.CustomerName, project.Employee.EmployeeName);
            projectViewModel.NewComment = new AddProjectCommentViewModel { ProjectId = project.Id };

            ViewBag.CustomerName = customer.CustomerName;
            return View(projectViewModel);
        }

        // POST: CustomerPortal/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddComment(AddProjectCommentViewModel model)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "User not logged in.");
            }

            var customer = await db.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId); // Include User to get CustomerName if needed directly
            if (customer == null)
            {
                 ViewBag.ErrorMessage = "No customer account is associated with your user profile. Please contact support.";
                return View("Error_CustomerAccess");  // Ensure this view is created later
            }

            var project = await db.Projects.Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == model.ProjectId && p.CustomerId == customer.Id);
            if (project == null)
            {
                return HttpNotFound("Project not found or you do not have permission to comment on it.");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate the ViewModel for the ProjectDetails view
                var projectViewModel = new CustomerProjectViewModel
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    ProjectManagerName = project.Employee.EmployeeName,
                    ProjectComments = project.Comments,
                    Tasks = await db.Tasks.Where(t => t.ProjectId == project.Id)
                                    .Include(t => t.Employee)
                                    .Select(t => new CustomerTaskViewModel {
                                        Id = t.Id,
                                        TaskName = t.TaskName,
                                        AssignedEmployeeName = t.Employee != null ? t.Employee.EmployeeName : "N/A",
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        TaskStatus = t.TaskStatus,
                                        Comments = t.Comments
                                    }).ToListAsync(),
                    CommentsList = ParseProjectComments(project.Comments, customer.CustomerName, project.Employee.EmployeeName),
                    NewComment = model // Pass back the model with validation errors
                };
                ViewBag.CustomerName = customer.CustomerName;
                TempData["ErrorMessage"] = "Comment text cannot be empty."; // Or use ModelState errors
                return View("ProjectDetails", projectViewModel);
            }

            string newCommentEntry = $"Customer ({customer.CustomerName} - {DateTime.Now:yyyy-MM-dd HH:mm}): {model.CommentText}";

            if (string.IsNullOrWhiteSpace(project.Comments))
            {
                project.Comments = newCommentEntry;
            }
            else
            {
                project.Comments += "\n------------------------------------\n" + newCommentEntry;
            }

            db.Entry(project).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Comment added successfully!";
            return RedirectToAction("ProjectDetails", new { id = model.ProjectId });
        }

        // Helper method to parse comments string into a structured list
        private List<CustomerProjectCommentViewModel> ParseProjectComments(string commentsString, string customerName, string projectManagerName)
        {
            var parsedComments = new List<CustomerProjectCommentViewModel>();
            if (string.IsNullOrWhiteSpace(commentsString))
            {
                return parsedComments;
            }

            var entries = commentsString.Split(new[] { "\n------------------------------------\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
                string author = "System";
                DateTime date = DateTime.MinValue;
                string text = entry.Trim();

                // Example format: "Role (Name - YYYY-MM-DD HH:MM): Comment Text"
                int headerEndIndex = entry.IndexOf("): ");
                if (headerEndIndex > 0 && headerEndIndex + 3 <= entry.Length) // ensure there's text after "): "
                {
                    string header = entry.Substring(0, headerEndIndex).Trim();
                    text = entry.Substring(headerEndIndex + 3).Trim();

                    int dateStartIndex = header.LastIndexOf(" - ");
                    if (dateStartIndex > 0 && dateStartIndex + 3 < header.Length)
                    {
                        string authorPart = header.Substring(0, dateStartIndex).Trim();
                        string dateString = header.Substring(dateStartIndex + 3).Trim();

                        // Basic author extraction
                        if (authorPart.StartsWith("Customer (") && authorPart.EndsWith(")"))
                             author = customerName; // Use the actual customer name for consistency
                        else if (authorPart.StartsWith("PM (") && authorPart.EndsWith(")"))
                             author = projectManagerName; // Use the actual PM name
                        else if (authorPart.Contains(customerName)) // Fallback if formatting is slightly off
                            author = customerName;
                        else if (authorPart.Contains(projectManagerName))
                            author = projectManagerName;
                        else
                            author = authorPart; // Raw author part if no specific role identified

                        DateTime.TryParse(dateString, out date);
                    } else {
                        author = header; // If no date, header is likely just the author part
                    }
                }
                // If parsing completely fails, the whole entry is 'text', author is 'System', date is MinValue

                parsedComments.Add(new CustomerProjectCommentViewModel
                {
                    AuthorName = author,
                    CommentDate = date == DateTime.MinValue ? DateTime.Now : date, // Sensible fallback for date
                    CommentText = text
                });
            }
            return parsedComments.OrderByDescending(c => c.CommentDate).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
