using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace jQueryUploadTest {
	public class Upload : IHttpHandler {
		public class FilesStatus {
			public string thumbnail_url { get; set; }
			public string name { get; set; }
			public string url { get; set; }
			public int size { get; set; }
			public string type { get; set; }
			public string delete_url { get; set; }
			public string delete_type { get; set; }
			public string error { get; set; }
			public string progress { get; set; }
		}
		private readonly JavaScriptSerializer js = new JavaScriptSerializer();
		private string ingestPath;
		public bool IsReusable { get { return false; } }
		public void ProcessRequest (HttpContext context) {
			var r = context.Response;
			ingestPath = @"C:\temp\ingest\";

			r.AddHeader("Pragma", "no-cache");
			r.AddHeader("Cache-Control", "private, no-cache");

			HandleMethod(context);
		}

		private void HandleMethod (HttpContext context) {
			switch (context.Request.HttpMethod) {
				case "HEAD":
				case "GET":
					ServeFile(context);
					break;

				case "POST":
					UploadFile(context);
					break;

				case "DELETE":
					DeleteFile(context);
					break;

				default:
					context.Response.ClearHeaders();
					context.Response.StatusCode = 405;
					break;
			}
		}

		private void DeleteFile (HttpContext context) {
			var filePath = ingestPath + context.Request["f"];
			if (File.Exists(filePath)) {
				File.Delete(filePath);
			}
		}

		private void UploadFile (HttpContext context) {
			var statuses = new List<FilesStatus>();

			for (int i = 0; i < context.Request.Files.Count; i++) {
				var file = context.Request.Files[i];
				file.SaveAs(ingestPath + Path.GetFileName(file.FileName));
				var fname = Path.GetFileName(file.FileName);
				statuses.Add(new FilesStatus
				{
					thumbnail_url = "Thumbnail.ashx?f=" + fname,
					url = "Upload.ashx?f=" + fname,
					name = fname,
					size = file.ContentLength,
					type = "image/png",
					delete_url = "Upload.ashx?f=" + fname,
					delete_type = "DELETE",
					progress = "1.0"
				});
			}

			WriteJsonIframeSafe(context, statuses);
		}

		private void WriteJsonIframeSafe(HttpContext context, List<FilesStatus> statuses) {
			context.Response.AddHeader("Vary", "Accept");
			try {
				if (context.Request["HTTP_ACCEPT"].Contains("application/json")) {
					context.Response.ContentType = "application/json";
				} else {
					context.Response.ContentType = "text/plain";
				}
			} catch {
				context.Response.ContentType = "text/plain";
			}

			var jsonObj = js.Serialize(statuses.ToArray());
			context.Response.Write(jsonObj);
		}

		private void ServeFile (HttpContext context) {
			if (string.IsNullOrEmpty(context.Request["f"])) ListCurrentFiles(context);
			else DeliverFile(context);
		}

		private void DeliverFile (HttpContext context) {
			var filePath = ingestPath + context.Request["f"];
			if (File.Exists(filePath)) {
				context.Response.ContentType = "application/octet-stream";
				context.Response.WriteFile(filePath);
				context.Response.AddHeader("Content-Disposition", "attachment, filename=\"" + context.Request["f"] + "\"");
			} else {
				context.Response.StatusCode = 404;
			}
		}

		private void ListCurrentFiles (HttpContext context) {
			var files = new List<FilesStatus>();

			var names = Directory.GetFiles(@"C:\temp\ingest", "*", SearchOption.TopDirectoryOnly);

			foreach (var name in names) {
				var f = new FileInfo(name);
				files.Add(new FilesStatus
				{
					thumbnail_url = "Thumbnail.ashx?f=" + f.Name,
					url = "Upload.ashx?f=" + f.Name,
					name = f.Name,
					size = (int)f.Length,
					type = "image/png",
					delete_url = "Upload.ashx?f=" + f.Name,
					delete_type = "DELETE"
				});
			}

			context.Response.AddHeader("Content-Disposition", "inline, filename=\"files.json\"");
			var jsonObj = js.Serialize(files.ToArray());
			context.Response.Write(jsonObj);
			context.Response.ContentType = "application/json";
		}
	}
}
