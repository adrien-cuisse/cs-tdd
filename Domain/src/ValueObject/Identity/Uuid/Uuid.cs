using System;
using System.Collections.Generic;
using CleanArch.Domain.ValueObject.Identity.Uuid;

namespace CleanArch.Domain.ValueObject.Identity.Uuid
{
    /// <summary>
    /// A RFC-compliant Uuid
    /// </summary>
    public abstract partial class Uuid : IUuid
    {
        /// The version of the Uuid
        public int Version => this.version;

        /// The variant of the Uuid
        public string Variant => GetVariant();

        public sealed override string ToString() => this.ToRfcUuidString();

        /// Bit-mask to extract the 4 least significant bits from a byte
        private const int LOWEST_4_BITS_MASK = 0b0000_1111;

        /// Bit-mask to extract the 4 most significant bits from a byte
        private const int HIGHEST_4_BITS_MASK = 0b1111_0000;

        /// Bit-mask to apply on timestamp-high-and-version to extract timestamp-high bits
        private const int TIMESTAMP_HIGH_BITS_MASK = LOWEST_4_BITS_MASK;

        /// Bit-mask to apply on timestamp-high-and-version to extract version bits
        private const int VERSION_BITS_MASK = HIGHEST_4_BITS_MASK;

        /// The variant used by the Apollo Network Computing System
        /// <see>https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants</see>
        private const int APOLLO_NCS_VARIANT = 0b0;

        /// The variant used by default when using constructor, as defined in RFC4122
        /// <see>https://datatracker.ietf.org/doc/html/rfc4122#section-4.1.1</see>
        private const int RFC_VARIANT = 0b10;

        /// How many bits the RFC variant takes
        private const int RFC_VARIANT_SIZE = 2;

        /// The variant used by old Windows platforms
        /// <see>https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants</see>
        private const int MICROSOFT_VARIANT = 0b110;

        /// The variant reserved for future specification
        /// <see>https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants</see>
        private const int FUTURE_VARIANT = 0b111;

        /// List<byte> - bytes 0 to 3
        private List<byte> timestampLow;

        /// List<byte> - bytes 4 to 5
        private List<byte> timestampMid;

        /// int - the version of the Uuid
        private int version;

        /// byte - 4 most significant bits of the byte 6
        private byte versionBits;

        /// List<byte> - 4 least significant bits of byte 6, byte 7
        private List<byte> timestampHigh;

        /// int - the variant used, may differ from default when made from string, 2 most significants bits of byte 8
        /// <see>https://datatracker.ietf.org/doc/html/rfc4122#section-4.1.1</name>
        private int variant;

        private byte variantBits;

        /// byte - byte 8
        private byte clockSequenceHighAndVariant;

        /// byte - byte 9
        private byte clockSequenceLow;

        /// List<byte> - bytes 10 to 15
        private List<byte> node;

        /// <summary>
        ///
        /// </summary>
        /// <param name="version">the version of the Uuid</param>
        /// <param name="timestampLow"></param>
        /// <param name="timestampMid"></param>
        /// <param name="timestampHigh"></param>
        /// <param name="clockSequenceHigh"></param>
        /// <param name="clockSequenceLow"></param>
        /// <param name="node"></param>
        protected Uuid(
            List<byte> timestampLow,
            List<byte> timestampMid,
            int version,
            List<byte> timestampHigh,
            byte clockSequenceHigh,
            byte clockSequenceLow,
            List<byte> node
        ) {
            // if (version > 0b_0000_1111) {
            //     throw new ArgumentException($"Version number must be 15 or lower");
            // }
            // this.version = version;

            // if (timestampLow.Count != 4) {
            //     throw new ArgumentException($"Timestamp-low bytes count must be 4, got {timestampLow.Count}");
            // }
            // this.timestampLow = timestampLow;

            // if (timestampMid.Count != 2) {
            //     throw new ArgumentException($"Timestamp-mid bytes count must be 2, got {timestampMid.Count}");
            // }
            // this.timestampMid = timestampMid;

            // if (timestampHigh.Count != 2) {
            //     throw new ArgumentException($"Timestamp-high bytes count must be 2, got {timestampHigh.Count}");
            // }
            // this.timestampHigh = timestampHigh;
            // /// store version in timestamp-high MSB
            // this.timestampHigh[0] = (byte) ((version << 4) | (this.timestampHigh[0] & 0b_0000_1111));

            // this.variant = RFC_VARIANT;
            // byte variantBits = RFC_VARIANT << (8 - RFC_VARIANT_SIZE);
            // this.clockSequenceHighAndVariant = (byte) (variantBits | (clockSequenceHigh & 0b_0011_1111));
            // this.clockSequenceLow = clockSequenceLow;

            // if (node.Count != 6) {
            //     throw new ArgumentException($"Node bytes count must be 6, got {node.Count}");
            // }
            // this.node = node;

            this.Initialize(
                timestampLow: timestampLow,
                timestampMid: timestampMid,
                version: version,
                timestampHigh: timestampHigh,
                variant: RFC_VARIANT,
                variantBits: RFC_VARIANT << 6,
                clockSequenceHigh: clockSequenceHigh,
                clockSequenceLow: clockSequenceLow,
                node: node
            );
        }

