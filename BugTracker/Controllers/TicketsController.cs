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
using Microsoft.AspNet.Identity;
using System.IO;
using System.Threading.Tasks;
using DevTrends.MvcDonutCaching;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [NoDirectAccess]
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelpers uh = new UserRolesHelpers();
        private TicketHelpers th = new TicketHelpers();
        private ProjectsHelper ph = new ProjectsHelper();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        //private UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>
        //            (new UserStore<ApplicationUser>(new ApplicationDbContext()));

        // GET: Tickets
        public ActionResult Index()
        {
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var ticketVM = new TicketIndexVM();
            //User tickets for developers assigned tickets and submitters owned tickets
            var tickets = new List<Ticket>();
            if (User.IsInRole("Developer"))
            {
                foreach (var ti in th.ListUserAssignedTickets(userId))
                {
                    tickets.Add(ti);
                }
            }
            if (User.IsInRole("Submitter"))
            {
                foreach (var ti in th.ListUserOwnedTickets(userId))
                {
                    tickets.Add(ti);
                }
            }
            //Use projects for the list of tickets in projects assigned
            var projects = new List<Project>();
            if (User.IsInRole("Admin"))
            {
                projects = db.Projects.ToList();
            }
            else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
            {
                projects = ph.ListUserProjects(userId).ToList();
            }
            else if (User.IsInRole("Submitter"))
            {
                projects = db.Projects.ToList();
            }
            //Use users to help display the right stuff. This is to make sense of the PMId, DeveloperIds, and OwnerId
            //var users = new List<ApplicationUser>();
            //var users = db.Users.ToList();
            //foreach(var us in allUsers)
            //{
            //    //Pulls out all of the users attached to the list of projects
            //    if (projects.All(p => p.Tickets.All(t => t.Users.Contains(us))))
            //    {
            //        users.Add(us);
            //    }
            //}
            //var DevList = uh.ListDevsOnMyProjects(userId);
            //var myTickets = th.ListUserTicketsInProjects(userId);
            ticketVM.Tickets = tickets.OrderByDescending(t => t.Created).ToList();
            ticketVM.Project = projects.OrderByDescending(p => p.Name).ToList();
            //ticketVM.Users = users;
            //ViewBag.Developers = new SelectList(DevList, "Id", "DisplayName");
            //ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title");
            //ViewBag.UnDevelopers = new SelectList(DevList, "Id", "DisplayName");
            //ViewBag.UnTickets = new MultiSelectList(myTickets, "Id", "Title");
            //ViewBag.AssignedTickets = th.ListUserAssignedTickets(userId).ToList();
            return View(ticketVM);
        }
        [Authorize(Roles = "Admin, Project Manager, Developer")]
        //GET: EditIndex
        public ActionResult EditIndex()
        {
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var ticketVM = new TicketIndexVM();
            //User tickets for developers assigned tickets and submitters owned tickets
            var tickets = new List<Ticket>();
            if (User.IsInRole("Developer"))
            {
                foreach (var ti in th.ListUserAssignedTickets(userId))
                {
                    tickets.Add(ti);
                }
            }
            if (User.IsInRole("Submitter"))
            {
                foreach (var ti in th.ListUserOwnedTickets(userId))
                {
                    tickets.Add(ti);
                }
            }
            //Use projects for the list of tickets in projects assigned
            var projects = new List<Project>();
            if (User.IsInRole("Admin"))
            {
                projects = db.Projects.OrderByDescending(p => p.Name).ToList();
            }
            else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
            {
                projects = ph.ListUserProjects(userId).OrderByDescending(p => p.Name).ToList();
            }
            else if (User.IsInRole("Submitter"))
            {
                projects = db.Projects.OrderByDescending(p => p.Name).ToList();
            }
            //Use users to help display the right stuff. This is to make sense of the PMId, DeveloperIds, and OwnerId
            //var users = new List<ApplicationUser>();
            //var users = db.Users.ToList();
            //foreach(var us in allUsers)
            //{
            //    //Pulls out all of the users attached to the list of projects
            //    if (projects.All(p => p.Tickets.All(t => t.Users.Contains(us))))
            //    {
            //        users.Add(us);
            //    }
            //}
            //var userList = uh.ListUsersOnMyProjects(userId);
            var myTickets = th.ListUserTicketsInProjects(userId).OrderByDescending(t => t.Created).ToList();
            ticketVM.Tickets = tickets.OrderByDescending(t => t.Created).ToList();
            ticketVM.Project = projects.OrderByDescending(p => p.Name).ToList();
            //ticketVM.Users = users;
            var DevList = uh.ListDevsOnMyProjects(userId).OrderByDescending(d => d.DisplayName).ToList();
            ViewBag.Developers = new SelectList(DevList, "Id", "DisplayName");
            ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.Tickets2 = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.Projects = new SelectList(projects, "Id", "Name");
            //ViewBag.UnProjects = new SelectList(projects, "Id", "Name");
            //ViewBag.UnTickets = new MultiSelectList(myTickets, "Id", "Title");
            //ViewBag.AssignedTickets = th.ListUserAssignedTickets(userId).ToList();
            return View(ticketVM);
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            //if (uh.IsUserInRole(userId, "Admin") ||
            //    (uh.IsUserInRole(userId, "Project Manager") && userId == ticket.PMId) ||
            //    (uh.IsUserInRole(userId, "Developer") && userId == ticket.DeveloperId) ||
            //    (uh.IsUserInRole(userId, "Submitter") && userId == ticket.OwnerUserId))
            //{
            //var users = db.Users.ToList();
            TicketDetailsVM TicketVM = new TicketDetailsVM();
            TicketVM.Ticket = ticket;
            //TicketVM.Users = users;
            ViewBag.Error = "";
            return View(TicketVM);
            //}
            //return View("Error");
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter, Admin")]
        public ActionResult Create()
        {
            //ViewBag.PriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            //ViewBag.TypeId = new SelectList(db.TicketTypes, "Id", "Name");
            //ViewBag.StatusId = new SelectList(db.TicketStatus, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Submitter, Admin")]
        public ActionResult Create([Bind(Include = "Title,Description")] Ticket ticket, string Submit, List<string> AttachmentDescription, IEnumerable<HttpPostedFileBase> upload)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                ticket.OwnerUserId = user.Id;
                //ticket.Users.Add(db.Users.FirstOrDefault(p => p.Roles.Any(r => r.RoleId == db.Roles.FirstOrDefault(role => role.Name == "Admin").Id))); //Add the Admin to the ticket as a User
                ticket.Project = db.Projects.FirstOrDefault(p => p.Name == "Unassigned");
                //Add the Project Manager of the Project to the ticket
                foreach (var us in ticket.Project.Users)
                {
                    if (!ticket.Users.Contains(us) && uh.IsUserInRole(us.Id, "Project Manager"))
                    {
                        ticket.Users.Add(us);
                    }
                }
                ticket.Created = DateTimeOffset.Now;
                ticket.Priority = db.TicketPriorities.FirstOrDefault(p => p.Name == "No Priority");
                ticket.Status = db.TicketStatus.FirstOrDefault(p => p.Name == "Unassigned");
                //This creates a new list of Ticket Types and then populates it with a single Type object "Default"
                //var TypeList = new List<TicketType>();
                //int TypeNum = db.TicketTypes.FirstOrDefault(p => p.Name == "Default").Id;
                //var DefaultType = db.TicketTypes.Find(TypeNum);
                //TypeList.Add(DefaultType);
                //ticket.Type = TypeList;
                //Below does the same thing as above
                ticket.Type.Add(db.TicketTypes.FirstOrDefault(p => p.Name == "Default"));


                db.Tickets.Add(ticket);
                db.SaveChanges();
                //Now, to handle the attachments//
                int num = 0;
                if (upload != null)
                {
                    foreach (var att in upload)
                    {
                        Attachments attachments = new Attachments();
                        if (ImageUploadValidator.IsWebFriendlyImage(att))
                        {
                            var fileName = Path.GetFileName(att.FileName);
                            att.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                            attachments.FileUrl = "/Uploads/" + fileName;
                        }
                        else if (FileUploadValidator.IsFileAcceptableFormat(att))
                        {
                            var fileName = Path.GetFileName(att.FileName);
                            att.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                            attachments.FileUrl = "/Uploads/" + fileName;
                        }
                        attachments.Description = AttachmentDescription[num];
                        attachments.Created = DateTimeOffset.Now;
                        attachments.UserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                        attachments.TicketId = db.Tickets.Find(ticket.Id).Id;
                        db.Attachments.Add(attachments);
                        db.Tickets.Find(ticket.Id).Attachments.Add(attachments);
                        num++;
                    }
                }
                db.SaveChanges();
                //if(Submit == "Add Attachment")
                //{
                //    return RedirectToAction("CreateAttachment", new { id = db.Tickets.Find(ticket.Id).Id });
                //}
                //else if(Submit == "Create")
                //{
                return RedirectToAction("Index", "Tickets");
                //}
            }
            //ViewBag.PriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.PriorityId);
            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            //ViewBag.StatusId = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
            return View(ticket);
        }

        // GET: CreateAttachment
        //public ActionResult CreateAttachment(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket ticket = db.Tickets.Find(id);
        //    if (ticket == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    var CTVM = new CreateTicketVM();
        //    CTVM.Ticket = ticket;
        //    //CTVM.Attachments = new Attachments();
        //    return View(CTVM);
        //}

        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            var selectedTicketTypes = new List<int>();
            foreach (var item in ticket.Type)
            {
                selectedTicketTypes.Add(item.Id);
            }
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var DevList = uh.ListDevsOnMyProjects(userId).OrderByDescending(d => d.DisplayName).ToList();
            ViewBag.DeveloperId = new SelectList(DevList, "Id", "DisplayName", ticket.DeveloperId);
            ViewBag.Types = new MultiSelectList(db.TicketTypes.OrderByDescending(tt => tt.Name).ToList(), "Id", "Name", selectedTicketTypes);
            ViewBag.PriorityId = new SelectList(db.TicketPriorities.OrderByDescending(tp => tp.Name).ToList(), "Id", "Name", ticket.PriorityId);
            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.StatusId = new SelectList(db.TicketStatus.OrderByDescending(ts => ts.Name).ToList(), "Id", "Name", ticket.StatusId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Description,StatusId,PriorityId,Due,DeveloperId")] Ticket ticket, List<string> Types)
        {
            if (ModelState.IsValid)
            {
                var Ticket = db.Tickets.Find(ticket.Id);
                var UserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;

                //Update Ticket Histories
                foreach (var prop in typeof(Ticket).GetProperties())
                {
                    //                    if (!(prop.Name == "Updated" || prop.Name == "Due" || prop.Name == "OwnerUserId" || prop.Name == "PMId" || prop.Name == "DeveloperId" || prop.Name == "Project" || prop.Name == "Comments" || prop.Name == "Attachments" || prop.Name == "Histories" || prop.Name == "Type" || prop.Name == "Notifications"))
                    if (prop.Name == "Title" || prop.Name == "Description" || (prop.Name == "StatusId" && prop.GetValue(ticket).ToString() != "0") || (prop.Name == "PriorityId" && prop.GetValue(ticket).ToString() != "0"))
                    {
                        if (prop.GetValue(ticket).ToString() != prop.GetValue(Ticket).ToString())
                        {
                            //if (prop.Name == "Title" || prop.Name == "Description" || (prop.Name == "StatusId" && prop.GetValue(ticket).ToString() != "0") || (prop.Name == "PriorityId" && prop.GetValue(ticket).ToString() != "0"))
                            //{
                            var oldVal = prop.GetValue(Ticket).ToString();
                            var newVal = prop.GetValue(ticket).ToString();
                            if (prop.Name == "StatusId")
                            {
                                oldVal = db.TicketStatus.Find(prop.GetValue(Ticket)).Name;
                                newVal = db.TicketStatus.Find(prop.GetValue(ticket)).Name;
                            }
                            else if (prop.Name == "PriorityId")
                            {
                                oldVal = db.TicketPriorities.Find(prop.GetValue(Ticket)).Name;
                                newVal = db.TicketPriorities.Find(prop.GetValue(ticket)).Name;
                            }

                            var history = th.CreateTicketHistory(UserId, Ticket.Id, prop.Name, oldVal, newVal);
                            db.Histories.Add(history);
                            db.Tickets.Find(Ticket.Id).Histories.Add(history);
                            prop.SetValue(Ticket, prop.GetValue(ticket));
                            //}
                            //else if (prop.Name == "DeveloperId")
                            //{
                            //    if (prop.GetValue(Ticket).ToString() != null && prop.GetValue(ticket).ToString() != null)
                            //    {
                            //        var oldDev = db.Users.Find(Ticket.DeveloperId);
                            //        var newDev = db.Users.Find(ticket.DeveloperId);
                            //        var history = th.CreateTicketHistory(UserId, Ticket.Id, prop.Name, oldDev.DisplayName, newDev.DisplayName);
                            //        db.Histories.Add(history);
                            //        db.Tickets.Find(Ticket.Id).Histories.Add(history);
                            //        oldDev.Tickets.Remove(Ticket);
                            //        newDev.Tickets.Add(Ticket);
                            //        Ticket.DeveloperId = newDev.Id;
                            //    }
                            //    else if (prop.GetValue(Ticket).ToString() == null && prop.GetValue(ticket).ToString() != null)
                            //    {
                            //        var newDev = db.Users.Find(ticket.DeveloperId);
                            //        var history = th.CreateTicketHistory(UserId, Ticket.Id, prop.Name, "", newDev.DisplayName);
                            //        db.Histories.Add(history);
                            //        db.Tickets.Find(Ticket.Id).Histories.Add(history);
                            //        newDev.Tickets.Add(Ticket);
                            //        Ticket.DeveloperId = newDev.Id;
                            //    }
                            //}
                        }
                    }
                    //Console.WriteLine(string.Format("Property Name: {0}, Property Value: {1}", prop.Name, prop.GetValue(myClass)));
                }
                //Ticket.Title = ticket.Title;
                //Ticket.Description = ticket.Description;
                Ticket.Updated = DateTimeOffset.Now;
                //if(ticket.StatusId != 0)
                //{
                //Ticket.StatusId = ticket.StatusId;
                //}                
                if (Ticket.DeveloperId != ticket.DeveloperId)
                {
                    if (Ticket.DeveloperId != null && ticket.DeveloperId != null)
                    {
                        var oldDev = db.Users.Find(Ticket.DeveloperId);
                        var newDev = db.Users.Find(ticket.DeveloperId);
                        var history = th.CreateTicketHistory(UserId, Ticket.Id, "Developer", oldDev.DisplayName, newDev.DisplayName);
                        db.Histories.Add(history);
                        db.Tickets.Find(Ticket.Id).Histories.Add(history);
                        oldDev.Tickets.Remove(Ticket);
                        newDev.Tickets.Add(Ticket);
                        Ticket.DeveloperId = newDev.Id;
                        await UserManager.SendEmailAsync(newDev.Id, "Ticket Assignment", "You have a new ticket assigned to you titled: " + Ticket.Title + ". Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                        var newDevNotice = th.CreateNotification(newDev.Id, Ticket.Id, "Ticket Assignment: " + Ticket.Title);
                        db.Tickets.Find(Ticket.Id).Notifications.Add(newDevNotice);
                        db.Users.Find(newDev.Id).Notifications.Add(newDevNotice);
                        await UserManager.SendEmailAsync(oldDev.Id, "Ticket Unassignment", "You have been unassigned from the ticket titled: " + Ticket.Title + ". Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                        var oldDevNotice = th.CreateNotification(oldDev.Id, Ticket.Id, "Ticket Unassigment: " + Ticket.Title);
                        db.Tickets.Find(Ticket.Id).Notifications.Add(oldDevNotice);
                        db.Users.Find(oldDev.Id).Notifications.Add(oldDevNotice);
                    }
                    else if (Ticket.DeveloperId == null && ticket.DeveloperId != null)
                    {
                        var newDev = db.Users.Find(ticket.DeveloperId);
                        var history = th.CreateTicketHistory(UserId, Ticket.Id, "Developer", "", newDev.DisplayName);
                        db.Histories.Add(history);
                        db.Tickets.Find(Ticket.Id).Histories.Add(history);
                        newDev.Tickets.Add(Ticket);
                        Ticket.DeveloperId = newDev.Id;
                        await UserManager.SendEmailAsync(newDev.Id, "Ticket Assignment", "You have a new ticket assigned to you titled: " + Ticket.Title + ". Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                        var newDevNotice = th.CreateNotification(newDev.Id, Ticket.Id, "Ticket Assignment: " + Ticket.Title);
                        db.Tickets.Find(Ticket.Id).Notifications.Add(newDevNotice);
                        db.Users.Find(newDev.Id).Notifications.Add(newDevNotice);
                    }
                    //This will cause problems because Developers cannot submit a DeveloperId from Edit.
                    //else if (Ticket.DeveloperId != null && ticket.DeveloperId == null)
                    //{
                    //    var oldDev = db.Users.Find(Ticket.DeveloperId);
                    //    oldDev.Tickets.Remove(Ticket);
                    //    Ticket.DeveloperId = null;
                    //}
                }
                //if (ticket.PriorityId != 0)
                //{
                //    Ticket.PriorityId = ticket.PriorityId;
                //}
                //if (ticket.Due != null)
                //{
                //    Ticket.Due = ticket.Due;
                //}
                if (ticket.Due != null)
                {
                    Ticket.Due = ticket.Due.Value.UtcDateTime;
                    var history = th.CreateTicketHistory(UserId, Ticket.Id, "Due", Ticket.Due.Value.Date.ToShortDateString(), ticket.Due.Value.Date.ToShortDateString());
                    db.Histories.Add(history);
                    db.Tickets.Find(Ticket.Id).Histories.Add(history);
                    Ticket.Due = ticket.Due;
                }

                //Ticket Types
                var oldTypes = new List<string>();
                var newTypes = new List<string>();
                var removeTypes = new List<int>();
                var addTypes = new List<int>();
                foreach (var type in Ticket.Type)
                {
                    if (Types != null)
                    {
                        foreach (var ty in Types)
                        {
                            var tyInt = Convert.ToInt32(ty);
                            if (type.Id != tyInt && !Ticket.Type.Contains(db.TicketTypes.Find(tyInt)))
                            {
                                addTypes.Add(db.TicketTypes.Find(tyInt).Id);
                                newTypes.Add(db.TicketTypes.Find(tyInt).Name);
                            }
                            else if (type.Id != tyInt && !Types.Contains(type.Id.ToString()))
                            {
                                oldTypes.Add(db.TicketTypes.Find(type.Id).Name);
                                removeTypes.Add(db.TicketTypes.Find(type.Id).Id);
                            }
                        }
                    }
                    else
                    {
                        oldTypes.Add(db.TicketTypes.Find(type.Id).Name);
                        removeTypes.Add(type.Id);
                    }
                }
                foreach (var ty in removeTypes)
                {
                    Ticket.Type.Remove(db.TicketTypes.Find(ty));
                }
                foreach (var ty in addTypes)
                {
                    Ticket.Type.Add(db.TicketTypes.Find(ty));
                }
                string oldT = "";
                foreach (var ot in oldTypes)
                {
                    if (ot != oldTypes.Last())
                    {
                        oldT += ot + ", ";
                    }
                    else
                    {
                        oldT += ot;
                    }
                }
                string newT = "";
                foreach (var nt in newTypes)
                {
                    if (nt != newTypes.Last())
                    {
                        newT += nt + ", ";
                    }
                    else
                    {
                        newT += nt;
                    }
                }
                if (oldT != newT)
                {
                    var history = th.CreateTicketHistory(UserId, Ticket.Id, "Types", oldT, newT);
                    db.Histories.Add(history);
                    db.Tickets.Find(Ticket.Id).Histories.Add(history);
                }
                //Ticket.Type.Clear();
                //foreach (var ty in Types)
                //{
                //    var tyInt = Convert.ToInt32(ty);
                //    if (!Ticket.Type.Contains(db.TicketTypes.Find(tyInt)))
                //    {
                //        Ticket.Type.Add(db.TicketTypes.Find(tyInt));
                //    }
                //}
                //}
                if (UserId != Ticket.DeveloperId && Ticket.DeveloperId != null)
                {
                    await UserManager.SendEmailAsync(Ticket.DeveloperId, "Ticket Change", "A ticket assigned to you titled: " + Ticket.Title + " has recently been changed/updated. Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                    var DevNotice = th.CreateNotification(Ticket.DeveloperId, Ticket.Id, "Ticket Change: " + ticket.Title);
                    db.Tickets.Find(ticket.Id).Notifications.Add(DevNotice);
                    db.Users.Find(Ticket.DeveloperId).Notifications.Add(DevNotice);
                }

                db.Entry(Ticket).State = EntityState.Modified;
                db.SaveChanges();
                if (!(uh.IsUserInRole(User.Identity.User().Id, "Admin") || uh.IsUserInRole(User.Identity.User().Id, "Project Manager") || uh.IsUserInRole(User.Identity.User().Id, "Developer")))
                {
                    return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
                }
                return RedirectToAction("EditIndex");
            }

            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var DevList = uh.ListDevsOnMyProjects(userId).OrderByDescending(d => d.DisplayName).ToList();
            ViewBag.Developers = new SelectList(DevList, "Id", "Name", ticket.DeveloperId);
            ViewBag.Types = new MultiSelectList(db.TicketTypes.OrderByDescending(tt => tt.Name).ToList(), "Id", "Name", Types);
            ViewBag.PriorityId = new SelectList(db.TicketPriorities.OrderByDescending(tp => tp.Name).ToList(), "Id", "Name", ticket.PriorityId);
            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.StatusId = new SelectList(db.TicketStatus.OrderByDescending(ts => ts.Name).ToList(), "Id", "Name", ticket.StatusId);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignTickets(string Developers, List<string> Tickets, string Selection)
        {
            if (ModelState.IsValid)
            {
                var UserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                if (Selection == "Assign to Ticket")
                {
                    var developer = db.Users.Find(Developers);
                    Ticket ticket = new Ticket();
                    if (Tickets != null)
                    {
                        foreach (var id in Tickets)
                        {
                            int intId = Convert.ToInt32(id);
                            ticket = db.Tickets.Find(intId);
                            string oldDev = "";
                            string newDev = "";
                            bool test = false;
                            if (ticket.DeveloperId != developer.Id && ticket.DeveloperId != null)
                            {
                                var oldDeveloper = db.Users.Find(ticket.DeveloperId);
                                oldDev = oldDeveloper.DisplayName;
                                oldDeveloper.Tickets.Remove(ticket);
                                await UserManager.SendEmailAsync(oldDeveloper.Id, "Ticket Unassignment", "You have been unassigned from the ticket titled: " + ticket.Title + ". Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                                var oldDevNotice = th.CreateNotification(oldDeveloper.Id, ticket.Id, "Ticket Unassignment: " + ticket.Title);
                                db.Tickets.Find(ticket.Id).Notifications.Add(oldDevNotice);
                                db.Users.Find(oldDeveloper.Id).Notifications.Add(oldDevNotice);
                                ticket.DeveloperId = null;
                                test = true;
                            }
                            if (ticket.DeveloperId == null)
                            {
                                if (!th.IsUserOnTicket(developer.Id, ticket.Id))
                                {
                                    developer.Tickets.Add(ticket);
                                }
                                ticket.DeveloperId = developer.Id;
                                newDev = developer.DisplayName;
                                await UserManager.SendEmailAsync(developer.Id, "Ticket Assignment", "You have a new ticket assigned to you titled: " + ticket.Title + ". Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                                var newDevNotice = th.CreateNotification(developer.Id, ticket.Id, "Ticket Assignment: " + ticket.Title);
                                db.Tickets.Find(ticket.Id).Notifications.Add(newDevNotice);
                                db.Users.Find(developer.Id).Notifications.Add(newDevNotice);
                                test = true;
                            }
                            if (test)
                            {
                                var history = th.CreateTicketHistory(UserId, ticket.Id, "Developer", oldDev, newDev);
                                db.Histories.Add(history);
                                db.Tickets.Find(ticket.Id).Histories.Add(history);
                            }
                        }
                    }
                    db.Entry(ticket).State = EntityState.Modified;
                    db.Entry(developer).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("EditIndex", "Tickets");
                }
                else if (Selection == "Unassign from Ticket")
                {
                    var developer = db.Users.Find(Developers);
                    Ticket ticket = new Ticket();
                    if (Tickets != null)
                    {
                        foreach (var id in Tickets)
                        {
                            int intId = Convert.ToInt32(id);
                            ticket = db.Tickets.Find(intId);
                            string oldDev = "";
                            string newDev = "";
                            bool test = false;
                            if (th.IsUserOnTicket(developer.Id, ticket.Id))
                            {
                                developer.Tickets.Remove(ticket);
                                await UserManager.SendEmailAsync(developer.Id, "Ticket Unassignment", "You have been unassigned from the ticket titled: " + ticket.Title + ". Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                                var oldDevNotice = th.CreateNotification(developer.Id, ticket.Id, "Ticket Unassignment: " + ticket.Title);
                                db.Tickets.Find(ticket.Id).Notifications.Add(oldDevNotice);
                                db.Users.Find(developer.Id).Notifications.Add(oldDevNotice);
                                ticket.DeveloperId = null;
                                oldDev = developer.DisplayName;
                                test = true;
                            }
                            if (test)
                            {
                                var history = th.CreateTicketHistory(UserId, ticket.Id, "Developer", oldDev, newDev);
                                db.Histories.Add(history);
                                db.Tickets.Find(ticket.Id).Histories.Add(history);
                            }
                        }
                    }
                    db.Entry(developer).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("EditIndex", "Tickets");
                }
                else
                {
                    var euserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                    var eticketVM = new TicketIndexVM();
                    //User tickets for developers assigned tickets and submitters owned tickets
                    var etickets = new List<Ticket>();
                    if (User.IsInRole("Developer"))
                    {
                        foreach (var ti in th.ListUserAssignedTickets(euserId))
                        {
                            etickets.Add(ti);
                        }
                    }
                    if (User.IsInRole("Submitter"))
                    {
                        foreach (var ti in th.ListUserTickets(euserId))
                        {
                            etickets.Add(ti);
                        }
                    }
                    //Use projects for the list of tickets in projects assigned
                    var eprojects = new List<Project>();
                    if (User.IsInRole("Admin") || User.IsInRole("Submitter"))
                    {
                        eprojects = db.Projects.ToList();
                    }
                    else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
                    {
                        eprojects = ph.ListUserProjects(euserId).ToList();
                    }
                    //Use users to help display the right stuff. This is to make sense of the PMId, DeveloperIds, and OwnerId
                    //var eusers = new List<ApplicationUser>();
                    //var eallUsers = db.Users.ToList();
                    //foreach (var us in eallUsers)
                    //{
                    //    //Pulls out all of the users attached to the list of projects
                    //    if (eprojects.All(p => p.Tickets.All(t => t.Users.Contains(us))))
                    //    {
                    //        eusers.Add(us);
                    //    }
                    //}
                    var eDevList = uh.ListDevsOnMyProjects(euserId).OrderByDescending(d => d.DisplayName).ToList();
                    var emyTickets = th.ListUserTicketsInProjects(euserId);
                    eticketVM.Tickets = etickets.OrderByDescending(t => t.Created).ToList();
                    eticketVM.Project = eprojects.OrderByDescending(p => p.Name).ToList();
                    //eticketVM.Users = eusers;
                    ViewBag.Developers = new MultiSelectList(eDevList, "Id", "DisplayName", Developers);
                    ViewBag.Tickets = new MultiSelectList(emyTickets, "Id", "Title", Tickets);
                    ViewBag.Tickets2 = new MultiSelectList(emyTickets, "Id", "Title");
                    ViewBag.Projects = new SelectList(eprojects, "Id", "Name");
                    //ViewBag.UnDevelopers = new MultiSelectList(DevList, "Id", "DisplayName");
                    //ViewBag.UnTickets = new MultiSelectList(myTickets, "Id", "Title");
                    ViewBag.ErrorMessage = "Something went wrong. Please try again";

                    return View("Index", eticketVM);
                }
            }
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var ticketVM = new TicketIndexVM();
            //User tickets for developers assigned tickets and submitters owned tickets
            var tickets = new List<Ticket>();
            if (User.IsInRole("Developer"))
            {
                foreach (var ti in th.ListUserAssignedTickets(userId))
                {
                    tickets.Add(ti);
                }
            }
            if (User.IsInRole("Submitter"))
            {
                foreach (var ti in th.ListUserTickets(userId))
                {
                    tickets.Add(ti);
                }
            }
            //Use projects for the list of tickets in projects assigned
            var projects = new List<Project>();
            if (User.IsInRole("Admin") || User.IsInRole("Submitter"))
            {
                projects = db.Projects.ToList();
            }
            else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
            {
                projects = ph.ListUserProjects(userId).ToList();
            }
            //Use users to help display the right stuff. This is to make sense of the PMId, DeveloperIds, and OwnerId
            //var users = new List<ApplicationUser>();
            //var allUsers = db.Users.ToList();
            //foreach (var us in allUsers)
            //{
            //    //Pulls out all of the users attached to the list of projects
            //    if (projects.All(p => p.Tickets.All(t => t.Users.Contains(us))))
            //    {
            //        users.Add(us);
            //    }
            //}
            var DevList = uh.ListDevsOnMyProjects(userId).OrderByDescending(d => d.DisplayName).ToList();
            var myTickets = th.ListUserTicketsInProjects(userId).OrderByDescending(t => t.Created).ToList();
            ticketVM.Tickets = tickets.OrderByDescending(t => t.Created).ToList();
            ticketVM.Project = projects.OrderByDescending(p => p.Name).ToList();
            //ticketVM.Users = users;
            ViewBag.Developers = new MultiSelectList(DevList, "Id", "DisplayName", Developers);
            ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title", Tickets);
            ViewBag.Tickets2 = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.Projects = new SelectList(projects, "Id", "Name");
            //ViewBag.UnDevelopers = new MultiSelectList(DevList, "Id", "DisplayName");
            //ViewBag.UnTickets = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.ErrorMessage = "Something went wrong. Please try again";

            return View("EditIndex", ticketVM);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult UnAssignTickets(string UnDevelopers, List<string> UnTickets)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = db.Users.Find(UnDevelopers);
        //        Ticket ticket = new Ticket();
        //        if (UnTickets != null)
        //        {
        //            foreach (var id in UnTickets)
        //            {
        //                int intId = Convert.ToInt32(id);
        //                ticket = db.Tickets.Find(intId);
        //                if (th.IsUserOnTicket(user.Id, ticket.Id))
        //                {
        //                    user.Tickets.Remove(ticket);
        //                    ticket.DeveloperId = null;
        //                }
        //            }
        //        }
        //        db.Entry(user).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index", "Tickets");
        //    }
        //    var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
        //    var ticketVM = new TicketIndexVM();
        //    //User tickets for developers assigned tickets and submitters owned tickets
        //    var tickets = new List<Ticket>();
        //    if (User.IsInRole("Developer"))
        //    {
        //        foreach (var ti in th.ListUserAssignedTickets(userId))
        //        {
        //            tickets.Add(ti);
        //        }
        //    }
        //    if (User.IsInRole("Submitter"))
        //    {
        //        foreach (var ti in th.ListUserTickets(userId))
        //        {
        //            tickets.Add(ti);
        //        }
        //    }
        //    //Use projects for the list of tickets in projects assigned
        //    var projects = new List<Project>();
        //    if (User.IsInRole("Admin") || User.IsInRole("Submitter"))
        //    {
        //        projects = db.Projects.ToList();
        //    }
        //    else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
        //    {
        //        projects = ph.ListUserProjects(userId).ToList();
        //    }
        //    //Use users to help display the right stuff. This is to make sense of the PMId, DeveloperIds, and OwnerId
        //    var users = new List<ApplicationUser>();
        //    var allUsers = db.Users.ToList();
        //    foreach (var us in allUsers)
        //    {
        //        //Pulls out all of the users attached to the list of projects
        //        if (projects.All(p => p.Tickets.All(t => t.Users.Contains(us))))
        //        {
        //            users.Add(us);
        //        }
        //    }
        //    var DevList = uh.ListDevsOnMyProjects(userId);
        //    var myTickets = th.ListUserTicketsInProjects(userId);
        //    ticketVM.Tickets = tickets;
        //    ticketVM.Project = projects;
        //    ticketVM.Users = users;
        //    ViewBag.Developers = new MultiSelectList(DevList, "Id", "DisplayName");
        //    ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title");
        //    ViewBag.UnDevelopers = new MultiSelectList(DevList, "Id", "DisplayName", UnDevelopers);
        //    ViewBag.UnTickets = new MultiSelectList(myTickets, "Id", "Title", UnTickets);
        //    ViewBag.ErrorMessage = "Something went wrong. Please try again";

        //    return View("Index", ticketVM);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignToProjects(List<string> Tickets2, string Projects)
        {
            if (ModelState.IsValid)
            {
                var UserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                int projectId = Convert.ToInt32(Projects);
                var project = db.Projects.Find(projectId);
                Ticket ticket = new Ticket();
                if (Tickets2 != null)
                {
                    foreach (var id in Tickets2)
                    {
                        int intId = Convert.ToInt32(id);
                        ticket = db.Tickets.Find(intId);
                        string oldProject = ticket.Project.Name;
                        string newProject = "";
                        string oldPM = "";
                        string newPM = "";
                        bool test1 = false;
                        bool test2 = true;
                        if (!th.IsTicketOnProject(ticket.Id, project.Id))
                        {
                            newProject = project.Name;
                            test1 = true;
                            project.Tickets.Add(ticket);
                            if (ticket.PMId != null)
                            {
                                oldPM = db.Users.Find(ticket.PMId).DisplayName;
                                test2 = true;
                                ticket.Users.Remove(db.Users.Find(ticket.PMId));
                            }
                            ticket.PMId = project.PMId;
                            if (ticket.PMId != null)
                            {
                                ticket.Users.Add(db.Users.Find(ticket.PMId));
                                newPM = db.Users.Find(project.PMId).DisplayName;
                                test2 = true;
                            }
                            //ticket.Developers.Add();
                        }
                        if (test1)
                        {
                            var history = th.CreateTicketHistory(UserId, ticket.Id, "Project", oldProject, newProject);
                            db.Histories.Add(history);
                            db.Tickets.Find(ticket.Id).Histories.Add(history);
                        }
                        if (test2)
                        {
                            var history = th.CreateTicketHistory(UserId, ticket.Id, "Project Manager", oldPM, newPM);
                            db.Histories.Add(history);
                            db.Tickets.Find(ticket.Id).Histories.Add(history);
                        }
                    }
                }
                //db.Entry(ticket).State = EntityState.Modified;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EditIndex", "Tickets");
            }
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var ticketVM = new TicketIndexVM();
            //User tickets for developers assigned tickets and submitters owned tickets
            var tickets = new List<Ticket>();
            if (User.IsInRole("Developer"))
            {
                foreach (var ti in th.ListUserAssignedTickets(userId))
                {
                    tickets.Add(ti);
                }
            }
            if (User.IsInRole("Submitter"))
            {
                foreach (var ti in th.ListUserTickets(userId))
                {
                    tickets.Add(ti);
                }
            }
            //Use projects for the list of tickets in projects assigned
            var projects = new List<Project>();
            if (User.IsInRole("Admin") || User.IsInRole("Submitter"))
            {
                projects = db.Projects.ToList();
            }
            else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
            {
                projects = ph.ListUserProjects(userId).ToList();
            }
            //Use users to help display the right stuff. This is to make sense of the PMId, DeveloperIds, and OwnerId
            //var users = new List<ApplicationUser>();
            //var allUsers = db.Users.ToList();
            //foreach (var us in allUsers)
            //{
            //    //Pulls out all of the users attached to the list of projects
            //    if (projects.All(p => p.Tickets.All(t => t.Users.Contains(us))))
            //    {
            //        users.Add(us);
            //    }
            //}
            var DevList = uh.ListDevsOnMyProjects(userId).OrderByDescending(d => d.DisplayName).ToList();
            var myTickets = th.ListUserTicketsInProjects(userId).OrderByDescending(t => t.Created).ToList();
            ticketVM.Tickets = tickets.OrderByDescending(t => t.Created).ToList();
            ticketVM.Project = projects.OrderByDescending(p => p.Name).ToList();
            //ticketVM.Users = users;
            ViewBag.Developers = new MultiSelectList(DevList, "Id", "DisplayName");
            ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.Tickets2 = new MultiSelectList(myTickets, "Id", "Title", Tickets2);
            ViewBag.Projects = new SelectList(projects, "Id", "Name", Projects);
            //ViewBag.UnDevelopers = new MultiSelectList(DevList, "Id", "DisplayName");
            //ViewBag.UnTickets = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.ErrorMessage = "Something went wrong. Please try again";

            return View("EditIndex", ticketVM);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult UnAssignFromProjects(List<string> UnTickets, string UnProjects)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        int projectId = Convert.ToInt32(UnProjects);
        //        var project = db.Projects.Find(projectId);
        //        Ticket ticket = new Ticket();
        //        if (UnTickets != null)
        //        {
        //            foreach (var id in UnTickets)
        //            {
        //                int intId = Convert.ToInt32(id);
        //                ticket = db.Tickets.Find(intId);
        //                if (th.IsTicketOnProject(ticket.Id, project.Id))
        //                {
        //                    //All tickets are on at least one project so if you unassign them, they go back to Unassigned
        //                    project.Tickets.Remove(ticket);
        //                    ticket.Project = db.Projects.FirstOrDefault(p => p.Name == "Unassigned");
        //                    //ticket.DeveloperIds.Remove(user.Id);
        //                }
        //            }
        //        }
        //        db.Entry(ticket).State = EntityState.Modified;
        //        db.Entry(project).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("EditIndex", "Tickets");
        //    }
        //    var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
        //    var ticketVM = new TicketIndexVM();
        //    //User tickets for developers assigned tickets and submitters owned tickets
        //    var tickets = new List<Ticket>();
        //    if (User.IsInRole("Developer"))
        //    {
        //        foreach (var ti in th.ListUserAssignedTickets(userId))
        //        {
        //            tickets.Add(ti);
        //        }
        //    }
        //    if (User.IsInRole("Submitter"))
        //    {
        //        foreach (var ti in th.ListUserTickets(userId))
        //        {
        //            tickets.Add(ti);
        //        }
        //    }
        //    //Use projects for the list of tickets in projects assigned
        //    var projects = new List<Project>();
        //    if (User.IsInRole("Admin") || User.IsInRole("Submitter"))
        //    {
        //        projects = db.Projects.ToList();
        //    }
        //    else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
        //    {
        //        projects = ph.ListUserProjects(userId).ToList();
        //    }
        //    //Use users to help display the right stuff. This is to make sense of the PMId, DeveloperIds, and OwnerId
        //    var users = new List<ApplicationUser>();
        //    var allUsers = db.Users.ToList();
        //    foreach (var us in allUsers)
        //    {
        //        //Pulls out all of the users attached to the list of projects
        //        if (projects.All(p => p.Tickets.All(t => t.Users.Contains(us))))
        //        {
        //            users.Add(us);
        //        }
        //    }
        //    var userList = uh.ListUsersOnMyProjects(userId);
        //    var myTickets = th.ListUserTicketsInProjects(userId);
        //    ticketVM.Tickets = tickets;
        //    ticketVM.Project = projects;
        //    ticketVM.Users = users;
        //    ViewBag.Projects = new SelectList(projects, "Id", "Name");
        //    ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title");
        //    ViewBag.UnProjects = new SelectList(projects, "Id", "Name", UnProjects);
        //    ViewBag.UnTickets = new MultiSelectList(myTickets, "Id", "Title", UnTickets);
        //    ViewBag.ErrorMessage = "Something went wrong. Please try again";

        //    return View("EditIndex", ticketVM);
        //}

        // GET: Tickets/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket ticket = db.Tickets.Find(id);
        //    if (ticket == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticket);
        //}

        // POST: Tickets/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Ticket ticket = db.Tickets.Find(id);
        //    db.Tickets.Remove(ticket);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
