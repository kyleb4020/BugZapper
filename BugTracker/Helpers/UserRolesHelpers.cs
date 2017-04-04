using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Helpers
{
    public class UserRolesHelpers
    {
        private UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>
            (new UserStore<ApplicationUser>(new ApplicationDbContext()));

        private ApplicationDbContext db = new ApplicationDbContext();

        public bool IsUserInRole(string userId, string roleName)
        {
            return userManager.IsInRole(userId, roleName);
        }

        public ICollection<string> ListUserRoles(string userId)
        {
            return userManager.GetRoles(userId);
        }

        public ICollection<ApplicationUser> ListUsersInRole(string roleName)
        {
            var users = new List<ApplicationUser>();
            var allUsers = db.Users.ToList();
            foreach (var au in allUsers)
            {
                if (IsUserInRole(au.Id, roleName))
                {
                    users.Add(au);
                }
            }
            return users;
        }
        public List<string> ListUserRolesId(string userId)
        {
            var userRoles = ListUserRoles(userId);
            List<string> userRolesIds = new List<string>();
            foreach (var ur in userRoles)
            {
                var roleId = db.Roles.FirstOrDefault(r => r.Name == ur).Id;
                userRolesIds.Add(roleId);
            }
            return userRolesIds;
        }

        public List<ApplicationUser> ListUsersOnMyProjects(string userId)
        {
            var User = db.Users.Find(userId);
            List<ApplicationUser> usersList = new List<ApplicationUser>();
            if (IsUserInRole(userId, "Admin"))
            {
                usersList = db.Users.ToList();
            }
            else if (IsUserInRole(userId, "Project Manager"))
            {
                usersList = new List<ApplicationUser>();
                var userProjects = User.Projects.ToList();
                foreach (var pro in userProjects)
                {
                    var proList = pro.Users.ToList();
                    foreach (var ulist in proList)
                    {
                        if (!usersList.Contains(ulist))
                        {
                            usersList.Add(ulist);
                        }
                    }
                }
            }
            return usersList;
        }

        public List<ApplicationUser> ListDevsOnMyProjects(string userId)
        {
            var User = db.Users.Find(userId);
            var allUsers = db.Users.ToList();
            List<ApplicationUser> devList = new List<ApplicationUser>();
            if (IsUserInRole(userId, "Admin"))
            {
                foreach (var u in allUsers)
                {
                    if (IsUserInRole(u.Id, "Developer"))
                    {
                        devList.Add(u);
                    }
                }
            }
            else if (IsUserInRole(userId, "Project Manager"))
            {
                var userProjects = User.Projects.ToList();
                foreach (var pro in userProjects)
                {
                    var proList = pro.Users.ToList();
                    foreach (var us in proList)
                    {
                        if(IsUserInRole(us.Id, "Developer"))
                        {
                            if (!devList.Contains(us))
                            {
                                devList.Add(us);
                            }
                        }
                    }
                }
            }
            return devList;
        }

        public bool AddUserToRole(string userId, string roleName)
        {
            var result = userManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }

        public bool AddOrUpdateUserToRole(string userId, string roleName)
        {
            if (!IsUserInRole(userId, roleName))
            {
                var result = userManager.AddToRole(userId, roleName);
                return result.Succeeded;
            }
            return false;
        }

        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = userManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        public bool RemoveOrUpdateUserFromRole(string userId, string roleName)
        {
            if (IsUserInRole(userId, roleName))
            {
                var result = userManager.RemoveFromRole(userId, roleName);
                return result.Succeeded;
            }
            return false;
        }

        public ICollection<ApplicationUser> UsersInRole(string roleName)
        {
            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();
            foreach (var user in List)
            {
                if (IsUserInRole(user.Id, roleName))
                {
                    resultList.Add(user);
                }
            }
            return resultList;
        }

        public ICollection<ApplicationUser> UsersNotInRole(string roleName)
        {
            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();
            foreach (var user in List)
            {
                if (!IsUserInRole(user.Id, roleName))
                {
                    resultList.Add(user);
                }
            }
            return resultList;
        }
    }
}