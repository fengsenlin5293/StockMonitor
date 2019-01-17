using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonHelpers
{
    /// <summary>
    ///     Json帮助类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        ///     将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(object o)
        {
            try
            {
                var json = JsonConvert.SerializeObject(o);
                return json;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        /// <summary>
        ///     解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return default(T);
            var serializer = new JsonSerializer();
            var sr = new StringReader(json);
            try
            {
                var o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
                var t = o as T;
                return t;
            }
            catch (Exception e) { }
            return null;
        }

        /// <summary>
        ///     解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            var serializer = new JsonSerializer();
            var sr = new StringReader(json);
            var o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            var list = o as List<T>;
            return list;
        }

        /// <summary>
        ///     解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="type">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static object DeserializeJsonToObject(string json, Type type)
        {
            var serializer = new JsonSerializer();
            var sr = new StringReader(json);
            var o = serializer.Deserialize(new JsonTextReader(sr), type);
            return o;
        }

        /// <summary>
        ///     反序列化JSON到给定的匿名对象.
        /// </summary>
        /// <typeparam name="T">匿名对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="anonymousTypeObject">匿名对象</param>
        /// <returns>匿名对象</returns>
        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
        {
            var t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            return t;
        }
    }
}
