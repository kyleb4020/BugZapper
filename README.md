# BugZapper

<---Features--->

This is a fully functional Bug Tracking app. Use this app to manage a team of developers as you tackle the bugs in your software. Admins can manage all user roles, create projects, assign tickets to projects, and assign tickets to users. Project managers can manage their team of developers for the projects to which they are assigned. Developers can edit tickets as they do work on the bugs. Submitters can create tickets, edit tickets, and view the status of their created tickets.

All who register are automatically assigned to the Submitter role. This would allow for end users to register with the software and begin submitting tickets immediately following registration. Submitters can view a list of tickets they have submitted and see the project manager responsible for overseeing their ticket.

Once a ticket is submitted, it is moved to the Unassigned project where an Admin or Project Manager assigned to the Unassigned project can move the ticket to the appropriate project and assign a developer to begin working on the project.

Developers have the ability to view and edit the tickets to which they are assigned. Developers receive email notifications as well as internal notifications when activity occurs on their tickets by a user other than themselves. Developers also receive a notification when they are assigned or unassigned to a project.

Admins have access to all features in the software, including the creation of a ticket. Admins have the sole right to assign or unassign other users to roles, thus enabling team management.


<---Download and Use Instructions--->

After dowloading/cloning this project to your local machine, open in Visual Studio 2015 Community or greater.
Once open, update the references and NuGet packages.
Then, open the package manager console and update the database (this will create a local database on your machine and seed in the demo users)
Once you have complete the above steps, this application will build and you can render it in the internet browser of your choice.
