using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RBACapi.Models;

namespace RBACapi.Data.EntityConfigurations
{
    public class CM_USERS_AUTHORIZE_Configuration : IEntityTypeConfiguration<CM_USERS_AUTHORIZE>
    {
        public void Configure(EntityTypeBuilder<CM_USERS_AUTHORIZE> builder)
        {
            builder.ToTable("CM_USERS_AUTHORIZE");

            builder.HasKey(e => e.AUTH_CODE);

            builder.Property(e => e.AUTH_CODE)
                .IsRequired();

            builder.HasOne<CM_APPS_ROLES>()
                .WithMany()
                .HasForeignKey(e => e.ROLE_CODE)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<CM_APPLICATIONS>()
                .WithMany()
                .HasForeignKey(e => e.APP_CODE)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}