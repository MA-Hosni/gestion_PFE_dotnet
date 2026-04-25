using System;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Application.Factories
{
    // GoF Pattern: Factory Method (Concrete Creator)
    public class UniversitySupervisorFactory : UserFactory
    {
        protected override User CreateUser(UserRegistrationDto dto)
        {
            if (dto is not UniversitySupervisorRegistrationDto supervisorDto)
            {
                throw new ArgumentException("Invalid DTO type for UniversitySupervisorFactory");
            }

            return new UniversitySupervisor
            {
                FullName = supervisorDto.FullName,
                Email = supervisorDto.Email,
                PhoneNumber = supervisorDto.PhoneNumber,
                Role = supervisorDto.Role,
                BadgeIMG = supervisorDto.BadgeIMG
            };
        }
    }
}
