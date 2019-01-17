using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonHelpers
{
    public class ResourceHelper
    {
        /// <summary>
        ///     根据资源索引查找对应的词条
        /// </summary>
        /// <param name="resourceKey">词条对应的资源索引</param>
        /// <returns></returns>
        public static string FindKey(string resourceKey)
        {
            return Application.Current?.TryFindResource(resourceKey)?.ToString();
        }

        public static object GetResource(string resourceKey)
        {
            return Application.Current?.TryFindResource(resourceKey);
        }

        public static void SetResourceByKey(string srcResourceKey, string destResourceKey)
        {
            if (string.IsNullOrEmpty(srcResourceKey) || string.IsNullOrEmpty(destResourceKey)) return;
            if (Application.Current.Resources.Contains(srcResourceKey) &&
                Application.Current.Resources.Contains(destResourceKey))
            {
                Application.Current.Resources[srcResourceKey] = Application.Current.Resources[destResourceKey];
            }
        }
    }
}
