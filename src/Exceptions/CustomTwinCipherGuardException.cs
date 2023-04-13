using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinCipherGuardLib.Exceptions
{
    public class CustomTwinCipherGuardException : Exception
    {
        public ErrorType ErrorType { get; }

        public CustomTwinCipherGuardException(ErrorType errorType, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorType = errorType;
        }
    }
}