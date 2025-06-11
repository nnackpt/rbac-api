using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBACapi.Models;

namespace RBACapi.Data.EntityConfigurations
{
    public class CM_APPLICATIONS_Configuration : IEntityTypeConfiguration<CM_APPLICATIONS>
    {
        public void Configure(EntityTypeBuilder<CM_APPLICATIONS> builder)
        {
            builder.ToTable("CM_APPLICATIONS");
            builder.HasKey(e => e.APP_CODE);

            builder.Property(e => e.APP_CODE).IsRequired();
            builder.Property(e => e.UPDATED_DATETIME);
        }
    }
}