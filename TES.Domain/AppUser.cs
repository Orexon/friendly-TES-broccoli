using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TES.Domain
{
    public class AppUser : IdentityUser<Guid> //<Guid> -> Converting Default ID from string to Guid.
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
