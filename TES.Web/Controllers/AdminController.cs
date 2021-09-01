using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TES.Domain;
using TES.Services.Dto;
using TES.Services.Dto.AdminDto;
using TES.Services.Interface;

namespace TES.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IAdminService _adminService;

        public AdminController(UserManager<AppUser> userManager, IAdminService adminService)
        {
            _userManager = userManager;
            _adminService = adminService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getAllUsers")]
        public IActionResult GetAllUsers()
        {
            List<AppUser> appUsers = _adminService.GetAllUsers();

            return Ok(appUsers);
        }

        // GET: api/Admin/getUser/Guid
        [HttpGet]
        [Route("getUser/{id}")]
        [Produces(typeof(AppUserDto))]
        public IActionResult GetUserById(Guid id)
        {
            try
            {
                AppUserDto appUser = _adminService.GetUserById(id);
                return Ok(appUser);
                //  return Ok(_adminService.GetUserById(id)); viable?
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }    
        }

        [HttpPost]
        [Route("createAdmin")]
        [Produces(typeof(NewAdminDto))]
        public async Task<IActionResult> CreateAdmin([FromBody] NewAdminDto adminDto)
        {
            try
            {
                await _adminService.CreateAdmin(adminDto);
                return Ok(new ResponseDto { Status = "Success", Message = "Admin created successfully!" });
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        // DELETE: api/Admin/deleteUser/Guid
        [HttpDelete]
        [Route("deleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _adminService.DeleteUser(id);
                return Ok(new ResponseDto { Status = "Success", Message = "User has been deleted!" });
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        //GetForEdit: api/Admin/editUser/Guid
        [HttpGet]
        [Route("editUser/{id}")]
        [Produces(typeof(AppUserDto))]
        public IActionResult UpdateUser(Guid id)
        {
            try
            {
                AppUserDto appUserDto = _adminService.GetUserById(id);
                return Ok(appUserDto);
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        //PATCH:"Edit" api/Admin/editUser/Guid
        [HttpPatch]
        [Route("editTest/{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] EditAppUserDto appUserDto)
        {
            try
            {
                await _adminService.UpdateUser(appUserDto); 
                return Ok(new ResponseDto { Status = "Success", Message = "User data been updated!" });
            }
            catch (Exception e)
            {

                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }            
        }
    }
}  