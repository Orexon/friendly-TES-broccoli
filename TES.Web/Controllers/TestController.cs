using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TES.Data;
using TES.Domain;
using TES.Services.Dto;
using TES.Services.Dto.TestDto;
using TES.Services.Interface;

namespace TES.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;

        public TestController(ITestService testService)
        {
            _testService = testService;
        }

        // GET: api/Test/getAllTests
        [HttpGet]
        [Route("getAllTests")]
        [Produces(typeof(List<Test>))]
        public async Task<IActionResult> GetAllTests()
        {

            List<Test> tests = await _testService.GetAllTests();

            if (tests.Count != 0)
            {
                return Ok(tests);
            }
            else {
                return NotFound(new ResponseDto { Status = "Error", Message = "Curently there are no tests" });
            }
        }

        // GET: api/Test/getTest/Guid
        [HttpGet]
        [Route("getTest/{id}")]
        [Produces(typeof(Test))]
        public async Task<IActionResult> GetTestById(Guid id)
        {
            try
            {
                Test test = await _testService.GetTestById(id);
                return Ok(test);
            }
            catch (Exception e)
            {
                return NotFound(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        [HttpGet]
        [Route("editTest/{id}")]
        [Produces(typeof(EditTestDto))]
        public async Task<IActionResult> UpdateTest(Guid id)
        {
            try
            {
                EditTestDto editTestDto = await _testService.GetEditTestDto(id);
                return Ok(editTestDto);
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        //PATCH: api/Test/editTest/Guid
        [HttpPatch]
        [Route("editTest/{id}")]
        public async Task<IActionResult> UpdateTest([FromBody] EditTestDto editTestDto)
        {
            try
            {
                await _testService.UpdateTest(editTestDto);
                return Ok(new ResponseDto { Status = "Success", Message = "Test has been updated!" });
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        // POST: api/Test/createTest
        [HttpPost]
        [Route("createTest")]
        [Produces(typeof(Test))]
        public async Task<IActionResult> CreateTest([FromForm] NewTestDto newTest)
        {
            try
            {
                Test test = await _testService.CreateTest(newTest);
                return Ok(test);
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        // DELETE: api/Test/deleteTest/Guid
        [HttpDelete]
        [Route("deleteTest/{id}")]
        public async Task<IActionResult> DeleteTest(Guid id)
        {
            try
            {
                await _testService.DeleteTest(id);
                return Ok(new ResponseDto { Status = "Success", Message = "Test has been deleted!" });
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }
    }
}
