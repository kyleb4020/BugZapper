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
            ticketVM.Tickets = tickets.OrderByDescending(t => t.Created).ToList();
            ticketVM.Project = projects.OrderByDescending(p => p.Name).ToList();
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
            var myTickets = th.ListUserTicketsInProjects(userId).OrderByDescending(t => t.Created).ToList();
            ticketVM.Tickets = tickets.OrderByDescending(t => t.Created).ToList();
            ticketVM.Project = projects.OrderByDescending(p => p.Name).ToList();
            var DevList = uh.ListDevsOnMyProjects(userId).OrderByDescending(d => d.DisplayName).ToList();
            ViewBag.Developers = new SelectList(DevList, "Id", "DisplayName");
            ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.Tickets2 = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.Projects = new SelectList(projects, "Id", "Name");
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
            TicketDetailsVM TicketVM = new TicketDetailsVM();
            TicketVM.Ticket = ticket;
            ViewBag.Error = "";
            return View(TicketVM);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter, Admin")]
        public ActionResult Create()
        {
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
                return RedirectToAction("Index", "Tickets");
            }
            return View(ticket);
        }
        

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
            ViewBag.StatusId = new SelectList(db.TicketStatus.OrderByDescending(ts => ts.Name).ToList(), "Id", "Name", ticket.StatusId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Description,StatusId,PriorityId,Due,DeveloperId")] Ticket ticket, List<int> Types)
        {
            if (ModelState.IsValid)
            {
                var Ticket = db.Tickets.Find(ticket.Id);
                var UserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;

                //Update Ticket Histories
                foreach (var prop in typeof(Ticket).GetProperties())
                {
                    if (prop.Name == "Title" || prop.Name == "Description" || (prop.Name == "StatusId" && prop.GetValue(ticket).ToString() != "0") || (prop.Name == "PriorityId" && prop.GetValue(ticket).ToString() != "0"))
                    {
                        if (prop.GetValue(ticket).ToString() != prop.GetValue(Ticket).ToString())
                        {
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
                           
                        }
                    }
                }
               
                Ticket.Updated = DateTimeOffset.Now;
                               
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
                    
                }
                
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
                            if (type.Id != ty && !Ticket.Type.Contains(db.TicketTypes.Find(ty)))
                            {
                                if (!addTypes.Contains(ty))
                                {
                                    addTypes.Add(db.TicketTypes.Find(ty).Id);
                                    newTypes.Add(db.TicketTypes.Find(ty).Name);
                                }                                
                            }
                            if (type.Id != ty && !Types.Contains(type.Id))
                            {
                                if (!removeTypes.Contains(type.Id))
                                {
                                    oldTypes.Add(db.TicketTypes.Find(type.Id).Name);
                                    removeTypes.Add(db.TicketTypes.Find(type.Id).Id);
                                }                                
                            }
                        }
                    }
                    else
                    {
                        oldTypes.Add(db.TicketTypes.Find(type.Id).Name);
                        removeTypes.Add(type.Id);
                    }
                }
                if(Ticket.Type.Count == 0)
                {
                    foreach(var ty in Types)
                    {
                        addTypes.Add(db.TicketTypes.Find(ty).Id);
                        newTypes.Add(db.TicketTypes.Find(ty).Name);
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
                    var eDevList = uh.ListDevsOnMyProjects(euserId).OrderByDescending(d => d.DisplayName).ToList();
                    var emyTickets = th.ListUserTicketsInProjects(euserId);
                    eticketVM.Tickets = etickets.OrderByDescending(t => t.Created).ToList();
                    eticketVM.Project = eprojects.OrderByDescending(p => p.Name).ToList();
                    ViewBag.Developers = new MultiSelectList(eDevList, "Id", "DisplayName", Developers);
                    ViewBag.Tickets = new MultiSelectList(emyTickets, "Id", "Title", Tickets);
                    ViewBag.Tickets2 = new MultiSelectList(emyTickets, "Id", "Title");
                    ViewBag.Projects = new SelectList(eprojects, "Id", "Name");
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
            var DevList = uh.ListDevsOnMyProjects(userId).OrderByDescending(d => d.DisplayName).ToList();
            var myTickets = th.ListUserTicketsInProjects(userId).OrderByDescending(t => t.Created).ToList();
            ticketVM.Tickets = tickets.OrderByDescending(t => t.Created).ToList();
            ticketVM.Project = projects.OrderByDescending(p => p.Name).ToList();
            ViewBag.Developers = new MultiSelectList(DevList, "Id", "DisplayName", Developers);
            ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title", Tickets);
            ViewBag.Tickets2 = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.Projects = new SelectList(projects, "Id", "Name");
            ViewBag.ErrorMessage = "Something went wrong. Please try again";

            return View("EditIndex", ticketVM);
        }

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
            var DevList = uh.ListDevsOnMyProjects(userId).OrderByDescending(d => d.DisplayName).ToList();
            var myTickets = th.ListUserTicketsInProjects(userId).OrderByDescending(t => t.Created).ToList();
            ticketVM.Tickets = tickets.OrderByDescending(t => t.Created).ToList();
            ticketVM.Project = projects.OrderByDescending(p => p.Name).ToList();
            ViewBag.Developers = new MultiSelectList(DevList, "Id", "DisplayName");
            ViewBag.Tickets = new MultiSelectList(myTickets, "Id", "Title");
            ViewBag.Tickets2 = new MultiSelectList(myTickets, "Id", "Title", Tickets2);
            ViewBag.Projects = new SelectList(projects, "Id", "Name", Projects);
            ViewBag.ErrorMessage = "Something went wrong. Please try again";

            return View("EditIndex", ticketVM);
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
