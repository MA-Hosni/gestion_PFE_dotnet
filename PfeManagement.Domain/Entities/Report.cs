using System;
using PfeManagement.Domain.Common;

namespace PfeManagement.Domain.Entities
{
    public class Report : SoftDeletableEntity
    {
        public int VersionLabel { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }
    }
}
