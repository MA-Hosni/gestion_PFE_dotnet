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

        public void ValidateUserStoryDueDate(DateTime dueDate)
        {
            if (dueDate < StartDate || dueDate > EndDate)
            {
                throw new PfeManagement.Domain.Exceptions.DomainConstraintException("[OCL] UserStoriesWithinSprint - la date d'echeance d'une user story doit etre contenue dans les dates du sprint.");
            }
        }

        public void ValidateUserStoriesWithinSprint(IEnumerable<UserStory> stories, DateTime candidateStart, DateTime candidateEnd)
        {
            foreach (var us in stories)
            {
                if (us.DueDate < candidateStart || us.DueDate > candidateEnd)
                {
                    throw new PfeManagement.Domain.Exceptions.DomainConstraintException("[OCL] UserStoriesWithinSprint - la date d'echeance d'une user story doit etre contenue dans les dates du sprint.");
                }
            }
        }
    }
}
