using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace S3ZipReader
{
	public class CloudStorageResourceRange
	{
		public CloudStorageResourceRange(long offset, long length)
		{
			Offset = offset;
			Length = length;
		}

		public long Offset { get; }
		public long Length { get; }
	}
}
