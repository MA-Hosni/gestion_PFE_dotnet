using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Infrastructure.Data.Configurations
{
    public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
    {
        public void Configure(EntityTypeBuilder<Meeting> builder)
        {
            builder.ToTable("Meetings");

            builder.HasOne(m => m.CreatedBy)
                .WithMany(s => s.CreatedMeetings)
                .HasForeignKey(m => m.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Validator)
                .WithMany()
                .HasForeignKey(m => m.ValidatorId)
                .OnDelete(DeleteBehavior.SetNull); 
        }
    }
}
