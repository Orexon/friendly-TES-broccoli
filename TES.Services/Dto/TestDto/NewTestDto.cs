using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;
using TES.Domain.Helpers;

namespace TES.Services.Dto.TestDto
{
    public class NewTestDto
    {
        [StringLength(maximumLength: 24, MinimumLength = 3, ErrorMessage = "The test Name must be atleast 3 and at max 24 characters long.")]
        [Required(ErrorMessage = "Test must be named!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Test must contain a description!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Test must contain questions!")]
        public List<NewTestQuestionDto> Questions { get; set; }

        [Required(ErrorMessage = "TestType is required!")]
        public TestType TestType { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "The datetime validFrom must be a valid time!")]
        [Required(ErrorMessage = "ValidFrom datetime is required!")]
        public DateTime ValidFrom { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "The datetime validTo must be a valid time!")]
        [Required(ErrorMessage = "ValidTo datetime is required!")]
        public DateTime ValidTo { get; set; }

        [Required(ErrorMessage = "TimeLimit is required!")]
        public TimeSpan TimeLimit { get; set; }
    }
}
