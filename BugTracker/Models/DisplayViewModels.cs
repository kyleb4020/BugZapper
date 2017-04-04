using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class CreateTicketVM
    {
        public Ticket Ticket { get; set; }
        public Attachments Attachments { get; set; }
    }

    public class DashboardVM
    {
        public ICollection<Ticket> AllTickets { get; set; }
        public ICollection<Ticket> MyTickets { get; set; }
    }

    public class AdminDashboardVM
    {
        public ICollection<Project> Projects { get; set; }
        public ICollection<ApplicationUser> Developers { get; set; }
    }

    public class TicketIndexVM
    {
        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Project> Project { get; set; }
        //public ICollection<ApplicationUser> Users { get; set; }
    }

    public class TicketDetailsVM
    {
        public Ticket Ticket { get; set; }
        public Attachments Attachment { get; set; }
        //public ICollection<ApplicationUser> Users { get; set; }
    }

    //public class ProjectIndexVM
    //{
    //    public ICollection<Project> Projects { get; set; }
    //    public ICollection<ApplicationUser> Users { get; set; }
    //}

    //public class ProjectDetailsVM
    //{
    //    public Project Project { get; set; }
    //    public ICollection<ApplicationUser> Users { get; set; }
    //}
}