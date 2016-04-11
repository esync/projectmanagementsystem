using System.Collections.Generic;
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
    [Authorize]
    public class EmployeeTasksController : Controller
    {
        private PmSyncDbContext db = new PmSyncDbContext();

        // GET: EmployeeTasks
        public async Task<ActionResult> Index()
        {
            var employeeUserId = User.Identity.GetUserId();

            var employee = await db.Employees
                .Include(e => e.Tasks)
                .FirstOrDefaultAsync(e => e.UserId == employeeUserId);

            var tasks = employee.Tasks.Select
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
            );

            return View(tasks.ToList());
        }

        // GET: EmployeeTasks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var task = await db.Tasks.FindAsync(id);
            if (task == null)
            {
                return HttpNotFound();
            }

            var model = new TaskModel
            {
                Id = task.Id,
                TaskName = task.TaskName,
                ProjectId = task.ProjectId,
                EmployeeId = task.EmployeeId,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                TaskStatus = task.TaskStatus,
                Comments = task.Comments,
                ProjectName = task.Project.ProjectName,
                EmployeeName = task.Employee.EmployeeName
            };

            return View(model);
        }

        // GET: EmployeeTasks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var task = await db.Tasks.FindAsync(id);
            if (task == null)
            {
                return HttpNotFound();
            }

            var model = new TaskModel
            {
                Id = task.Id,
                TaskName = task.TaskName,
                ProjectId = task.ProjectId,
                EmployeeId = task.EmployeeId,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                TaskStatus = task.TaskStatus,
                Comments = task.Comments,
                ProjectName = task.Project.ProjectName,
                EmployeeName = task.Employee.EmployeeName
            };

            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "EmployeeName", model.EmployeeId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "ProjectName", model.ProjectId);

            var taskStatus = new List<SelectListItem>
            {
                new SelectListItem { Value = "Assigned", Text = "Assigned" },
                new SelectListItem { Value = "Started", Text = "Started" },
                new SelectListItem { Value = "On Hold", Text = "On Hold" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Dropped", Text = "Dropped" },
                new SelectListItem { Value = "Not Started", Text = "Not Started" }
            };

            ViewBag.TaskStatus = new SelectList(taskStatus, "Value", "Text", model.TaskStatus);

            return View(model);
        }

        // POST: EmployeeTasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TaskModel model)
        {
            if (ModelState.IsValid)
            {
                var task = await db.Tasks.FindAsync(model.Id);

                task.TaskName = model.TaskName;
                task.ProjectId = model.ProjectId;
                task.EmployeeId = model.EmployeeId;
                task.StartDate = model.StartDate;
                task.EndDate = model.EndDate;
                task.TaskStatus = model.TaskStatus;
                task.Comments = model.Comments;

                db.Entry(task).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "EmployeeName", model.EmployeeId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "ProjectName", model.ProjectId);

            var taskStatus = new List<SelectListItem>
            {
                new SelectListItem { Value = "Assigned", Text = "Assigned" },
                new SelectListItem { Value = "Started", Text = "Started" },
                new SelectListItem { Value = "On Hold", Text = "On Hold" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Dropped", Text = "Dropped" },
                new SelectListItem { Value = "Not Started", Text = "Not Started" }
            };

            ViewBag.TaskStatus = new SelectList(taskStatus, "Value", "Text", model.TaskStatus);

            return View(model);
        }
    }
}