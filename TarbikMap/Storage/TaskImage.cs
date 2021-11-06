namespace TarbikMap.Storage
{
    using System;

    internal class TaskImage
    {
        public TaskImage(AccessType access, Uri url)
        {
            this.Access = access;
            this.Url = url;
        }

        public enum AccessType
        {
            HTTP,
        }

        public AccessType Access { get; private set; }

        public Uri Url { get; private set; }

        public byte[]? Cached { get; set; }
    }
}
