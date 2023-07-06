using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Application.Exceptions
{
    public class ValidatorException : Exception
    {
        public ValidatorException()
        { }

        public ValidatorException(string message)
            : base(message)
        { }

        public ValidatorException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
