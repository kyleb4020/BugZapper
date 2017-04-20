using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Helpers;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [NoDirectAccess]
    [Authorize]
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProjectsHelper ph = new ProjectsHelper();
        private UserRolesHelpers uh = new UserRolesHelpers();
        // GET: Projects
        public ActionResult Index()
        {
            var user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var projects = db.Projects.OrderByDescending(p=>p.Name).ToList();
            return View(projects);
        }
        // GET: Edit Projects
        public ActionResult IndexEdit()
        {
            var user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var projects = db.Projects.OrderByDescending(p => p.Name).ToList();
            return View(projects);
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: Projects/Create
        public ActionResult Create()
        {
            var projectM = uh.ListUsersInRole("Project Manager").OrderByDescending(p => p.DisplayName).ToList();
            ViewBag.PMId = new SelectList(projectM, "Id", "DisplayName");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,PMId")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Created = DateTimeOffset.Now;
                project.Users.Add(db.Users.Find(project.PMId));
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            var projectM = uh.ListUsersInRole("Project Manager").OrderByDescending(p => p.DisplayName).ToList();
            ViewBag.PMId = new SelectList(projectM, "Id", "DisplayName",project.PMId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,PMId")] Project project)
        {
            if (ModelState.IsValid)
            {
                var Project = db.Projects.Find(project.Id);
                Project.Name = project.Name;
                Project.Description = project.Description;
                var oldPMId = Project.PMId;
                Project.PMId = project.PMId;
                if(oldPMId != null)
                {
                    Project.Users.Remove(db.Users.Find(oldPMId));
                }
                if(Project.PMId != null)
                {
                    Project.Users.Add(db.Users.Find(Project.PMId));
                }
                db.Entry(Project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("IndexEdit");
            }
            return View(project);
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
