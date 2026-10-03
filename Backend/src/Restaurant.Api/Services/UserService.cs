using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IPinHasher _pinHasher;

        public UserService(AppDbContext db, IPinHasher pinHasher)
        {
            _db = db;
            _pinHasher = pinHasher;
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                return (false, "User not found.");
            }

            if (user.Role == Role.Waiter)
            {
                return (false, "Waiters have no password. They log in with a PIN.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return (false, "Current password is incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<UserDto>> GetAllManagersAsync(int adminId)
        {
           var managers = await _db.Users.Where(u => u.RegisteredById == adminId && u.Role == Role.Manager)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return managers;
        }

        public async Task<List<UserDto>> GetAllWaitersAsync(int managerId)
        {
           var waiters = await _db.Users.Where(u => u.RegisteredById == managerId && u.Role == Role.Waiter)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return waiters;
        }

        public async Task<UserDto?> GetProfileAsync(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            };
        }

        public async Task<UserDto?> RegisterManagerAsync(RegisterRequest request, int adminId)
        {
            var alreadyExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (alreadyExists)
            {
                return null;
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Role.Manager,
                IsActive = true,
                RegisteredById = adminId
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            };
        }

        public async Task<(UserDto? Waiter, string? Error)> RegisterWaiterAsync(RegisterWaiterRequest request, int managerId)
        {
            var alreadyExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (alreadyExists)
            {
                return (null,"User with this email already exists, choose another one.");
            }
            var pinHash = _pinHasher.HashPin(request.Pin);

            var pinAlreadyExists = await _db.Users.AnyAsync(u => u.PinHash == pinHash && u.IsActive);  

            if (pinAlreadyExists)
            {
                return (null,"Pin already exists, choose another one.");
            }
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                PinHash = pinHash,
                Role = Role.Waiter,
                IsActive = true,
                RegisteredById = managerId
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

           var dto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            };
            return(dto,null);
        }

        public async Task<bool> SetManagersProfileStatus(int managerId, int adminId, bool isActive)
        {
           var manager = await _db.Users.FirstOrDefaultAsync(u => u.Id == managerId && u.RegisteredById == adminId && u.Role == Role.Manager);

            if (manager is null)
            {
                return false;
            }
            manager.IsActive = isActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Found, string? Error)> SetWaitersProfileStatus(int waiterId, int managerId, bool isActive)
        {
            var waiter = await _db.Users.FirstOrDefaultAsync(u => u.Id == waiterId && u.RegisteredById == managerId
                && u.Role == Role.Waiter);

            if (waiter is null)
            {
                return (false, null);
            }

            if (isActive)
            {
                var pinTaken = await _db.Users.AnyAsync(u =>
                    u.Id != waiter.Id && u.IsActive && u.PinHash == waiter.PinHash);

                if (pinTaken)
                {
                    return (true, "PIN is already used by another active waiter. Change the PIN first.");
                }
            }

            waiter.IsActive = isActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Found, string? Error)> UpdateManagerAsync(int managerId, int adminId, UpdateManagerRequest request)
        {
            var manager = await _db.Users.FirstOrDefaultAsync(u =>
            u.Id == managerId && u.RegisteredById == adminId && u.Role == Role.Manager);

            if (manager is null)
            {
                return (false, null);
            }

            var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != managerId);
            if (emailExists)
            {
                return (true, "Email is already in use by another user.");
            }

            manager.FirstName = request.FirstName;
            manager.LastName = request.LastName;
            manager.Email = request.Email;
            await _db.SaveChangesAsync();
            return (true, null);

        }

        public async Task<(bool Found, string? Error)> UpdateWaiterAsync(int waiterId, int managerId, UpdateWaiterRequest request)
        {
            var waiter = await _db.Users.FirstOrDefaultAsync(u =>
                u.Id == waiterId && u.RegisteredById == managerId && u.Role == Role.Waiter);

            if (waiter is null)
            {
                return (false, null);
            }

            var emailTaken = await _db.Users.AnyAsync(u => u.Id != waiter.Id && u.Email == request.Email);
            if (emailTaken)
            {
                return (true, "Email is already in use.");
            }

            if (request.Pin is not null)
            {
                var pinHash = _pinHasher.HashPin(request.Pin);
                var pinTaken = await _db.Users.AnyAsync(u =>
                    u.Id != waiter.Id && u.IsActive && u.PinHash == pinHash);

                if (pinTaken)
                {
                    return (true, "PIN is already in use. Choose another one.");
                }

                waiter.PinHash = pinHash;
            }

            waiter.FirstName = request.FirstName;
            waiter.LastName = request.LastName;
            waiter.Email = request.Email;

            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}
