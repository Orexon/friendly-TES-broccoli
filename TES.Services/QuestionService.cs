using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Data;
using TES.Domain;
using TES.Services.Dto.QuestionDto;
using TES.Services.Interface;

namespace TES.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly Context _context;

        public QuestionService(Context context)
        {
            _context = context;
        }

        public async Task<List<Question>> GetAllQuestions()
        {
            List<Question> questions = await _context.Questions.ToListAsync();
            return questions;
        }

        public async Task<Question> GetQuestionById(Guid id)
        {
            if (!QuestionExists(id))
            {
                throw new ArgumentNullException($"Quesion with {id} does not exist");
            }
            return await _context.Questions.FindAsync(id);
        }

        public async Task<EditQuestionDto> GetEditQuestionDto(Guid id)
        {

            if (!QuestionExists(id))
            {
                throw new ArgumentNullException($"Quesion with {id} does not exist");
            }
            if (await IsQuestionActive(id))
            {
                throw new ArgumentException("Can't modify Quesion while it's in a test that is active!");
            }

            Question question = _context.Questions.Where(x => x.Id == id).FirstOrDefault();

            EditQuestionDto editQuestionDto = new EditQuestionDto
            {
                Id = question.Id,
                Name = question.Name,
                Description = question.Description,
                QuestionType = question.QuestionType,
                WorthOfPoints = question.WorthOfPoints,
                CorrectSolutionPath = question.SolutionFilePath
            };
            return editQuestionDto;
        }


        public async Task<bool> UpdateQuestion(EditQuestionDto editQuestionDto)
        {
            if (editQuestionDto == null)
            {
                throw new ArgumentException("Edit data not found, please try again!");
            }

            if (!QuestionExists(editQuestionDto.Id))
            {
                throw new ArgumentException("Question does not exist");
            }

            if (await IsQuestionActive(editQuestionDto.Id))
            {
                throw new ArgumentException("Can't modify Quesion with while it's in a test that is active!");
            }

            Question question = _context.Questions.Where(x => x.Id == editQuestionDto.Id).FirstOrDefault();

            question.Name = editQuestionDto.Name;
            question.Description = editQuestionDto.Description;
            question.QuestionType = editQuestionDto.QuestionType;
            question.SolutionFilePath = editQuestionDto.CorrectSolutionPath;
            question.WorthOfPoints = editQuestionDto.WorthOfPoints;

            try
            {
                _context.Questions.Update(question);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException) when (!QuestionExists(question.Id))
            {
                throw new ArgumentException("Question update failed, please try again!");
            }
        }

        public async Task<bool> IsQuestionActive(Guid id)
        {
            Question question = await GetQuestionById(id);
            List<Test> relatedTests = await _context.Tests.Where(x => x.Questions.Contains(question)).ToListAsync();
            return relatedTests.Any(x => x.ValidFrom >= DateTime.Now && DateTime.Now >= x.ValidTo);
        }

        public async Task<Question> CreateQuestion(NewQuestionDto newQuestion)
        {
            Question question = new Question
            {
                Name = newQuestion.Name,
                Description = newQuestion.Description,
                QuestionType = newQuestion.QuestionType,
                SolutionFilePath = newQuestion.CorrectSolutionPath,
                WorthOfPoints = newQuestion.WorthOfPoints,
            };

            try
            {
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                return question;
            }
            catch (DbUpdateConcurrencyException) when (QuestionExists(question.Id))
            {
                throw new InvalidOperationException("Question creation failed! Duplicate found, try again."); ;
            }
        }

        public async Task<bool> DeleteQuestion(Guid id)
        {
            if (await IsQuestionActive(id))
            {
                throw new ArgumentException("Can't delete Question while it's in a test that is active!");
            }
            Question question = await GetQuestionById(id);
            try
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException) when (!QuestionExists(id))
            {
                throw new ArgumentException("Question not found!");
            }
        }

        public bool QuestionExists(Guid id) =>
        _context.Questions.Any(e => e.Id == id);
    }
}
