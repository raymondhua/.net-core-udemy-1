using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using BulkyBook.Utility;
using Microsoft.Extensions.Configuration;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db, IConfiguration iConfiguration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void Initialize()
        {
            //migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }
            //create roles if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();
            }
            // if roles are not created, it gets created
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "userName@example.com",
                Email = "userName@example.com",
                Name = "Ray Jackson",
                PhoneNumber = "0271938485",
                StreetAddress = "Test 12 Ave",
                State = "Otago",
                PostalCode = "9023",
                City = "Dunedin"
            }, "Admin@123").GetAwaiter().GetResult();

            ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "userName@example.com");
            _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            return;
            //string initialAdminEmail = _configuration.GetConnectionString("InitialAdminSettings:Email");
            //string initialAdminPassword = _configuration.GetConnectionString("InitialAdminSettings:Password");
            //// if roles are not created, it gets created
            //_userManager.CreateAsync(new ApplicationUser
            //{
            //    UserName = initialAdminEmail,
            //    Email = initialAdminEmail,
            //    Name = "Ray Jackson",
            //    PhoneNumber = "0271938485",
            //    StreetAddress = "Test 12 Ave",
            //    State = "Otago",
            //    PostalCode = "9023",
            //    City = "Dunedin"
            //}, initialAdminPassword).GetAwaiter().GetResult();

            //ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == initialAdminEmail);
            //_userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            //return;
        }
    }
}
