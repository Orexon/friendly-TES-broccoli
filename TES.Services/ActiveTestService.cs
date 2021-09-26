using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TES.Data;
using TES.Domain;
using TES.Services.Dto.ActiveTestDto;
using TES.Services.Dto.TestDto;
using TES.Services.Interface;

namespace TES.Services
{
    public class ActiveTestService : IActiveTestService
    {
        private readonly Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private const string ProjContent = "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>netstandard2.0</TargetFramework><RuntimeFrameworkVersion>3.1.202</RuntimeFrameworkVersion></PropertyGroup></Project>";
        public ActiveTestService(Context context, IWebHostEnvironment hostingEnvironment)
        {
            _webHostEnvironment = hostingEnvironment;
            _context = context;
        }

        public bool TestExists(Guid id) =>
        _context.Tests.Any(e => e.Id == id);

        public ActiveTestBeforeDto ActiveTest(Guid id)
        {
            TestLink testLink =  _context.TestLinks.Where(x => x.Id == id).FirstOrDefault();
            if (testLink == null)
            {
                throw new ArgumentException("TestLink not found!");
            }

            Test test = _context.Tests.Include(x=>x.UrlLinkId).Where(x => x.UrlLinkId == testLink).Include(x=>x.Questions).FirstOrDefault();

            if (!TestExists(test.Id))
            {
                throw new ArgumentException("Test no longer exists");
            }

            if (test.ValidTo < DateTime.Now)
            {
                throw new ArgumentException("Test active time has expired!");
            }
            if (test.ValidFrom > DateTime.Now)
            {
                throw new ArgumentException("Test is not yet active. Try again later!");
            }

            ActiveTestBeforeDto testDto = new ActiveTestBeforeDto
            {
                Id = test.Id,
                Name = test.Name,
                Description = test.Description,
                TestType = test.TestType,
                TimeLimit = test.TimeLimit,
                ValidFrom = test.ValidFrom,
                ValidTo = test.ValidTo,
                TestUrl = test.UrlLinkId,
            };
            return testDto;
        }

        public async Task<bool> StartTest(TestStartDto testStartDto)
        {
            if (UserTookTest(testStartDto.TestId, testStartDto.Email))
            {
                throw new ArgumentException("This user already took the test!");
            }

            UniqueApplicant applicant = await NewApplicant(testStartDto.Email, testStartDto.FirstName, testStartDto.LastName);

            Test test = _context.Tests.Where(x => x.Id == testStartDto.TestId).Include(x => x.UrlLinkId).Include(x => x.Questions).FirstOrDefault();

            TestResult result = new TestResult
            {
                Applicant = applicant,
                StartedAt = DateTime.Now,
                Test = test,
                TotalPoints = 0
            };

            try
            {
                _context.Results.Add(result);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw new ArgumentException("Test could not be started. Couldn't save start of the test. Try again!");
            }

        }

        public async Task<ActiveTestQuestionDto> GetTestQuestion(Guid id)
        {

            Test test = await _context.Tests.Where(x => x.Id == id).Include(x=>x.Questions).Include(x => x.UrlLinkId).FirstOrDefaultAsync();

            List<Question> questions =  test.Questions;

            List<ActiveQuestionDto> questionsDto = new List<ActiveQuestionDto>();

            foreach (var question in questions)
            {
                ActiveQuestionDto active = new ActiveQuestionDto
                {
                    Id = question.Id,
                    Description = question.Description,
                    Name = question.Name,
                    QuestionType = question.QuestionType,
                    WorthOfPoints = question.WorthOfPoints
                };
                questionsDto.Add(active);
            }

            ActiveTestQuestionDto aTQDTO = new ActiveTestQuestionDto
            {
                Id = test.Id,
                Description = test.Description,
                Name = test.Name,
                Questions = questionsDto,
                TestType = test.TestType,
                TestUrl = test.UrlLinkId,
                TimeLimit = test.TimeLimit,
                ValidFrom = test.ValidFrom,
                ValidTo = test.ValidTo
            };

            return aTQDTO;
        }

