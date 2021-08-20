using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;

namespace TES.Services.Interface
{
    public interface IResultService
    {
       Task<List<TestResult>> GetAllResults();
       Task<List<TestResult>> GetTestResults(Guid id);
    }
}
