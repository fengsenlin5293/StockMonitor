using Interfaces.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Service
{
    public class LanguageService : ILanguageService
    {
        public bool LoadLanguage(string languageKey)
        {
            try
            {
                var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                mergedDictionaries.ToList().ForEach(m =>
                {
                    if (m.Source.OriginalString.Contains("Language"))
                        mergedDictionaries.Remove(m);
                });

                var uri = new Uri($"pack://application:,,,/Resource;component/Language/{languageKey}.xaml", UriKind.RelativeOrAbsolute);
                var resourceDictionary = new ResourceDictionary { Source = uri };
                mergedDictionaries.Add(resourceDictionary);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
