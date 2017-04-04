using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Helpers
{
    public class ProjectsHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public bool IsUserOnProject(string userId, int projectId)
        {
            if (db.Projects.Find(projectId).Users.Contains(db.Users.Find(userId)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ICollection<Project> ListUserProjects(string userId)
        {
            List<Project> ProjectsList = new List<Project>();
            UserRolesHelpers ur = new UserRolesHelpers();
            if (ur.IsUserInRole(userId, "Admin"))
            {
                ProjectsList = db.Projects.ToList();
            }
            else
            {
                var user = db.Users.Find(userId);
                ProjectsList = user.Projects.ToList();
            }

            return ProjectsList;
        }

        public List<int> ListUserProjectId(string userId)
        {
            var userProjects = ListUserProjects(userId);
            List<int> userProjectIds = new List<int>();
            foreach (var up in userProjects)
            {
                var projectId = db.Projects.Find(up.Id).Id;
                userProjectIds.Add(projectId);
            }
            return userProjectIds;
        }

        public void AddToProject(string userId, int projectId)
        {
            db.Projects.Find(projectId).Users.Add(db.Users.Find(userId));
            db.Users.Find(userId).Projects.Add(db.Projects.Find(projectId));
        }

        public void RemoveFromProject(string userId, int projectId)
        {
            db.Projects.Find(projectId).Users.Remove(db.Users.Find(userId));
            db.Users.Find(userId).Projects.Remove(db.Projects.Find(projectId));

        }


    }
}