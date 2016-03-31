using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity.EntityFramework;
using ProjectManagementSystem.Web.Models;
using ProjectManagementSystem.Web.ViewModels;

namespace ProjectManagementSystem.Web.Controllers
{
    public class CustomerController : ApiController
    {
        private PmSyncDbContext db = new PmSyncDbContext();
        private ApplicationDbContext userDb = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public CustomerController()
        {
        }

        public CustomerController(ApplicationUserManager userManager)
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

        // GET: api/Customer
        public IQueryable<CustomerModel> GetCustomers()
        {
            var customers = db.Customers.Select
            (
                c => new CustomerModel
                {
                    Id = c.Id,
                    CustomerName = c.CustomerName,
                    ContactPerson = c.ContactPerson,
                    ContactPhone = c.ContactPhone,
                    UserId = c.UserId
                }
            );

            return customers;
        }

        // GET: api/Customer/5
        [ResponseType(typeof(CustomerModel))]
        public async Task<IHttpActionResult> GetCustomer(int id)
        {
            Customer customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            CustomerModel model = new CustomerModel
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                ContactPerson = customer.ContactPerson,
                ContactPhone = customer.ContactPhone,
                UserId = customer.UserId
            };

            return Ok(model);
        }

        // PUT: api/Customer/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutCustomer(int id, CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            Customer customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            customer.CustomerName = model.CustomerName;
            customer.ContactPerson = model.ContactPerson;
            customer.ContactPhone = model.ContactPhone;

            db.Entry(customer).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Customer
        [ResponseType(typeof(CustomerModel))]
        public async Task<IHttpActionResult> PostCustomer(CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, PhoneNumber = model.ContactPhone };

            var result = await UserManager.CreateAsync(user);

            if (result.Succeeded)
            {
                Customer customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    ContactPerson = model.ContactPerson,
                    ContactPhone = model.ContactPhone,
                    UserId = user.Id
                };

                db.Customers.Add(customer);
                await db.SaveChangesAsync();

                model.Id = customer.Id;
                model.UserId = user.Id;

                return CreatedAtRoute("DefaultApi", new { id = customer.Id }, model);
            }
            else
            {
                return BadRequest(ModelState);
            }
            
        }

        // DELETE: api/Customer/5
        [ResponseType(typeof(CustomerModel))]
        public async Task<IHttpActionResult> DeleteCustomer(int id)
        {
            Customer customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            CustomerModel model = new CustomerModel
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                ContactPerson = customer.ContactPerson,
                ContactPhone = customer.ContactPhone,
                UserId = customer.UserId
            };

            db.Customers.Remove(customer);
            await db.SaveChangesAsync();

            return Ok(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CustomerExists(int id)
        {
            return db.Customers.Count(e => e.Id == id) > 0;
        }
    }
}