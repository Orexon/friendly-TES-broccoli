using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain.Helpers;

namespace TES.Domain
{
    public class TestResult
    {
        public Guid Id { get; set; }
        public Test Test { get; set; }
        public UniqueApplicant Applicant { get; set; }
        public int MinutesOvertime { get; set; }
        public DateTime StartedAt { get; set; }
        public List<UserSolution> UserSolutions { get; set; } 
        public double TotalPoints { get; set; }
    }
}
