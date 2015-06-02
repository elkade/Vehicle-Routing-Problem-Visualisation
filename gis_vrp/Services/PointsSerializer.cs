using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using gis_vrp.Models;

namespace gis_vrp.Services
{
    public interface IPointsSerializer
    {
        IEnumerable<Point> Deserialize(Stream inputStream);
        String Serialize(IEnumerable<Point> points);
    }

    public class PointsSerializer : IPointsSerializer
    {
        public IEnumerable<Point> Deserialize(Stream inputStream)
        {
            var xml = new XmlDocument();

            try
            {
                xml.Load(inputStream);
                //xml.Schemas.Add(null, HttpContext.Current.Server.MapPath("~/XSD/Point.xsd"));
                //xml.Validate(ValidationEventHandler);
            }
            catch (Exception e)
            {
                return null;
            }


            var serializer = new XmlSerializer(typeof(List<Point>));
            var list = serializer.Deserialize(new StringReader(xml.InnerXml)) as List<Point>;

            return list;
        }

        public String Serialize(IEnumerable<Point> points)
        {
            var pts = points as List<Point> ?? points.ToList();
            var serializer = new XmlSerializer(typeof(List<Point>));

            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8
            };

            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(textWriter, xmlWriterSettings))
                {
                    serializer.Serialize(xmlWriter, pts);
                }

                return textWriter.ToString();
            }
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs validationEventArgs)
        {
            Debug.WriteLine(validationEventArgs.Exception);
            Debug.WriteLine(validationEventArgs.Message);
        }
    }
}