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
	/// Stores the metadata of a zip file entry. Thread safe.
	/// </summary>
    public class ZipArchiveEntryMetadata
    {
        internal ZipArchiveEntryMetadata(ZipArchiveEntry entry)
        {
			CompressedLength = entry.CompressedLength;
			FullName = entry.FullName;
			Length = entry.Length;
			Name = entry.Name;
        }

		public long CompressedLength { get; }
		public string FullName { get; }
        public long Length { get; }
		public string Name { get; }
	}
}