        protected Uuid(string rfcUuidRepresentation)
        {
            var validationRegex = @"^[a-z0-9]{8}-([a-z0-9]{4}-){3}[a-z0-9]{12}$";

            var match = System.Text.RegularExpressions.Regex.Match(rfcUuidRepresentation, validationRegex);
            if (match.Success == false) {
                throw new ArgumentException($"The Uuid-string {rfcUuidRepresentation} is not RFC-compliant");
            }

            var allBytes = new List<byte>(16);

            rfcUuidRepresentation = rfcUuidRepresentation.Replace("-", String.Empty);

            for (int position = 0; position < rfcUuidRepresentation.Length; position += 2) {
                string hexByte = rfcUuidRepresentation.Substring(position, 2);
                allBytes.Add(Convert.ToByte(hexByte, fromBase: 16));
            }

            Console.Write("[");
            allBytes.ForEach(b => {
                Console.Write(String.Format("{0:x2}", b) + ' ');
            });
            Console.WriteLine("]");

            int variant = 0;
            byte variantBits = (byte) ((allBytes.GetRange(9, 1)[0] & 0b_1110_0000) >> 5);
            byte clockSequenceHigh = (byte) (allBytes.GetRange(9, 1)[0] & 0b_1110_0000);
            if (Array.Exists(new byte[] { 0b111 }, bits => bits == variantBits)) {
                variant = 0b111;
                variantBits = 0b111 << 5;
                clockSequenceHigh = (byte) (allBytes.GetRange(8, 1)[0] & 0b_0001_1111);
            } else if (Array.Exists(new byte[] { 0b110 }, bits => bits == variantBits)) {
                variant = 0b110;
                variantBits = 0b110 << 5;
                clockSequenceHigh = (byte) (allBytes.GetRange(8, 1)[0] & 0b_0001_1111);
            } else if (Array.Exists(new byte[] { 0b101, 0b100 }, bits => bits == variantBits)) {
                variant = 0b10;
                variantBits = 0b10 << 6;
                clockSequenceHigh = (byte) (allBytes.GetRange(8, 1)[0] & 0b_0011_1111);
            } else {
                variant = 0b0;
                variantBits = 0b0 << 7;
                clockSequenceHigh = (byte) (allBytes.GetRange(8, 1)[0] & 0b_0111_1111);
            }

            this.Initialize(
                timestampLow: allBytes.GetRange(0, 4),
                timestampMid: allBytes.GetRange(4, 2),
                version: (allBytes.GetRange(6, 1)[0] & 0b_1111_0000) >> 4,
                timestampHigh: allBytes.GetRange(6, 2),
                variant: variant,
                variantBits: variantBits,
                clockSequenceHigh: clockSequenceHigh,
                clockSequenceLow: allBytes.GetRange(9, 1)[0],
                node: allBytes.GetRange(10, 6)
            );
        }

        private void Initialize(
            List<byte> timestampLow,
            List<byte> timestampMid,
            int version,
            List<byte> timestampHigh,
            int variant,
            byte variantBits,
            byte clockSequenceHigh,
            byte clockSequenceLow,
            List<byte> node
        )
        {
            if (version > 0b_0000_1111) {
                throw new ArgumentException($"Version number must be 15 or lower");
            }
            this.version = version;

            if (timestampLow.Count != 4) {
                throw new ArgumentException($"Timestamp-low bytes count must be 4, got {timestampLow.Count}");
            }
            this.timestampLow = timestampLow;

            if (timestampMid.Count != 2) {
                throw new ArgumentException($"Timestamp-mid bytes count must be 2, got {timestampMid.Count}");
            }
            this.timestampMid = timestampMid;

            if (timestampHigh.Count != 2) {
                throw new ArgumentException($"Timestamp-high bytes count must be 2, got {timestampHigh.Count}");
            }
            this.timestampHigh = timestampHigh;
            timestampHigh.ForEach(b => Console.WriteLine((b & 0b1111).ToString()));
            this.timestampHigh[0] &= 0b_0000_1111;
            /// store version in timestamp-high MSB
            // this.timestampHigh[0] = (byte) ((version << 4) | (this.timestampHigh[0] & 0b_0000_1111));
            this.variant = variant;
            this.clockSequenceHighAndVariant = (byte) (variantBits | clockSequenceHigh);
            // Console.WriteLine(this.variant);
            // Console.WriteLine(variantBits);
            // Console.WriteLine(this.clockSequenceHighAndVariant.ToString());
            this.clockSequenceLow = clockSequenceLow;

            if (node.Count != 6) {
                throw new ArgumentException($"Node bytes count must be 6, got {node.Count}");
            }
            this.node = node;
        }

