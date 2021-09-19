using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Data;
using TES.Domain;
using TES.Services.Dto.TestDto;

namespace TES.Services.Interface
{
    public class TestService : ITestService
    {
        private readonly Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TestService(Context context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<Test>> GetAllTests()
        {
            List<Test> tests = await _context.Tests.Include(x=>x.UrlLinkId).ToListAsync();
            return tests;
        }

        public async Task<Test> GetTestById(Guid id)
        {

            if (!TestExists(id))
            {
                throw new ArgumentNullException($"Test with {id} does not exist");
            }
            Test test = await _context.Tests.Include(x => x.Questions).Include(x=>x.UrlLinkId).Where(x=>x.Id == id).FirstOrDefaultAsync();
            return test;
        }

        public async Task<EditTestDto> GetEditTestDto(Guid id)
        {

            if (!TestExists(id))
            {
                throw new ArgumentNullException($"Test with {id} does not exist");
            }
            Test test = await _context.Tests.Include(x => x.Questions).Include(x => x.UrlLinkId).Where(x => x.Id == id).FirstOrDefaultAsync();

            if (DateTime.Now >= test.ValidFrom && test.ValidTo >= DateTime.Now)
            {
                throw new ArgumentException("Can't modify Test while it's active!");
            }

            //var timelimit = TimeSpan.FromTicks(test.TimeLimit); Converting in Front-end.

            EditTestDto editTestDto = new EditTestDto
            {
                Id = test.Id,
                Name = test.Name,
                Description = test.Description,
                Questions = test.Questions, 
                ValidFrom = test.ValidFrom, 
                ValidTo = test.ValidTo,
                TimeLimit = test.TimeLimit,
                TestType = test.TestType,
            };
            return editTestDto;
        }

        public async Task<bool> UpdateTest(EditTestDto editTestDto)
        {

            if (editTestDto == null)
            {
                throw new ArgumentException("Edit data not found, please try again!");
            }

            if (!TestExists(editTestDto.Id))
            {
                throw new ArgumentException("Test no longer exists");
            }

            if (DateTime.Now > editTestDto.ValidTo)
            {
                throw new ArgumentException("Valid To can't be a past date. Test must be valid to a certain day in future!");
            }
            if (editTestDto.ValidFrom > DateTime.Now)
            {
                throw new ArgumentException("Valid From can't be a past date. Test must be valid from a future date!");
            }
            if (editTestDto.ValidFrom > editTestDto.ValidTo)
            {
                throw new ArgumentException("Valid From must be a higher date than Valid To!");
            }
            if (!editTestDto.Questions.Any())
            {
                throw new ArgumentException("Test must contain questions!");
            }

            Test test = _context.Tests.Where(x => x.Id == editTestDto.Id).FirstOrDefault();

            if (DateTime.Now >= test.ValidFrom && test.ValidTo >= DateTime.Now)
            {
                throw new ArgumentException("Can't modify Test while it's active!");
            }

            

            var TimeLimit = TimeSpan.FromMinutes(editTestDto.TimeLimit);

            test.Name = editTestDto.Name;
            test.Description = editTestDto.Description;
            test.Questions = editTestDto.Questions;
            test.ValidFrom = editTestDto.ValidFrom;
            test.ValidTo = editTestDto.ValidTo;
            test.TimeLimit = TimeLimit.Ticks;
            test.TestType = editTestDto.TestType;

            try
            {
                _context.Tests.Update(test);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException) when (!TestExists(test.Id))
            {
                throw new ArgumentException("Test update failed while saving test, please try again!");
            }
        }

        public async Task<Test> CreateTest(NewTestDto newTest)
        {
            if (newTest.ValidTo < DateTime.Now) // If test is valid to right now. Test automatically is over. 
            {
                throw new ArgumentException("Valid To can't be a past date. Test must be valid to a certain day in future!");
            }
            if (newTest.ValidFrom <= DateTime.Now)
            {
                throw new ArgumentException("Valid From can't be a past date. Test must be valid from a future date!");
            }
            if (newTest.ValidFrom > newTest.ValidTo)
            {
                throw new ArgumentException("Valid From must be a higher date than Valid To!");
            }
            if (!newTest.Questions.Any())
            {
                throw new ArgumentException("Test must contain questions!");
            }

            TestLink tstlnk = NewTestLink();

            List<Question> questions = NewTestQuestion(newTest.Questions);

            //converting minutes to timeSpan & saving only ticks as int64 incase value is over 24hours.
            //timespan values over 24hours can't be saved in db. 

            var spanOfTime = TimeSpan.FromMinutes(newTest.TimeLimit);

            Test test = new Test
            {
                Name = newTest.Name,
                Description = newTest.Description,
                Questions = questions,
                TestType = newTest.TestType,
                CreateTime = DateTime.Now,
                ValidFrom = newTest.ValidFrom,
                ValidTo = newTest.ValidTo,
                TimeLimit = spanOfTime.Ticks,
                UrlLinkId = tstlnk,
            };

            await GenerateUrl(test.UrlLinkId.Id);

            try
            {
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new Exception("Test creation failed. Database failure found!");
            }

            return test;
        }

        public List<Question> NewTestQuestion(List<NewTestQuestionDto> questionList)
        {
            string uniqueFileName = null;
            List<Question> newTestQuestions = new List<Question>();
            foreach (var question in questionList)
            {
                if(question.SubmittedSolution == null)
                {
                    throw new ArgumentException("Didn't submit solution to a Question!");
                }   
                else if(question.SubmittedSolution != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + question.SubmittedSolution.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        question.SubmittedSolution.CopyToAsync(stream);
                    }
                }

                Question newQuestion = new Question
                {
                    Name = question.Name,
                    Description = question.Description,
                    QuestionType = question.QuestionType,
                    WorthOfPoints = question.WorthOfPoints,
                    SolutionFilePath = uniqueFileName
                };

                _context.Questions.Add(newQuestion);
                newTestQuestions.Add(newQuestion);
            }

            try
            {
                _context.SaveChangesAsync();
                return newTestQuestions;
            }
            catch (Exception)
            {
                throw new ArgumentException("Could not create Question.Database failure found!");
            }
        }

