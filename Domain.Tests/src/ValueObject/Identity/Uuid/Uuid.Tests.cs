using System;
using NUnit.Framework;
using System.Collections.Generic;
// using CleanArch.Domain.ValueObject.Identity.Uuid;

namespace Domain.Tests.ValueObject.Identity.Uuid
{
    internal class UuidImplementation : CleanArch.Domain.ValueObject.Identity.Uuid.Uuid
    {
        internal UuidImplementation(
            List<byte> timestampLow = null,
            List<byte> timestampMid = null,
            int version = 0x0f,
            List<byte> timestampHigh = null,
            byte clockSequenceHigh = 0x8,
            byte clockSequenceLow = 0x09,
            List<byte> node = null
        ) : base(
            timestampLow ?? new List<byte> { 0x00, 0x01, 0x02, 0x03 },
            timestampMid ?? new List<byte> { 0x04, 0x05 },
            version,
            timestampHigh ?? new List<byte> { 0x06, 0x07 },
            clockSequenceHigh,
            clockSequenceLow,
            node ?? new List<byte> { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f }
        ) { }

        internal UuidImplementation(string rfcUuidRepresentation) : base(rfcUuidRepresentation)
        {
        }
    }

    public class Tests
    {
        // Is it even possible ?
        // static object[] InvalidBytesCountProvider =
        // {
        //     new { name = "timestampLow", timestampLow = new List<byte> { 0x42 } },
        //     new { name = "timestampMid", timestampMid = new List<byte> { 0x42 } },
        //     new { name = "timestampHigh", timestampHigh = new List<byte> { 0x42 } },
        //     new { name = "node", node = new List<byte> { 0x42 } },
        // };

        // [Test, TestCaseSource("InvalidBytesCountProvider")]
        // public void ExpectsExactBytesCount(string bytesNames, List<byte> bytes)
        // {
        //     Assert.Equals(new UuidImplementation { bytesNames: bytes }, null);
        //     // Assert.Equals(1, 1);
        // }

        // [SetUp]
        // public void Setup()
        // {
        // }

        // [Test]
        // public void Expects4BitsVersion()
        // {
        //     // given a version which requires more than 4 bits to store
        //     var version = 0x42;

        //     // when trying to create an Uuid from it
        //     TestDelegate instanciation = () => new UuidImplementation(version: version);

        //     // then an exception should be thrown
        //     Assert.That(instanciation, Throws.ArgumentException);
        // }

        // [Test]
        // public void Expects4TimestampLowBytes()
        // {
        //     // given an invalid count of timestamp-low bytes
        //     var timestampLowBytes = new List<byte> { 0x42 };

        //     // when trying to create an Uuid from it
        //     TestDelegate instanciation = () => new UuidImplementation(timestampLow: timestampLowBytes);

        //     // then an exception should be thrown
        //     Assert.That(instanciation, Throws.ArgumentException);
        // }

        // [Test]
        // public void Expects2TimestampMidBytes()
        // {
        //     // given an invalid count of timestamp-mid bytes
        //     var timestampMidBytes = new List<byte> { 0x42 };

        //     // when trying to create an Uuid from it
        //     TestDelegate instanciation = () => new UuidImplementation(timestampMid: timestampMidBytes);

        //     // then an exception should be thrown
        //     Assert.That(instanciation, Throws.ArgumentException);
        // }

        // [Test]
        // public void Expects2TimestampHighBytes()
        // {
        //     // given an invalid count of timestamp-high bytes
        //     var timestampHighBytes = new List<byte> { 0x42 };

        //     // when trying to create an Uuid from it
        //     TestDelegate instanciation = () => new UuidImplementation(timestampHigh: timestampHighBytes);

        //     // then an exception should be thrown
        //     Assert.That(instanciation, Throws.ArgumentException);
        // }

        // [Test]
        // public void Expects6NodeBytes()
        // {
        //     // given an invalid count of timestamp-high bytes
        //     var nodeBytes = new List<byte> { 0x42 };

