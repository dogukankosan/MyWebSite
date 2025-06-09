using System.Security.Cryptography;
using System.Text;
namespace MyWebSite.Classes
{
    internal class HashingControl
    {
        internal static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                StringBuilder sb = new();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
        internal static bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            string hashedEnteredPassword = HashPassword(enteredPassword);
            return hashedEnteredPassword == storedHashedPassword;
        }
        internal static string key = "12345678901234567890123456789012";
        internal static string iv = "1234567890123456";
        internal async static Task<string> Encrypt(string plainText)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key);
                    aesAlg.IV = Encoding.UTF8.GetBytes(iv);
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    using (var msEncrypt = new MemoryStream())
                    using (var encryptor = aesAlg.CreateEncryptor())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        await swEncrypt.WriteAsync(plainText);
                        await swEncrypt.FlushAsync();
                        csEncrypt.FlushFinalBlock();
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Şifreleme Hatası: ", ex.Message);
                return null;
            }
        }
        internal async static Task<string> Decrypt(string cipherText)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key);
                    aesAlg.IV = Encoding.UTF8.GetBytes(iv);
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    using (var msDecrypt = new MemoryStream(cipherBytes))
                    using (var decryptor = aesAlg.CreateDecryptor())
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                        return await srDecrypt.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Şifre Çözme Hatası: ", ex.Message);
                return null;
            }
        }
    }
}