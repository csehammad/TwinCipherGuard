using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using TwinCipherGuardLib.Configuration;
using TwinCipherGuardLib.Exceptions;

namespace TwinCipherGuardLib
{
    public class TwinCipherGuard : ITwinCipherGuard
    {
        private readonly CryptographyClient _cryptographyClient;

        public TwinCipherGuard(IOptions<AzureKeyVaultSettings> options)
        {
            try
            {
                var settings = options.Value;
                if (string.IsNullOrWhiteSpace(settings.ClientId) ||
                    string.IsNullOrWhiteSpace(settings.ClientSecret) ||
                    string.IsNullOrWhiteSpace(settings.TenantId) ||
                    string.IsNullOrWhiteSpace(settings.VaultUri))
                {
                    throw new ArgumentException("Invalid Azure Key Vault configuration.");
                }

                var credential = new ClientSecretCredential(settings.TenantId, settings.ClientId, settings.ClientSecret);
                _cryptographyClient = new CryptographyClient(new Uri(settings.VaultUri), credential);
            }
            catch (Exception ex)
            {
                throw new CustomTwinCipherGuardException(ErrorType.InvalidConfiguration, "Error initializing TwinCipherGuard.", ex);
            }
        }

        // Other methods remain the same as in the previous answer, but add try-catch blocks and throw custom exceptions

        public byte[] GenerateDek(string email)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    string input = email;
                    byte[] dek = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                    return dek;
                }
            }
            catch (Exception ex)
            {
                throw new CustomTwinCipherGuardException(ErrorType.KeyGenerationError, "Error generating DEK.", ex);
            }
        }

        public byte[] GenerateEncryptedDek(string email)
        {
            try
            {
                byte[] dek = GenerateDek(email);
                WrapResult wrapResult = _cryptographyClient.WrapKey(KeyWrapAlgorithm.RsaOaep, dek);
                byte[] encryptedDek = wrapResult.EncryptedKey;
                return encryptedDek;
            }
            catch (Exception ex)
            {
                throw new CustomTwinCipherGuardException(ErrorType.KeyGenerationError, "Error generating encrypted DEK.", ex);
            }
        }

        public byte[] Encrypt(string input, byte[] encryptedDek)
        {
            try
            {
                UnwrapResult unwrapResult = _cryptographyClient.UnwrapKey(KeyWrapAlgorithm.RsaOaep, encryptedDek);
                byte[] decryptedDek = unwrapResult.Key;

                // Encrypt the input string with the decrypted DEK
                byte[] encryptedData = EncryptDataWithDek(input, decryptedDek);

                return encryptedData;
            }
            catch (Exception ex)
            {
                throw new CustomTwinCipherGuardException(ErrorType.EncryptionError, "Error encrypting data.", ex);
            }
        }

        public string Decrypt(byte[] input, byte[] encryptedDek)
        {
            try
            {
                UnwrapResult unwrapResult = _cryptographyClient.UnwrapKey(KeyWrapAlgorithm.RsaOaep, encryptedDek);
                byte[] decryptedDek = unwrapResult.Key;

                // Decrypt the encrypted data with the decrypted DEK
                string decryptedData = DecryptDataWithDek(input, decryptedDek);

                return decryptedData;
            }
            catch (Exception ex)
            {
                throw new CustomTwinCipherGuardException(ErrorType.DecryptionError, "Error decrypting data.", ex);
            }
        }

        private byte[] EncryptDataWithDek(string input, byte[] dek)
        {
            try
            {
                using (var aes = new AesManaged())
                {
                    aes.Key = dek;
                    aes.GenerateIV(); // Generate a random initialization vector (IV)

                    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length); // Write the IV to the beginning of the MemoryStream
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(input);
                        }
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomTwinCipherGuardException(ErrorType.DecryptionError, "Error  Encrypting data with DEK.", ex);
            }
        }

        public string DecryptDataWithDek(byte[] input, byte[] encryptedDek)
        {
            try
            {
                using (var aes = new AesManaged())
                {
                    aes.Key = encryptedDek;
                    // Get the initialization vector (IV) from the beginning of the encryptedData array
                    byte[] iv = new byte[aes.IV.Length];
                    Buffer.BlockCopy(input, 0, iv, 0, aes.IV.Length);

                    // Get the encrypted data without the IV
                    byte[] encrypted = new byte[input.Length - aes.IV.Length];
                    Buffer.BlockCopy(input, aes.IV.Length, encrypted, 0, encrypted.Length);

                    // Decrypt the data using the DEK and IV
                    var decryptor = aes.CreateDecryptor(aes.Key, iv);
                    using (var ms = new MemoryStream(encrypted))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomTwinCipherGuardException(ErrorType.DecryptionError, "Error decrypting data with DEK.", ex);
            }
        }
    }
}