using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;

namespace TES.Services.Dto.ActiveTestDto
{
    public class TestStartDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid TestId { get; set; }
    }
}