        //     // when trying to create an Uuid from it
        //     TestDelegate instanciation = () => new UuidImplementation(node: nodeBytes);

        //     // then an exception should be thrown
        //     Assert.That(instanciation, Throws.ArgumentException);
        // }

        // [Test]
        // public void HasRfcCompliantRepresentation()
        // {
        //     // given a valid Uuid
        //     var uuid = new UuidImplementation();

        //     // when checking its string-representation
        //     string rfcRepresentation = uuid.ToString();

        //     // then it should comply with RFC format
        //     StringAssert.IsMatch(@"^[a-z0-9]{8}-([a-z0-9]{4}-){3}[a-z0-9]{12}$", rfcRepresentation);
        // }

        // [Test]
        // public void InterlopsVersionInTimestampHighMostSignificantByte()
        // {
        //     // given an uuid version 7
        //     var uuid = new UuidImplementation(version: 7);

        //     // when checking its string-representation
        //     string rfcRepresentation = uuid.ToString();

        //     // then version should be stored in 12th digit
        //     Assert.AreEqual('7', rfcRepresentation[14]);
        // }

        // [Test]
        // public void InterlopsVariantInClockSequenceHigh()
        // {
        //     // given an uuid with default variant
        //     var uuid = new UuidImplementation();

        //     // when checking the variant digit in its string-representation
        //     string variantHexaByte = uuid.ToString().Substring(startIndex: 19, length: 2);
        //     byte variantByte = Convert.ToByte(value: variantHexaByte, fromBase: 16);

        //     // then it should have 2 most significants bits set to 10
        //     Assert.AreEqual(0b_1000_0000, variantByte & 0b_1100_0000);
        // }

        // [Test]
        // public void HasRfcVariantByDefault()
        // {
        //     // given an uuid made from bytes
        //     var uuid = new UuidImplementation();

        //     // when checking its variant
        //     string variant = uuid.Variant;

        //     // then it should be RFC by default
        //     StringAssert.AreEqualIgnoringCase("RFC", variant);
        // }

        // [Test]
        // public void PutsAllBytesAtCorrectionPosition()
        // {
        //     // given an uuid with known bytes
        //     var uuid = new UuidImplementation(
        //         timestampLow: new List<byte>{ 0xde, 0xaf, 0xba, 0xbe },
        //         timestampMid: new List<byte> { 0xde, 0xad },
        //         version: 0x7,
        //         timestampHigh: new List<byte> { 0xe, 0xef },
        //         clockSequenceHigh: 0xa,
        //         clockSequenceLow: 0x84,
        //         node: new List<byte> { 0xc0, 0xff, 0xee, 0xba, 0x0b, 0xab }
        //     );

        //     // when checking its string-representation
        //     string rfcRepresentation = uuid.ToString();

        //     // then it should match the expected string
        //     Assert.AreEqual("deafbabe-dead-7eef-8a84-c0ffeeba0bab", rfcRepresentation);
        // }

        // [Test]
        // public void RequiresValidRfcUuidString()
        // {
        //     // given an uuid-string with one missing digit
        //     var uuidString = "000000-0000-000-0000-000000000000";

        //     // when trying to create an Uuid from it
        //     TestDelegate instanciation = () => new UuidImplementation(uuidString);

        //     // then an exception should be thrown
        //     Assert.That(instanciation, Throws.ArgumentException);
        // }

        [Test]
        public void CreatesInstanceFromString()
        {
            // given a RFC-compliant Uuid-string
            var uuidString = "02eb9d06-941c-4780-b5bd-01aaf3393a40";

            // when creating an instance from it
            var uuid = new UuidImplementation(uuidString);

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
            var uuid = new UuidImplementation($"cafecafe-0000-cafe-{variantDigit}000-cafecafecafe");
            // var uuid = new UuidImplementation($"10000000-2000-3000-{variantDigit}000-000000000000");

            // whecn cheking its variant
            string detectedVariant = uuid.Variant;

            // then it should match the expectation
            Assert.AreEqual(expectedVariant, detectedVariant);
        }
    }
}

