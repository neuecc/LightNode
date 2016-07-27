using System;
using System.IO;
using System.Threading.Tasks;

namespace LightNode.Server
{
    // protect stream close

    internal class UnclosableStream : Stream
    {
        readonly Stream baseStream;

        public UnclosableStream(Stream baseStream)
        {
            if (baseStream == null) throw new ArgumentNullException("baseStream");

            this.baseStream = baseStream;
        }

        public Stream BaseStream
        {
            get
            {
                return this.baseStream;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.baseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.baseStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.baseStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this.baseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.baseStream.Position;
            }

            set
            {
                this.baseStream.Position = value;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return this.baseStream.CanTimeout;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this.baseStream.ReadTimeout;
            }

            set
            {
                this.baseStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.baseStream.WriteTimeout;
            }

            set
            {
                this.baseStream.WriteTimeout = value;
            }
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.baseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.baseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.baseStream.Write(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return this.baseStream.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            this.baseStream.WriteByte(value);
        }

        public override void Flush()
        {
            this.baseStream.Flush();
        }

        public override Task FlushAsync(System.Threading.CancellationToken cancellationToken)
        {
            return this.baseStream.FlushAsync(cancellationToken);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, System.Threading.CancellationToken cancellationToken)
        {
            return this.baseStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            return this.baseStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            return this.baseStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        // protect closing

        protected override void Dispose(bool disposing)
        {
        }
    }
}