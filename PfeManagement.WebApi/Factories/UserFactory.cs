using PfeManagement.WebApi.Models;
using PfeManagement.WebApi.Interfaces;

namespace PfeManagement.WebApi.Factories
{
    // GoF Pattern: Factory Method (Creator)
    public abstract class UserFactory
    {
        // Template Method
        public User CreateAndRegister(UserRegistrationDto dto, IPasswordHasher hasher)
        {
            var user = CreateUser(dto);
            user.PasswordHash = hasher.HashPassword(dto.Password);
            user.VerificationToken = System.Guid.NewGuid().ToString("N");
            return user;
        }

        protected abstract User CreateUser(UserRegistrationDto dto);
    }
}
