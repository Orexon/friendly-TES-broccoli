using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain.Helpers;

namespace TES.Services.Dto.TestDto
{
    public class NewTestQuestionDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public QuestionType QuestionType { get; set; }
        public IFormFile SubmittedSolution { get; set; }
        public int WorthOfPoints { get; set; }
    }
}
