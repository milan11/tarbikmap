namespace TarbikMap.Common
{
    using System;
    using System.IO;
    using System.Text;

    public class BinaryFormatReader
    {
        private BinaryReader? br;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1508", Justification = "Conflicting with CA1062")]
        public void Read(Stream stream, Action lineAction)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (lineAction == null)
            {
                throw new ArgumentNullException(nameof(lineAction));
            }

            using (stream)
            using (var br = new BinaryReader(stream))
            {
                this.br = br;

                while (true)
                {
                    ushort itemLength = 0;

                    try
                    {
                        itemLength = br.ReadUInt16();
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }

                    long nextItemPos = stream.Position + itemLength;

                    lineAction();

                    if (stream.Position != nextItemPos)
                    {
                        stream.Seek(nextItemPos - stream.Position, SeekOrigin.Current);
                    }
                }
            }
        }

        public uint ReadId()
        {
            return this.br!.ReadUInt32();
        }

        public double ReadCoordinate()
        {
            return this.br!.ReadDouble();
        }

        public ushort ReadLength()
        {
            return this.br!.ReadUInt16();
        }

        public string ReadString()
        {
            ushort length = this.ReadLength();
            byte[] stringBytes = this.br!.ReadBytes(length);
            return Encoding.UTF8.GetString(stringBytes);
        }
    }
}