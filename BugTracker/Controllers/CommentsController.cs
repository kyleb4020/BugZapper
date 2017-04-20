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
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [NoDirectAccess]
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketHelpers th = new TicketHelpers();
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
        

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles ="Admin, Project Manager, Developer, Submitter")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Body,TicketId")] Comment comment)
        {
            var Ticket = db.Tickets.Find(comment.TicketId);
            if (ModelState.IsValid)
            {
                comment.Created = DateTimeOffset.Now;
                comment.TicketId = Ticket.Id;
                comment.UserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                db.Comments.Add(comment);
                if (comment.UserId != Ticket.DeveloperId && Ticket.DeveloperId != null)
                {
                    await UserManager.SendEmailAsync(Ticket.DeveloperId, "Ticket Comment", "A ticket assigned to you titled: " + Ticket.Title + " has recently had a new comment. Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                    var DevNotice = th.CreateNotification(Ticket.DeveloperId, Ticket.Id, "New Ticket Comment: " + Ticket.Title);
                    db.Tickets.Find(Ticket.Id).Notifications.Add(DevNotice);
                    db.Users.Find(Ticket.DeveloperId).Notifications.Add(DevNotice);
                }

                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
            }

            ViewBag.Error = "There was an Error. Please try again.";
            return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
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