// final class UuidTest extends TestCase
// {
//     /**
//      * @return UuidInterface - an instance to test
//      */
//     private function createInstance(
//         int $version = 0,
//         array $timestampLowBytes = [0, 0, 0, 0],
//         array $timestampMidBytes = [0, 0],
//         array $timestampHighBytes = [0, 0],
//         int $clockSequenceHighByte = 0,
//         int $clockSequenceLowByte = 0,
//         array $nodeBytes = [0, 0, 0, 0, 0, 0],
//     ): UuidInterface {
//         return new class(
//             $version,
//             $timestampLowBytes,
//             $timestampMidBytes,
//             $timestampHighBytes,
//             $clockSequenceHighByte,
//             $clockSequenceLowByte,
//             $nodeBytes,
//         ) extends Uuid {
//             public function __construct(
//                 int $version,
//                 array $timestampLowBytes,
//                 array $timestampMidBytes,
//                 array $timestampHighBytes,
//                 int $clockSequenceHighByte,
//                 int $clockSequenceLowByte,
//                 array $nodeBytes,
//             ) {
//                 parent::__construct(
//                     version: $version,
//                     timestampLowBytes: $timestampLowBytes,
//                     timestampMidBytes: $timestampMidBytes,
//                     timestampHighBytes: $timestampHighBytes,
//                     clockSequenceHighByte: $clockSequenceHighByte,
//                     clockSequenceLowByte: $clockSequenceLowByte,
//                     nodeBytes: $nodeBytes,
//                 );
//             }

//             public static function fromString(string $uuidString): static
//             {
//                 return parent::fromString(rfcUuidString: $uuidString);
//             }
//         };
//     }

//

//     /**
//      * @test
//      * @testdox Interlops version in timestamp-high MSB
//      * @covers ::__toString
//      * @covers ::toRfcUuidString
//      * @covers ::hexaStringFrom
//      */
//     public function interlops_version_in_time_high_MSB(): void
//     {
//         // given some Uuid
//         $uuid = $this->createInstance(version: 0b0000_0101, timestampHighBytes: [0b0000_1010, 0x0000_0110]);

//         // when extracting its byte 6
//         $seventhByteHexaString = substr(string: (string) $uuid, offset: 14, length: 2);
//         sscanf($seventhByteHexaString, '%2x', $seventhByte);

//         // then version should be interloped with timestamp-high MSB
//         $this->assertSame(
//             expected: 0b0101_1010,
//             actual: $seventhByte,
//             message: "Uuid should interlop version in timestamp-high MSB"
//         );
//     }

//     /**
//      * @test
//      * @testdox Interlops RFC variant in clock-sequence-high byte
//      * @covers ::__toString
//      * @covers ::toRfcUuidString
//      * @covers ::hexaStringFrom
//      */
//     public function interlops_RFC_variant_in_clock_seq_high(): void
//     {
//         // given some Uuid
//         $uuid = $this->createInstance(clockSequenceHighByte: 0b0011_1111);

//         // when extracting its byte 8
//         $ninthByteHexaString = substr(string: (string) $uuid, offset: 19, length: 2);
//         sscanf($ninthByteHexaString, '%2x', $ninthByte);

//         // then variant should be interloped with clock-sequence-high
//         $this->assertSame(
//             expected: 0b1011_1111,
//             actual: $ninthByte,
//             message: "Uuid should interlop variant in clock-sequence-high"
//         );
//     }

//

