using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinCipherGuardLib.Exceptions
{
    public enum ErrorType
    {
        InvalidConfiguration,
        EncryptionError,
        DecryptionError,
        KeyGenerationError
    }
}