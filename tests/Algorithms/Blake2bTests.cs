using System;
using NSec.Cryptography;
using Xunit;

namespace NSec.Tests.Algorithms
{
    public static class Blake2bTests
    {
        #region Properties

        [Fact]
        public static void KeyProperties()
        {
            var a = new Blake2b();

            Assert.True(a.MinKeySize >= 0);
            Assert.True(a.DefaultKeySize > 0);
            Assert.True(a.DefaultKeySize >= a.MinKeySize);
            Assert.True(a.MaxKeySize >= a.DefaultKeySize);
        }

        [Fact]
        public static void Properties()
        {
            var a = new Blake2b();

            Assert.Equal(16, a.MinKeySize);
            Assert.Equal(32, a.DefaultKeySize);
            Assert.Equal(64, a.MaxKeySize);

            Assert.Equal(32, a.MinHashSize);
            Assert.Equal(32, a.DefaultHashSize);
            Assert.Equal(64, a.MaxHashSize);
        }

        #endregion

        #region Export #1

        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        public static void ExportImportRaw(int keySize)
        {
            var a = new Blake2b();
            var b = Utilities.RandomBytes.Slice(0, keySize);

            using (var k = Key.Import(a, b, KeyBlobFormat.RawSymmetricKey, KeyFlags.AllowArchiving))
            {
                Assert.Equal(KeyFlags.AllowArchiving, k.Flags);

                var expected = b.ToArray();
                var actual = k.Export(KeyBlobFormat.RawSymmetricKey);

                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        public static void ExportImportNSec(int keySize)
        {
            var a = new Blake2b();
            var b = Utilities.RandomBytes.Slice(0, keySize);

            using (var k1 = Key.Import(a, b, KeyBlobFormat.RawSymmetricKey, KeyFlags.AllowArchiving))
            {
                Assert.Equal(KeyFlags.AllowArchiving, k1.Flags);

                var n = k1.Export(KeyBlobFormat.NSecSymmetricKey);
                Assert.NotNull(n);

                using (var k2 = Key.Import(a, n, KeyBlobFormat.NSecSymmetricKey, KeyFlags.AllowArchiving))
                {
                    var expected = b.ToArray();
                    var actual = k2.Export(KeyBlobFormat.RawSymmetricKey);

                    Assert.Equal(expected, actual);
                }
            }
        }

        #endregion

        #region Hash #1

        [Fact]
        public static void HashWithNullKey()
        {
            var a = new Blake2b();

            Assert.Throws<ArgumentNullException>("key", () => a.Hash(null, ReadOnlySpan<byte>.Empty));
        }

        [Fact]
        public static void HashWithWrongKey()
        {
            var a = new Blake2b();

            using (var k = new Key(new Ed25519()))
            {
                Assert.Throws<ArgumentException>("key", () => a.Hash(k, ReadOnlySpan<byte>.Empty));
            }
        }

        public static void HashWithKeySuccess()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                var b = a.Hash(k, ReadOnlySpan<byte>.Empty);

                Assert.NotNull(b);
                Assert.Equal(a.DefaultHashSize, b.Length);
            }
        }

        #endregion

        #region Hash #2

        [Fact]
        public static void HashWithSizeWithNullKey()
        {
            var a = new Blake2b();

            Assert.Throws<ArgumentNullException>("key", () => a.Hash(null, ReadOnlySpan<byte>.Empty, 0));
        }

        [Fact]
        public static void HashWithSizeWithWrongKey()
        {
            var a = new Blake2b();

            using (var k = new Key(new Ed25519()))
            {
                Assert.Throws<ArgumentException>("key", () => a.Hash(k, ReadOnlySpan<byte>.Empty, 0));
            }
        }

        [Fact]
        public static void HashWithSizeTooSmall()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                Assert.Throws<ArgumentOutOfRangeException>("hashSize", () => a.Hash(k, ReadOnlySpan<byte>.Empty, a.MinHashSize - 1));
            }
        }

        [Fact]
        public static void HashWithSizeTooLarge()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                Assert.Throws<ArgumentOutOfRangeException>("hashSize", () => a.Hash(k, ReadOnlySpan<byte>.Empty, a.MaxHashSize + 1));
            }
        }

        [Fact]
        public static void HashWithMinSizeSuccess()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                var b = a.Hash(k, ReadOnlySpan<byte>.Empty, a.MinHashSize);

                Assert.NotNull(b);
                Assert.Equal(a.MinHashSize, b.Length);
            }
        }

        [Fact]
        public static void HashWithMaxSizeSuccess()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                var b = a.Hash(k, ReadOnlySpan<byte>.Empty, a.MaxHashSize);

                Assert.NotNull(b);
                Assert.Equal(a.MaxHashSize, b.Length);
            }
        }

        #endregion

        #region Hash #3

        [Fact]
        public static void HashWithSpanWithNullKey()
        {
            var a = new Blake2b();

            Assert.Throws<ArgumentNullException>("key", () => a.Hash(null, ReadOnlySpan<byte>.Empty, Span<byte>.Empty));
        }

        [Fact]
        public static void HashWithSpanWithWrongKey()
        {
            var a = new Blake2b();

            using (var k = new Key(new Ed25519()))
            {
                Assert.Throws<ArgumentException>("key", () => a.Hash(k, ReadOnlySpan<byte>.Empty, Span<byte>.Empty));
            }
        }

        [Fact]
        public static void HashWithSpanTooSmall()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                Assert.Throws<ArgumentException>("hash", () => a.Hash(k, ReadOnlySpan<byte>.Empty, new byte[a.MinHashSize - 1]));
            }
        }

        [Fact]
        public static void HashWithSpanTooLarge()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                Assert.Throws<ArgumentException>("hash", () => a.Hash(k, ReadOnlySpan<byte>.Empty, new byte[a.MaxHashSize + 1]));
            }
        }

        [Fact]
        public static void HashWithMinSpanSuccess()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                a.Hash(k, ReadOnlySpan<byte>.Empty, new byte[a.MinHashSize]);
            }
        }

        [Fact]
        public static void HashWithMaxSpanSuccess()
        {
            var a = new Blake2b();

            using (var k = new Key(a))
            {
                a.Hash(k, ReadOnlySpan<byte>.Empty, new byte[a.MaxHashSize]);
            }
        }

        #endregion

        #region CreateKey

        [Fact]
        public static void CreateKey()
        {
            var a = new Blake2b();

            using (var k = new Key(a, KeyFlags.AllowArchiving))
            {
                var actual = k.Export(KeyBlobFormat.RawSymmetricKey);

                var unexpected = new byte[actual.Length];
                Utilities.Fill(unexpected, 0xDB);

                Assert.NotEqual(unexpected, actual);
            }
        }

        #endregion
    }
}
