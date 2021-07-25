namespace CleanArch.Domain.ValueObject.Identity.Uuid
{
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