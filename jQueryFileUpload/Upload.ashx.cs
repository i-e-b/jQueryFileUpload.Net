using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace WebInterface.General.Handlers
{
    /// <summary>
    /// Summary description for ImageUpload
    /// </summary>
    public class ImageUpload : IHttpHandler
    {
        /// <summary>
        /// Uploaded Files
        /// </summary>
        public class FilesStatus
        {
            public string name { get; set; }
            public string type { get; set; }
            public int size { get; set; }
            public string progress { get; set; }
            public string url { get; set; }
            public string thumbnail_url { get; set; }
            public string delete_url { get; set; }
            public string delete_type { get; set; }
            public string error { get; set; }

            public FilesStatus(){}

            public FilesStatus(FileInfo fileInfo)
            { this.SetValues(fileInfo.Name, (int)fileInfo.Length); }

            public FilesStatus(string FileName, int FileLength)
            { this.SetValues(FileName, FileLength); }

            private void SetValues(string FileName, int FileLength)
            {
                name = FileName;
                type = "image/png";
                size = FileLength;
                progress = "1.0";
                url = HandlerPath + "Upload.ashx?f=" + FileName;
                thumbnail_url = HandlerPath + "Thumbnail.ashx?f=" + FileName;
                delete_url = HandlerPath + "Upload.ashx?f=" + FileName;
                delete_type = "DELETE";
            }
        }

        private readonly JavaScriptSerializer js = new JavaScriptSerializer();
        private const string HandlerPath = "/General/Handlers/";
        private string ImagePath;
        private string ThumbPath;
        
        public bool IsReusable { get { return false; } }

        // Process incoming request
        public void ProcessRequest(HttpContext context)
        {
            // Set the image paths
            ImagePath = context.Server.MapPath("~/Content/UserContent/Images/");
            ThumbPath = context.Server.MapPath("~/Content/UserContent/Images/Thumbs/");

            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "private, no-cache");

            HandleMethod(context);
        }

        // Handle request based on method
        private void HandleMethod(HttpContext context)
        {
            switch (context.Request.HttpMethod)
            {
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

        // Delete file from the server
        private void DeleteFile(HttpContext context)
        {
            var filePath = ImagePath + context.Request["f"];
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        // Upload file to the server
        private void UploadFile(HttpContext context)
        {
            var statuses = new List<FilesStatus>();
            var headers = context.Request.Headers;

            if (string.IsNullOrEmpty(headers["X-File-Name"]))
            {
                UploadWholeFile(context, statuses);
            }
            else
            {
                UploadPartialFile(headers["X-File-Name"], context, statuses);
            }

            WriteJsonIframeSafe(context, statuses);
        }

        // Upload partial file
        private void UploadPartialFile(string fileName, HttpContext context, List<FilesStatus> statuses)
        {
            if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var inputStream = context.Request.Files[0].InputStream;
            var fullName = ImagePath + Path.GetFileName(fileName);

            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            statuses.Add(new FilesStatus(new FileInfo(fullName)));
        }

        // Upload entire file
        private void UploadWholeFile(HttpContext context, List<FilesStatus> statuses)
        {
            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                file.SaveAs(ImagePath + Path.GetFileName(file.FileName));

                string fullName = Path.GetFileName(file.FileName);
                statuses.Add(new FilesStatus(fullName, file.ContentLength));
            }
        }

        private void WriteJsonIframeSafe(HttpContext context, List<FilesStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
                    context.Response.ContentType = "application/json";
                else
                    context.Response.ContentType = "text/plain";
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }

            var jsonObj = js.Serialize(statuses.ToArray());
            context.Response.Write(jsonObj);
        }

        private void ServeFile(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request["f"])) ListCurrentFiles(context);
            else DeliverFile(context);
        }

        private void DeliverFile(HttpContext context)
        {
            string filePath = ThumbPath + context.Request["f"];

            if (File.Exists(filePath))
            {
                context.Response.ContentType = "application/octet-stream";
                context.Response.WriteFile(filePath);
                context.Response.AddHeader("Content-Disposition", "attachment, filename=\"" + context.Request["f"] + "\"");
            }
            else
                context.Response.StatusCode = 404;
        }

        private void ListCurrentFiles(HttpContext context)
        {
            var FileList = new List<FilesStatus>();
            var names = Directory.GetFiles(ImagePath, "*", SearchOption.TopDirectoryOnly);

            foreach (var name in names)
                FileList.Add(new FilesStatus(new FileInfo(name)));

            string jsonObj = js.Serialize(FileList.ToArray());
            context.Response.AddHeader("Content-Disposition", "inline, filename=\"files.json\"");
            context.Response.Write(jsonObj);
            context.Response.ContentType = "application/json";
        }
    }
}