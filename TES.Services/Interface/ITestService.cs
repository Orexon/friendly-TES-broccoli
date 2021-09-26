using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;
using TES.Services.Dto.TestDto;

namespace TES.Services.Interface
{
    public interface ITestService
    {
        Task<List<Test>> GetAllTests();
        Task<Test> GetTestById(Guid id);
        Task<EditTestDto> GetEditTestDto(Guid id);
        Task<bool> UpdateTest(NewTestDto editTestDto);
        Task<Test> CreateTest(NewTestDto newTest);
        Task<bool> GenerateUrl(Guid Id);
        Task<bool> DeleteTest(Guid id);
        bool TestExists(Guid id);
    }
}
