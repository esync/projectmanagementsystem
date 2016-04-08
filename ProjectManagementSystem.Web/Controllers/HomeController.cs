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

            if (!roleManager.RoleExists("Project Manager"))
                roleManager.Create(new IdentityRole("Project Manager"));

            if (!roleManager.RoleExists("Business Analyst"))
                roleManager.Create(new IdentityRole("Business Analyst"));

            if (!roleManager.RoleExists("System Analyst"))
                roleManager.Create(new IdentityRole("System Analyst"));

            if (!roleManager.RoleExists("UI Developer"))
                roleManager.Create(new IdentityRole("UI Developer"));

            if (!roleManager.RoleExists("DB Developer"))
                roleManager.Create(new IdentityRole("DB Developer"));

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