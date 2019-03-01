using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3ZipReader
{
	/// <summary>
	/// Allows a zip file to be accessed from S3. Not thread safe - not to be used concurrently on different threads,
	/// except for the result of GetEntry(), which may be used on a separate thread from the original.
	/// </summary>
	public class CloudZipArchive : IDisposable
    {
        public CloudZipArchive(ICloudStorage storage, ICloudStorageResourceName resourceName)
        {
	        _CloudStorage = storage;
            _ResourceName = resourceName;

            _stream = new CloudStorageSeekableStream(_CloudStorage, _ResourceName);

			_Archive = new ZipArchive(_stream);
        }

		private ICloudStorage _CloudStorage;
		private ICloudStorageResourceName _ResourceName;
		private CloudStorageSeekableStream _stream;
        private ZipArchive _Archive;

        public List<ZipArchiveEntryMetadata> Entries => _Archive.Entries.Select(entry => new ZipArchiveEntryMetadata(entry)).ToList();

		/// <summary>
		/// Allows a single entry to be read
		/// </summary>
		/// <param name="fullName">The FullName of the member</param>
		/// <returns>
		/// An object which may be used to read the specified entry on a separate thread from the caller
		/// </returns>
		public CloudZipArchiveEntry GetEntry(string fullName)
        {
			return new CloudZipArchiveEntry(_CloudStorage, _ResourceName, _Archive.GetEntry(fullName));
        }

		/// <summary>
		/// Allows a single entry to be read
		/// </summary>
		/// <param name="metadata">The metadata of the member</param>
		/// <returns>
		/// An object which may be used to read the specified entry on a separate thread from the caller
		/// </returns>
		public CloudZipArchiveEntry GetEntry(ZipArchiveEntryMetadata metadata)
		{
			return GetEntry(metadata.FullName);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					_Archive.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~S3ZipArchive() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}

}
