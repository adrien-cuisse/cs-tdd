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

            List<byte> timestampHighAndVersion = allBytes.GetRange(6, 2);
            int versionBits = timestampHighAndVersion[0] & VERSION_BITS_MASK;
            int version = versionBits >> 4;

            byte clockSequenceHighAndVariant = allBytes.GetRange(8, 1)[0];
            // byte variantBits = Convert.ToByte(clockSequenceHighAndVariant >> 5);
            var (variant, variantSize) = this.GetVariantWithSize(clockSequenceHighAndVariant);

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
            if (version > LOWEST_4_BITS_MASK) {
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
            this.timestampHigh[0] &= LOWEST_4_BITS_MASK;

            this.variant = variant;
            this.variantSize = variantSize;

            byte clockSequenceHighBitMask = Convert.ToByte(0b_1111_1111 >> this.variantSize);
            this.clockSequenceHigh = Convert.ToByte(clockSequenceHigh & clockSequenceHighBitMask);

            this.clockSequenceLow = clockSequenceLow;

            if (node.Count != 6) {
                throw new ArgumentException($"Node bytes count must be 6, got {node.Count}");
            }
            this.node = node;
        }

        private string GetVariant()
        {
            switch (this.variant) {
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

        public string ToRfcUuidString()
        {
            byte versionBits = Convert.ToByte(this.version << 4);
            byte timestampHighAndVersion = Convert.ToByte(versionBits | this.timestampHigh[0]);

            byte variantBits = Convert.ToByte(this.variant << (8 - this.variantSize));
            byte clockSequenceHighAndVariant = Convert.ToByte(variantBits | this.clockSequenceHigh);

            var allBytes = new List<List<byte>> {
                this.timestampLow,
                this.timestampMid,
                new List<byte> { timestampHighAndVersion, this.timestampHigh[1] },
                new List<byte> { clockSequenceHighAndVariant, this.clockSequenceLow },
                this.node
            };

            var builder = new System.Text.StringBuilder(36);

            foreach (var byteList in allBytes) {
                if (builder.Length > 0) {
                    builder.Append('-');
                }
                byteList.ForEach(octet => builder.AppendFormat("{0:x2}", octet));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Checks if the bits match the variant
        /// </summary>
        /// <param name="variant">List of bit-pattern the variant can have</param>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns></returns>
        private bool IsVariant(byte[] variant, byte clockSequenceHigh)
        {
            return Array.Exists(variant, bits => (clockSequenceHigh & bits) == bits);
        }

        /// <summary>
        /// Checks if the variant bits match the RFC variant
        /// </summary>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>True if the byte matches 0b_10xx_xxxx</returns>
        private bool IsRfcVariant(byte clockSequenceHigh)
        {
            var rfc = new byte[] { 0b_1000_0000, 0b_1010_0000 };
            return IsVariant(rfc, clockSequenceHigh);
        }

        /// <summary>
        /// Checks if the variant bits match the Microsoft variant
        /// </summary>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>True if the byte matches 0b_110x_xxxx</returns>
        private bool IsMicrosoftVariant(byte clockSequenceHigh)
        {
            var microsoft = new byte[] { 0b_1100_0000 };
            return IsVariant(microsoft, clockSequenceHigh);
        }

        /// <summary>
        /// Checks if the variant bits match the Future variant
        /// </summary>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>True if the byte matches 0b_111x_xxxx</returns>
        private bool IsFutureVariant(byte clockSequenceHigh)
        {
            var future = new byte[] { 0b_1110_0000 };
            return IsVariant(future, clockSequenceHigh);
        }

        /// <summary>
        /// Extracts the variant and its size from the clock-sequence-high byte
        /// </summary>
        /// <param name="clockSequenceHigh">Clock sequence high (byte 8)</param>
        /// <returns>Tuple with variant pattern, and its size in bits</returns>
        private (int, int) GetVariantWithSize(byte clockSequenceHigh)
        {
            if (this.IsFutureVariant(clockSequenceHigh))
            {
                return (FUTURE_VARIANT, FUTURE_VARIANT_SIZE);
            }
            else if (this.IsMicrosoftVariant(clockSequenceHigh))
            {
                return (MICROSOFT_VARIANT, MICROSOFT_VARIANT_SIZE);
            }
            else if (this.IsRfcVariant(clockSequenceHigh))
            {
                return (RFC_VARIANT, RFC_VARIANT_SIZE);
            }

            return (APOLLO_NCS_VARIANT, APOLLO_NCS_VARIANT_SIZE);
        }
    }
}