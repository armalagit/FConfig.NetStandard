using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class ByteEncryption
{

    /// <summary>
    /// Encrypts the given raw bytes using the specified password.
    /// </summary>
    /// <param name="rawBytes">The raw bytes to encrypt.</param>
    /// <param name="secretKey">The password to use for encryption.</param>
    /// <returns>The encrypted bytes.</returns>
    public static byte[] Encrypt(byte[] rawBytes, string secretKey) {
        // Initialize the return bytes as an empty array
        byte[] returnBytes = new byte[0];

        // Create a PasswordDeriveBytes object using the specified password and salt
        PasswordDeriveBytes pdb = new PasswordDeriveBytes(secretKey, new byte[] { 0x43, 0x87, 0x23, 0x72 });

        // Use a MemoryStream to store the encrypted bytes
        using (MemoryStream memoryStream = new MemoryStream()) {
            // Create an AesManaged object
            Aes aesManaged = new AesManaged();

            // Set the key and IV of the AesManaged object using the derived bytes
            aesManaged.Key = pdb.GetBytes(aesManaged.KeySize / 8);
            aesManaged.IV = pdb.GetBytes(aesManaged.BlockSize / 8);

            // Use a CryptoStream to encrypt the raw bytes and write them to the MemoryStream
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateEncryptor(), CryptoStreamMode.Write)) {
                cryptoStream.Write(rawBytes, 0, rawBytes.Length);
            }

            // Set the return bytes as the bytes from the MemoryStream
            returnBytes = memoryStream.ToArray();
        }

        // Return the encrypted bytes
        return returnBytes;
    }

    /// <summary>
    /// Decrypts the given encrypted bytes using the specified password.
    /// </summary>
    /// <param name="secretInput">The encrypted bytes to decrypt.</param>
    /// <param name="password">The password to use for decryption.</param>
    /// <returns>The decrypted bytes.</returns>
    public static byte[] Decrypt(byte[] secretInput, string password = null) {
        // Initialize the return bytes as an empty array
        byte[] returnBytes = new byte[0];

        // Create a PasswordDeriveBytes object using the specified password and salt
        PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x43, 0x87, 0x23, 0x72 });

        // Use a MemoryStream to store the decrypted bytes
        using (MemoryStream memoryStream = new MemoryStream()) {
            // Create an AesManaged object
            Aes aesManaged = new AesManaged();

            // Set the key and IV of the AesManaged object using the derived bytes
            aesManaged.Key = pdb.GetBytes(aesManaged.KeySize / 8);
            aesManaged.IV = pdb.GetBytes(aesManaged.BlockSize / 8);

            // Use a CryptoStream to decrypt the secret input and write the decrypted bytes to the MemoryStream
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateDecryptor(), CryptoStreamMode.Write)) {
                cryptoStream.Write(secretInput, 0, secretInput.Length);
            }

            // Set the return bytes as the bytes from the MemoryStream
            returnBytes = memoryStream.ToArray();
        }

        // Return the decrypted bytes
        return returnBytes;
    }

    /// <summary>
    /// Encrypts the given raw bytes.
    /// </summary>
    /// <param name="rawBytes">The raw bytes to encrypt.</param>
    /// <param name="salt">An optional salt to use for encryption. If not specified, the salt will be null.</param>
    /// <param name="useMd5">A boolean indicating whether to use MD5 for encryption. If true, MD5 will be used. If false, it will not be used.</param>
    /// <returns>The encrypted bytes.</returns>
    public static byte[] GetMd5Hash(byte[] rawBytes, string salt = null) {
        // Initialize the return bytes as an empty array
        byte[] returnBytes = new byte[0];

        // If a salt is specified, prepend it to the raw bytes
        if (salt != null) {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] combinedBytes = new byte[saltBytes.Length + rawBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(rawBytes, 0, combinedBytes, saltBytes.Length, rawBytes.Length);
            rawBytes = combinedBytes;
        }

        // Use an MD5 object to encrypt the raw bytes
        using (MD5 md5 = MD5.Create()) {
            md5.TransformFinalBlock(rawBytes, 0, rawBytes.Length);
            returnBytes = md5.Hash;
        }

        // Return the encrypted bytes
        return returnBytes;
    }

}
