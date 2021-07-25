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
        /// <value>The version of the Uuid</value>
        public int Version => this.version;

        /// <value>The variant of the Uuid</value>
        public string Variant => GetVariantDescription();

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

        /// How many bits the Apollo NCS variant takes
        private const int APOLLO_NCS_VARIANT_SIZE = 1;

        /// The variant used by default when using constructor, as defined in RFC4122
        /// <see>https://datatracker.ietf.org/doc/html/rfc4122#section-4.1.1</see>
        private const int RFC_VARIANT = 0b10;

        /// How many bits the RFC variant takes
        private const int RFC_VARIANT_SIZE = 2;

        /// The variant used by old Windows platforms
        /// <see>https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants</see>
        private const int MICROSOFT_VARIANT = 0b110;

        /// How many bits the Microsoft variant takes
        private const int MICROSOFT_VARIANT_SIZE = 3;

        /// The variant reserved for future specification
        /// <see>https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants</see>
        private const int FUTURE_VARIANT = 0b111;

        /// How many bits the Future variant takes
        private const int FUTURE_VARIANT_SIZE = 3;

        /// Bytes 0 to 3
        private List<byte> timestampLow;

        /// Bytes 4 to 5
        private List<byte> timestampMid;

        /// The version of the Uuid
        private int version;

        /// The 4 least significant bits of byte 6, byte 7
        private List<byte> timestampHigh;

        /// The variant used, may differ from default when made from string, 2 most significants bits of byte 8
        /// <see>https://datatracker.ietf.org/doc/html/rfc4122#section-4.1.1</name>
        private int variant = RFC_VARIANT;

        /// Default variant size
        private int variantSize = RFC_VARIANT_SIZE;

        /// Byte 8
        private byte clockSequenceHigh;

        /// Byte 9
        private byte clockSequenceLow;

        /// Bytes 10 to 15
        private List<byte> node;

        protected Uuid(
            List<byte> timestampLow,
            List<byte> timestampMid,
            int version,
            List<byte> timestampHigh,
            byte clockSequenceHigh,
            byte clockSequenceLow,
            List<byte> node
        ) {
            this.Initialize(
                timestampLow: timestampLow,
                timestampMid: timestampMid,
                version: version,
                timestampHigh: timestampHigh,
                variant: RFC_VARIANT,
                variantSize: RFC_VARIANT_SIZE,
                clockSequenceHigh: clockSequenceHigh,
                clockSequenceLow: clockSequenceLow,
                node: node
            );
        }

        protected Uuid(string uuidString)
        {
            if (IsNotRfcCompliant(uuidString))
            {
                throw new ArgumentException($"The Uuid-string {uuidString} is not RFC-compliant");
            }
            uuidString = uuidString.Replace("-", String.Empty);

            List<byte> allBytes = ParseHexadecimalString(uuidString);

            List<byte> timestampHighAndVersion = allBytes.GetRange(6, 2);
            int version = timestampHighAndVersion[0] >> 4;

            byte clockSequenceHighAndVariant = allBytes.GetRange(8, 1)[0];
            var (variant, variantSize) = GetVariantWithSize(clockSequenceHighAndVariant);

            this.Initialize(
                timestampLow: allBytes.GetRange(0, 4),
                timestampMid: allBytes.GetRange(4, 2),
                version: version,
                timestampHigh: timestampHighAndVersion,
                variant: variant,
                variantSize: variantSize,
                clockSequenceHigh: clockSequenceHighAndVariant,
                clockSequenceLow: allBytes.GetRange(9, 1)[0],
                node: allBytes.GetRange(10, 6)
            );
        }

        /// <summary>
        /// Initializes the internal state.
        /// </summary>
        /// <param name="timestampLow">Bytes 0 to 3</param>
        /// <param name="timestampMid">Bytes 4 to 5</param>
        /// <param name="version">The version used, must fit in 4 bits (ie., being lesser than 16)</param>
        /// <param name="timestampHigh">Bytes 6 to 7, 4 most significant bits of byte 6 will be removed</param>
        /// <param name="variant">The variant used</param>
        /// <param name="variantSize">The size of the variant used, in number of bits</param>
        /// <param name="clockSequenceHigh">Byte 8, 2 most significant bits will be removed</param>
        /// <param name="clockSequenceLow">Byte 9</param>
        /// <param name="node">Bytes 10 to 15</param>
        /// <exception cref="System.ArgumentException">
        /// If any of these situations occur:
        /// <list>
        /// <item>If <see cref="version"> is greater than 15</item>
        /// <item>If <see cref="timestampLow"> doesn't contain 4 bytes</item>
        /// <item>If <see cref="timestampMid"> doesn't contain 2 bytes</item>
        /// <item>If <see cref="timestampHigh"> doesn't contain 2 bytes</item>
        /// <item>If <see cref="node"> doesn't contain 6 bytes</item>
        /// </list>
        /// </exception>
        private void Initialize(
            List<byte> timestampLow,
            List<byte> timestampMid,
            int version,
            List<byte> timestampHigh,
            int variant,
            int variantSize,
            byte clockSequenceHigh,
            byte clockSequenceLow,
            List<byte> node
        )
        {
            if (version > LOWEST_4_BITS_MASK)
            {
                throw new ArgumentException($"Version number must be 15 or lower");
            }
            this.version = version;

            if (timestampLow.Count != 4)
            {
                throw new ArgumentException($"Timestamp-low bytes count must be 4, got {timestampLow.Count}");
            }
            this.timestampLow = timestampLow;

            if (timestampMid.Count != 2)
            {
                throw new ArgumentException($"Timestamp-mid bytes count must be 2, got {timestampMid.Count}");
            }
            this.timestampMid = timestampMid;

            if (timestampHigh.Count != 2)
            {
                throw new ArgumentException($"Timestamp-high bytes count must be 2, got {timestampHigh.Count}");
            }
            this.timestampHigh = timestampHigh;
            this.timestampHigh[0] &= LOWEST_4_BITS_MASK;

            this.variant = variant;
            this.variantSize = variantSize;

            byte clockSequenceHighBitMask = Convert.ToByte(0b_1111_1111 >> this.variantSize);
            this.clockSequenceHigh = Convert.ToByte(clockSequenceHigh & clockSequenceHighBitMask);

            this.clockSequenceLow = clockSequenceLow;

            if (node.Count != 6)
            {
                throw new ArgumentException($"Node bytes count must be 6, got {node.Count}");
            }
            this.node = node;
        }

        /// <summary>
        /// Describes the variant.
        /// </summary>
        /// <returns>Description of the variant</returns>
        private string GetVariantDescription()
        {
            switch (this.variant)
            {
                case FUTURE_VARIANT:
                    return "Reserved (future definition)";
                case MICROSOFT_VARIANT:
                    return "Microsoft (backward compatibility)";
                case RFC_VARIANT:
                    return "RFC";
                default:
                    return "Apollo NCS (backward compatibility)";
            }
        }

        /// <summary>
        /// Puts all the bytes at the appropriate position in a string.
        /// </summary>
        /// <returns>
        /// RFC-compliant string representation (eg., 01234567-89ab-cdef-0123456789ab)
        /// </returns>
        public string ToRfcUuidString()
        {
            byte versionBits = Convert.ToByte(this.version << 4);
            byte timestampHighAndVersion = Convert.ToByte(versionBits | this.timestampHigh[0]);

            byte variantBits = Convert.ToByte(this.variant << (8 - this.variantSize));
            byte clockSequenceHighAndVariant = Convert.ToByte(variantBits | this.clockSequenceHigh);

            var timestamp = new List<byte>
            {
                timestampHighAndVersion,
                this.timestampHigh[1],
            };

            var clockSequence = new List<byte>
            {
                clockSequenceHighAndVariant,
                this.clockSequenceLow,
            };

            var allBytes = new List<List<byte>>
            {
                this.timestampLow,
                this.timestampMid,
                timestamp,
                clockSequence,
                this.node,
            };

            return BytesToHexaString(allBytes, "-");
        }

        /// <summary>
        /// Checks if the bits match the variant.
        /// </summary>
        /// <param name="variant">The bit-pattern of the variant to test</param>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>True if the byte matches the variant</returns>
        private static bool IsVariant(byte variantPattern, byte clockSequenceHigh)
        {
            bool patternMatches = (variantPattern == (variantPattern & clockSequenceHigh));
            return patternMatches;
        }

        /// <summary>
        /// Checks if the variant bits match the RFC variant.
        /// </summary>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>True if the byte matches 0b_10xx_xxxx</returns>
        private static bool IsRfcVariant(byte clockSequenceHigh)
        {
            byte rfcPattern = 0b_1000_0000;
            return IsVariant(rfcPattern, clockSequenceHigh);
        }

        /// <summary>
        /// Checks if the variant bits match the Microsoft variant.
        /// </summary>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>True if the byte matches 0b_110x_xxxx</returns>
        private static bool IsMicrosoftVariant(byte clockSequenceHigh)
        {
            byte microsoftPattern = 0b_1100_0000;
            return IsVariant(microsoftPattern, clockSequenceHigh);
        }

        /// <summary>
        /// Checks if the variant bits match the Future variant.
        /// </summary>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>True if the byte matches 0b_111x_xxxx</returns>
        private static bool IsFutureVariant(byte clockSequenceHigh)
        {
            byte futurePattern = 0b_1110_0000;
            return IsVariant(futurePattern, clockSequenceHigh);
        }

        /// <summary>
        /// Extracts the variant and its size from the clock-sequence-high byte.
        /// </summary>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>Tuple with variant pattern, and its size in bits</returns>
        private static (int, int) GetVariantWithSize(byte clockSequenceHigh)
        {
            if (IsFutureVariant(clockSequenceHigh))
            {
                return (FUTURE_VARIANT, FUTURE_VARIANT_SIZE);
            }
            else if (IsMicrosoftVariant(clockSequenceHigh))
            {
                return (MICROSOFT_VARIANT, MICROSOFT_VARIANT_SIZE);
            }
            else if (IsRfcVariant(clockSequenceHigh))
            {
                return (RFC_VARIANT, RFC_VARIANT_SIZE);
            }

            return (APOLLO_NCS_VARIANT, APOLLO_NCS_VARIANT_SIZE);
        }

        /// <summary>
        /// Checks if the string matches the format.
        /// </summary>
        /// <param name="uuidString">The uuid-string to check</param>
        /// <returns>True if the string does NOT match RFC format</returns>
        private static bool IsNotRfcCompliant(string uuidString)
        {
            var validationRegex = "^[a-z0-9]{8}-([a-z0-9]{4}-){3}[a-z0-9]{12}$";

            var match = System.Text.RegularExpressions.Regex.Match(uuidString, validationRegex);

            return match.Success == false;
        }

        /// <summary>
        /// Formats the byte to hexadecimal.
        /// </summary>
        /// <param name="octet">The byte to format</param>
        /// <returns>The byte formated in hexadecimal</returns>
        private static string ByteToHexaString(byte octet)
        {
            return new System.Text.StringBuilder(2)
                .AppendFormat("{0:x2}", octet)
                .ToString();
        }

        /// <summary>
        /// Formats the bytes to hexadecimal.
        /// </summary>
        /// <param name="bytes">The bytes to format</param>
        /// <returns>The bytes formated in hexadecimal</returns>
        private static string BytesToHexaString(List<byte> bytes)
        {
            string hexaString = "";

            foreach (var octet in bytes)
            {
                hexaString += ByteToHexaString(octet);
            }

            return hexaString;
        }

        /// <summary>
        /// Formats the group of bytes to hexadecimal.
        /// </summary>
        /// <param name="octet">The bytes to format</param>
        /// <param name="delimiter">Separator to insert between groups of bytes</param>
        /// <returns>The bytes formated in hexadecimal, with separator between group of bytes</returns>
        private static string BytesToHexaString(List<List<byte>> bytesLists, string delimiter = "")
        {
            string hexaString = "";

            foreach (var bytesList in bytesLists)
            {
                if (hexaString.Length > 0)
                {
                    hexaString += delimiter;
                }

                hexaString += BytesToHexaString(bytesList);
            }

            return hexaString;
        }

        /// <summary>
        /// Parses the hexadecimal string and retrieves the bytes it's made of.
        /// </summary>
        /// <param name="hexadecimal">The hexadecimal string to parse</param>
        /// <returns>List of bytes parsed from the hexadecimal string</returns>
        /// <remarks>String length must be even</remarks>
        private static List<byte> ParseHexadecimalString(string hexadecimal)
        {
            var bytes = new List<byte>();

            for (var position = 0; position < hexadecimal.Length; position += 2)
            {
                string hexByte = hexadecimal.Substring(position, 2);
                bytes.Add(Convert.ToByte(hexByte, fromBase: 16));
            }

            return bytes;
        }
    }
}