//     public function byteProvider(): Generator
//     {
//         $bytes = [
//             'version' => 0b1111,
//             'timestampLowBytes' => [0xde, 0xaf, 0xba, 0xbe],
//             'timestampMidBytes' => [0xde, 0xad],
//             'timestampHighBytes' => [0xbe, 0xef],
//             'clockSequenceHighByte' => 0x80,
//             'clockSequenceLowByte' => 0x00,
//             'nodeBytes' => [0xc0, 0xff, 0xee, 0xba, 0x0b, 0xab],
//         ];

//         yield 'timestamp-low byte 0' => [
//             $bytes,
//             'timestamp-low byte 0',
//             $bytes['timestampLowBytes'][0],
//             0
//         ];
//         yield 'timestamp-low byte 1' => [
//             $bytes,
//             'timestamp-low byte 1',
//             $bytes['timestampLowBytes'][1],
//             2
//         ];
//         yield 'timestamp-low byte 2' => [
//             $bytes,
//             'timestamp-low byte 2',
//             $bytes['timestampLowBytes'][2],
//             4
//         ];
//         yield 'timestamp-low byte 3' => [
//             $bytes,
//             'timestamp-low byte 3',
//             $bytes['timestampLowBytes'][3],
//             6
//         ];
//         yield 'timestamp-mid byte 0' => [
//             $bytes,
//             'timestamp-mid byte 0',
//             $bytes['timestampMidBytes'][0],
//             9
//         ];
//         yield 'timestamp-mid byte 1' => [
//             $bytes,
//             'timestamp-mid byte 1',
//             $bytes['timestampMidBytes'][1],
//             11
//         ];
//         yield 'timestamp-high byte 0 and version' => [
//             $bytes,
//             'timestamp-high byte 0 and version',
//             $bytes['timestampHighBytes'][0],
//             14
//         ];
//         yield 'timestamp-high byte 1' => [
//             $bytes,
//             'timestamp-high byte 1',
//             $bytes['timestampHighBytes'][1],
//             16
//         ];
//         yield 'clock-sequence-high and variant' => [
//             $bytes,
//             'clock-sequence-high and variant',
//             $bytes['clockSequenceHighByte'],
//             19
//         ];
//         yield 'clock-sequence-low' => [
//             $bytes,
//             'clock-sequence-low',
//             $bytes['clockSequenceLowByte'],
//             21
//         ];
//         yield 'node byte 0' => [
//             $bytes,
//             'node byte 0',
//             $bytes['nodeBytes'][0],
//             24
//         ];
//         yield 'node byte 1' => [
//             $bytes,
//             'node byte 1',
//             $bytes['nodeBytes'][1],
//             26
//         ];
//         yield 'node byte 2' => [
//             $bytes,
//             'node byte 2',
//             $bytes['nodeBytes'][2],
//             28
//         ];
//         yield 'node byte 3' => [
//             $bytes,
//             'node byte 3',
//             $bytes['nodeBytes'][3],
//             30
//         ];
//         yield 'node byte 4' => [
//             $bytes,
//             'node byte 4',
//             $bytes['nodeBytes'][4],
//             32
//         ];
//         yield 'node byte 5' => [
//             $bytes,
//             'node byte 5',
//             $bytes['nodeBytes'][5],
//             34
//         ];
//     }

//     /**
//      * @test
//      * @testdox Puts $byteName at correct position in string
//      * @dataProvider byteProvider
//      * @covers ::__construct
//      * @covers ::__toString
//      * @covers ::toRfcUuidString
//      * @covers ::hexaStringFrom
//      */
//     public function puts_byte_at_correct_position_in_string(array $uuidBytes, string $byteName, int $expectedByteValue, int $positionInString): void
//     {
//         // given an Uuid as a string
//         $uuid = $this->createInstance(...$uuidBytes);
//         $uuidString = (string) $uuid;

//         // when accessing the byte at given position in the string
//         $byteHexaString = substr($uuidString, $positionInString, 2);
//         sscanf($byteHexaString, '%2x', $byte);

