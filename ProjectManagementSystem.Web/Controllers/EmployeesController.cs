using System.Collections.Generic;
using System.Data.Entity;
using System.Linq; // Ensure System.Linq is imported
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using ProjectManagementSystem.Web.Models;
using ProjectManagementSystem.Web.ViewModels;

namespace ProjectManagementSystem.Web.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class EmployeesController : Controller
    {
        private PmSyncDbContext db = new PmSyncDbContext();
        private ApplicationDbContext userDb = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public EmployeesController()
        {
        }

        public EmployeesController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? new ApplicationUserManager(new UserStore<ApplicationUser>(userDb));
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Employees
        public async Task<ActionResult> Index()
        {
            var employees = db.Employees.Select
            (
                e => new EmployeeModel
                {
                    Id = e.Id,
                    EmployeeName = e.EmployeeName,
                    Department = e.Department,
                    UserId = e.UserId,
                    UserName = e.User.UserName,
                    Email = e.User.Email,
                    PhoneNumber = e.User.PhoneNumber
                }
            );

            return View(await employees.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Include related data for details view
            var employee = await db.Employees
                                   .Include(e => e.Projects.Select(p => p.Tasks)) // Projects managed by employee
                                   .Include(e => e.Tasks) // Tasks directly assigned to employee
                                   .SingleOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            var model = new EmployeeModel
            {
                Id = employee.Id,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                UserId = employee.UserId,
                UserName = employee.User?.UserName, // Null conditional for safety
                Email = employee.User?.Email,       // Null conditional for safety
                PhoneNumber = employee.User?.PhoneNumber, // Null conditional for safety
                Projects = employee.Projects.Select(p => new ProjectModel { Id = p.Id, ProjectName = p.ProjectName }).ToList(),
                Tasks = employee.Tasks.Select(t => new TaskModel { Id = t.Id, TaskName = t.TaskName }).ToList()
            };

            return View(model);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            var departments  = new List<SelectListItem>
            {
                new SelectListItem { Value = "Project Management", Text = "Project Management" },
                new SelectListItem { Value = "Business Analysis", Text = "Business Analysis" },
                new SelectListItem { Value = "System Analysis", Text = "System Analysis" },
                new SelectListItem { Value = "UI Development", Text = "UI Development" },
                new SelectListItem { Value = "DB Development", Text = "DB Development" }
            };

            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Project Manager", Text = "Project Manager" },
                new SelectListItem { Value = "Business Analyst", Text = "Business Analyst" },
                new SelectListItem { Value = "System Analyst", Text = "System Analyst" },
                new SelectListItem { Value = "UI Developer", Text = "UI Developer" },
                new SelectListItem { Value = "DB Developer", Text = "DB Developer" }

            };

            ViewBag.Department = new SelectList(departments, "Value", "Text");
            ViewBag.Role = new SelectList(roles, "Value", "Text");

            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EmployeeModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, model.Role);
                    var employee = new Employee
                    {
                        EmployeeName = model.EmployeeName,
                        Department = model.Department,
                        UserId = user.Id
                    };
                    db.Employees.Add(employee);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            var departmentsList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Project Management", Text = "Project Management" },
                new SelectListItem { Value = "Business Analysis", Text = "Business Analysis" },
                new SelectListItem { Value = "System Analysis", Text = "System Analysis" },
                new SelectListItem { Value = "UI Development", Text = "UI Development" },
                new SelectListItem { Value = "DB Development", Text = "DB Development" }
            };
            var rolesList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Project Manager", Text = "Project Manager" },
                new SelectListItem { Value = "Business Analyst", Text = "Business Analyst" },
                new SelectListItem { Value = "System Analyst", Text = "System Analyst" },
                new SelectListItem { Value = "UI Developer", Text = "UI Developer" },
                new SelectListItem { Value = "DB Developer", Text = "DB Developer" }
            };
            ViewBag.Department = new SelectList(departmentsList, "Value", "Text", model.Department);
            ViewBag.Role = new SelectList(rolesList, "Value", "Text", model.Role);
            return View(model);
        }

        // GET: Employees/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            var model = new EmployeeModel
            {
                Id = employee.Id,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                UserId = employee.UserId
            };
            var departments = new List<SelectListItem>
            {
                new SelectListItem { Value = "Project Management", Text = "Project Management" },
                new SelectListItem { Value = "Business Analysis", Text = "Business Analysis" },
                new SelectListItem { Value = "System Analysis", Text = "System Analysis" },
                new SelectListItem { Value = "UI Development", Text = "UI Development" },
                new SelectListItem { Value = "DB Development", Text = "DB Development" }
            };
            ViewBag.Department = new SelectList(departments, "Value", "Text", model.Department);
            return View(model);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EmployeeModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = await db.Employees.FindAsync(model.Id);
                if (employee == null)
                {
                    return HttpNotFound();
                }
                employee.EmployeeName = model.EmployeeName;
                employee.Department = model.Department;
                db.Entry(employee).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var departments = new List<SelectListItem>
            {
                new SelectListItem { Value = "Project Management", Text = "Project Management" },
                new SelectListItem { Value = "Business Analysis", Text = "Business Analysis" },
                new SelectListItem { Value = "System Analysis", Text = "System Analysis" },
                new SelectListItem { Value = "UI Development", Text = "UI Development" },
                new SelectListItem { Value = "DB Development", Text = "DB Development" }
            };
            ViewBag.Department = new SelectList(departments, "Value", "Text", model.Department);
            return View(model);
        }

        // GET: Employees/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            var model = new EmployeeModel
            {
                Id = employee.Id,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                UserId = employee.UserId
            };
            return View(model);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            string userIdToDelete = employee.UserId;

            // Delete associated ApplicationUser first
            if (!string.IsNullOrEmpty(userIdToDelete))
            {
                var user = await UserManager.FindByIdAsync(userIdToDelete);
                if (user != null)
                {
                    var userResult = await UserManager.DeleteAsync(user);
                    if (!userResult.Succeeded)
                    {
                        foreach (var error in userResult.Errors)
                        {
                            ModelState.AddModelError("", "Error deleting associated user account: " + error);
                        }
                        // Decide if you want to stop the whole delete process if user deletion fails
                        // For now, we'll assume we want to proceed with employee deletion if possible,
                        // but this might leave an orphaned employee record if user deletion is critical.
                        // Or, if employee deletion fails due to FK, this error will be important.
                        TempData["ErrorMessage"] = "Failed to delete associated user account. Employee record deletion may also fail or be incomplete.";
                        // return View(employeeViewModel); // Re-display with error, need to fetch view model
                        // return RedirectToAction("Delete", new { id = id, error = "UserDeletionFailed" }); // Or redirect
                    }
                }
            }

            // Retrieve and remove projects managed by this employee and their tasks
            var projectsManaged = db.Projects.Where(p => p.EmployeeId == id).ToList();
            if (projectsManaged.Any())
            {
                foreach (var project in projectsManaged)
                {
                    var tasksForThisProject = db.Tasks.Where(t => t.ProjectId == project.Id).ToList();
                    if (tasksForThisProject.Any())
                    {
                        db.Tasks.RemoveRange(tasksForThisProject);
                    }
                }
                db.Projects.RemoveRange(projectsManaged);
            }

            // Retrieve and remove tasks directly assigned to this employee
            var tasksAssigned = db.Tasks.Where(t => t.EmployeeId == id).ToList();
            if (tasksAssigned.Any())
            {
                db.Tasks.RemoveRange(tasksAssigned);
            }

            // Remove the employee
            db.Employees.Remove(employee);

            // Save all changes
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
                if (userDb != null)
                {
                    userDb.Dispose();
                    userDb = null;
                }
                if (db != null)
                {
                   db.Dispose();
                   db = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
