using Amazon.S3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace S3ZipReader
{
	#region S3CloudStorage 

	/// <summary>
	/// Helper to talk to S3
	/// </summary>
	public class S3CloudStorage : ICloudStorage
	{
		/// <summary>
		/// S3 Client
		/// </summary>
		public static AmazonS3Client S3Client = new AmazonS3Client();

		/// <summary>
		/// Get Object
		/// </summary>
		/// <param name="resource"></param>
		/// <param name="range">(optional)</param>
		/// <returns></returns>
		public async Task<Stream> GetObjectStreamAsync(
			ICloudStorageResourceName resource, CloudStorageResourceRange range = null)
		{
			Stream stream = null;

			if (resource is S3CloudStorageResourceName s3resource)
			{
				var req = new GetObjectRequest {BucketName = s3resource.Name, Key = s3resource.ObjectKey};

				if (range != null)
				{
					req.ByteRange = new ByteRange(range.Offset, range.Offset + range.Length - 1);
				}

				var objectResponse = await S3Client.GetObjectAsync(req);
				stream = objectResponse.ResponseStream;
			}

			return stream;
		}

		public async Task<long> GetObjectLengthAsync(ICloudStorageResourceName resource)
		{
			if (resource is S3CloudStorageResourceName s3resource)
			{
				var metadata = await S3Client.GetObjectMetadataAsync(s3resource.Name, s3resource.ObjectKey);
				return metadata.ContentLength;
			}

			return -1;
		}

	}

	#endregion
}
