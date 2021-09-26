using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;

namespace TES.Services.Dto.ActiveTestDto
{
    public class SubmitUserSolutionDto
    {
        public Guid TestId { get; set; }
        public Guid QuestionID { get; set; } 
        public IFormFile SubmitedFile { get; set; }
        public string Email { get; set; }
    }
}
