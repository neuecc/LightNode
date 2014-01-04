using System;
using System.IO;
using System.Threading.Tasks;

namespace LightNode.Server
{
    // protect stream flush using Dispose

    internal class UnflushableStream : Stream
    {
        readonly Stream baseStream;

        public UnflushableStream(Stream baseStream)
        {
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

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.baseStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.baseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.baseStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.baseStream.EndWrite(asyncResult);
        }

        public override int ReadByte()
        {
            return this.baseStream.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            this.baseStream.WriteByte(value);
        }

        public override void Close()
        {
        }

        public new void Dispose()
        {
        }

        public override void Flush()
        {
        }

        static Task NullTask = Task.FromResult<object>(null);

        public override Task FlushAsync(System.Threading.CancellationToken cancellationToken)
        {
            return NullTask;
        }
        protected override void Dispose(bool disposing)
        {
        }
    }
}