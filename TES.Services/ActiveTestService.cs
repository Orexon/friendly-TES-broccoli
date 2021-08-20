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
        private IHostingEnvironment _hostingEnvironment;

        private const string ProjContent = "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>netstandard2.0</TargetFramework><RuntimeFrameworkVersion>3.1.202</RuntimeFrameworkVersion></PropertyGroup></Project>";
        public ActiveTestService(Context context, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
        }

        public bool TestExists(Guid id) =>
        _context.Tests.Any(e => e.Id == id);

        public TestDto ActiveTest(Guid id)
        {
            TestLink testLink =  _context.TestLinks.Where(x => x.Id == id).FirstOrDefault();
            if (testLink == null)
            {
                throw new ArgumentException("TestLink not found!");
            }

            Test test = _context.Tests.Where(x => x.UrlLinkId == testLink).Include(x=>x.Questions).FirstOrDefault();

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

            TestDto testDto = new TestDto
            {
                Name = test.Name,
                Description = test.Description,
                Questions = test.Questions,
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
            Test test = await _context.Tests.FindAsync(testStartDto.TestId);

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
                string uploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "Uploads");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + solutionDto.SubmitedFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                solutionDto.SubmitedFile.CopyTo(new FileStream(filePath, FileMode.Create));
            }

            UniqueApplicant aplicant = _context.Applicant.Where(x => x.Email == solutionDto.Email).FirstOrDefault();
            Question question = _context.Questions.Where(x => x.Id == solutionDto.QuestionID).FirstOrDefault();
            Test test = _context.Tests.Where(x => x.Id == solutionDto.TestId).FirstOrDefault();

            //Check if it's the first solution to this question, if not delete the last solution.
            UserSolution uniqueSolution = await _context.UserSolutions.Where(x => x.Test.Id == test.Id && x.Question.Id == question.Id && x.AplicantId.Id == aplicant.Id).FirstOrDefaultAsync();
            if(uniqueSolution.SubmitedFilePath.Length > 0)
            {
                try
                {
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
                AplicantId = aplicant,
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
            UserSolution userSolution = await _context.UserSolutions.Where(x => x.Test.Id == solutionResponseDto.TestId && x.Question.Id == solutionResponseDto.QuestionId && x.AplicantId.Id == solutionResponseDto.ApplicantId).FirstOrDefaultAsync();
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
            UserSolution userSolution = await _context.UserSolutions.Where(x => x.Test.Id == solutionResponseDto.TestId && x.Question.Id == solutionResponseDto.QuestionId && x.AplicantId.Id == solutionResponseDto.ApplicantId).FirstOrDefaultAsync();

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

        public async Task<double> FinishTest(SubmitUserSolutionDto solutionDto)
        {
            if (solutionDto == null)
            {
                throw new ArgumentException("Solution data not found!");
            }

            UniqueApplicant aplicant = _context.Applicant.Where(x => x.Email == solutionDto.Email).FirstOrDefault();
            Test test = _context.Tests.Where(x => x.Id == solutionDto.TestId).FirstOrDefault();

            TestResult testResult = await _context.Results.Where(x => x.Test.Id == test.Id && x.Applicant.Id == aplicant.Id).Include(x=>x.UserSolutions).FirstOrDefaultAsync();


            TimeSpan difference = DateTime.Now.Subtract(testResult.StartedAt);
            int minutes = difference.Minutes;
            testResult.MinutesOvertime = minutes;

            List<UserSolution> userSolutions = testResult.UserSolutions;

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
            string content = File.ReadAllText(userSolution);
            bool canCompile = Compile(userSolution, content, out string generatedDll);
            if (!canCompile)
            {
                return 0;
            }
            //File.WriteAllText(userSolution, content); //for testing restores Task.cs delete later

            return RunTests(argFile, generatedDll);

        }

        public bool Compile(string fileName, string content, out string generatedDll)
        {
            var directory = Path.Combine(_hostingEnvironment.ContentRootPath, "CoreCompilerTempFiles");

            var fullName = Path.Combine(directory, fileName);
            File.WriteAllText(fullName, content);

            var globalJson = Path.Combine(directory, "global.json");
            File.WriteAllText(globalJson, "{\r\n  \"sdk\": {\r\n    \"version\": \"5.0.204\"\r\n  }\r\n}");

            var projFile = Path.ChangeExtension(fullName, ".csproj");
            File.WriteAllText(projFile, ProjContent);

            generatedDll = Path.Combine(directory, $"bin\\Debug\\netstandard2.0\\{Path.GetFileNameWithoutExtension(fullName)}.dll");

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
                // ignored
            }
        }

        public static double RunTests(string argFile, string generatedDll)
        {
            string[] lines = File.ReadAllLines(argFile);
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
                object result = Execute(generatedDll, "Task", "ElementTest", obj); //Lacks logic
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