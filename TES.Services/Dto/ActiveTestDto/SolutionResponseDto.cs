using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TES.Services.Dto.ActiveTestDto
{
    public class SolutionResponseDto
    {
        public Guid TestId { get; set; }
        public Guid ApplicantId { get; set; }
        public Guid QuestionId { get; set; }
        public int QuestionWorth { get; set; }
    }
}
