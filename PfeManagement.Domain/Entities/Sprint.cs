using System;
using System.Collections.Generic;
using PfeManagement.Domain.Common;

namespace PfeManagement.Domain.Entities
{
    public class Sprint : SoftDeletableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int OrderIndex { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public virtual ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();

        // Creator Pattern: Sprint creates UserStory objects
        public UserStory AddUserStory(string name, string description, int points, Enums.Priority priority, DateTime due)
        {
            var userStory = new UserStory
            {
                StoryName = name,
                Description = description,
                StoryPointEstimate = points,
                Priority = priority,
                DueDate = due,
                StartDate = DateTime.UtcNow,
                SprintId = this.Id
            };
            
            UserStories.Add(userStory);
            return userStory;
        }
    }
}
