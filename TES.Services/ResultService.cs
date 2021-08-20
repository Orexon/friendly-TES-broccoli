using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Data;
using TES.Domain;
using TES.Services.Interface;

namespace TES.Services
{
    public class ResultService : IResultService
    {
        private readonly Context _context;

        public ResultService(Context context)
        {
            _context = context;
        }

        public async Task<List<TestResult>> GetAllResults()
        {
            List<TestResult> results = await _context.Results.Include(x => x.Test).ToListAsync();
            return results;
        }

        public async Task<List<TestResult>> GetTestResults(Guid id)
        {
            if (!TestExists(id))
            {
                throw new ArgumentNullException($"Test with {id} does not exist");
            }
            if(TestResultsEmpty(id))
            {
                throw new ArgumentException("Test has no results yet");
            }

            List<TestResult> testResults = await _context.Results.Include(x => x.Test.Name).Where(x => x.Test.Id == id).ToListAsync(); 
            return testResults;
        }

        public bool TestExists(Guid id) =>
        _context.Tests.Any(e => e.Id == id);

        public bool TestResultsEmpty(Guid id)
        {
            Test test = _context.Tests.Where(x => x.Id == id).Include(c=>c.Results).FirstOrDefault(); //gets the test with Results.
            return test.Results.Any(); //Checks if there are any results.
        }
       
    }
}
