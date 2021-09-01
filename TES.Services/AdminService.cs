using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TES.Data;
using TES.Domain;
using TES.Services.Dto.AdminDto;
using TES.Services.Interface;

namespace TES.Services
{
    public class AdminService : IAdminService
    {
        private readonly Context _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminService(Context context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<AppUser> GetAllUsers()
        {
            List<AppUser> appUsers = _context.Users.ToList();
            return appUsers;
        }

        public AppUserDto GetUserById(Guid id)
        {
            if(!UserExists(id))
            {
                throw new ArgumentNullException($"User with {id} does not exist");
            }
            AppUser user = _context.Users.Where(x=>x.Id == id).FirstOrDefault();

            AppUserDto appUserDto = new AppUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
            };
            return appUserDto;
        }

        public async Task<bool> CreateAdmin(NewAdminDto adminDto)
        {
            var usernameExists = _context.Users.Where(x => x.UserName == adminDto.Username).FirstOrDefault();
            if (usernameExists != null)
            {
                throw new ArgumentException("User with that Username already exists!");
            }

            var emailExists = _context.Users.Where(x => x.Email == adminDto.Email).FirstOrDefault();
            if (emailExists != null)
            {
                throw new ArgumentException("User with that Username already exists!");
            }

            AppUser user = new AppUser()
            {
                Email = adminDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = adminDto.Username,
                Firstname = adminDto.FirstName,
                Lastname = adminDto.LastName,
            };

            var result = await _userManager.CreateAsync(user, adminDto.ConfirmPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("User creation failed! Please check user details and try again.");
            }

            var addedToRole = await _userManager.AddToRoleAsync(user, "Admin");
            if(!result.Succeeded)
            {
                throw new InvalidOperationException("User creation failed! User was not added to Admin role.");
            }
            return true;
        }

        public async Task<int> DeleteUser(Guid id)
        {
            AppUserDto userDto = GetUserById(id);
            AppUser user = _context.Users.Where(x => x.Id == userDto.Id).FirstOrDefault();
            if(user == null)
            {
                throw new ArgumentException("Such user does not exists anymore!");
            }

            try
            {
                _context.Users.Remove(user);
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!UserExists(id))
            {
                throw new InvalidOperationException("Something went wrong while deleting the user!");
            };
        }

        public async Task<bool> UpdateUser(EditAppUserDto appUserDto)
        {
            if (appUserDto == null)
            {
                throw new ArgumentException("Edit data not found, please try again!");
            }

            AppUser user = _context.Users.Where(x => x.Id == appUserDto.Id).FirstOrDefault();


            user.Firstname = appUserDto.Firstname;
            user.Lastname = appUserDto.Lastname;
            user.UserName = appUserDto.Username;
            user.Email = appUserDto.Email;

            if (appUserDto.ConfirmPassword == null)
            {
                throw new ArgumentException("Edit data not found, please try again!");
            }
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, appUserDto.ConfirmPassword);

            if (!UserExists(appUserDto.Id))
            {
                throw new ArgumentNullException("Such user no longer exist");
            }

            try
            {
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException) when (!UserExists(appUserDto.Id))
            {
                throw new ArgumentNullException($"User with {appUserDto.Id} does not exist, please check again!");
            }
        }

        public bool UserExists(Guid id) 
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
