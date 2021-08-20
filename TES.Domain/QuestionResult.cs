using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TES.Domain
{
    public class QuestionResult
    {
        public Guid Id { get; set; }
        public Test Test { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserSolution UserSolutions { get; set; }
        public int Score { get; set; }
    }
}