        private string GetVariant()
        {
            Console.Write("<<<");
            Console.Write(this.clockSequenceHighAndVariant.ToString());
            Console.WriteLine(">>>");

            switch (this.variant) {
                case 0b111:
                    return "Reserved (future definition)";
                case 0b110:
                    return "Microsoft (backward compatibility)";
                case 0b10:
                    return "RFC";
                case 0b0:
                    return "Apollo NCS (backward compatibility)";
                default:
                    return "Unknown";
            }
        }

        public string ToRfcUuidString()
        {
            var builder = new System.Text.StringBuilder(36);

            byte timestampHighAndVersion = (byte) ((this.version << 4) | (this.timestampHigh[0]));

            Console.WriteLine(this.timestampHigh[0].ToString());
            var allBytes = new List<List<byte>> {
                this.timestampMid,
                new List<byte> { timestampHighAndVersion, this.timestampHigh[1] },
                new List<byte> { this.clockSequenceHighAndVariant, this.clockSequenceLow },
                this.node
            };

            this.timestampLow.ForEach(b => builder.AppendFormat("{0:x2}", b));
            foreach (var byteList in allBytes) {
                builder.Append('-');
                byteList.ForEach(b => builder.AppendFormat("{0:x2}", b));
            }

            return builder.ToString();
        }
    }

}


// /**
//  * @see UuidInterface
//  */
// abstract class Uuid implements UuidInterface
// {
//     /**
//      * Bit-mask to clamp an integer to byte-range
//      */
//     private const BYTE_MASK = 0b1111_1111;

//     /**
//      * Bit-mask to extract the 4 least significant bits from a byte
//      */
//     private const LOWEST_4_BITS_MASK = 0b0000_1111;

//     /**
//      * Bit-mask to extract the 4 most significant bits from a byte
//      */
//     private const HIGHEST_4_BITS_MASK = 0b1111_0000;

//     /**
//      * Bit-mask to apply on timestamp-high-and-version to extract timestamp-high bits
//      */
//     private const TIMESTAMP_HIGH_BITS_MASK = self::LOWEST_4_BITS_MASK;

//     /**
//      * Bit-mask to apply on timestamp-high-and-version to extract version bits
//      */
//     private const VERSION_BITS_MASK = self::HIGHEST_4_BITS_MASK;

//     /**
//      * The variant used by the Apollo Network Computing System
//      *
//      * @link https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants
//      */
//     private const APOLLO_NCS_VARIANT = 0b0;

//     /**
//      * The variant used by default when using constructor, as defined in RFC4122
//      *
//      * @link https://datatracker.ietf.org/doc/html/rfc4122#section-4.1.1
//      */
//     private const RFC_VARIANT = 0b10;

//     /**
//      * How many bits the RFC variant takes
//      */
//     private const RFC_VARIANT_SIZE = 2;

//     /**
//      * The variant used by old Windows platforms
//      *
//      * @link https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants
//      */
//     private const MICROSOFT_VARIANT = 0b110;

//     /**
//      * The variant reserved for future specification
//      *
//      * @link https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants
//      */
//     private const FUTURE_VARIANT = 0b111;

//     /**
//      * @var int[] - bytes 0 to 3
//      */
//     private array $timestampLowBytes;

//     /**
//      * @var int[] - bytes 4 to 5
//      */
//     private array $timestampMidBytes;

//     /**
//      * @var int - the version of the Uuid
//      */
//     private int $version;

//     /**
//      * @var int - 4 most significant bits of the byte 6
//      */
//     private int $versionBits;

//     /**
//      * @var int[] - 4 least significant bits of byte 6, byte 7
//      */
//     private array $timestampHighBytes;

