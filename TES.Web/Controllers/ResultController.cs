using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TES.Domain;
using TES.Services.Dto;
using TES.Services.Interface;

namespace TES.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultController : ControllerBase
    {
        private readonly IResultService _resultService;

        public ResultController(IResultService resultService)
        {
            _resultService = resultService;
        }

        // GET: api/getAllResults
        [HttpGet]
        [Route("getAllResults")]
        [Produces(typeof(List<TestResult>))]
        public async Task<IActionResult> GetAllResults()
        {
            List<TestResult> results = await _resultService.GetAllResults();
            return Ok(results);
        }

        // GET: api/GetTestResults
        [HttpGet]
        [Route("getTest/{id}")]
        [Produces(typeof(List<TestResult>))]
        public async Task<IActionResult> GetTestResults(Guid id)
        {
            try
            {
                List<TestResult> result = await _resultService.GetTestResults(id);
                return Ok(result);
            }
            catch (Exception e)
            {
                return NotFound(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }
    }
}
