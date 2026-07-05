using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace VRLearning.Core
{
    /// <summary>
    /// AES-256 (CBC) encryption for on-device data at rest and in transit (NFR-06).
    /// A random 256-bit key is generated once per install and stored locally. Each ciphertext
    /// carries its own random IV (prepended), so the same plaintext never encrypts identically.
    ///
    /// NOTE: for production, move the key into the Android Keystore / iOS Keychain rather than
    /// PlayerPrefs — this is a device-local key, adequate for anonymised learner data.
    /// </summary>
    public static class EncryptionService
    {
        private const string KeyPref = "vrl_enc_key_v1";
        private const int KeyBytes = 32; // 256-bit
        private const int IvBytes = 16;

        private static byte[] _key;

        private static byte[] Key
        {
            get
            {
                if (_key != null) return _key;

                string stored = PlayerPrefs.GetString(KeyPref, string.Empty);
                if (!string.IsNullOrEmpty(stored))
                {
                    _key = Convert.FromBase64String(stored);
                    return _key;
                }

                _key = new byte[KeyBytes];
                using (var rng = RandomNumberGenerator.Create())
                    rng.GetBytes(_key);
                PlayerPrefs.SetString(KeyPref, Convert.ToBase64String(_key));
                PlayerPrefs.Save();
                return _key;
            }
        }

        /// <summary>Encrypt a UTF-8 string; returns Base64 of [IV || ciphertext].</summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            return Convert.ToBase64String(EncryptBytes(Encoding.UTF8.GetBytes(plainText)));
        }

        /// <summary>Decrypt a Base64 [IV || ciphertext] string produced by <see cref="Encrypt"/>.</summary>
        public static string Decrypt(string cipherBase64)
        {
            if (string.IsNullOrEmpty(cipherBase64)) return cipherBase64;
            return Encoding.UTF8.GetString(DecryptBytes(Convert.FromBase64String(cipherBase64)));
        }

        /// <summary>Encrypt raw bytes; returns [IV || ciphertext].</summary>
        public static byte[] EncryptBytes(byte[] plain)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = Key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var enc = aes.CreateEncryptor();
            byte[] cipher = enc.TransformFinalBlock(plain, 0, plain.Length);

            byte[] result = new byte[aes.IV.Length + cipher.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipher, 0, result, aes.IV.Length, cipher.Length);
            return result;
        }

        /// <summary>Decrypt [IV || ciphertext] bytes.</summary>
        public static byte[] DecryptBytes(byte[] ivAndCipher)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = Key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[IvBytes];
            Buffer.BlockCopy(ivAndCipher, 0, iv, 0, IvBytes);
            aes.IV = iv;

            using var dec = aes.CreateDecryptor();
            return dec.TransformFinalBlock(ivAndCipher, IvBytes, ivAndCipher.Length - IvBytes);
        }
    }
}
