using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBACapi.Models;

namespace RBACapi.Data.EntityConfigurations
{
    public class CM_APPS_ROLES_Configuration : IEntityTypeConfiguration<CM_APPS_ROLES>
    {
        public void Configure(EntityTypeBuilder<CM_APPS_ROLES> builder)
        {
            builder.ToTable("CM_APPS_ROLES");
            builder.HasKey(e => e.ROLE_CODE);

            builder.Property(e => e.ROLE_CODE).IsRequired();

            builder.HasOne(e => e.CM_APPLICATIONS)
                    .WithMany()
                    .HasForeignKey(e => e.APP_CODE)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}