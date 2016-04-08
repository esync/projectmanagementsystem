using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ProjectManagementSystem.Web.Models;

namespace ProjectManagementSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext userDb = new ApplicationDbContext();
        
        public ActionResult Index()
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userDb));

            if (!roleManager.RoleExists("Administrators"))
                roleManager.Create(new IdentityRole("Administrators"));

            if (!roleManager.RoleExists("Project Managers"))
                roleManager.Create(new IdentityRole("Project Managers"));

            if (!roleManager.RoleExists("Employees"))
                roleManager.Create(new IdentityRole("Employees"));

            if (!roleManager.RoleExists("Customers"))
                roleManager.Create(new IdentityRole("Customers"));

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}