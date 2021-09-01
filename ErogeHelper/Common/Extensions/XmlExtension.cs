using ErogeHelper.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ErogeHelper.Common.Extensions
{
    public static class XmlExtension
    {
        public static T? XmlDeserialize<T>(string xmlString)
        {
            T? t = default;
            var xmlSerializer = new XmlSerializer(typeof(List<CloudGameDataEntity>));
            using (Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
            {
                using var xmlReader = XmlReader.Create(xmlStream);
                var obj = xmlSerializer.Deserialize(xmlReader);
                if (obj is null)
                {
                    return default;
                }
                t = (T)obj;
            }
            return t;
        }

        public static string ToXmlString<T>(this T input)
        {
            using var writer = new StringWriter();
            input.ToXml(writer);
            return writer.ToString();
        }

        private static void ToXml<T>(this T objectToSerialize, StringWriter writer) =>
            new XmlSerializer(typeof(T)).Serialize(writer, objectToSerialize);
    }
}
