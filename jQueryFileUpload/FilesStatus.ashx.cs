using System.IO;

namespace jQueryUploadTest {
	/// <summary>
	/// Uploaded Files
	/// </summary>
	public class FilesStatus {
		public const string HandlerPath = "/";

		public string group { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public int size { get; set; }
		public string progress { get; set; }
		public string url { get; set; }
		public string thumbnail_url { get; set; }
		public string delete_url { get; set; }
		public string delete_type { get; set; }
		public string error { get; set; }

		public FilesStatus () { }

		public FilesStatus (FileInfo fileInfo) { this.SetValues(fileInfo.Name, (int)fileInfo.Length); }

		public FilesStatus (string FileName, int FileLength) { this.SetValues(FileName, FileLength); }

		private void SetValues (string FileName, int FileLength) {
			name = FileName;
			type = "image/png";
			size = FileLength;
			progress = "1.0";
			url = HandlerPath + "FileTransferHandler.ashx?f=" + FileName;
			thumbnail_url = HandlerPath + "Thumbnail.ashx?f=" + FileName;
			delete_url = HandlerPath + "FileTransferHandler.ashx?f=" + FileName;
			delete_type = "DELETE";
		}
	}
}