        private async Task<UniqueApplicant> NewApplicant(string email, string firstName, string lastName)
        {
            if (ApplicantExists(email))
            {
                throw new ArgumentException("This email is used!");
            }

            UniqueApplicant applicant = new UniqueApplicant
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            try
            {
                _context.Applicant.Add(applicant);
                await _context.SaveChangesAsync();
                return applicant;
            }
            catch (Exception)
            {
                throw new ArgumentException("Applicant creation failed. Database failure found!");
            }
        }

        public async Task<SolutionResponseDto> SubmitSolution(SubmitUserSolutionDto solutionDto)
        {
            if(solutionDto == null)
            {
                throw new ArgumentException("Solution data not found!");
            }

            string uniqueFileName = null;
           
            if(solutionDto.SubmitedFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads");  
                //AppContext.BaseDirectory; 

                uniqueFileName = Guid.NewGuid().ToString() + solutionDto.SubmitedFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await solutionDto.SubmitedFile.CopyToAsync(stream);
                }
            }

            UniqueApplicant aplicant = _context.Applicant.Where(x => x.Email == solutionDto.Email).FirstOrDefault();
            Question question = _context.Questions.Where(x => x.Id == solutionDto.QuestionID).FirstOrDefault();
            Test test = _context.Tests.Where(x => x.Id == solutionDto.TestId).FirstOrDefault();

            //Check if it's the first solution to this question, if not delete the last solution.
            bool SolutionAlreadyExists = _context.UserSolutions.Where(x => x.Test.Id == test.Id && x.Question.Id == question.Id && x.ApplicantId.Id == aplicant.Id).Any();
            if (SolutionAlreadyExists)
            {
                try
                {
                    UserSolution uniqueSolution = await _context.UserSolutions.Where(x => x.Test.Id == test.Id && x.Question.Id == question.Id && x.ApplicantId.Id == aplicant.Id).FirstOrDefaultAsync();
                    _context.UserSolutions.Remove(uniqueSolution);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    throw new ArgumentException("Could't delete user Solution active!");
                }
            }


            UserSolution userSolution = new UserSolution
            {
                ApplicantId = aplicant,
                Question = question,
                Test = test,
                SubmitedFilePath = uniqueFileName
            };

            try
            {
                _context.UserSolutions.Add(userSolution);
                await _context.SaveChangesAsync();

                SolutionResponseDto solutionResponseDto = new SolutionResponseDto
                {
                    ApplicantId = aplicant.Id,
                    QuestionId = question.Id,
                    TestId = test.Id,
                    QuestionWorth = question.WorthOfPoints
                };

                return solutionResponseDto;
            }
            catch (Exception)
            {
                throw new ArgumentException("Applicant creation failed. Database failure found!");
            }
        }

        public async Task<string> GetSolutionPath(Guid id)
        {
            Question question = await _context.Questions.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (question == null)
            {
                throw new ArgumentException("Data transfer failure, try again!");
            }
            return question.SolutionFilePath;
        }

        public async Task<string> GetUserSolutionPath(SolutionResponseDto solutionResponseDto)
        {
            if(solutionResponseDto == null)
            {
                throw new ArgumentException("Data transfer failure, try again!");
            }
            UserSolution userSolution = await _context.UserSolutions.Where(x => x.Test.Id == solutionResponseDto.TestId && x.Question.Id == solutionResponseDto.QuestionId && x.ApplicantId.Id == solutionResponseDto.ApplicantId).FirstOrDefaultAsync();
            return userSolution.SubmitedFilePath;
        }

        public bool ApplicantExists(string email)
        {
            return _context.Applicant.Any(x => x.Email == email);
        }

        public bool UserTookTest(Guid id, string email)
        {
            List<TestResult> result = _context.Results.Include(x => x.Applicant).Where(x => x.Test.Id == id).ToList();
            return result.Any(x => x.Applicant.Email == email);
        }

        public async Task<bool> SaveQuestionScore(SolutionResponseDto solutionResponseDto, double taskresult)
        {
            if (solutionResponseDto == null)
            {
                throw new ArgumentException("Data transfer failure, try again!");
            }
            UserSolution userSolution = await _context.UserSolutions.Where(x => x.Test.Id == solutionResponseDto.TestId && x.Question.Id == solutionResponseDto.QuestionId && x.ApplicantId.Id == solutionResponseDto.ApplicantId).FirstOrDefaultAsync();

            userSolution.PointsScored = taskresult;

            try
            {
                _context.UserSolutions.Update(userSolution);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw new ArgumentException("Could not update user score!");
            }
        }

