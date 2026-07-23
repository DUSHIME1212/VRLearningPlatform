using NUnit.Framework;
using VRLearning.Core;

namespace VRLearning.Tests.EditMode
{
    public class EncryptionServiceTests
    {
        [Test]
        public void Encrypt_ThenDecrypt_RoundTripsExactly()
        {
            const string original = "Hello, Kigali! Ubwenge 123";

            string cipher = EncryptionService.Encrypt(original);
            string result = EncryptionService.Decrypt(cipher);

            Assert.AreEqual(original, result);
        }

        [Test]
        public void Encrypt_SamePlaintextTwice_ProducesDifferentCiphertext()
        {
            const string plain = "repeat me";

            string a = EncryptionService.Encrypt(plain);
            string b = EncryptionService.Encrypt(plain);

            Assert.AreNotEqual(a, b, "Each call must use a fresh random IV, so identical plaintext must not produce identical ciphertext.");
        }

        [Test]
        public void Encrypt_NullOrEmpty_PassesThroughUnchanged()
        {
            Assert.IsNull(EncryptionService.Encrypt(null));
            Assert.AreEqual(string.Empty, EncryptionService.Encrypt(string.Empty));
        }

        [Test]
        public void Decrypt_NullOrEmpty_PassesThroughUnchanged()
        {
            Assert.IsNull(EncryptionService.Decrypt(null));
            Assert.AreEqual(string.Empty, EncryptionService.Decrypt(string.Empty));
        }

        [Test]
        public void EncryptBytes_ThenDecryptBytes_RoundTripsBinaryData()
        {
            byte[] original = { 0, 1, 2, 255, 254, 253, 10, 13 };

            byte[] cipher = EncryptionService.EncryptBytes(original);
            byte[] result = EncryptionService.DecryptBytes(cipher);

            CollectionAssert.AreEqual(original, result);
        }

        [Test]
        public void Decrypt_TamperedCiphertext_ThrowsInsteadOfReturningGarbageData()
        {
            // Corrupting the final PKCS7 padding byte always yields an out-of-range padding
            // length (1-16 XOR 0xFF never maps back into 1-16), so this is a deterministic throw,
            // not a probabilistic one. This guards NFR-11 (zero silent data corruption).
            string cipher = EncryptionService.Encrypt("sensitive learner data");
            byte[] bytes = System.Convert.FromBase64String(cipher);
            bytes[bytes.Length - 1] ^= 0xFF;
            string tampered = System.Convert.ToBase64String(bytes);

            Assert.Throws<System.Security.Cryptography.CryptographicException>(() => EncryptionService.Decrypt(tampered));
        }
    }
}
