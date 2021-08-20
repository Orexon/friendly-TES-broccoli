using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;
using TES.Services.Dto.ActiveTestDto;
using TES.Services.Dto.TestDto;

namespace TES.Services.Interface
{
    public interface IActiveTestService
    {
        TestDto ActiveTest(Guid id);

        Task<bool> StartTest(TestStartDto testStartDto);

        Task<SolutionResponseDto> SubmitSolution(SubmitUserSolutionDto solutionDto);

        Task<string> GetSolutionPath(Guid id);

        Task<string> GetUserSolutionPath(SolutionResponseDto solutionResponseDto);

        double TestScore(string argFile, string userSolution);

        Task<bool> SaveQuestionScore(SolutionResponseDto solutionResponseDto, double taskresult);

        Task<double> FinishTest(SubmitUserSolutionDto solutionDto);
    }
}
