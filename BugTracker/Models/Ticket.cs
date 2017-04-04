using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Ticket
    {
        public Ticket() //This is a constructor
        {
            this.Users = new HashSet<ApplicationUser>();
            this.Type = new HashSet<TicketType>();
            this.Attachments = new HashSet<Attachments>();
            this.Comments = new HashSet<Comment>();
            this.Histories = new HashSet<Histories>();
            this.Notifications = new HashSet<Notification>();
            //this.DeveloperIds = new HashSet<DeveloperId>();
            //this.Developers = new HashSet<Developer>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public DateTimeOffset? Due { get; set; }
        public int ProjectId { get; set; }
        public int StatusId { get; set; }
        public int PriorityId { get; set; }
        public string OwnerUserId { get; set; }
        public string PMId { get; set; }
        public string DeveloperId { get; set; }
        //public List<string> DeveloperIds { get; set; }

        //public virtual ICollection<DeveloperId> DeveloperIds { get; set; }
        public virtual Project Project { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Attachments> Attachments { get; set; }
        public virtual ICollection<Histories> Histories { get; set; }
        public virtual ICollection<TicketType> Type { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual TicketStatus Status { get; set; }
        public virtual TicketPriority Priority { get; set; }
        //public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}