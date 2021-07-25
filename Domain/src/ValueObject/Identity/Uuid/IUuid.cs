namespace CleanArch.Domain.ValueObject.Identity.Uuid
{
    /// <summary>
    /// Universally Unique Identifier, an unique identity stored in 128 bits
    /// <see>https://datatracker.ietf.org/doc/html/rfc4122</see>
    /// </summary>
    public interface IUuid : IIdentity
    {
        public int Version
        {
            get;
        }

        public string Variant
        {
            get;
        }

        public string ToRfcUuidString();
    }
}