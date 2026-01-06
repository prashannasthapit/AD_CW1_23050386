using Application.Common;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

/// <summary>
/// Service for user management and authentication
/// </summary>
public class UserService(IJournalDbAccess dbAccess) : IUserService
{
    private UserDisplayModel? _currentUser;

    public async Task<ServiceResult<UserDisplayModel>> LoginAsync(UserLoginModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Username))
                return ServiceResult<UserDisplayModel>.Fail("Username is required.");
            if (string.IsNullOrWhiteSpace(model.Pin))
                return ServiceResult<UserDisplayModel>.Fail("PIN is required.");

            var user = await dbAccess.GetUserByUsernameAsync(model.Username);
            if (user == null)
                return ServiceResult<UserDisplayModel>.Fail("User not found.");

            var hashedPin = HashPin(model.Pin);
            if (user.Pin != hashedPin)
                return ServiceResult<UserDisplayModel>.Fail("Invalid PIN.");

            var display = MapToDisplay(user);
            _currentUser = display;
            return ServiceResult<UserDisplayModel>.Ok(display);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<UserDisplayModel>> RegisterAsync(UserLoginModel model, string theme = "Light")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Username))
                return ServiceResult<UserDisplayModel>.Fail("Username is required.");
            if (string.IsNullOrWhiteSpace(model.Pin) || model.Pin.Length < 4)
                return ServiceResult<UserDisplayModel>.Fail("PIN must be at least 4 characters.");

            var existingUser = await dbAccess.GetUserByUsernameAsync(model.Username);
            if (existingUser != null)
                return ServiceResult<UserDisplayModel>.Fail("Username already exists.");

            var hashedPin = HashPin(model.Pin);
            var user = await dbAccess.CreateUserAsync(model.Username, hashedPin, theme);
            
            var display = MapToDisplay(user);
            _currentUser = display;
            return ServiceResult<UserDisplayModel>.Ok(display);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDisplayModel>.Fail(ex.Message);
        }
    }

    public Task<ServiceResult<UserDisplayModel>> GetCurrentUserAsync()
    {
        if (_currentUser == null)
            return Task.FromResult(ServiceResult<UserDisplayModel>.Fail("No user is currently logged in."));
        
        return Task.FromResult(ServiceResult<UserDisplayModel>.Ok(_currentUser));
    }

    public async Task<ServiceResult<UserDisplayModel>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await dbAccess.GetUserByIdAsync(id);
            if (user == null)
                return ServiceResult<UserDisplayModel>.Fail("User not found.");

            return ServiceResult<UserDisplayModel>.Ok(MapToDisplay(user));
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDisplayModel>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<List<UserDisplayModel>>> GetAllUsersAsync()
    {
        try
        {
            var users = await dbAccess.GetAllUsersAsync();
            var displays = users.Select(MapToDisplay).ToList();
            return ServiceResult<List<UserDisplayModel>>.Ok(displays);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<UserDisplayModel>>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(Guid id)
    {
        try
        {
            var user = await dbAccess.GetUserByIdAsync(id);
            if (user == null)
                return ServiceResult<bool>.Fail("User not found.");

            await dbAccess.DeleteUserAsync(id);
            
            if (_currentUser?.Id == id)
                _currentUser = null;
            
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<UserDisplayModel>> UpdateThemeAsync(Guid userId, string theme)
    {
        try
        {
            var user = await dbAccess.GetUserByIdAsync(userId);
            if (user == null)
                return ServiceResult<UserDisplayModel>.Fail("User not found.");

            user.Theme = theme;
            await dbAccess.UpdateUserAsync(user);
            
            var display = MapToDisplay(user);
            if (_currentUser?.Id == userId)
                _currentUser = display;
            
            return ServiceResult<UserDisplayModel>.Ok(display);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDisplayModel>.Fail(ex.Message);
        }
    }

    public void SetCurrentUser(UserDisplayModel? user)
    {
        _currentUser = user;
    }

    private static string HashPin(string pin)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pin));
        return Convert.ToBase64String(bytes);
    }

    private static UserDisplayModel MapToDisplay(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Theme = user.Theme,
        CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm")
    };
}

