using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace S3ZipReader
{
	/// <summary>
	/// Allows a zip file member to be read directly from S3. Not thread safe - not to be used concurrently on different threads. 
	/// </summary>
	public class CloudZipArchiveEntry : ZipArchiveEntryMetadata
	{
        internal CloudZipArchiveEntry(ICloudStorage storage, ICloudStorageResourceName resourceName, ZipArchiveEntry entry)
			: base(entry)
        {
	        _CloudStorage = storage;
	        _ResourceName = resourceName;

			CompressionMethod = (ushort)methodProperty.GetValue(entry);
			OffsetOfCompressedData = (long)offsetProperty.GetValue(entry);
        }

        private ICloudStorage _CloudStorage;
        private ICloudStorageResourceName _ResourceName;

		private static PropertyInfo offsetProperty = typeof(ZipArchiveEntry).GetProperty("OffsetOfCompressedData", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo methodProperty = typeof(ZipArchiveEntry).GetProperty("CompressionMethod", BindingFlags.NonPublic | BindingFlags.Instance);

		public async Task<Stream> OpenAsync()
		{
			var range = new CloudStorageResourceRange(OffsetOfCompressedData, CompressedLength);

	        Console.WriteLine($"Cloud zip archive entry requesting byte range at {range.Offset} length {range.Length}");

			var resp = await _CloudStorage.GetObjectStreamAsync(_ResourceName, range);

	        return GetDataDecompressor(resp);
        }

		private Stream GetDataDecompressor(Stream underlying)
		{
			// Alternatively, use reflection to call ZipArchiveEntry.GetDataDecompressor

			Stream stream;

			switch (CompressionMethod)
			{
				case 8: // Deflate
					stream = new DeflateStream(underlying, CompressionMode.Decompress);
					break;

				case 0: // Stored
					stream = underlying;
					break;

				// Deflate64, BZip2 etc
				default:
					throw new InvalidDataException(
					$"Zip entry {FullName} has unsupported compression method {CompressionMethod}");
			}

			return stream;
		}

		public ushort CompressionMethod { get; }
		public long OffsetOfCompressedData { get; }
	}

}
