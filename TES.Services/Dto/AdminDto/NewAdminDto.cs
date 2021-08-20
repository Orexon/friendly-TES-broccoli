using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TES.Services.Dto.AdminDto
{
    public class NewAdminDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password Is Required.")]
        [DataType(DataType.Password, ErrorMessage = "The Password must have UpperCase and lowercase characters & Special Character")]
        [StringLength(30, ErrorMessage = "The {0} must be atleast {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password Is Required.")]
        [DataType(DataType.Password, ErrorMessage = "The Password must have UpperCase and lowercase characters & Special Character")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do no match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "FirstName Is Required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName Is Required.")]
        public string LastName { get; set; }
    }
}
