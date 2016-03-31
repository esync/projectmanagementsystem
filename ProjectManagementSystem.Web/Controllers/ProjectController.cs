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
    public class ProjectController : ApiController
    {
        private PmSyncDbContext db = new PmSyncDbContext();

        // GET: api/Project
        public IQueryable<ProjectModel> GetProjects()
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

            return projects;
        }

        // GET: api/Project/5
        [ResponseType(typeof(ProjectModel))]
        public async Task<IHttpActionResult> GetProject(int id)
        {
            Project project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
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

            return Ok(model);
        }

        // PUT: api/Project/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutProject(int id, ProjectModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            Project project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.ProjectName = model.ProjectName;
            project.CustomerId = model.CustomerId;
            project.EmployeeId = model.EmployeeId;
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;
            project.Comments = model.Comments;
            project.Attachment = model.Attachment;

            db.Entry(project).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        // POST: api/Project
        [ResponseType(typeof(ProjectModel))]
        public async Task<IHttpActionResult> PostProject(ProjectModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            model.Id = project.Id;
            model.CustomerName = project.Customer.CustomerName;
            model.EmployeeName = project.Employee.EmployeeName;

            return CreatedAtRoute("DefaultApi", new { id = project.Id }, model);
        }

        // DELETE: api/Project/5
        [ResponseType(typeof(ProjectModel))]
        public async Task<IHttpActionResult> DeleteProject(int id)
        {
            Project project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
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

            db.Projects.Remove(project);
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

        private bool ProjectExists(int id)
        {
            return db.Projects.Count(e => e.Id == id) > 0;
        }
    }
}