        public async Task<double> FinishTest(FinishTestDto finishTestDto)
        {
            if (finishTestDto == null)
            {
                throw new ArgumentException("Solution data not found!");
            }

            UniqueApplicant applicant = _context.Applicant.Where(x => x.Email == finishTestDto.Email).FirstOrDefault();
            Test test = _context.Tests.Where(x => x.Id == finishTestDto.TestId).FirstOrDefault();

            TestResult testResult = await _context.Results.Where(x => x.Test.Id == test.Id && x.Applicant.Id == applicant.Id).FirstOrDefaultAsync();

            DateTime date1 = testResult.StartedAt;
            DateTime date2 = DateTime.Now;

            TimeSpan ts = date2 - date1;

            double ticks = test.TimeLimit;
            double days = Math.Floor(ticks / 864000000000);
            double hours = Math.Round(ticks / 36000000000) % 24;
            double mins = Math.Round((ticks / (60 * 10000000)) % 60);

            double totalHours = days * 24 + hours;
            double totalMinutes = totalHours * 60;
            double totalSeconds = totalMinutes * 60;

            TimeSpan testTimeLimit = TimeSpan.FromSeconds(totalSeconds); 
            if(ts > testTimeLimit)
            {
                double min = ts.TotalMinutes - testTimeLimit.TotalMinutes;
                int minutes = (int)Math.Round(min);
                testResult.MinutesOvertime = minutes;
            }
            
            List<UserSolution> userSolutions = await _context.UserSolutions.Where(x => x.Test.Id == test.Id && x.ApplicantId.Id == applicant.Id).ToListAsync();

            double TotalScore = 0;

            foreach (var solution in userSolutions)
            {
                TotalScore += solution.PointsScored;
            }
            testResult.TotalPoints = TotalScore;

            try
            {
                _context.Results.Update(testResult);
                await _context.SaveChangesAsync();
                return TotalScore;
            }
            catch (Exception)
            {
                throw new ArgumentException("Could not finalize user score!");
            }
        }

        public double TestScore(string argFile, string userSolution)
        {
            string content = File.ReadAllText(Path.Combine(_webHostEnvironment.ContentRootPath, $"Uploads/{userSolution}"));
            // File.ReadAllText(Path.Combine(AppContext.BaseDirectory, userSolution));
            // Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads/"+));
            bool canCompile = Compile(userSolution, content, out string generatedDll);
            if (!canCompile)
            {
                return 0;
            }
            //File.WriteAllText(userSolution, content); //for testing restores Task.cs delete later

            double score = RunTests(argFile, generatedDll);

           

            try
            {
                SafeDeleteFile(Path.Combine(AppContext.BaseDirectory, userSolution));
            }
            catch (Exception)
            {
                throw new ArgumentException("Could not delete temp file. Try again");
            }


            return score;
        }

        public static bool Compile(string fileName, string content, out string generatedDll)
        {
            var directory = AppContext.BaseDirectory;
            //"C:\\Users\\P3CK\\Desktop\\Examples\\TES Project\\Back-end\\tes-back-end\\TES.Web\\bin\\Debug\\net5.0"; 
            //Path.Combine(_webHostEnvironment.ContentRootPath, "CoreCompilerTempFiles");

            var fullName = Path.Combine(directory, fileName);
            File.WriteAllText(fullName, content);
            
            var globalJson = Path.Combine(directory, "global.json");            // 5.0.204 was before.
            File.WriteAllText(globalJson, "{\r\n  \"sdk\": {\r\n    \"version\": \"5.0.302\"\r\n  }\r\n}");

            var projFile = Path.ChangeExtension(fullName, ".csproj");
            File.WriteAllText(projFile, ProjContent);

            generatedDll = Path.Combine(directory, $"bin\\Debug\\netstandard2.0\\{Path.GetFileNameWithoutExtension(fullName)}.dll");

            //read on ProcessStartInfo 
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "build --no-dependencies --nologo",
                    WorkingDirectory = directory
                };

