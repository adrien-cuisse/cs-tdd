using System.Collections.Immutable;
using System.Linq;
using System;
using NUnit.Framework;
using System.Collections.Generic;
// using CleanArch.Domain.ValueObject.Identity.Uuid;

namespace Domain.Tests.ValueObject.Identity.Uuid
{
    internal class ConcreteUuid : CleanArch.Domain.ValueObject.Identity.Uuid.Uuid
    {
        internal ConcreteUuid(int version, List<byte> bytes): base(version, bytes)
        {
        }

        internal ConcreteUuid(int version, List<byte> timestamp, List<byte> clockSequence, List<byte> node)
            : base(
                version,
                timestamp,
                clockSequence,
                node)
        {
        }

        internal ConcreteUuid(int version, string uuidString) : base(version, uuidString)
        {
        }

        internal static List<byte> DefaultBytes()
        {
            var bytes = new List<byte>(16);
            bytes.AddRange(DefaultTimestamp());
            bytes.AddRange(DefaultClockSequence());
            bytes.AddRange(DefaultNode());

            return bytes;
        }

        internal static List<byte> DefaultTimestamp() => new List<byte>
        {
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
        };

        internal static List<byte> DefaultClockSequence() => new List<byte>
        {
            0x00, 0x00,
        };

        internal static List<byte> DefaultNode() => new List<byte>
        {
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00,
        };

        internal static ConcreteUuid Constructor1(int version = 0, List<byte> bytes = null)
        {
            return new ConcreteUuid(version, bytes ?? DefaultBytes());
        }

        internal static ConcreteUuid Constructor2(int version = 0, List<byte> timestamp = null, List<byte> clockSequence = null, List<byte> node = null)
        {
            return new ConcreteUuid(
                version,
                timestamp ?? DefaultTimestamp(),
                clockSequence ?? DefaultClockSequence(),
                node ?? DefaultNode());
        }

        internal static ConcreteUuid Constructor3(int version = 0, string uuidString = "00000000-0000-0000-0000-000000000000")
        {
            return new ConcreteUuid(version, uuidString);
        }
    }

