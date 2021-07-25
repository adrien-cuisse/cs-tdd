using System.Collections.Generic;

namespace CleanArch.Domain.ValueObject.Identity.Uuid
{
    public sealed partial class UuidV4 : Uuid
    {
        private static System.Random generator = new System.Random();

        internal UuidV4() : base(
            UuidV4.GenerateRandomBytes(4),
            UuidV4.GenerateRandomBytes(2),
            4,
            UuidV4.GenerateRandomBytes(2),
            UuidV4.GenerateRandomByte(),
            UuidV4.GenerateRandomByte(),
            UuidV4.GenerateRandomBytes(6)
        ) { }

        private static byte GenerateRandomByte() => (byte) UuidV4.generator.Next();

        private static List<byte> GenerateRandomBytes(int count)
        {
            byte[] bytes = new byte[count];

            UuidV4.generator.NextBytes(bytes);

            return new List<byte>(bytes);
        }
    }
}