using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProjectManagementSystem.Web.Models;
using ProjectManagementSystem.Web.ViewModels;

namespace ProjectManagementSystem.Web.Controllers
{
    [Authorize(Roles = "Customers")]
    public class CustomerProjectsController : Controller
    {
        private PmSyncDbContext db = new PmSyncDbContext();

        // GET: CustomerProjects
        public async Task<ActionResult> Index()
        {
            var customerUserId = User.Identity.GetUserId();

            var customer = await db.Customers
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.UserId == customerUserId);
            
            var projects = customer.Projects.Select
            (
                p => new ProjectModel
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    CustomerId = p.CustomerId,
                    EmployeeId = p.EmployeeId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Comments = p.Comments,
                    Attachment = p.Attachment,
                    CustomerName = p.Customer.CustomerName,
                    EmployeeName = p.Employee.EmployeeName
                }
            );
            
            return View(projects.ToList());
        }

        // GET: CustomerProjects/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            var model = new ProjectModel
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                CustomerId = project.CustomerId,
                EmployeeId = project.EmployeeId,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Comments = project.Comments,
                Attachment = project.Attachment,
                CustomerName = project.Customer.CustomerName,
                EmployeeName = project.Employee.EmployeeName,
                Tasks = project.Tasks.Select
                (
                    t => new TaskModel
                    {
                        Id = t.Id,
                        TaskName = t.TaskName,
                        ProjectId = t.ProjectId,
                        EmployeeId = t.EmployeeId,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        TaskStatus = t.TaskStatus,
                        Comments = t.Comments,
                        ProjectName = t.Project.ProjectName,
                        EmployeeName = t.Employee.EmployeeName
                    }
                ).ToList()
            };

            return View(model);
        }
    }
}