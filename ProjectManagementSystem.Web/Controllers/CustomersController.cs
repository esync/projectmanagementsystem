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
            Customer customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            CustomerModel model = new CustomerModel
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                ContactPerson = customer.ContactPerson,
                ContactPhone = customer.ContactPhone,
                UserId = customer.UserId,
                UserName = customer.User.UserName,
                Email = customer.User.Email
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

                    Customer customer = new Customer
                    {
                        CustomerName = model.CustomerName,
                        ContactPerson = model.ContactPerson,
                        ContactPhone = model.ContactPhone,
                        UserId = user.Id
                    };

                    db.Customers.Add(customer);
                    await db.SaveChangesAsync();
                }

                return RedirectToAction("Index");
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
            Customer customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            CustomerModel model = new CustomerModel
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
                Customer customer = await db.Customers.FindAsync(model.Id);

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
            Customer customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            CustomerModel model = new CustomerModel
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
            Customer customer = await db.Customers.FindAsync(id);
            db.Customers.Remove(customer);
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
