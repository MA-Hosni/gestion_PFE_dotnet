using System;
using System.Collections.Generic;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.Entities
{
    public class Student : User
    {
        public string Cin { get; set; } = string.Empty;
        public string StudentIdCardIMG { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        
        public Degree Degree { get; set; }
        public DegreeType DegreeType { get; set; }
        
        // Navigation properties
        public Guid CompSupervisorId { get; set; }
        public virtual CompanySupervisor? CompSupervisor { get; set; }
        
        public Guid UniSupervisorId { get; set; }
        public virtual UniversitySupervisor? UniSupervisor { get; set; }
        
        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        
        public virtual ICollection<Meeting> CreatedMeetings { get; set; } = new List<Meeting>();
    }
}
