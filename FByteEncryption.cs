using System.IO;
using System.Security.Cryptography;

public static class FByteEncryption
{

    public static byte[] Encrypt(byte[] rawBytes, string password)
    {

        byte[] returnBytes = new byte[0];

        PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x43, 0x87, 0x23, 0x72 });
        using (MemoryStream memoryStream = new MemoryStream())
        {

            Aes aesManaged = new AesManaged();
            aesManaged.Key = pdb.GetBytes(aesManaged.KeySize / 8);
            aesManaged.IV = pdb.GetBytes(aesManaged.BlockSize / 8);

            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(rawBytes, 0, rawBytes.Length);
            };

            returnBytes = memoryStream.ToArray();

        };

        return returnBytes;

    }

    public static byte[] Decrypt(byte[] secretInput, string password)
    {

        byte[] returnBytes = new byte[0];

        PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x43, 0x87, 0x23, 0x72 });
        using (MemoryStream memoryStream = new MemoryStream())
        {

            Aes aesManaged = new AesManaged();
            aesManaged.Key = pdb.GetBytes(aesManaged.KeySize / 8);
            aesManaged.IV = pdb.GetBytes(aesManaged.BlockSize / 8);

            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(secretInput, 0, secretInput.Length);
            };

            returnBytes = memoryStream.ToArray();

        };

        return returnBytes;

    }

}
