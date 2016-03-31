using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ProjectManagementSystem.Web.Models;
using ProjectManagementSystem.Web.ViewModels;

namespace ProjectManagementSystem.Web.Controllers
{
    public class TaskController : ApiController
    {
        private PmSyncDbContext db = new PmSyncDbContext();

        // GET: api/Task
        public IQueryable<TaskModel> GetTasks()
        {
            var tasks = db.Tasks.Select
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

            return tasks;
        }

        // GET: api/Task/5
        [ResponseType(typeof(TaskModel))]
        public async Task<IHttpActionResult> GetTask(int id)
        {
            Models.Task task = await db.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            TaskModel model = new TaskModel
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

            return Ok(model);
        }

        // PUT: api/Task/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTask(int id, TaskModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            Models.Task task = await db.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.TaskName = model.TaskName;
            task.ProjectId = model.ProjectId;
            task.EmployeeId = model.EmployeeId;
            task.StartDate = model.StartDate;
            task.EndDate = model.EndDate;
            task.TaskStatus = model.TaskStatus;
            task.Comments = model.Comments;

            db.Entry(task).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
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

        // POST: api/Task
        [ResponseType(typeof(TaskModel))]
        public async Task<IHttpActionResult> PostTask(TaskModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Models.Task task = new Models.Task
            {
                TaskName = model.TaskName,
                ProjectId = model.ProjectId,
                EmployeeId = model.EmployeeId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TaskStatus = model.TaskStatus,
                Comments = model.Comments
            };

            db.Tasks.Add(task);
            await db.SaveChangesAsync();

            model.Id = task.Id;
            model.ProjectName = task.Project.ProjectName;
            model.EmployeeName = task.Employee.EmployeeName;

            return CreatedAtRoute("DefaultApi", new { id = task.Id }, model);
        }

        // DELETE: api/Task/5
        [ResponseType(typeof(TaskModel))]
        public async Task<IHttpActionResult> DeleteTask(int id)
        {
            Models.Task task = await db.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            TaskModel model = new TaskModel
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

            db.Tasks.Remove(task);
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

        private bool TaskExists(int id)
        {
            return db.Tasks.Count(e => e.Id == id) > 0;
        }
    }
}