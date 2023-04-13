using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinCipherGuardLib
{
    public interface ITwinCipherGuard
    {
        byte[] GenerateDek(string email);

        byte[] GenerateEncryptedDek(string email);

        byte[] Encrypt(string input, byte[] encryptedDek);

        string Decrypt(byte[] input, byte[] encryptedDek);
    }
}