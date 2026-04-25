using System;
using System.Collections.Generic;
using PfeManagement.Domain.Common;

namespace PfeManagement.Domain.Entities
{
    public class Project : SoftDeletableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Mappings for Many-to-Many / One-to-Many
        // A Project can have multiple Student contributors
        public virtual ICollection<Student> Contributors { get; set; } = new List<Student>();
        
        // Composition: Sprints and Reports are owned by Project
        public virtual ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();

        // Domain Logic (Information Expert)
        public double CalculateProgress()
        {
            // Calculate progress based on Sprints -> UserStories -> Tasks
            int totalTasks = 0;
            int completedTasks = 0;

            foreach (var sprint in Sprints)
            {
                foreach (var us in sprint.UserStories)
                {
                    foreach (var task in us.Tasks)
                    {
                        totalTasks++;
                        if (task.Status == Enums.TaskItemStatus.Done)
                        {
                            completedTasks++;
                        }
                    }
                }
            }

            return totalTasks == 0 ? 0 : (double)completedTasks / totalTasks * 100;
        }
    }
}
