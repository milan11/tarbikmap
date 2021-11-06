namespace TarbikMap
{
    using System;

    public class ExternalDataException : Exception
    {
        public ExternalDataException()
        {
        }

        public ExternalDataException(string message)
            : base(message)
        {
        }

        public ExternalDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}