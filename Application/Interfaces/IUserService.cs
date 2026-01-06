using Application.Common;
using Application.Models;

namespace Application.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserDisplayModel>> LoginAsync(UserLoginModel model);
    Task<ServiceResult<UserDisplayModel>> RegisterAsync(UserLoginModel model, string theme = "Light");
    Task<ServiceResult<UserDisplayModel>> GetCurrentUserAsync();
    Task<ServiceResult<UserDisplayModel>> GetUserByIdAsync(Guid id);
    Task<ServiceResult<List<UserDisplayModel>>> GetAllUsersAsync();
    Task<ServiceResult<bool>> DeleteUserAsync(Guid id);
    Task<ServiceResult<UserDisplayModel>> UpdateThemeAsync(Guid userId, string theme);
    void SetCurrentUser(UserDisplayModel? user);
}
