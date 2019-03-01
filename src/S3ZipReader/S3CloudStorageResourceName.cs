using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace S3ZipReader
{
	public class S3CloudStorageResourceName : ICloudStorageResourceName
	{
		public S3CloudStorageResourceName(string bucketName, string objectKey)
		{
			Name = bucketName;
			ObjectKey = objectKey;
		}

		public string Name { get; }
		public string ObjectKey { get; }
	}
}
