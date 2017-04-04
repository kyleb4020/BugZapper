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

        // GET: Histories
        public ActionResult Index()
        {
            var histories = db.Histories.Include(h => h.Ticket).Include(h => h.User);
            return View(histories.ToList());
        }

        // GET: Histories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Histories histories = db.Histories.Find(id);
            if (histories == null)
            {
                return HttpNotFound();
            }
            return View(histories);
        }

        // GET: Histories/Create
        public ActionResult Create()
        {
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName");
            return View();
        }

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

        // GET: Histories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Histories histories = db.Histories.Find(id);
            if (histories == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", histories.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", histories.UserId);
            return View(histories);
        }

        // POST: Histories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Property,OldValue,NewValue,Changed,TicketId,UserId")] Histories histories)
        {
            if (ModelState.IsValid)
            {
                db.Entry(histories).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", histories.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", histories.UserId);
            return View(histories);
        }

        // GET: Histories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Histories histories = db.Histories.Find(id);
            if (histories == null)
            {
                return HttpNotFound();
            }
            return View(histories);
        }

        // POST: Histories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Histories histories = db.Histories.Find(id);
            db.Histories.Remove(histories);
            db.SaveChanges();
            return RedirectToAction("Index");
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
