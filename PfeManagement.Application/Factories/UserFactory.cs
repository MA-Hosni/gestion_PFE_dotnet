using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Application.Factories
{
    // GoF Pattern: Factory Method (Creator)
    public abstract class UserFactory
    {
        // Template Method
        public User CreateAndRegister(UserRegistrationDto dto, IPasswordHasher hasher)
        {
            var user = CreateUser(dto);
            user.PasswordHash = hasher.HashPassword(dto.Password);
            user.GenerateVerificationToken();
            return user;
        }

        protected abstract User CreateUser(UserRegistrationDto dto);
    }
}
