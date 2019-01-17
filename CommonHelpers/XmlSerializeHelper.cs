using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CommonHelpers
{
    /// <summary>
    /// XML序列化
    /// </summary>
    public static class XmlSerializeHelper
    {
        /// <summary>
        /// XML序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string XmlSerialize<T>(T obj)
        {
            string xmlString;
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, obj);
                xmlString = Encoding.UTF8.GetString(ms.ToArray());
            }
            return xmlString;
        }

        /// <summary>
        /// XML反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T XmlDeserialize<T>(string xmlString)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xmlString);
            }
            catch
            {
                return default(T);
            }
            T t;
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
            {
                using (XmlReader xmlReader = XmlReader.Create(xmlStream))
                {
                    try
                    {
                        var obj = xmlSerializer.Deserialize(xmlReader);
                        t = (T)obj;
                    }
                    catch
                    {
                        return default(T);
                    }
                }
            }
            return t;
        }
    }
}
