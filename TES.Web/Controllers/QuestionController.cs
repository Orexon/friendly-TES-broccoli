using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TES.Data;
using TES.Domain;
using TES.Services.Dto;
using TES.Services.Dto.QuestionDto;
using TES.Services.Interface;

namespace TES.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }
        
        // GET: api/Question/getAllQuestions
        [HttpGet]
        [Route("getAllQuestions")]
        public async Task<ActionResult<List<Question>>> GetAllQuestions()
        {
            List<Question> questions = await _questionService.GetAllQuestions();
            return Ok(questions);
        }

        // GET: api/Question/GetQuestionById/Guid
        [HttpGet]
        [Route("getQuestion/{id}")]
        [Produces(typeof(Question))]
        public async Task<IActionResult> GetQuestionById(Guid id)
        {
            try
            {
                Question question = await _questionService.GetQuestionById(id);
                return Ok(question);
            }
            catch (Exception e)
            {
                return NotFound(new ResponseDto { Status = "Error", Message = e.Message });
            }
            
        }

        //Get: api/Question/editQuestion/Guid
        [HttpGet]
        [Route("editQuestion/{id}")]
        [Produces(typeof(EditQuestionDto))]
        public async Task<IActionResult> UpdateQuestion(Guid id)
        {
            try
            {
                EditQuestionDto editQuestionDto = await _questionService.GetEditQuestionDto(id);
                return Ok(editQuestionDto);
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        //PATCH: api/Question/editQuestion/Guid
        [HttpPatch]
        [Route("editQuestion/{id}")]
        public async Task<IActionResult> UpdateQuestion([FromBody] EditQuestionDto editQuestionDto)
        {
            try
            {
                await _questionService.UpdateQuestion(editQuestionDto);
                return Ok(new ResponseDto { Status = "Success", Message = "Question has been edited!" });
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        // POST: api/Question/createQuestion
        [HttpPost]
        [Route("createQuestion")]
        [Produces(typeof(Question))] 
        public async Task<IActionResult> CreateQuestion([FromBody] NewQuestionDto newQuestion)
        {
            try
            {
                Question question = await _questionService.CreateQuestion(newQuestion);
                return Ok(new ResponseDto { Status = "Success", Message = "Question has been created!" });
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }

        // DELETE: api/Question/deleteQuestion/Guid
        [HttpDelete]
        [Route("deleteQuestion/{id}")]
        public async Task<IActionResult> DeleteTest(Guid id)
        {
            try
            {
                await _questionService.DeleteQuestion(id);
                return Ok(new ResponseDto { Status = "Success", Message = "Question has been deleted!" });
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDto { Status = "Error", Message = e.Message });
            }
        }
    }
}
