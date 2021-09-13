using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;
using TES.Domain.Helpers;

namespace TES.Services.Dto.QuestionDto
{
    public class NewQuestionDto
    {
        [Required(ErrorMessage = "Question must have a name!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Question must have a description!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Question must have a specified type!")]
        public QuestionType QuestionType { get; set; }

        [Required(ErrorMessage = "Question must have a correct solution!")]
        public string CorrectSolutionPath { get; set; }

        [Required(ErrorMessage = "Question must have a worth of points assigned!")]
        [Range(0,100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int WorthOfPoints { get; set; }
    }
}
