using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using BugTracker.Helpers;

namespace BugTracker.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketHelpers th = new TicketHelpers();

        public ActionResult OldIndex2()
        {
            //ViewBag.ReturnUrl = "/Home/Dashboard";
            //return View();
            return RedirectToAction("Index");
        }

        public ActionResult Error()
        {
            return View();
        }

        [Authorize]
        [DonutOutputCache(Duration = 0)]
        public PartialViewResult _Notifications()
        {
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var notices = db.Users.Find(userId).Notifications.OrderByDescending(n=>n.Create).ToList();
            return PartialView(notices);
        }

        [Authorize]
        public ActionResult Index()
        {
            var userId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var dashboardVM = new DashboardVM();
            dashboardVM.AllTickets = db.Tickets.OrderByDescending(t=>t.Created).ToList();
            var myTickets = new List<Ticket>();
            if (User.IsInRole("Admin"))
            {
                myTickets = db.Tickets.ToList();
            }
            if (User.IsInRole("Project Manager"))
            {
                myTickets = th.ListUserTicketsInProjects(userId).ToList();
            }
            if (User.IsInRole("Developer"))
            {
                foreach (var ti in th.ListUserAssignedTickets(userId))
                {
                    if (!myTickets.Contains(ti))
                    {
                        myTickets.Add(ti);
                    }
                }
            }
            if (User.IsInRole("Submitter"))
            {
                foreach (var ti in th.ListUserOwnedTickets(userId))
                {
                    if (!myTickets.Contains(ti))
                    {
                        myTickets.Add(ti);
                    }
                }
            }
            dashboardVM.MyTickets = myTickets.OrderByDescending(t=>t.Created).ToList();

            return View(dashboardVM);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var body = "<p>Email From: <bold>{0}</bold>({1})</p><p> Message:</p><p>{2}</p> ";
                    var from = "BugTracker<contact@BugZap.com>";
                    model.Body = "This is a message from your bug tracker site. The name and the email of the contacting person is above.";
                    var email = new MailMessage(from,
                ConfigurationManager.AppSettings["emailto"])
                    {
                        Subject = "BugTracker Contact Email",
                        Body = string.Format(body, model.FromName, model.FromEmail,
                             model.Body),
                        IsBodyHtml = true
                    };
                    var svc = new PersonalEmail();
                    await svc.SendAsync(email);
                    return RedirectToAction("Sent");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }
            return View(model);
        }
    }
}