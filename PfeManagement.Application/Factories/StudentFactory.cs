using System;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Application.Factories
{
    // GoF Pattern: Factory Method (Concrete Creator)
    public class StudentFactory : UserFactory
    {
        protected override User CreateUser(UserRegistrationDto dto)
        {
            if (dto is not StudentRegistrationDto studentDto)
            {
                throw new ArgumentException("Invalid DTO type for StudentFactory");
            }

            return new Student
            {
                FullName = studentDto.FullName,
                Email = studentDto.Email,
                PhoneNumber = studentDto.PhoneNumber,
                Role = studentDto.Role,
                Cin = studentDto.Cin,
                StudentIdCardIMG = studentDto.StudentIdCardIMG,
                CompanyName = studentDto.CompanyName,
                Degree = studentDto.Degree,
                DegreeType = studentDto.DegreeType,
                CompSupervisorId = studentDto.CompSupervisorId,
                UniSupervisorId = studentDto.UniSupervisorId
            };
        }
    }
}