//     /**
//      * @var int - the variant used, may differ from default when made from string, 2 most significants bits of byte 8
//      *
//      * @link https://datatracker.ietf.org/doc/html/rfc4122#section-4.1.1
//      */
//     private int $variant = self::RFC_VARIANT;

//     /**
//      * @var int - byte 8
//      */
//     private int $clockSequenceHighAndVariantByte;

//     /**
//      * @var int - byte 9
//      */
//     private int $clockSequenceLowByte;

//     /**
//      * @var int[] - bytes 10 to 15
//      */
//     private array $nodeBytes;

//     /**
//      * @throws InvalidUuidTimestampLowBytesCountException - if the number of timestamp-low bytes is invalid
//      * @throws InvalidUuidTimestampMidBytesCountException - if the number of timestamp-mid bytes is invalid
//      * @throws InvalidUuidTimestampHighBytesCountException - if the number of timestamp-high bytes is invalid
//      * @throws InvalidUuidVersionException - if the version exceeds 15
//      * @throws InvalidUuidNodeBytesCountException - if the number of node bytes is invalid
//      */
//     protected function __construct(
//         int $version,
//         array $timestampLowBytes,
//         array $timestampMidBytes,
//         array $timestampHighBytes,
//         int $clockSequenceHighByte,
//         int $clockSequenceLowByte,
//         array $nodeBytes,
//     ) {
//         if (count(value: $timestampLowBytes) !== 4) {
//             throw new InvalidUuidTimestampLowBytesCountException(bytes: $timestampLowBytes);
//         }
//         $this->timestampLowBytes = $this->clampToBytes(integers: $timestampLowBytes);

//         if (count(value: $timestampMidBytes) !== 2) {
//             throw new InvalidUuidTimestampMidBytesCountException(bytes: $timestampMidBytes);
//         }
//         $this->timestampMidBytes = $this->clampToBytes(integers: $timestampMidBytes);

//         if (count(value: $timestampHighBytes) !== 2) {
//             throw new InvalidUuidTimestampHighBytesCountException(bytes: $timestampHighBytes);
//         }
//         $this->timestampHighBytes = $this->clampToBytes(integers: $timestampHighBytes);
//         $this->timestampHighBytes[0] &= self::TIMESTAMP_HIGH_BITS_MASK;

//         if ($version > 0b0000_1111) {
//             throw new InvalidUuidVersionException(version: $version);
//         }
//         $this->version = $version;
//         $this->versionBits = $this->version << 4;

//         $variantBits = ($this->variant << (8 - self::RFC_VARIANT_SIZE));
//         $this->clockSequenceHighAndVariantByte = $variantBits | $this->clampToByte(value: $clockSequenceHighByte);
//         $this->clockSequenceLowByte = $this->clampToByte(value: $clockSequenceLowByte);

//         if (count(value: $nodeBytes) !== 6) {
//             throw new InvalidUuidNodeBytesCountException(bytes: $nodeBytes);
//         }
//         $this->nodeBytes = $this->clampToBytes(integers: $nodeBytes);
//     }

//     /**
//      * @see UuidInterface
//      */
//     final public function getVersion(): int
//     {
//         return $this->version;
//     }

//     /**
//      * @see UuidInterface
//      */
//     final public function getVariant(): string
//     {
//         return match ($this->variant) {
//             self::APOLLO_NCS_VARIANT => 'Apollo NCS (backward compatibility)',
//             self::RFC_VARIANT => 'RFC',
//             self::MICROSOFT_VARIANT => 'Microsoft (backward compatibility)',
//             self::FUTURE_VARIANT => 'Reserved (future definition)',
//         };
//     }

//     /**
//      * Puts all bits in the right place and generates the string representation
//      *
//      * @see UuidInterface
//      */
//     final public function toRfcUuidString(): string
//     {
//         $versionAndTimeHighBytes = [
//             $this->versionBits | $this->timestampHighBytes[0],
//             $this->timestampHighBytes[1]
//         ];

//         $clockSequenceAndVariantBytes = [
//             $this->clockSequenceHighAndVariantByte,
//             $this->clockSequenceLowByte
//         ];

//         return sprintf(
//             "%s-%s-%s-%s-%s",
//             $this->hexaStringFrom(bytes: $this->timestampLowBytes),
//             $this->hexaStringFrom(bytes: $this->timestampMidBytes),
//             $this->hexaStringFrom(bytes: $versionAndTimeHighBytes),
//             $this->hexaStringFrom(bytes: $clockSequenceAndVariantBytes),
//             $this->hexaStringFrom(bytes: $this->nodeBytes)
//         );
//     }

