using BugTracker.Models;
using BugTracker.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [NoDirectAccess]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelpers uh = new UserRolesHelpers();
        private ProjectsHelper ph = new ProjectsHelper();
        private TicketHelpers th = new TicketHelpers();

        // GET: Admin
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Index()
        {
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var devsList = uh.ListDevsOnMyProjects(userId);
            var projects = ph.ListUserProjects(userId).ToList();
            var admindash = new AdminDashboardVM();
            admindash.Projects = projects;
            admindash.Developers = devsList;
            return View(admindash);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Roles()
        {
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var userList = uh.ListUsersOnMyProjects(userId).OrderByDescending(u=>u.DisplayName);
            ICollection<IdentityRole> rolesList = db.Roles.OrderByDescending(r=>r.Name).ToList();
            ViewBag.Roles = new MultiSelectList(rolesList, "Id", "Name");
            ViewBag.Users = new MultiSelectList(userList, "Id", "DisplayName");
            ViewBag.ErrorMessage = "";

            return View(rolesList);
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRoles(List<string> Users, List<string> Roles, string Selection)
        {
            if (ModelState.IsValid)
            {
                if(Selection == "Assign Roles")
                {
                    foreach (var u in Users)
                    {
                        var user = db.Users.Find(u);
                        
                        if (Roles != null)
                        {
                            foreach (var id in Roles)
                            {
                                var role = db.Roles.Find(id);
                                uh.AddOrUpdateUserToRole(user.Id, role.Name);
                            }
                        }
                        else
                        {
                            uh.AddOrUpdateUserToRole(user.Id, "Submitter");
                            uh.AddOrUpdateUserToRole(db.Users.FirstOrDefault(us => us.UserName == "kyle.r.bartholomew@gmail.com").Id, "Admin");
                        }
                        db.Entry(user).State = EntityState.Modified; //EntityState.Modified says that if anything has changed, you updated all values in the table for this case.
                    }
                    db.SaveChanges();
                    return RedirectToAction("Roles", "Admin");
                }
                else if(Selection == "Unassign Roles")
                {
                    foreach (var u in Users)
                    {
                        var user = db.Users.Find(u);
                        
                        if (Roles != null)
                        {
                            foreach (var id in Roles)
                            {
                                var role = db.Roles.Find(id);
                                uh.RemoveOrUpdateUserFromRole(user.Id, role.Name);
                            }
                        }

                        if (user.Roles.Count == 0)
                        {
                            uh.AddUserToRole(user.Id, "Submitter");
                        }

                        db.Entry(user).State = EntityState.Modified; //EntityState.Modified says that if anything has changed, you updated all values in the table for this case.
                    }
                    if (!uh.IsUserInRole(db.Users.FirstOrDefault(us => us.UserName == "krbbugtrackeradmin@mailinator.com").Id, "Admin"))
                    {
                        uh.AddUserToRole(db.Users.FirstOrDefault(us => us.UserName == "krbbugtrackeradmin@mailinator.com").Id, "Admin");
                    }

                    db.SaveChanges();
                    return RedirectToAction("Roles", "Admin");
                }
                else
                {
                    var euserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                    var euserList = uh.ListUsersOnMyProjects(euserId);
                    var erolesList = db.Roles.ToList();
                    ViewBag.Roles = new MultiSelectList(erolesList, "Id", "Name");
                    ViewBag.Users = new MultiSelectList(euserList, "Id", "DisplayName");
                    ViewBag.ErrorMessage = "Something went wrong. Please try again";

                    return View("Roles");
                }
            }
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var userList = uh.ListUsersOnMyProjects(userId).OrderByDescending(u=>u.DisplayName);
            var rolesList = db.Roles.OrderByDescending(r=>r.Name).ToList();
            ViewBag.Roles = new MultiSelectList(rolesList, "Id", "Name");
            ViewBag.Users = new MultiSelectList(userList, "Id", "DisplayName");
            ViewBag.ErrorMessage = "Something went wrong. Please try again";

            return View("Roles");
        }

        
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Projects()
        {
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var usersList = uh.ListUsersOnMyProjects(userId).OrderByDescending(u=>u.DisplayName);
            ICollection<Project> projectList = db.Projects.OrderByDescending(p=>p.Name).ToList();
            ViewBag.Projects = new MultiSelectList(projectList, "Id", "Name");
            ViewBag.Users = new MultiSelectList(usersList, "Id", "DisplayName");
            ViewBag.ErrorMessage = "";

            return View(projectList);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignProjects(List<string> Users, List<string> Projects, string Selection)
        {
            if (ModelState.IsValid)
            {
                var UserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                if (Selection == "Assign to Project")
                {
                    foreach (var us in Users)
                    {
                        var user = db.Users.Find(us);
                        Project project = new Project();
                        if (Projects != null)
                        {
                            foreach (var id in Projects)
                            {
                                int intId = Convert.ToInt32(id);
                                project = db.Projects.Find(intId);
                                if (!ph.IsUserOnProject(user.Id, project.Id))
                                {
                                    user.Projects.Add(project);
                                    if (uh.IsUserInRole(user.Id, "Project Manager") && project.Name != "Unassigned")
                                    {
                                        project.PMId = user.Id;
                                        foreach(var ti in project.Tickets)
                                        {
                                            var oldPM = "";
                                            if (ti.PMId != null)
                                            {
                                                oldPM = db.Users.Find(ti.PMId).DisplayName;
                                            }                                            
                                            var newPM = db.Users.Find(user.Id).DisplayName;
                                            ti.PMId = user.Id;
                                            var history = th.CreateTicketHistory(UserId, ti.Id, "Project Manager", oldPM, newPM);
                                            db.Histories.Add(history);
                                            db.Tickets.Find(ti.Id).Histories.Add(history);
                                        }
                                    }
                                }
                            }
                        }

                        if (user.Projects.Count == 0)
                        {
                            user.Projects.Add(db.Projects.FirstOrDefault(p => p.Name == "Unassigned"));
                        }
                        db.Entry(user).State = EntityState.Modified; //EntityState.Modified says that if anything has changed, you updated all values in the table for this case.
                    }
                    db.SaveChanges();
                    return RedirectToAction("Projects", "Admin");
                }
                else if(Selection == "Unassign from Project")
                {
                    foreach (var us in Users)
                    {
                        var user = db.Users.Find(us);
                        Project project = new Project();
                        if (Projects != null)
                        {
                            foreach (var id in Projects)
                            {
                                int intId = Convert.ToInt32(id);
                                project = db.Projects.Find(intId);
                                if (ph.IsUserOnProject(user.Id, project.Id))
                                {
                                    user.Projects.Remove(project);
                                    if (uh.IsUserInRole(user.Id, "Project Manager"))
                                    {
                                        project.PMId = null;
                                        foreach (var ti in project.Tickets)
                                        {
                                            if (ti.PMId != null)
                                            {
                                                var oldPM = db.Users.Find(ti.PMId).DisplayName;
                                                var newPM = "";
                                                ti.PMId = null;
                                                var history = th.CreateTicketHistory(UserId, ti.Id, "Project Manager", oldPM, newPM);
                                                db.Histories.Add(history);
                                                db.Tickets.Find(ti.Id).Histories.Add(history);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (user.Projects.Count == 0)
                        {
                            user.Projects.Add(db.Projects.FirstOrDefault(p => p.Name == "Unassigned"));
                        }
                        db.Entry(user).State = EntityState.Modified; //EntityState.Modified says that if anything has changed, you updated all values in the table for this case.
                    }
                    db.SaveChanges();
                    return RedirectToAction("Projects", "Admin");
                }
                else
                {
                    var euserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                    var eusersList = uh.ListUsersOnMyProjects(euserId).OrderByDescending(u=>u.DisplayName);
                    var eprojectList = db.Projects.OrderByDescending(p=>p.Name).ToList();
                    ViewBag.Projects = new MultiSelectList(eprojectList, "Id", "Name", Projects);
                    ViewBag.Users = new MultiSelectList(eusersList, "Id", "DisplayName", Users);
                    ViewBag.ErrorMessage = "Something went wrong. Please try again";

                    return View("Projects", eprojectList);
                }
            }
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var usersList = uh.ListUsersOnMyProjects(userId).OrderByDescending(u=>u.DisplayName);
            var projectList = db.Projects.OrderByDescending(p=>p.Name).ToList();
            ViewBag.Projects = new MultiSelectList(projectList, "Id", "Name", Projects);
            ViewBag.Users = new MultiSelectList(usersList, "Id", "DisplayName", Users);
            ViewBag.ErrorMessage = "Something went wrong. Please try again";

            return View("Projects", projectList);
        }
    }
}