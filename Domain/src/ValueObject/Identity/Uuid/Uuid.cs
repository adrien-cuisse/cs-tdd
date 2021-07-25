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

            int variant = 0;
            byte variantBitsBlock = (byte) ((allBytes.GetRange(8, 1)[0] & 0b_1110_0000) >> 5);

            if (Array.Exists(new byte[] { 0b111 }, bits => bits == variantBitsBlock)) {
                variant = FUTURE_VARIANT;
                variantSize = FUTURE_VARIANT_SIZE;
            } else if (Array.Exists(new byte[] { 0b110 }, bits => bits == variantBitsBlock)) {
                variant = MICROSOFT_VARIANT;
                variantSize = FUTURE_VARIANT_SIZE;
            } else if (Array.Exists(new byte[] { 0b101, 0b100 }, bits => bits == variantBitsBlock)) {
                variant = RFC_VARIANT;
                variantSize = RFC_VARIANT_SIZE;
            } else {
                variant = APOLLO_NCS_VARIANT;
                variantSize = APOLLO_NCS_VARIANT_SIZE;
            }

            this.Initialize(
                timestampLow: allBytes.GetRange(0, 4),
                timestampMid: allBytes.GetRange(4, 2),
                version: (allBytes.GetRange(6, 1)[0] & VERSION_BITS_MASK) >> 4,
                timestampHigh: allBytes.GetRange(6, 2),
                variant: variant,
                variantSize: variantSize,
                clockSequenceHigh: allBytes.GetRange(8, 1)[0],
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

            if (this.variantSize == 1) {
                this.clockSequenceHigh = (byte) (clockSequenceHigh & 0b_0111_1111);
            } else if (this.variantSize == 2) {
                this.clockSequenceHigh = (byte) (clockSequenceHigh & 0b_0011_1111);
            } else if (this.variantSize == 3) {
                this.clockSequenceHigh = (byte) (clockSequenceHigh & 0b_0001_1111);
            }
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
            var builder = new System.Text.StringBuilder(36);

            byte versionBits = (byte) (this.version << 4);
            byte timestampHighAndVersion = (byte) (versionBits | this.timestampHigh[0]);

            byte variantBits = (byte) (this.variant << (8 - this.variantSize));
            byte clockSequenceHighAndVariant = (byte) (variantBits | this.clockSequenceHigh);

            var allBytes = new List<List<byte>> {
                this.timestampMid,
                new List<byte> { timestampHighAndVersion, this.timestampHigh[1] },
                new List<byte> { clockSequenceHighAndVariant, this.clockSequenceLow },
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