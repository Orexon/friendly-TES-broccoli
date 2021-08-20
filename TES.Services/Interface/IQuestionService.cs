using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;
using TES.Services.Dto.QuestionDto;

namespace TES.Services.Interface
{
    public interface IQuestionService
    {
        Task<List<Question>> GetAllQuestions();
        Task<Question> GetQuestionById(Guid id);
        Task<EditQuestionDto> GetEditQuestionDto(Guid id);
        Task<bool> IsQuestionActive(Guid id);
        Task<bool> UpdateQuestion(EditQuestionDto editQuestionDto);
        Task<Question> CreateQuestion(NewQuestionDto newQuestion);
        Task<bool> DeleteQuestion(Guid id);
        bool QuestionExists(Guid id);

    }
}
