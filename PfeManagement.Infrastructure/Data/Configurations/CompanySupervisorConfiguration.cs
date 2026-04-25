using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Infrastructure.Data.Configurations
{
    public class CompanySupervisorConfiguration : IEntityTypeConfiguration<CompanySupervisor>
    {
        public void Configure(EntityTypeBuilder<CompanySupervisor> builder)
        {
            builder.ToTable("CompanySupervisors");
        }
    }
}
