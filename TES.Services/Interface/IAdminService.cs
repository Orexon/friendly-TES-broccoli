using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TES.Domain;
using TES.Services.Dto.AdminDto;

namespace TES.Services.Interface
{
    public interface IAdminService
    {
        List<AppUser> GetAllUsers();
        Task<bool> CreateAdmin(NewAdminDto adminDto);

        AppUserDto GetUserById(Guid id);

        Task<int> DeleteUser(Guid id);

        Task<bool> UpdateUser(EditAppUserDto appUserDto);

        bool UserExists(Guid id);

    }
}
