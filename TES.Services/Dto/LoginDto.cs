using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TES.Services.Dto
{
    public class LoginDto
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password Is Required.")]
        [DataType(DataType.Password, ErrorMessage = "The Password must have UpperCase and lowercase characters & Special Character")]
        [StringLength(30, ErrorMessage = "The password must be atleast 6 and at max 30 characters long.", MinimumLength = 6)]
        public string  Password { get; set; }
    }
}