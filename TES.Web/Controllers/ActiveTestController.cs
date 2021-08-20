using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TES.Domain;
using TES.Services.Dto;
using TES.Services.Dto.ActiveTestDto;
using TES.Services.Dto.TestDto;
using TES.Services.Interface;

namespace TES.Web.Controllers
{ //ADD submit logic which has partial submit/ and PUT is when there is one questiion or smth.// Add logic which takes score of submitted answer.
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ActiveTestController : ControllerBase
    {
        private readonly IActiveTestService _activeTestService;

        public ActiveTestController(IActiveTestService activeTestService)
        {
            _activeTestService = activeTestService;
        }

        // GET: api/ActiveTest/{id}
        [HttpGet]
        [Route("{id}")]
        [Produces(typeof(TestDto))]
        public IActionResult ActiveTest()
        {
            var idString = Request.RouteValues["id"].ToString();
            Guid urlGuid = Guid.Parse(idString);

            try
            {
                TestDto testDto = _activeTestService.ActiveTest(urlGuid);
                return Ok(testDto);
            }
            catch (Exception e)
            {
                return NotFound(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        // POST: api/ActiveTest/startTest/{id}
        [HttpPost]
        [Route("startTest/{id}")]
        [Produces(typeof(TestStartDto))]
        public IActionResult StartTest(TestStartDto testStartDto)
        {
            try
            {
                return Ok(_activeTestService.StartTest(testStartDto));
            }
            catch (Exception e)
            {
                return NotFound(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        // POST: api/ActiveTest/submitSolution/{id}
        [HttpPost]
        [Route("submitSolution/{id}")]
        [Produces(typeof(SubmitionResultDto))]
        public async Task<IActionResult> SubmitUserSolution(SubmitUserSolutionDto solutionDto)
        {
            try
            {
                SolutionResponseDto solutionResponseDto = await _activeTestService.SubmitSolution(solutionDto);

                string argFile = await _activeTestService.GetSolutionPath(solutionResponseDto.QuestionId);
                string userSolution = await _activeTestService.GetUserSolutionPath(solutionResponseDto);

                double score = _activeTestService.TestScore(argFile, userSolution);

                double taskresult = score * solutionResponseDto.QuestionWorth;
                taskresult = (double)Math.Round(taskresult, 2);

                await _activeTestService.SaveQuestionScore(solutionResponseDto, taskresult);

                SubmitionResultDto scoreDto = new SubmitionResultDto
                {
                    Scored = taskresult,
                    TotalWorth = solutionResponseDto.QuestionWorth,
                };
                return Ok(scoreDto);
            }
            catch (Exception e)
            {
                return NotFound(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }


        // POST: api/ActiveTest/finishTest/{id}
        [HttpPost]
        [Route("finishTest/{id}")]
        [Produces(typeof(SubmitionResultDto))]
        public async Task<IActionResult> FinishTest(SubmitUserSolutionDto solutionDto)
        {
            try
            {
                return Ok(await _activeTestService.FinishTest(solutionDto));
            }
            catch (Exception e)
            {
                return NotFound(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

    }
}
