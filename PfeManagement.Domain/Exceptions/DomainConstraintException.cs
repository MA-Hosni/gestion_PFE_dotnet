using System;

namespace PfeManagement.Domain.Exceptions
{
    public class DomainConstraintException : Exception
    {
        public DomainConstraintException(string message) : base(message)
        {
        }
    }
}