                var process = StartProcess(startInfo, outputLine => { });

                if (process == null)
                    return false;

                process.WaitForExit();
                //File.OpenRead(generatedDll); to check if file is valid readable etc.
                return File.Exists(generatedDll);
            } 
            catch (Exception)
            {
                return false;
            }
            finally
            {
                //SafeDeleteFile(fullName);
                SafeDeleteFile(globalJson);
                SafeDeleteFile(projFile);
            }
        }

        private static Process StartProcess(ProcessStartInfo processInfo, Action<string> redirectOutput)
        {
            processInfo.RedirectStandardOutput = true;

            //this is required for redirected output:
            processInfo.UseShellExecute = false;

            var process = new Process { StartInfo = processInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    redirectOutput(e.Data);
            };

            process.Start();

            process.BeginOutputReadLine();

            return process;
        }

        private static void SafeDeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch
            {
                throw new ArgumentException("Error ocured while moving system files. File not found! Retry or Contact page Admin.");
            }
        }

        public double RunTests(string argFile, string generatedDll)
        {
            string[] lines = File.ReadAllLines(Path.Combine(_webHostEnvironment.ContentRootPath, $"Uploads/{argFile}"));
            double goodResults = 0;
            foreach (string line in lines)
            {
                string[] parts;
                parts = line.Split("; ");
                object expected = Convert(parts[parts.Length - 1]);

                object[] obj = new object[parts.Length - 1];

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    bool str = false;
                    if (parts[i].StartsWith('['))
                    {
                        int index = parts[i].IndexOf(']');
                        string tp = parts[i].Substring(1, index);
                        parts[i] = parts[i].Remove(0, index + 1);
                        if (parts[i].StartsWith("\"\""))
                        {
                            parts[i] = parts[i].Remove(0, 2);
                            str = true;
                        }
                        if (!str)
                        {
                            var enumer = parts[i].Split(' ');
                            obj[i] = ArrayConvert(parts[i], tp);
                        }
                        else
                        {
                            var enumerable = parts[i].Split(' ');
                            obj[i] = enumerable;
                        }
                    }
                    else
                    {
                        if (!str) obj[i] = Convert(parts[i]);
                        else obj[i] = parts[i];
                    }
                }
                object result = Execute(generatedDll, "TaskAnswer", "ElementTest", obj); //Lacks logic
                Console.WriteLine(result + "    Expected: " + expected);
                if (object.Equals(result.ToString(), expected.ToString())) goodResults++;
            }
            return goodResults / lines.Length;
        }

        static object Convert(string ob)
        {
            if (bool.TryParse(ob, out var result1)) return result1;
            if (int.TryParse(ob, out var result2)) return result2;
            if (double.TryParse(ob, out var result)) return result;
            if (float.TryParse(ob, out var result3)) return result3;
            if (char.TryParse(ob, out var result4)) return result4;

            return ob;
        }
        static object ArrayConvert(string part, string type)
        {
            if (type.Contains("bool"))
            {
                var enumerable = part.Split(' ').Select(bool.Parse);
                return enumerable;
            }
            if (type.Contains("int"))
            {
                var enumerable = part.Split(' ').Select(int.Parse);
                return enumerable;
            }
            if (type.Contains("double"))
            {
                var enumerable = part.Split(' ').Select(double.Parse);
                return enumerable;
            }
            if (type.Contains("float"))
            {
                var enumerable = part.Split(' ').Select(float.Parse);
                return enumerable;
            }
            if (type.Contains("char"))
            {
                var enumerable = part.Split(' ').Select(char.Parse);
                return enumerable;
            }
            return part.Split(' ');
        }

        public static object Execute(string path, string className, string function, object[] args)
        {
            var assembly = Assembly.LoadFile(path);

            var type = assembly.GetTypes().First(t => t.Name == className);

            var testInstance = Activator.CreateInstance(type);

            var methodInfo = type.GetMethods().First(m => m.Name == function);

            return methodInfo.Invoke(testInstance, args);
        }
    }
}