using System.Collections.Generic;
using FluentValidation;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.Validators
{
    // OCL Constraint: context Student inv: DegreeTypes[self.degree]->includes(self.degreeType)
    public class StudentValidator : AbstractValidator<Student>
    {
        private static readonly Dictionary<Degree, List<DegreeType>> DegreeTypeMapping = new()
        {
            { Degree.Engineer, new List<DegreeType> { DegreeType.INLOG, DegreeType.INREV } },
            { Degree.Master, new List<DegreeType> { DegreeType.Pro_IM, DegreeType.Pro_DCA, DegreeType.Pro_PAR, DegreeType.R_DISR, DegreeType.R_TMAC } },
            { Degree.Bachelor, new List<DegreeType> { DegreeType.AV, DegreeType.CMM, DegreeType.IMM, DegreeType.BD, DegreeType.MIME, DegreeType.Coco_JV, DegreeType.Coco_3D } }
        };

        public StudentValidator()
        {
            RuleFor(s => s)
                .Must(s => DegreeTypeMapping.ContainsKey(s.Degree) && DegreeTypeMapping[s.Degree].Contains(s.DegreeType))
                .WithMessage("The selected Degree Type is not valid for the chosen Degree");

            RuleFor(s => s.Cin)
                .Matches(@"^\d{8}$")
                .WithMessage("CIN must be exactly 8 digits");
        }
    }
}
