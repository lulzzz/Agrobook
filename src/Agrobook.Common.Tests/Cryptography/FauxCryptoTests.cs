﻿using Agrobook.Common.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agrobook.Common.Tests.Cryptography
{
    [TestClass]
    public class FauxCryptoTests
    {
        private FauxCrypto sut = new FauxCrypto();

        [TestMethod]
        public void CanEncrypt()
        {
            var text = "pass";
            var encrypted = this.sut.Encrypt(text);

            Assert.AreNotEqual("pass", encrypted);
        }

        [TestMethod]
        public void CanDecryp()
        {
            var text = "pass";
            var encrypted = this.sut.Encrypt(text);
            var decrypted = this.sut.Decrypt(encrypted);

            Assert.AreEqual(text, decrypted);
        }

        [TestMethod]
        public void CanDecrypMultipleTimesTheSameValue()
        {
            var text = "pass";
            var encrypted = this.sut.Encrypt(text);
            var decrypted = this.sut.Decrypt(encrypted);

            Assert.AreEqual(text, decrypted);

            var enc2 = this.sut.Encrypt(text);
            var dec2 = this.sut.Decrypt(encrypted);

            Assert.AreEqual(encrypted, enc2);
            Assert.AreEqual(decrypted, dec2);
        }
    }
}
