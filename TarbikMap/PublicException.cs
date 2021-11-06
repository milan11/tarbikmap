namespace TarbikMap
{
    using System;

    public class PublicException : Exception
    {
        public PublicException()
        {
        }

        public PublicException(string message)
            : base(message)
        {
        }

        public PublicException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}