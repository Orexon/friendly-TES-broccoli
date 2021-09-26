using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;
using TES.Domain.Helpers;

namespace TES.Services.Dto.TestDto
{
    public class ActiveTestQuestionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TestType TestType { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public Int64 TimeLimit { get; set; }
        public TestLink TestUrl { get; set; }
        public List<Question> Questions { get; set; }
    }
}
