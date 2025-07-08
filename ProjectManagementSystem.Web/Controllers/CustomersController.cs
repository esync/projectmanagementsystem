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
using System.Collections.Generic; // Required for List<T>

namespace ProjectManagementSystem.Web.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class CustomersController : Controller
    {
        private PmSyncDbContext db = new PmSyncDbContext();
        private ApplicationDbContext userDb = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public CustomersController()
        {
        }

        public CustomersController(ApplicationUserManager userManager)
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

        // GET: Customers
        public async Task<ActionResult> Index()
        {
            var customers = db.Customers.Select
            (
                c => new CustomerModel
                {
                    Id = c.Id,
                    CustomerName = c.CustomerName,
                    ContactPerson = c.ContactPerson,
                    ContactPhone = c.ContactPhone,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    Email = c.User.Email
                }
            );

            return View(await customers.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Include Projects and their Tasks when fetching Customer details
            var customer = await db.Customers
                                   .Include(c => c.Projects.Select(p => p.Tasks))
                                   .SingleOrDefaultAsync(c => c.Id == id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            var model = new CustomerModel
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                ContactPerson = customer.ContactPerson,
                ContactPhone = customer.ContactPhone,
                UserId = customer.UserId,
                UserName = customer.User?.UserName, // Use null conditional for safety
                Email = customer.User?.Email,      // Use null conditional for safety
                Projects = customer.Projects.Select(p => new ProjectModel // Populate projects
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    // other project properties as needed for display
                    Tasks = p.Tasks.Select(t => new TaskModel
                    {
                        Id = t.Id,
                        TaskName = t.TaskName
                        // other task properties
                    }).ToList()
                }).ToList()
            };
            return View(model);
        }

        // GET: Customers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.ContactPhone };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Customers");

                    var customer = new Customer
                    {
                        CustomerName = model.CustomerName,
                        ContactPerson = model.ContactPerson,
                        ContactPhone = model.ContactPhone,
                        UserId = user.Id
                    };

                    db.Customers.Add(customer);
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

            return View(model);
        }

        // GET: Customers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            var model = new CustomerModel
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                ContactPerson = customer.ContactPerson,
                ContactPhone = customer.ContactPhone,
                UserId = customer.UserId
            };
            return View(model);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = await db.Customers.FindAsync(model.Id);
                if (customer == null)
                {
                    return HttpNotFound();
                }

                customer.CustomerName = model.CustomerName;
                customer.ContactPerson = model.ContactPerson;
                customer.ContactPhone = model.ContactPhone;

                db.Entry(customer).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Customers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            var model = new CustomerModel
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                ContactPerson = customer.ContactPerson,
                ContactPhone = customer.ContactPhone,
                UserId = customer.UserId
            };
            return View(model);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            string userIdToDelete = customer.UserId;

            // Delete associated ApplicationUser first (if exists)
            if (!string.IsNullOrEmpty(userIdToDelete))
            {
                var user = await UserManager.FindByIdAsync(userIdToDelete);
                if (user != null)
                {
                    var userResult = await UserManager.DeleteAsync(user);
                    if (!userResult.Succeeded)
                    {
                        // Log error, add to ModelState, or handle the failure
                        foreach (var error in userResult.Errors)
                        {
                            ModelState.AddModelError("", "Error deleting associated user account: " + error);
                        }
                        // Optionally, prevent customer deletion if user deletion fails critically
                        // For this example, we'll allow customer deletion to proceed but show an error.
                        TempData["ErrorMessage"] = "Failed to delete associated user account. Customer record was still deleted.";
                        // return View(customer); // Or redirect with error
                    }
                }
            }

            // Retrieve and remove associated projects and their tasks
            var projectsToDelete = db.Projects.Where(p => p.CustomerId == id).ToList();
            if (projectsToDelete.Any())
            {
                foreach (var project in projectsToDelete)
                {
                    var tasksForThisProject = db.Tasks.Where(t => t.ProjectId == project.Id).ToList();
                    if (tasksForThisProject.Any())
                    {
                        db.Tasks.RemoveRange(tasksForThisProject);
                    }
                }
                db.Projects.RemoveRange(projectsToDelete);
            }

            // Remove the customer
            db.Customers.Remove(customer);

            // Save all changes to the database
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
