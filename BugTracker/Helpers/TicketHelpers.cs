using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace BugTracker.Helpers
{
    public class TicketHelpers
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public bool IsUserOnTicket(string userId, int ticketId)
        {
            if (db.Tickets.Find(ticketId).Users.Contains(db.Users.Find(userId)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsTicketOnProject(int ticketId, int projectId)
        {
            if (db.Projects.Find(projectId).Tickets.Contains(db.Tickets.Find(ticketId)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ICollection<Ticket> ListUserTicketsInProjects(string userId)
        {
            List<Ticket> TicketsList = new List<Ticket>();
            UserRolesHelpers ur = new UserRolesHelpers();
            ProjectsHelper ph = new ProjectsHelper();

            var projects = ph.ListUserProjects(userId);
            foreach (var pr in projects)
            {
                foreach (var ti in pr.Tickets)
                {
                    TicketsList.Add(db.Tickets.Find(ti.Id));
                }
            }
            return TicketsList;
        }

        public ICollection<Ticket> ListUserTickets(string userId)
        {
            List<Ticket> TicketsList = new List<Ticket>();
            UserRolesHelpers ur = new UserRolesHelpers();
            ProjectsHelper ph = new ProjectsHelper();
            var user = db.Users.Find(userId);

            //Admins can view a list of all tickets
            if (ur.IsUserInRole(userId, "Admin"))
            {
                TicketsList = db.Tickets.ToList();
                return TicketsList;
            }
            //Project Managers must be able to view a list of all tickets belonging to the projects to which they are assigned
            //Developers must be able to view a list of all tickets belonging to the projects to which they are assigned
            else if (ur.IsUserInRole(userId, "Project Manager") || ur.IsUserInRole(userId, "Developer"))
            {
                TicketsList = ListUserTicketsInProjects(userId).ToList();

                return TicketsList;
            }
            //Submitters must be able to view a list of all tickets which they own
            else if (ur.IsUserInRole(userId, "Submitter"))
            {
                var allTickets = db.Tickets.ToList();
                foreach(var ti in allTickets)
                {
                    if(ti.OwnerUserId == userId)
                    {
                        TicketsList.Add(ti);
                    }
                }
                return TicketsList;
            }
            else
            {
            return TicketsList;
            }
        }

        public ICollection<Ticket> ListUserOwnedTickets(string userId)
        {
            List<Ticket> TicketsList = new List<Ticket>();
            var allTickets = db.Tickets.ToList();
            foreach (var ti in allTickets)
            {
                if (ti.OwnerUserId == userId)
                {
                    TicketsList.Add(ti);
                }
            }
            return TicketsList;
        }

        //Developers must be able to view a list of all tickets to which they are assigned
        public ICollection<Ticket> ListUserAssignedTickets(string userId)
        {
            List<Ticket> TicketsList = new List<Ticket>();
            UserRolesHelpers ur = new UserRolesHelpers();
            var user = db.Users.Find(userId);
            if(ur.IsUserInRole(userId, "Developer"))
            {
                var allTickets = db.Tickets.ToList();
                foreach (var ti in allTickets)
                {
                    if(ti.Users.Any(us=>us.Id == userId))
                    {
                        TicketsList.Add(ti);
                    }
                }
                return TicketsList;
            }
            else
            {
                return TicketsList;
            }
        }


        public List<int> ListUserTicketIds(string userId)
        {
            var userTickets = ListUserTickets(userId);
            List<int> userTicketIds = new List<int>();
            foreach (var up in userTickets)
            {
                var ticketId = db.Tickets.Find(up.Id).Id;
                userTicketIds.Add(ticketId);
            }
            return userTicketIds;
        }

        public void AddToTicket(string userId, int ticketId)
        {
            db.Tickets.Find(ticketId).Users.Add(db.Users.Find(userId));
            db.Users.Find(userId).Projects.Add(db.Projects.Find(ticketId));
        }

        public void RemoveFromTicket(string userId, int ticketId)
        {
            db.Tickets.Find(ticketId).Users.Remove(db.Users.Find(userId));
            db.Users.Find(userId).Tickets.Remove(db.Tickets.Find(ticketId));

        }

        public Histories CreateTicketHistory(string userId, int ticketId, string PropertyName, string OldValue, string NewValue)
        {
            Histories history = new Histories();
            history.UserId = db.Users.Find(userId).Id;
            history.TicketId = db.Tickets.Find(ticketId).Id;
            history.Property = PropertyName;
            history.OldValue = OldValue;
            history.NewValue = NewValue;
            history.Changed = DateTimeOffset.Now;
            return history;
        }

        public Notification CreateNotification(string userId, int ticketId, string Message)
        {
            Notification notice = new Notification();
            notice.Viewed = false;
            notice.Description = Message;
            notice.Create = DateTimeOffset.Now;
            notice.UserId = db.Users.Find(userId).Id;
            notice.TicketId = db.Tickets.Find(ticketId).Id;
            return notice;
        }

        //public async Task SendNotification(string devId, int ticketId, string modNote)
        //{
        //    //EmailModel model = new EmailModel();
        //    var dev = db.Users.Find(devId);
        //    var ticket = db.Tickets.Find(ticketId);
        //    try
        //    {
        //        var body = "<p>Notification that <bold>{0}</bold> has been {1}</p>";
        //        var from = "BugTracker<contact@BugZap.com>";
        //        var ticketInfo = ticket.Title;
        //        //model.Body = "This is a message from your bug tracker site. The name and the email of the contacting person is above.";
        //        var email = new MailMessage(from,
        //    ConfigurationManager.AppSettings[dev.Email])
        //        {
        //            Subject = "BugTracker Contact Email",
        //            Body = string.Format(body, ticketInfo, modNote),
        //            IsBodyHtml = true
        //        };
        //        var svc = new PersonalEmail();
        //        await svc.SendAsync(email);
        //        //return RedirectToAction("Sent");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        await Task.FromResult(0);
        //    }
        //}
    }
}