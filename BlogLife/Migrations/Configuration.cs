namespace BlogLife.Migrations
{
    using BlogLife.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    

    internal sealed class Configuration : DbMigrationsConfiguration<BlogLife.Models.ApplicationDbContext>
    {
        
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
        protected override void Seed(BlogLife.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            if(!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }
            if (!context.Roles.Any(r => r.Name == "Moderator"))
            {
                roleManager.Create(new IdentityRole { Name = "Moderator" });
            }
            
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "brandon.o.swaney@gmail.com"))
            {
                userManager.Create(new ApplicationUser()
                {
                    Email = "brandon.o.swaney@gmail.com",
                    UserName = "brandon.o.swaney@gmail.com",
                    FirstName = "Brandon",
                    LastName = "Swaney",
                    DisplayName = "2ShortGaming"
                }, "Biggin112!");

                //get the id that just created by adding the above user
                var userId = userManager.FindByEmail("brandon.o.swaney@gmail.com").Id;

                userManager.AddToRole(userId, "Admin");
            }
            
            if(!context.Users.Any(u => u.Email == "arussell@coderfoundry.com"))
            {
                userManager.Create(new ApplicationUser()
                {
                    Email = "arussell@coderfoundry.com",
                    UserName = "arussell@coderfoundry.com",
                    FirstName = "Andrew",
                    LastName = "Russell",
                    DisplayName = "AndyStache"
                }, "Abc&123!");

                var modId = userManager.FindByEmail("arussell@coderfoundry.com").Id;
                userManager.AddToRole(modId, "Moderator");
            }

            
        } 
    }
}
