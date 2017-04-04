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
using System.IO;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [NoDirectAccess]
    public class AttachmentsController : Controller
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

        // GET: Attachments
        //public ActionResult Index()
        //{
        //    var attachments = db.Attachments.Include(a => a.Ticket).Include(a => a.User);
        //    return View(attachments.ToList());
        //}

        // GET: Attachments/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Attachments attachments = db.Attachments.Find(id);
        //    if (attachments == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(attachments);
        //}

        // GET: Attachments/Create
        //public ActionResult Create()
        //{
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName");
        //    return View();
        //}

        // POST: Attachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,TicketId")] Attachments attachments, HttpPostedFileBase upload, string Submit)
        {
            var Ticket = db.Tickets.Find(attachments.TicketId);
            if (ModelState.IsValid)
            {
                if (ImageUploadValidator.IsWebFriendlyImage(upload))
                {
                    var fileName = Path.GetFileName(upload.FileName);
                    upload.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    attachments.FileUrl = "/Uploads/" + fileName;
                }
                else if (FileUploadValidator.IsFileAcceptableFormat(upload))
                {
                    var fileName = Path.GetFileName(upload.FileName);
                    upload.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    attachments.FileUrl = "/Uploads/" + fileName;
                }
                attachments.Created = DateTimeOffset.Now;
                attachments.UserId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
                attachments.TicketId = Ticket.Id;
                db.Attachments.Add(attachments);
                if (attachments.UserId != Ticket.DeveloperId && Ticket.DeveloperId != null)
                {
                    await UserManager.SendEmailAsync(Ticket.DeveloperId, "Ticket Attachment", "A ticket assigned to you titled: " + Ticket.Title + " has recently had a new attachment uploaded. Please <a href=\"kbartholomew-bugtracker.azurewebsites.net\">log in</a> to your account to view the details.");
                    var DevNotice = th.CreateNotification(Ticket.DeveloperId, Ticket.Id, "New Ticket Attachment: " + Ticket.Title);
                    db.Tickets.Find(Ticket.Id).Notifications.Add(DevNotice);
                    db.Users.Find(Ticket.DeveloperId).Notifications.Add(DevNotice);
                }
                db.SaveChanges();
                if(Submit == "Add New Attachment")
                {
                    return RedirectToAction("Details", "Tickets", new { id = attachments.TicketId });
                }
                else if(Submit == "Add Attachment")
                {
                    return RedirectToAction("Index", "Tickets");
                }
            }

            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", attachments.TicketId);
            //ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", attachments.UserId);
            return View("CreateAttachment","Tickets", attachments);
        }

        // GET: Attachments/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Attachments attachments = db.Attachments.Find(id);
        //    if (attachments == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", attachments.TicketId);
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", attachments.UserId);
        //    return View(attachments);
        //}

        // POST: Attachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,FilePath,Description,Created,FileUrl,TicketId,UserId")] Attachments attachments)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(attachments).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", attachments.TicketId);
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", attachments.UserId);
        //    return View(attachments);
        //}

        // GET: Attachments/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Attachments attachments = db.Attachments.Find(id);
        //    if (attachments == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(attachments);
        //}

        //// POST: Attachments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Attachments attachments = db.Attachments.Find(id);
        //    db.Attachments.Remove(attachments);
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
