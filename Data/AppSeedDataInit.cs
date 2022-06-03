using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveRequestAPP.Data
{
    public static class AppSeedDataInit
    {
        public static void SeedData(this IApplicationBuilder app, RoleManager<ApplicationRole> role)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<ApplicationDbContext>(), role);
            }
        }

        public static void SeedData(ApplicationDbContext context, RoleManager<ApplicationRole> role)
        {
            //context.Database.SetCommandTimeout(100000000);
            context.Database.Migrate();

            //Create All different kind of roles in the system
            if (!role.RoleExistsAsync(RoleType.Manager.ToString()).Result)
            {
                ApplicationRole theAdmin = new ApplicationRole
                {
                    Name = RoleType.Manager.ToString(),
                    NormalizedName = "Manager",
                    Description = "Manager Role",
                    Status = "Active",
                };
                var createRole = role.CreateAsync(theAdmin).Result;
            }

            if (!role.RoleExistsAsync(RoleType.User.ToString()).Result)
            {
                ApplicationRole theUser = new ApplicationRole
                {
                    Name = RoleType.User.ToString(),
                    NormalizedName = "User",
                    Description = "User Role",
                    Status = "Active",
                };
                var createRole = role.CreateAsync(theUser).Result;
            }

            if (!role.RoleExistsAsync(RoleType.HumanResource.ToString()).Result)
            {
                ApplicationRole theUser = new ApplicationRole
                {
                    Name = "Human Resource",
                    NormalizedName = RoleType.HumanResource.ToString(),
                    Description = "Human resource role",
                    Status = "Active",
                };
                var createRole = role.CreateAsync(theUser).Result;
            }

            if (!role.RoleExistsAsync(RoleType.Accountant.ToString()).Result)
            {
                ApplicationRole theUser = new ApplicationRole
                {
                    Name = "Accountant",
                    NormalizedName = RoleType.Accountant.ToString(),
                    Description = "Accountant role",
                    Status = "Active",
                };
                var createRole = role.CreateAsync(theUser).Result;
            }

            if (!role.RoleExistsAsync(RoleType.BusinessDeveloment.ToString()).Result)
            {
                ApplicationRole theUser = new ApplicationRole
                {
                    Name = "Business Development",
                    NormalizedName = RoleType.BusinessDeveloment.ToString(),
                    Description = "Business Development role",
                    Status = "Active",
                };
                var createRole = role.CreateAsync(theUser).Result;
            }

            if (!role.RoleExistsAsync(RoleType.Administration.ToString()).Result)
            {
                ApplicationRole theUser = new ApplicationRole
                {
                    Name = "Administration",
                    NormalizedName = RoleType.Administration.ToString(),
                    Description = "Administration role",
                    Status = "Active",
                };
                var createRole = role.CreateAsync(theUser).Result;
            }

            if (!role.RoleExistsAsync(RoleType.SoftwareEngineer.ToString()).Result)
            {
                ApplicationRole theUser = new ApplicationRole
                {
                    Name = "Software Deployment Engineer",
                    NormalizedName = RoleType.SoftwareEngineer.ToString(),
                    Description = "Software Deployment Engineer role",
                    Status = "Active",
                };
                var createRole = role.CreateAsync(theUser).Result;
            }

            if (!role.RoleExistsAsync(RoleType.PetroleumEngineer.ToString()).Result)
            {
                ApplicationRole theUser = new ApplicationRole
                {
                    Name = "Petroleum Engineer",
                    NormalizedName = RoleType.PetroleumEngineer.ToString(),
                    Description = "Petroleum Engineer role",
                    Status = "Active",
                };
                var createRole = role.CreateAsync(theUser).Result;
            }

            context.SaveChanges();
        }
    }
}
