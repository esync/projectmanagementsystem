using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using ProjectManagementSystem.Web.Models;
using ProjectManagementSystem.Web.ViewModels;

namespace ProjectManagementSystem.Web.Controllers
{
    [Authorize]
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
            Employee employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            EmployeeModel model = new EmployeeModel
            {
                Id = employee.Id,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                UserId = employee.UserId,
                UserName = employee.User.UserName,
                Email = employee.User.Email,
                PhoneNumber = employee.User.PhoneNumber
            };

            return View(model);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            var departments  = new List<SelectListItem>
            {
                new SelectListItem { Value = "PM", Text = "PM" },
                new SelectListItem { Value = "BA", Text = "BA" },
                new SelectListItem { Value = "SA", Text = "SA" },
                new SelectListItem { Value = "UI developers", Text = "UI developers" },
                new SelectListItem { Value = "DB developers", Text = "DB developers" }
            };

            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Employees", Text = "Employees" },
                new SelectListItem { Value = "Project Managers", Text = "Project Managers" }
            };

            ViewBag.Department = new SelectList(departments, "Value", "Text");
            ViewBag.Role = new SelectList(roles, "Value", "Text");

            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

                    Employee employee = new Employee
                    {
                        EmployeeName = model.EmployeeName,
                        Department = model.Department,
                        UserId = user.Id
                    };

                    db.Employees.Add(employee);
                    await db.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            var departments = new List<SelectListItem>
            {
                new SelectListItem { Value = "PM", Text = "PM" },
                new SelectListItem { Value = "BA", Text = "BA" },
                new SelectListItem { Value = "SA", Text = "SA" },
                new SelectListItem { Value = "UI developers", Text = "UI developers" },
                new SelectListItem { Value = "DB developers", Text = "DB developers" }
            };

            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Employees", Text = "Employees" },
                new SelectListItem { Value = "Project Managers", Text = "Project Managers" }
            };

            ViewBag.Department = new SelectList(departments, "Value", "Text");
            ViewBag.Role = new SelectList(roles, "Value", "Text");

            return View(model);
        }

        // GET: Employees/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            EmployeeModel model = new EmployeeModel
            {
                Id = employee.Id,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                UserId = employee.UserId
            };

            var departments = new List<SelectListItem>
            {
                new SelectListItem { Value = "PM", Text = "PM" },
                new SelectListItem { Value = "BA", Text = "BA" },
                new SelectListItem { Value = "SA", Text = "SA" },
                new SelectListItem { Value = "UI developers", Text = "UI developers" },
                new SelectListItem { Value = "DB developers", Text = "DB developers" }
            };

            ViewBag.Department = new SelectList(departments, "Value", "Text");

            return View(model);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EmployeeModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = await db.Employees.FindAsync(model.Id);

                employee.EmployeeName = model.EmployeeName;
                employee.Department = model.Department;

                db.Entry(employee).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var departments = new List<SelectListItem>
            {
                new SelectListItem { Value = "PM", Text = "PM" },
                new SelectListItem { Value = "BA", Text = "BA" },
                new SelectListItem { Value = "SA", Text = "SA" },
                new SelectListItem { Value = "UI developers", Text = "UI developers" },
                new SelectListItem { Value = "DB developers", Text = "DB developers" }
            };

            ViewBag.Department = new SelectList(departments, "Value", "Text");

            return View(model);
        }

        // GET: Employees/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            EmployeeModel model = new EmployeeModel
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
            Employee employee = await db.Employees.FindAsync(id);
            db.Employees.Remove(employee);
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
