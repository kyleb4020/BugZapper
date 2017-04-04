using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Attachments
    {
        public int Id { get; set; }
        public string FilePath { get; set; } //What is this all about?
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public string FileUrl { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}