//         // then the byte should have the given value
//         $this->assertSame(
//             expected: $expectedByteValue,
//             actual: $byte & $expectedByteValue,
//             message: "Expected {$byteName} ($expectedByteValue) to be at position {$positionInString} in string {$uuidString}, found {$byteHexaString} ({$byte})"
//         );
//     }

//     /**
//      * @test
//      * @testdox Creating an instance requires RFC-compliant Uuid-string
//      * @covers ::fromString
//      * @uses Alphonse\CleanArch\Domain\Fields\Identity\Uuid\InvalidUuidStringException
//      */
//     public function expects_rfc_compliant_uuid_string(): void
//     {
//         $this->expectException(InvalidUuidStringException::class);

//         // given an invalid Uuid-string
//         $invalidUuidString = 'invalid string';

//         // when trying to create an Uuid form it
//         call_user_func_array(
//             callback: $this->createInstance()::class . '::fromString',
//             args: [$invalidUuidString],
//         );

//         // then instantiation should be rejected
//     }

//     /**
//      * @test
//      * @testdox Uuid made from string gives correct string back
//      * @covers ::fromString
//      * @covers ::__toString
//      * @covers ::toRfcUuidString
//      */
//     public function creates_uuid_matching_base_string(): void
//     {
//         // given an rfc-compliant uuid-string
//         $uuidString = '6b9b83fb-916b-471d-c37f-1980f0bf78bd';

//         // when creating an uuid instance from it
//         $uuid = call_user_func_array(
//             callback: $this->createInstance()::class . '::fromString',
//             args: [$uuidString],
//         );

//         // then it should match back the uuid-string when turned back to string
//         $this->assertSame(
//             expected: $uuidString,
//             actual: (string) $uuid,
//             message: "Expected to find back uuid-string {$uuidString} when make an Uuid from it, got {$uuid}",
//         );
//     }

//     /**
//      * @test
//      * @testdox Creates versioned Uuid from string
//      * @covers ::fromString
//      * @covers ::getVersion
//      */
//     public function creates_versioned_uuid_from_string(): void
//     {
//         // given an versioned uuid-string
//         $uuidString = '00000000-0000-7000-0000-000000000000';

//         // when making an uuid from it
//         $uuid = $uuid = call_user_func_array(
//             callback: $this->createInstance()::class . '::fromString',
//             args: [$uuidString],
//         );

//         // then version of the instance should be 7
//         $this->assertSame(
//             expected: 7,
//             actual: $uuid->getVersion(),
//             message: "Expected version 7 to be parsed from string, got {$uuid->getVersion()}",
//         );
//     }

//     public function variantProvider(): Generator
//     {
//         yield [
//             ['0', '1', '2', '3', '4', '5', '6', '7'],
//             'Apollo NCS (backward compatibility)'
//         ];
//         yield [
//             ['8', '9', 'a', 'b'],
//             'RFC'
//         ];
//         yield [
//             ['c', 'd'],
//             'Microsoft (backward compatibility)'
//         ];
//         yield [
//             ['e', 'f'],
//             'Reserved (future definition)'
//         ];
//     }

//     /**
//      * @test
//      * @testdox Creates Uuid from string with variant $expectedVariant
//      * @dataProvider variantProvider
//      * @covers ::fromString
//      * @covers ::getVariant
//      */
//     public function creates_variant_uuid_from_string(array $variantDigits, string $expectedVariant): void
//     {
//         foreach ($variantDigits as $variantDigit) {
//             // given an Uuid made from string with variant
//             $uuid = $uuid = call_user_func_array(
//                 callback: $this->createInstance()::class . '::fromString',
//                 args: ["00000000-0000-0000-{$variantDigit}000-000000000000"],
//             );

//             // when checking the variant it's made from
//             $variant = $uuid->getVariant();

//             // then it should match the expectation
//             $this->assertSame(
//                 expected: $expectedVariant,
//                 actual: $variant,
//                 message: "Failed to detect that digit {$variantDigit} belongs to {$expectedVariant}",
//             );
//         }
//     }
// }