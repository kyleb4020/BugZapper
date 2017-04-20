using BugTracker.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace BugTracker.Helpers
{
    public static class DisplayHelpers
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static string DisplayName(this IIdentity user)
        {
            var disName = db.Users.FirstOrDefault(u => u.UserName == user.Name);
            if (disName.DisplayName != null)
            {
                return disName.DisplayName;
            }
            else
            {
                return null;
            }
        }

        public static ApplicationUser User(this IIdentity user)
        {
            var thisUser = db.Users.FirstOrDefault(u => u.UserName == user.Name);
            if (thisUser != null)
            {
                return thisUser;
            }
            else
            {
                return null;
            }
        }

        public static string FirstName(this IIdentity user)
        {
            var firstName = db.Users.FirstOrDefault(u => u.UserName == user.Name);
            if (firstName.DisplayName != null)
            {
                return firstName.FirstName;
            }
            else
            {
                return null;
            }
        }

        public static string LastName(this IIdentity user)
        {
            var lastName = db.Users.FirstOrDefault(u => u.UserName == user.Name);
            if (lastName.DisplayName != null)
            {
                return lastName.LastName;
            }
            else
            {
                return null;
            }
        }

        public static string DisplayName(this IdentityUserRole user)
        {
            var disName = db.Users.Find(user.UserId);
            if (disName.DisplayName != null)
            {
                return disName.DisplayName;
            }
            else
            {
                return null;
            }
        }

        public static string RoleName(this IdentityUserRole user)
        {
            var Role = db.Roles.Find(user.RoleId);
            if (Role.Name != null)
            {
                return Role.Name;
            }
            else
            {
                return null;
            }
        }

        public static ICollection<Project> UserProjects (this IdentityUserRole user)
        {
            var User = db.Users.Find(user.UserId);
            var userProjects = new List<Project>();
            if (User.Projects != null)
            {
                foreach(var pr in User.Projects)
                {
                    userProjects.Add(pr);
                }
                return userProjects;
            }
            else
            {
                return null;
            }
        }
    }

    public static class DisplayHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static string DisplayName(string userId)
        {
            return db.Users.Find(userId).DisplayName;
        }

        public static string FirstName(string userId)
        {
            return db.Users.Find(userId).FirstName;
        }

        public static string LastName(string userId)
        {
            return db.Users.Find(userId).LastName;
        }

        public static ApplicationUser User(string userId)
        {
            return db.Users.Find(userId);
        }

        public static DateTimeOffset AdjustTime(DateTimeOffset currentDateTime, int Offset)
        {
            DateTimeOffset AdjustedTime = currentDateTime.ToUniversalTime().AddMinutes(-Offset);
            return AdjustedTime;
        }
    }
}