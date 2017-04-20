using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using BugTracker.Models;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [NoDirectAccess]
    public class NotificationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        

        // POST: Notifications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,UserId")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                db.Notifications.Add(notification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", notification.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", notification.UserId);
            return View(notification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Viewed(int NoticeId, int TicketId)
        {
            var notice = db.Notifications.Find(NoticeId);
            notice.Viewed = true;

            db.Entry(notice).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Details","Tickets", new { id = TicketId });
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
