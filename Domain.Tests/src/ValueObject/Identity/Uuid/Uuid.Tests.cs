using System;
using NUnit.Framework;
using System.Collections.Generic;
// using CleanArch.Domain.ValueObject.Identity.Uuid;

namespace Domain.Tests.ValueObject.Identity.Uuid
{
    internal class ConcreteUuid : CleanArch.Domain.ValueObject.Identity.Uuid.Uuid
    {
        internal ConcreteUuid(
            List<byte> timestampLow = null,
            List<byte> timestampMid = null,
            int version = 0x00,
            List<byte> timestampHigh = null,
            byte clockSequenceHigh = 0x00,
            byte clockSequenceLow = 0x00,
            List<byte> node = null
        ) : base(
            timestampLow ?? new List<byte> { 0x00, 0x00, 0x00, 0x00 },
            timestampMid ?? new List<byte> { 0x00, 0x00 },
            version,
            timestampHigh ?? new List<byte> { 0x00, 0x00 },
            clockSequenceHigh,
            clockSequenceLow,
            node ?? new List<byte> { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
        ) { }

        internal ConcreteUuid(string rfcUuidRepresentation) : base(rfcUuidRepresentation)
        {
        }
    }

    public class Tests
    {
        [Test]
        public void Expects4BitsVersion()
        {
            // given a version which requires more than 4 bits to store
            var version = 0x42;

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => new ConcreteUuid(version: version);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void Expects4TimestampLowBytes()
        {
            // given an invalid count of timestamp-low bytes
            var timestampLowBytes = new List<byte> { 0x42 };

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => new ConcreteUuid(timestampLow: timestampLowBytes);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void Expects2TimestampMidBytes()
        {
            // given an invalid count of timestamp-mid bytes
            var timestampMidBytes = new List<byte> { 0x42 };

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => new ConcreteUuid(timestampMid: timestampMidBytes);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void Expects2TimestampHighBytes()
        {
            // given an invalid count of timestamp-high bytes
            var timestampHighBytes = new List<byte> { 0x42 };

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => new ConcreteUuid(timestampHigh: timestampHighBytes);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void Expects6NodeBytes()
        {
            // given an invalid count of timestamp-high bytes
            var nodeBytes = new List<byte> { 0x42 };

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => new ConcreteUuid(node: nodeBytes);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void HasRfcCompliantRepresentation()
        {
            // given a valid Uuid
            var uuid = new ConcreteUuid();

            // when checking its string-representation
            string rfcRepresentation = uuid.ToString();

            // then it should comply with RFC format
            StringAssert.IsMatch(@"^[a-z0-9]{8}-([a-z0-9]{4}-){3}[a-z0-9]{12}$", rfcRepresentation);
        }

        [Test]
        public void InterlopsVersionInTimestampHighMostSignificantByte()
        {
            // given an uuid version 7
            var uuid = new ConcreteUuid(version: 7);

            // when checking its string-representation
            string rfcRepresentation = uuid.ToString();

            // then version should be stored in 12th digit
            Assert.AreEqual('7', rfcRepresentation[14]);
        }

        [Test]
        public void InterlopsVariantInClockSequenceHigh()
        {
            // given an uuid with default variant
            var uuid = new ConcreteUuid();

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
            var uuid = new ConcreteUuid();

            // when checking its variant
            string variant = uuid.Variant;

            // then it should be RFC by default
            StringAssert.AreEqualIgnoringCase("RFC", variant);
        }

        [Test]
        public void PutsAllBytesAtCorrectionPosition()
        {
            // given an uuid with known bytes
            var uuid = new ConcreteUuid(
                timestampLow: new List<byte>{ 0xde, 0xaf, 0xba, 0xbe },
                timestampMid: new List<byte> { 0xde, 0xad },
                version: 0x7,
                timestampHigh: new List<byte> { 0xe, 0xef },
                clockSequenceHigh: 0xa,
                clockSequenceLow: 0x84,
                node: new List<byte> { 0xc0, 0xff, 0xee, 0xba, 0x0b, 0xab }
            );

            // when checking its string-representation
            string rfcRepresentation = uuid.ToString();

            // then it should match the expected string
            Assert.AreEqual("deafbabe-dead-7eef-8a84-c0ffeeba0bab", rfcRepresentation);
        }

        [Test]
        public void RequiresValidRfcUuidString()
        {
            // given an uuid-string with one missing digit
            var uuidString = "000000-0000-000-0000-000000000000";

            // when trying to create an Uuid from it
            TestDelegate instanciation = () => new ConcreteUuid(uuidString);

            // then an exception should be thrown
            Assert.That(instanciation, Throws.ArgumentException);
        }

        [Test]
        public void CreatesInstanceFromString()
        {
            // given a RFC-compliant Uuid-string
            var uuidString = "02eb9d06-941c-4780-b5bd-01aaf3393a40";

            // when creating an instance from it
            var uuid = new ConcreteUuid(uuidString);

            // then it should give back the base Uuid-string
            Assert.AreEqual(uuidString, uuid.ToString());
        }

        private static IEnumerable<string[]> VariantDigitProvider()
        {
            foreach (char variantDigit in "01234567".ToCharArray()) {
                yield return new string[] { variantDigit.ToString(), "Apollo NCS (backward compatibility)" };
            }
            foreach (char variantDigit in "89ab".ToCharArray()) {
                yield return new string[] { variantDigit.ToString(), "RFC" };
            }
            foreach (char variantDigit in "cd".ToCharArray()) {
                yield return new string[] { variantDigit.ToString(), "Microsoft (backward compatibility)" };
            }
            foreach (char variantDigit in "ef".ToCharArray()) {
                yield return new string[] { variantDigit.ToString(), "Reserved (future definition)" };
            }
        }

        [Test]
        [TestCaseSource("VariantDigitProvider")]
        public void DetectsVariants(string variantDigit, string expectedVariant)
        {
            // given an Uuid made from a string with a given variant digit
            var uuid = new ConcreteUuid($"00000000-0000-cafe-{variantDigit}000-cafe00000000");
            // var uuid = new ConcreteUuid($"10000000-2000-3000-{variantDigit}000-000000000000");

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
            var (versionDigit, expectedVersion) = version;

            // given a versioned uuid-string
            var uuidString = $"00000000-0000-{versionDigit}000-0000-000000000000";

            // when creating an Uuid from it
            var uuid = new ConcreteUuid(uuidString);

            // then its version number should have been extracted from the given string
            Assert.AreEqual(expectedVersion, uuid.Version);
        }
    }
}