//     /**
//      * @see Stringable
//      */
//     final public function __toString(): string
//     {
//         return $this->toRfcUuidString();
//     }

//     /**
//      * Creates an UUID from a RFC representation string
//      *
//      * @param string $rfcUuidString - the RFC representation to build the Uuid from
//      *
//      * @return static
//      */
//     protected static function fromString(string $rfcUuidString): static
//     {
//         $rfcValidationRegex = '/^[[:xdigit:]]{8}-([[:xdigit:]]{4}-){3}[[:xdigit:]]{12}$/';
//         if (preg_match(pattern: $rfcValidationRegex, subject: $rfcUuidString) !== 1) {
//             throw new InvalidUuidStringException(uuidString: $rfcUuidString);
//         }

//         $instance = (new ReflectionClass(static::class))->newInstanceWithoutConstructor();

//         [$timestampLow, $timestampMid, $timestampHighAndVersion, $clockSequenceAndVariant, $node] = explode(separator: '-', string: $rfcUuidString);

//         $instance->timestampLowBytes = sscanf(string: $timestampLow, format: '%2x%2x%2x%2x');
//         $instance->timestampMidBytes = sscanf(string: $timestampMid, format: '%2x%2x');
//         $instance->timestampHighBytes = sscanf(string: $timestampHighAndVersion, format: '%2x%2x');
//         $instance->timestampHighBytes[0] &= self::TIMESTAMP_HIGH_BITS_MASK;

//         $instance->versionBits = sscanf(string: $timestampHighAndVersion, format: '%2x')[0] & self::VERSION_BITS_MASK;
//         $instance->version = $instance->versionBits >> 4;

//         $variantDigit = sscanf(string: $clockSequenceAndVariant, format: '%1c')[0];
//         $instance->variant = match ($variantDigit) {
//             '0', '1', '2', '3', '4', '5', '6', '7' => self::APOLLO_NCS_VARIANT,
//             '8', '9', 'a', 'b' => self::RFC_VARIANT,
//             'c', 'd' => self::MICROSOFT_VARIANT,
//             'e', 'f' => self::FUTURE_VARIANT,
//         };

//         $instance->clockSequenceHighAndVariantByte = sscanf(string: $clockSequenceAndVariant, format: '%2x')[0];
//         $instance->clockSequenceLowByte = sscanf(string: $clockSequenceAndVariant, format: '%2x%2x')[1];

//         $instance->nodeBytes = sscanf(string: $node, format: '%2x%2x%2x%2x%2x%2x');

//         return $instance;
//     }

//     /**
//      * Generates a random byte, an unsigned integers between 0 and 255
//      *
//      * @return int - an integers in between 0 and 255
//      */
//     final protected function randomByte(): int
//     {
//         $ascii = openssl_random_pseudo_bytes(length: 1);

//         return ord(character: $ascii);
//     }

//     /**
//      * Generates random bytes, with bytes being unsigned integers between 0 and 255
//      *
//      * @param int $numberOfBytes - how many bytes to generate
//      *
//      * @return int[] - array of integers between 0 and 255 of specified size
//      */
//     final protected function randomBytes(int $numberOfBytes): array
//     {
//         $binaryString = openssl_random_pseudo_bytes(length: $numberOfBytes);

//         return array_map(
//             callback: fn (string $ascii) => ord(character: $ascii),
//             array: str_split($binaryString),
//         );
//     }

//     /**
//      * Removes overflowing bits from the given value
//      *
//      * @param int $value - the integer to clamp
//      *
//      * @return int - 8 lowest bits of the given value
//      */
//     private function clampToByte(int $value): int
//     {
//         return $value & self::BYTE_MASK;
//     }

//     /**
//      * Clamps the given integers to bytes, ensuring no invalid value is send from extending classes
//      *
//      * @param int[] $integers - the integers to turn to bytes
//      *
//      * @return array - 8 lowest bits of each given integer
//      */
//     private function clampToBytes(array $integers): array
//     {
//         return array_map(
//             callback: fn (int $value) => $this->clampToByte($value),
//             array: $integers
//         );
//     }

//     /**
//      * Writes the given bytes in an hexadecimal string
//      *
//      * @param int[] $bytes - array of unsigned byte to write
//      *
//      * @return string - the bytes as an hexadecimal string
//      */
//     private function hexaStringFrom(array $bytes): string
//     {
//         $bytesCount = count(value: $bytes);

//         $binaryString = pack("C{$bytesCount}", ...$bytes);

//         return bin2hex(string: $binaryString);
//     }
// }