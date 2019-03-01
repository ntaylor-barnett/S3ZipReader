using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace S3ZipReader
{
	public interface ICloudStorageResourceName
	{
		string Name { get; }
	}
}
