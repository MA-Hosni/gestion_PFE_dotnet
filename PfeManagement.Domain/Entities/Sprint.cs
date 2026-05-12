using System;
using System.Collections.Generic;
using PfeManagement.Domain.Common;
using PfeManagement.Domain.Enums;

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

        /// <summary>
        /// GRASP Creator: Sprint aggregates UserStory instances and owns their lifecycle boundary.
        /// </summary>
        public UserStory AddUserStory(
            string storyName,
            string description,
            int storyPointEstimate,
            Priority priority,
            DateTime dueDate,
            DateTime startDateUtc)
        {
            var userStory = new UserStory
            {
                StoryName = storyName,
                Description = description,
                StoryPointEstimate = storyPointEstimate,
                Priority = priority,
                DueDate = dueDate,
                StartDate = startDateUtc,
                SprintId = Id,
                Sprint = this
            };

            UserStories.Add(userStory);
            return userStory;
        }
    }
}
