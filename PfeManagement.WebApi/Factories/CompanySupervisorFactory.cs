using System;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Factories
{
    // GoF Pattern: Factory Method (Concrete Creator)
    public class CompanySupervisorFactory : UserFactory
    {
        protected override User CreateUser(UserRegistrationDto dto)
        {
            if (dto is not CompanySupervisorRegistrationDto supervisorDto)
            {
                throw new ArgumentException("Invalid DTO type for CompanySupervisorFactory");
            }

            return new CompanySupervisor
            {
                FullName = supervisorDto.FullName,
                Email = supervisorDto.Email,
                PhoneNumber = supervisorDto.PhoneNumber,
                Role = supervisorDto.Role,
                CompanyName = supervisorDto.CompanyName,
                BadgeIMG = supervisorDto.BadgeIMG
            };
        }
    }
}
