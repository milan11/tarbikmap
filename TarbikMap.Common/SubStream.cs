namespace TarbikMap.Common
{
    using System;
    using System.IO;

    public class SubStream : Stream
    {
        private readonly Stream origStream;
        private readonly long start;
        private readonly long length;

        public SubStream(Stream origStream, long start, long? end)
        {
            if (origStream == null)
            {
                throw new ArgumentNullException(nameof(origStream));
            }

            this.origStream = origStream;
            this.start = start;

            this.length = end.HasValue ? (end.Value - start + 1) : origStream.Length - start;

            this.origStream.Seek(start, SeekOrigin.Begin);
        }

        public override bool CanRead => this.origStream.CanRead;

        public override bool CanSeek => this.origStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => this.length;

        public override long Position
        {
            get
            {
                return this.origStream.Position - this.start;
            }

            set
            {
                if (value <= this.length)
                {
                    this.origStream.Position = this.start + value;
                }
                else
                {
                    throw new IOException("Invalid position");
                }
            }
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = Math.Min(count, (int)(this.length - (this.origStream.Position - this.start)));
            if (count < 0)
            {
                throw new IOException("Invalid count");
            }

            this.origStream.Read(buffer, offset, count);

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                if (offset <= this.length)
                {
                    return this.origStream.Seek(this.start + offset, origin) - this.start;
                }
                else
                {
                    throw new IOException("Invalid offset");
                }
            }

            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
    }
}