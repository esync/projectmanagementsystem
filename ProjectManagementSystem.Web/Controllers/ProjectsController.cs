using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using ProjectManagementSystem.Web.Models;
using ProjectManagementSystem.Web.ViewModels;

namespace ProjectManagementSystem.Web.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private PmSyncDbContext db = new PmSyncDbContext();

        // GET: Projects
        public async Task<ActionResult> Index()
        {
            var projects = db.Projects.Select
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

            return View(await projects.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            ProjectModel model = new ProjectModel
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

        // GET: Projects/Create
        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "CustomerName");
            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "EmployeeName");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProjectModel model)
        {
            if (ModelState.IsValid)
            {
                Project project = new Project
                {
                    ProjectName = model.ProjectName,
                    CustomerId = model.CustomerId,
                    EmployeeId = model.EmployeeId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Comments = model.Comments,
                    Attachment = model.Attachment
                };

                db.Projects.Add(project);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "CustomerName", model.CustomerId);
            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "EmployeeName", model.EmployeeId);
            return View(model);
        }

        // GET: Projects/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            ProjectModel model = new ProjectModel
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
                EmployeeName = project.Employee.EmployeeName
            };

            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "CustomerName", model.CustomerId);
            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "EmployeeName", model.EmployeeId);
            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ProjectModel model)
        {
            if (ModelState.IsValid)
            {
                Project project = await db.Projects.FindAsync(model.Id);

                project.ProjectName = model.ProjectName;
                project.CustomerId = model.CustomerId;
                project.EmployeeId = model.EmployeeId;
                project.StartDate = model.StartDate;
                project.EndDate = model.EndDate;
                project.Comments = project.Comments;
                project.Attachment = model.Attachment;

                db.Entry(project).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "CustomerName", model.CustomerId);
            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "EmployeeName", model.EmployeeId);
            return View(model);
        }

        // GET: Projects/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            ProjectModel model = new ProjectModel
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
                EmployeeName = project.Employee.EmployeeName
            };

            return View(model);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Project project = await db.Projects.FindAsync(id);
            db.Projects.Remove(project);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