        public async Task<bool> DeleteTest(Guid id)
        {
            Test test = await GetTestById(id);

            if (DateTime.Now >= test.ValidFrom && test.ValidTo >= DateTime.Now)
            {
                throw new ArgumentException("Can't modify Test with while it's active!");
            }

            try
            {
                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException) when (!TestExists(id))
            {
                throw new ArgumentException("Test delete failed.Test not found!");
            }
        }
        
        private TestLink NewTestLink()
        {
            TestLink testLink = new TestLink();
            try
            {
                _context.TestLinks.Add(testLink);
                _context.SaveChangesAsync();
                return testLink;
            }
            catch (Exception)
            {
                throw new ArgumentException("TestLink creation failed!");
            }
        } 

        public async Task<bool> IsTestActive(Guid id)
        {
            Test test = await GetTestById(id);
            bool active = DateTime.Now >= test.ValidFrom && test.ValidTo >= DateTime.Now;
            return active;
        }

        public bool TestExists(Guid id) =>
        _context.Tests.Any(e => e.Id == id);
        
        public bool TestLinkIdExists(Guid id) =>
        _context.TestLinks.Any(e => e.Id == id);

        public async Task<bool> GenerateUrl(Guid id)
        {
            if (!TestLinkIdExists(id))
            {
                throw new ArgumentNullException($"TestLink with {id} does not exist");
            }
            TestLink testLink = _context.TestLinks.Where(x => x.Id == id).FirstOrDefault();
            testLink.UrlLink = "http://localhost:44372/activeTest/" + testLink.Id.ToString();

            try
            {
                _context.TestLinks.Update(testLink);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw new ArgumentException("Could not create TestLink.Try again!");
            }
        }
    }
}
