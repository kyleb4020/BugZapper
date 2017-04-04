using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Project
    {
        public Project() //This is a constructor
        {
            this.Tickets = new HashSet<Ticket>();
            this.Users = new HashSet<ApplicationUser>();
            //this.DeveloperIds = new HashSet<DeveloperId>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PMId { get; set; }
        public DateTimeOffset Created { get; set; }
        //public virtual ICollection<DeveloperId> DeveloperIds { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}