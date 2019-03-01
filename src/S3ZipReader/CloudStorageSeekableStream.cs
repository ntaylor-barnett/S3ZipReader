using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace S3ZipReader
{
    public class CloudStorageSeekableStream : Stream
    {
        public CloudStorageSeekableStream(ICloudStorage storage, ICloudStorageResourceName resourceName)
        {
	        _CloudStorage = storage;
	        _ResourceName = resourceName;

	        Length = Task.Run(() => { return _CloudStorage.GetObjectLengthAsync(_ResourceName); }).Result;
		}

		private ICloudStorage _CloudStorage;
        private ICloudStorageResourceName _ResourceName;

		private const int bufferSize = 32 * 1024;
        private const int bufferCount = 32;
        private List<Buffer> _Buffers = new List<Buffer>();

        private class Buffer
        {
            public long Position { get; set; }
            public MemoryStream Data { get; set; }
            public long BytesAvail { get; set; }
            public bool IsLast { get; set; }
        }

        private Buffer LoadBuffer(long position)
        {
            bool isLast = false;
            if (position + bufferSize >= Length)
            {
                position = Math.Max(0, Length - bufferSize);
                isLast = true;
            }
			var range = new CloudStorageResourceRange(position, Math.Min(bufferSize, Length - position));
            
            Console.WriteLine($"Seekable stream requesting byte range at {range.Offset} length {range.Length}");

            var objReq = Task.Run(() => { return _CloudStorage.GetObjectStreamAsync(_ResourceName, range); }).Result;

            var ms = new MemoryStream((int)range.Length);

            objReq.CopyTo(ms);
            
            return new Buffer()
            {
                Position = position,
                Data = ms,
                BytesAvail = ms.Length,
                IsLast = isLast
            };
        }

        private void AddBuffer(Buffer buff)
        {
            _Buffers.Add(buff);
            if (_Buffers.Count > bufferCount)
            {
                _Buffers.RemoveAt(0);
            }
        }

        private Buffer GetBuffer(long position)
        {
            if (position >= Length)
            {
                return null;
            }
            var foundBuff = _Buffers.Find(b =>
                b.Position == position || (position > b.Position && position < (b.Position + b.BytesAvail)));
            if (foundBuff != null)
            {
                return foundBuff;
            }
            else
            {
                foundBuff = LoadBuffer(position);
                AddBuffer(foundBuff);
                return foundBuff;
            }
        }



        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                Buffer buffToUse;

                //long posFix = this.Position;
                sb.Append($"Position: {Position}, ");
                buffToUse = GetBuffer(Position);
                if (buffToUse == null)
                {
                    return 0;
                }
                //this.Position = posFix;
                //sb.AppendLine($"BufferPos: {buffToUse.Position}");
                long buffOffset = (Position - buffToUse.Position);
                if (buffOffset < 0)
                {
                    throw new Exception(
                        $"This doesn't make sense. {Position} - {buffToUse.Position} = {Position - buffToUse.Position}");
                }
                //sb.AppendLine($"Offset: {buffOffset}");
                buffToUse.Data.Seek(buffOffset, SeekOrigin.Begin);
                //buffToUse.Data.Position = buffOffset;
                int bytesRead = buffToUse.Data.Read(buffer, offset, count);
                sb.Append($"bytesRead: {bytesRead}");
                this.Position += bytesRead;

				//Console.WriteLine(sb);
                return bytesRead;
            }
            catch (Exception ex)
            {
                throw new Exception(sb.ToString(), ex);

            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPos;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPos = offset;
                    break;
                case SeekOrigin.Current:
                    newPos = Position + offset;
                    break;
                case SeekOrigin.End:
                    newPos = Length + offset;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Position = newPos;
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get => true;
        }

        public override bool CanSeek
        {
            get => true;
        }

        public override bool CanWrite
        {
            get => false;
        }

        public override long Length { get; }
        public override long Position { get; set; }
    }

}
