using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ProjectManagementSystem.Web.Models;
using ProjectManagementSystem.Web.ViewModels;

namespace ProjectManagementSystem.Web.Controllers
{
    public class EmployeeController : ApiController
    {
        private PmSyncDbContext db = new PmSyncDbContext();

        // GET: api/Employee
        public IQueryable<EmployeeModel> GetEmployees()
        {
            var employees = db.Employees.Select
            (
                e => new EmployeeModel
                {
                    Id = e.Id,
                    EmployeeName = e.EmployeeName,
                    Department = e.Department,
                    UserId = e.UserId
                }
            );
            
            return employees;
        }

        // GET: api/Employee/5
        [ResponseType(typeof(EmployeeModel))]
        public async Task<IHttpActionResult> GetEmployee(int id)
        {
            Employee employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            EmployeeModel model = new EmployeeModel
            {
                Id = employee.Id,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                UserId = employee.UserId
            };

            return Ok(model);
        }

        // PUT: api/Employee/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEmployee(int id, EmployeeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            Employee employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.EmployeeName = model.EmployeeName;
            employee.Department = model.Department;

            db.Entry(employee).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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

        // POST: api/Employee
        [ResponseType(typeof(EmployeeModel))]
        public async Task<IHttpActionResult> PostEmployee(EmployeeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Employee employee = new Employee
            {
                EmployeeName = model.EmployeeName,
                Department = model.Department
            };

            db.Employees.Add(employee);
            await db.SaveChangesAsync();

            model.Id = employee.Id;

            return CreatedAtRoute("DefaultApi", new { id = employee.Id }, model);
        }

        // DELETE: api/Employee/5
        [ResponseType(typeof(EmployeeModel))]
        public async Task<IHttpActionResult> DeleteEmployee(int id)
        {
            Employee employee = await db.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            EmployeeModel model = new EmployeeModel
            {
                Id = employee.Id,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                UserId = employee.UserId
            };

            db.Employees.Remove(employee);
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

        private bool EmployeeExists(int id)
        {
            return db.Employees.Count(e => e.Id == id) > 0;
        }
    }
}