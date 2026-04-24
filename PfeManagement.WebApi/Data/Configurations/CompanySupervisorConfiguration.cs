using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Data.Configurations
{
    public class CompanySupervisorConfiguration : IEntityTypeConfiguration<CompanySupervisor>
    {
        public void Configure(EntityTypeBuilder<CompanySupervisor> builder)
        {
            builder.ToTable("CompanySupervisors");
        }
    }
}
