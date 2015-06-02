using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Web.Mvc;
using gis_vrp.Models;
using gis_vrp.Services;
using Newtonsoft.Json;

namespace gis_vrp.Controllers
{
    public class FileController : Controller
    {
        private readonly IPointsSerializer _pointsSerializer;

        public FileController(IPointsSerializer pointsSerializer)
        {
            _pointsSerializer = pointsSerializer;
        }

        [HttpPost]
        public string GetPoints()
        {
            var httpPostedFile = HttpContext.Request.Files["UploadedFile"];
            if (httpPostedFile == null) return null;
            var list = _pointsSerializer.Deserialize(httpPostedFile.InputStream);

            //return new JsonResult {Data = list, ContentType = "json"};
            return JsonConvert.SerializeObject(list);

        }

        [HttpPost]
        public FileResult ExportPoints(List<Point> points)
        {
            if(points == null) return null;
            var p = JsonConvert.DeserializeObject<Point[]>(Request.Form["points"]);
            var fileName = DateTime.Now.ToString("ddmmyyyyhhss") + ".xml";
            var filePath = Server.MapPath("~/") + fileName;

            var data = _pointsSerializer.Serialize(p);
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    streamWriter.Write(data);
                }
            }

            var file = File(filePath, MediaTypeNames.Text.Xml, fileName);
            return file;
        }

	}
}