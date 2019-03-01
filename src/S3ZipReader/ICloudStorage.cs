using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace S3ZipReader
{
	public interface ICloudStorage
	{
		Task<Stream> GetObjectStreamAsync(ICloudStorageResourceName resource, CloudStorageResourceRange range = null);
		Task<long> GetObjectLengthAsync(ICloudStorageResourceName resource);
	}
}
