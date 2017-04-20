using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [NoDirectAccess]
    public class HistoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        // POST: Histories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Property,OldValue,NewValue,Changed,TicketId,UserId")] Histories histories)
        {
            if (ModelState.IsValid)
            {
                db.Histories.Add(histories);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", histories.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", histories.UserId);
            return View(histories);
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
