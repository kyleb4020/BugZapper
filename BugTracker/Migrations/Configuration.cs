namespace BugTracker.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }

            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }

            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            //Set up 1 Demo Admin
            if (!context.Users.Any(u => u.Email == "krbbugtrackeradmin@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackeradmin@mailinator.com",
                    Email = "krbbugtrackeradmin@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Admin",
                    DisplayName = "DAdmin"
                }, "DemoAdmin123!");
                var userId = userManager.FindByEmail("krbbugtrackeradmin@mailinator.com").Id;
                userManager.AddToRole(userId, "Admin");
            }
            //Set up 2 Demo Project Manager
            if (!context.Users.Any(u => u.Email == "krbbugtrackerpm1@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackerpm1@mailinator.com",
                    Email = "krbbugtrackerpm1@mailinator.com",
                    FirstName = "Demo",
                    LastName = "PM1",
                    DisplayName = "DPM1"
                }, "DemoPM123!");
                var userId = userManager.FindByEmail("krbbugtrackerpm1@mailinator.com").Id;
                userManager.AddToRole(userId, "Project Manager");
            }
            if (!context.Users.Any(u => u.Email == "krbbugtrackerpm2@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackerpm2@mailinator.com",
                    Email = "krbbugtrackerpm2@mailinator.com",
                    FirstName = "Demo",
                    LastName = "PM2",
                    DisplayName = "DPM2"
                }, "DemoPM123!");
                var userId = userManager.FindByEmail("krbbugtrackerpm2@mailinator.com").Id;
                userManager.AddToRole(userId, "Project Manager");
            }
            //Set up 6 Demo Developers
            if (!context.Users.Any(u => u.Email == "krbbugtrackerdev1@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackerdev1@mailinator.com",
                    Email = "krbbugtrackerdev1@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Developer1",
                    DisplayName = "DDeveloper1"
                }, "DemoDev123!");
                var userId = userManager.FindByEmail("krbbugtrackerdev1@mailinator.com").Id;
                userManager.AddToRole(userId, "Developer");
            }
            if (!context.Users.Any(u => u.Email == "krbbugtrackerdev2@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackerdev2@mailinator.com",
                    Email = "krbbugtrackerdev2@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Developer2",
                    DisplayName = "DDeveloper2"
                }, "DemoDev123!");
                var userId = userManager.FindByEmail("krbbugtrackerdev2@mailinator.com").Id;
                userManager.AddToRole(userId, "Developer");
            }
            if (!context.Users.Any(u => u.Email == "krbbugtrackerdev3@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackerdev3@mailinator.com",
                    Email = "krbbugtrackerdev3@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Developer3",
                    DisplayName = "DDeveloper3"
                }, "DemoDev123!");
                var userId = userManager.FindByEmail("krbbugtrackerdev3@mailinator.com").Id;
                userManager.AddToRole(userId, "Developer");
            }
            if (!context.Users.Any(u => u.Email == "krbbugtrackerdev4@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackerdev4@mailinator.com",
                    Email = "krbbugtrackerdev4@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Developer4",
                    DisplayName = "DDeveloper4"
                }, "DemoDev123!");
                var userId = userManager.FindByEmail("krbbugtrackerdev4@mailinator.com").Id;
                userManager.AddToRole(userId, "Developer");
            }
            if (!context.Users.Any(u => u.Email == "krbbugtrackerdev5@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackerdev5@mailinator.com",
                    Email = "krbbugtrackerdev5@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Developer5",
                    DisplayName = "DDeveloper5"
                }, "DemoDev123!");
                var userId = userManager.FindByEmail("krbbugtrackerdev5@mailinator.com").Id;
                userManager.AddToRole(userId, "Developer");
            }
            if (!context.Users.Any(u => u.Email == "krbbugtrackerdev6@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackerdev6@mailinator.com",
                    Email = "krbbugtrackerdev6@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Developer6",
                    DisplayName = "DDeveloper6"
                }, "DemoDev123!");
                var userId = userManager.FindByEmail("krbbugtrackerdev6@mailinator.com").Id;
                userManager.AddToRole(userId, "Developer");
            }
            //Set up 1 Demo Submitter
            if (!context.Users.Any(u => u.Email == "krbbugtrackersub@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "krbbugtrackersub@mailinator.com",
                    Email = "krbbugtrackersub@mailinator.com",
                    FirstName = "Demo",
                    LastName = "Submitter",
                    DisplayName = "DSubmitter"
                }, "DemoSub123!");
                var userId = userManager.FindByEmail("krbbugtrackersub@mailinator.com").Id;
                userManager.AddToRole(userId, "Submitter");
            }

            var submitterId = userManager.FindByEmail("krbbugtrackersub@mailinator.com").Id;

            //Set up Unassigned Tickets Project & 4 Demo Projects
            context.Projects.AddOrUpdate(p => p.Name, new Project { Name = "Unassigned", Description = "This is the Project for all Tickets when they are first created", Created = DateTimeOffset.Now }
            , new Project { Name = "Demo1", Description = "This is the first demo Project", Created = DateTimeOffset.Now }
            , new Project { Name = "Demo2", Description = "This is the second demo Project", Created = DateTimeOffset.Now }
            , new Project { Name = "Demo3", Description = "This is the third demo Project", Created = DateTimeOffset.Now }
            , new Project { Name = "Demo4", Description = "This is the fourth demo Project", Created = DateTimeOffset.Now });

            //if (!context.Projects.Any(p=>p.Users.Any(u=>u.Roles.Any(r=>r.RoleId == context.Roles.FirstOrDefault(t=>t.Name == "Admin").Id))))
            //{
            //    var adminId = userManager.FindByEmail("kyle.r.bartholomew@gmail.com").Id;
            //    context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Users.Add(context.Users.Find(adminId));
            //}

            //Set up Demo Developer as a Developer
            //var devId = userManager.FindByEmail("demodev@demo.com").Id;
            //context.Developers.AddOrUpdate(d => d.UserId, new Developer { UserId = devId });

            //Set up Statuses
            context.TicketStatus.AddOrUpdate(ts => ts.Name, new TicketStatus { Name = "Unassigned" },
                new TicketStatus { Name = "Assigned" }, new TicketStatus { Name = "In Progress" }, 
                new TicketStatus { Name = "Testing" }, new TicketStatus { Name = "Resolved" });

            //Set up Priorities
            context.TicketPriorities.AddOrUpdate(tp => tp.Name, new TicketPriority { Name = "No Priority" }, 
                new TicketPriority { Name = "Low Priority" }, new TicketPriority { Name = "Medium Priority" }, 
                new TicketPriority { Name = "High Priority" }, new TicketPriority { Name = "Urgent" },
                new TicketPriority { Name = "Emergency" });

            //Set up Types
            context.TicketTypes.AddOrUpdate(tt => tt.Name, new TicketType { Name = "Default" }, new TicketType { Name = "Bug Fix" }, 
                new TicketType { Name = "Project Task" }, new TicketType { Name = "Software Update" }, new TicketType { Name = "Maintenance" });


            //Create Demo Tickets *****THIS HAS TO BE DONE AFTER THE INITIAL SEED*****
            context.Tickets.AddOrUpdate(t => t.Title,
                new Ticket
                {
                    Title = "Demo Ticket 1",
                    Description = "This is demo ticket 1",
                    Created = DateTimeOffset.Now.AddDays(-7),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Unassigned").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "No Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 2",
                    Description = "This is demo ticket 2",
                    Created = DateTimeOffset.Now.AddDays(-7),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Unassigned").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "No Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 3",
                    Description = "This is demo ticket 3",
                    Created = DateTimeOffset.Now.AddDays(-6),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Assigned").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "Emergency").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 4",
                    Description = "This is demo ticket 4",
                    Created = DateTimeOffset.Now.AddDays(-5),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "In Progress").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "Urgent").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 5",
                    Description = "This is demo ticket 5",
                    Created = DateTimeOffset.Now.AddDays(-5),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "In Progress").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "Urgent").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 6",
                    Description = "This is demo ticket 6",
                    Created = DateTimeOffset.Now.AddDays(-5),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Testing").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "High Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 7",
                    Description = "This is demo ticket 7",
                    Created = DateTimeOffset.Now.AddDays(-4),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Resolved").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "High Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 8",
                    Description = "This is demo ticket 8",
                    Created = DateTimeOffset.Now.AddDays(-3),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Resolved").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "Low Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 9",
                    Description = "This is demo ticket 9",
                    Created = DateTimeOffset.Now.AddDays(-3),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Testing").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "Medium Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 10",
                    Description = "This is demo ticket 10",
                    Created = DateTimeOffset.Now.AddDays(-3),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Assigned").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "High Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 11",
                    Description = "This is demo ticket 11",
                    Created = DateTimeOffset.Now.AddDays(-3),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "In Progress").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "Urgent").Id,
                    OwnerUserId = submitterId,
                },
                new Ticket
                {
                    Title = "Demo Ticket 12",
                    Description = "This is demo ticket 12",
                    Created = DateTimeOffset.Now.AddDays(-2),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Resolved").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "No Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 13",
                    Description = "This is demo ticket 13",
                    Created = DateTimeOffset.Now.AddDays(-2),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Unassigned").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "No Priority").Id,
                    OwnerUserId = submitterId
                },
                new Ticket
                {
                    Title = "Demo Ticket 14",
                    Description = "This is demo ticket 14",
                    Created = DateTimeOffset.Now.AddDays(-7),
                    ProjectId = context.Projects.FirstOrDefault(p => p.Name == "Unassigned").Id,
                    StatusId = context.TicketStatus.FirstOrDefault(ts => ts.Name == "Unassigned").Id,
                    PriorityId = context.TicketPriorities.FirstOrDefault(tp => tp.Name == "No Priority").Id,
                    OwnerUserId = submitterId
                }
                );

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