    public class Tests
    {
        [Test]
        public void Expects16Bytes()
        {
            // given a list of bytes which size is lesser than 16
            var bytes = new List<byte>();

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => ConcreteUuid.Constructor1(bytes: bytes);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void Expects4BitsVersion()
        {
            // given a version which requires more than 4 bits to store
            var version = 0x42;

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => ConcreteUuid.Constructor1(version: version);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void Expects8TimestampBytes()
        {
            // given an invalid count of clock-sequence bytes
            var timestampBytes = new List<byte>
            {
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                // requires more than 8 bytes for some reason, otherwise its unable to detect the (Count == 8) mutation
                0x00,
            };

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => ConcreteUuid.Constructor2(timestamp: timestampBytes);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void Expects2ClockSequenceBytes()
        {
            // given an invalid count of clock-sequence bytes
            var clockSequenceBytes = new List<byte>();

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => ConcreteUuid.Constructor2(clockSequence: clockSequenceBytes);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void Expects6NodeBytes()
        {
            // given an invalid count of node bytes
            var nodeBytes = new List<byte>();

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => ConcreteUuid.Constructor2(node: nodeBytes);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void HasRfcCompliantRepresentation()
        {
            // given a valid Uuid
            var uuid = ConcreteUuid.Constructor1();

            // when checking its string-representation
            string rfcRepresentation = uuid.ToString();

            // then it should comply with RFC format
            StringAssert.IsMatch(@"^[a-z0-9]{8}-([a-z0-9]{4}-){3}[a-z0-9]{12}$", rfcRepresentation);
        }

        [Test]
        public void InterlopsVersionInTimestampHighMostSignificantByte()
        {
            // given an uuid version 7
            var uuid = ConcreteUuid.Constructor1(version: 7);

            // when checking its string-representation
            string rfcRepresentation = uuid.ToString();

            // then version should be stored in 12th digit
            Assert.AreEqual('7', rfcRepresentation[14]);
        }

        [Test]
        public void InterlopsVariantInClockSequenceHigh()
        {
            // given an uuid with default variant
            var uuid = ConcreteUuid.Constructor1();

            // when checking the variant digit in its string-representation
            string variantHexaByte = uuid.ToString().Substring(startIndex: 19, length: 2);
            byte variantByte = Convert.ToByte(value: variantHexaByte, fromBase: 16);

            // then it should have 2 most significants bits set to 10
            Assert.AreEqual(0b_1000_0000, variantByte & 0b_1100_0000);
        }

        [Test]
        public void HasRfcVariantByDefault()
        {
            // given an uuid made from bytes
            var uuid = ConcreteUuid.Constructor1();

            // when checking its variant
            string variant = uuid.Variant;

            // then it should be RFC by default
            StringAssert.AreEqualIgnoringCase("RFC", variant);
        }

        [Test]
        public void PutsAllBytesAtCorrectionPosition()
        {
            // given an uuid with known bytes
            var uuid = ConcreteUuid.Constructor1(bytes: new List<byte>
            {
                0xde, 0xaf, 0xba, 0xbe,
                0xde, 0xad, 0x01, 0x23,
                0xbe, 0xef,
                0xc0, 0xff, 0xee,
                0xba, 0x0b, 0xab,
            });

            // when checking its string-representation
            string rfcRepresentation = uuid.ToString();

            // then it should match the expected string
            Assert.AreEqual("deafbabe-dead-0123-beef-c0ffeeba0bab", rfcRepresentation);
        }

        [Test]
        public void RequiresValidRfcUuidString()
        {
            // given an uuid-string with one missing digit
            var uuidString = "000000-0000-000-0000-000000000000";

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => ConcreteUuid.Constructor3(uuidString: uuidString);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void RequiresCorrectVersionInString()
        {
            // given a RFC-compliant Uuid-string but with incorrect version
            var uuidString = "00000000-0000-7000-0000-000000000000";

            // when creating an instance from it and specifying a different version
            TestDelegate instanciation = () => ConcreteUuid.Constructor3(version: 5, uuidString: uuidString);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void CreatesInstanceFromString()
        {
            // given a RFC-compliant Uuid-string
            var uuidString = "02eb9d06-941c-0780-b5bd-01aaf3393a40";

            // when creating an instance from it
            var uuid = ConcreteUuid.Constructor3(uuidString: uuidString);

            // then it should give back the base Uuid-string
            Assert.AreEqual(uuidString, uuid.ToString());
        }

        private static IEnumerable<string[]> VariantDigitProvider()
        {
            var variants = new Dictionary<string, string>
            {
                { "01234567", "Apollo NCS (backward compatibility)" },
                { "89ab", "RFC" },
                { "cd", "Microsoft (backward compatibility)" },
                { "ef", "Reserved (future definition)" },
            };

            foreach (KeyValuePair<string,string> variant in variants)
            {
                foreach (char variantDigit in variant.Key.ToCharArray())
                {
                    yield return new string[]
                    {
                        variantDigit.ToString(),
                        variant.Value,
                    };
                }
            }
        }

        [Test]
        [TestCaseSource("VariantDigitProvider")]
        public void DetectsVariants(string variantDigit, string expectedVariant)
        {
            // given an Uuid made from a string with a given variant digit
            var uuid = ConcreteUuid.Constructor3(
                version: 0,
                uuidString: $"00000000-0000-0000-{variantDigit}000-000000000000");

            // whecn cheking its variant
            string detectedVariant = uuid.Variant;

            // then it should match the expectation
            Assert.AreEqual(expectedVariant, detectedVariant);
        }

        private static IEnumerable<(char, int)> VersionDigitProvider()
        {
            foreach (char versionDigit in "0123456789abcdef".ToCharArray()) {
                int version = Convert.ToInt32($"0x{versionDigit}", 16);
                yield return (versionDigit, version);
            }
        }

        [Test]
        [TestCaseSource("VersionDigitProvider")]
        // public void CreatesVersionedUuidFromString((char, int) version)
        public void CreatesVersionedUuidFromString((char, int) version)
        {
            // given a versioned uuid-string
            var (versionDigit, expectedVersion) = version;
            var uuidString = $"00000000-0000-{versionDigit}000-0000-000000000000";

            // when creating an Uuid from it
            var uuid = ConcreteUuid.Constructor3(version: expectedVersion, uuidString: uuidString);

            // then its version number should have been extracted from the given string
            Assert.AreEqual(expectedVersion, uuid.Version);
        }
    }
}