using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.String;

namespace FutsalFusion.Domain.Utilities;

public partial class Password
{
    public static readonly int PasswordSalt = 16;

    public static string CreateSalt(int size)
    {
        var rng = new RNGCryptoServiceProvider();
        
        var buff = new byte[size]; 

        rng.GetBytes(buff);
        
        return Convert.ToBase64String(buff);
    }
    
    public static string CreatePasswordHash(string password, string salt)
    {
        var saltAndPwd = Concat(password, salt);
        
        var pass = new List<byte>(Encoding.Unicode.GetBytes(saltAndPwd));
        
        var hashedPassword = Convert.ToBase64String(SHA512.HashData(pass.ToArray()));
        
        hashedPassword = Concat(hashedPassword, salt);
        
        return hashedPassword;
    }
    
    public static bool VerifyPassword(string userPassword, string dbUserPassword, int saltSize)
    {
        if (IsNullOrEmpty(dbUserPassword)) return false;
        
        var salt = dbUserPassword[^CreateSalt(saltSize).Length..];
        
        var hashedPasswordAndSalt = CreatePasswordHash(userPassword, salt);
        
        return hashedPasswordAndSalt.Equals(dbUserPassword);
    }
    
    private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
    {
        if (cipherText is not { Length: > 0 })
        {
            throw new ArgumentNullException("cipherText");
        }
        if (key is not { Length: > 0 })
        {
            throw new ArgumentNullException("key");
        }
        if (iv is not { Length: > 0 })
        {
            throw new ArgumentNullException("key");
        }

        string plaintext = null;

        using var rijAlg = new RijndaelManaged();

        rijAlg.Mode = CipherMode.CBC;
        
        rijAlg.Padding = PaddingMode.PKCS7;
        
        rijAlg.FeedbackSize = 128;

        rijAlg.Key = key;
        
        rijAlg.IV = iv;

        var decryptedString = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

        try
        {
            using var msDecrypt = new MemoryStream(cipherText);
            
            using var csDecrypt = new CryptoStream(msDecrypt, decryptedString, CryptoStreamMode.Read);
            
            using var srDecrypt = new StreamReader(csDecrypt);

            plaintext = srDecrypt.ReadToEnd();
        }
        catch
        {
            plaintext = "keyError";
        }

        return plaintext;
    }
    
    private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
    {
        if (plainText is not { Length: > 0 })
        {
            throw new ArgumentNullException("plainText");
        }
        if (key is not { Length: > 0 })
        {
            throw new ArgumentNullException("key");
        }
        if (iv is not { Length: > 0 })
        {
            throw new ArgumentNullException("key");
        }
        
        byte[] encrypted;
        
        using var rijAlg = new RijndaelManaged();
        
        rijAlg.Mode = CipherMode.CBC;
        
        rijAlg.Padding = PaddingMode.PKCS7;
        
        rijAlg.FeedbackSize = 128;

        rijAlg.Key = key;
        
        rijAlg.IV = iv;

        var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

        using var msEncrypt = new MemoryStream();
        
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        
        encrypted = msEncrypt.ToArray();

        return encrypted;
    }
    
    public static string DecryptStringAES(string cipherText)
    {
        var keyBytes = "8080808080808080"u8.ToArray();
        
        var iv = "8080808080808080"u8.ToArray();

        var encrypted = Convert.FromBase64String(cipherText);
        
        var decryptedFromJavaScript = DecryptStringFromBytes(encrypted, keyBytes, iv);
        
        return Format(decryptedFromJavaScript);
    }
    
    public static bool ValidatePassword(string password, out string errorMessage)
    {
        var input = password;
        
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new Exception("Password should not be empty");
        }

        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");
        var hasMiniMaxChars = new Regex(@".{8,20}");
        var hasLowerChar = new Regex(@"[a-z]+");
        var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

        if (!hasLowerChar.IsMatch(input))
        {
            errorMessage = "Password should contain at least one lower case letter.";
        }
        else if (!hasUpperChar.IsMatch(input))
        {
            errorMessage = "Password should contain at least one upper case letter.";
        }
        else if (!hasMiniMaxChars.IsMatch(input))
        {
            errorMessage = "Password should not be lesser than 8 or greater than 20 characters.";
        }
        else if (!hasNumber.IsMatch(input))
        {
            errorMessage = "Password should contain at least one numeric value.";
        }

        else if (!hasSymbols.IsMatch(input))
        {
            errorMessage = "Password should contain at least one special case character.";
        }

        return errorMessage.Length <= 0;
    }
}