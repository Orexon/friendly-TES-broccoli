using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;

namespace TES.Data
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder builder)
        {

            //Create static Guid. 
            Guid ADMIN_ID = Guid.Parse("92DC875E-5FBF-4C4A-84C2-C5973AC096FD");
            Guid ADMIN_ROLE = Guid.Parse("36D7216E-6345-40C7-864D-3172693FEEBA");

            builder.Entity<IdentityRole<Guid>>().HasData(new IdentityRole<Guid>
            {
                Name = "Admin",
                NormalizedName = "ADMIN",
                Id = ADMIN_ROLE,
                ConcurrencyStamp = "27747190-7b7d-453d-ba7b-5bfa31119160"
            });

            //seed user
            builder.Entity<AppUser>().HasData(new AppUser
            {
                Id = ADMIN_ID,
                Firstname = "Nimda",
                Lastname = "Nimdaman",
                Email = "admin@admin.com",
                NormalizedEmail = "ADMIN@ADMIN.COM",
                EmailConfirmed = true,
                UserName = "SuperAdmin",
                NormalizedUserName = "SUPERADMIN",
                PasswordHash = "AQAAAAEAACcQAAAAEOpvPVTNsK5osJyR0T+4qh/+6m4CKrv7u+KH+rrB+ptHxAyVknaIysUmJm/UTPOhkw==",
                ConcurrencyStamp = "27747190-7b7d-453d-ba7b-5bfa31119160",
                SecurityStamp = "IHDXOW62GL33UAOIJKMU6JBSKSBC63JJ",
            });

            //set user role to admin
            builder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>
            {
                RoleId = ADMIN_ROLE,
                UserId = ADMIN_ID
            });
        }
    }
}
