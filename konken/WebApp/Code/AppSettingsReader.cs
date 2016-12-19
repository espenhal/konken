using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace WebApp.Code
{
    public static class AppSettingsReader
    {
        public static string CDNPAth
        {
            get
            {
                var url = GetSetting("CDNPAth");
                if (string.IsNullOrWhiteSpace(url))
                    return null;

                if (url.EndsWith("/"))
                    url = url.TrimEnd('/');

                return url;
            }
        }
        
        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}