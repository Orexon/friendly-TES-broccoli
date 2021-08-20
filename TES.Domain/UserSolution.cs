using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain.Helpers;

namespace TES.Domain
{
    public class UserSolution
    {
        public Guid Id { get; set; }
        public Test Test { get; set; } 
        public Question Question { get; set; } 
        public string SubmitedFilePath { get; set; } 
        public UniqueApplicant AplicantId { get; set; }
        public double PointsScored { get; set; }
    }
}
