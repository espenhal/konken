using System.Web;

namespace WebApp.Code
{
    public static class CDNScripts
    {
        public static IHtmlString Render(params string[] paths)
        {
            //if (string.IsNullOrWhiteSpace(AppSettingsReader.CDNPAth))
            //{
            //    return System.Web.Optimization.Scripts.Render(paths);
            //}
            //else
            //{
                return new HtmlString(System.Web.Optimization.Scripts.Render(paths).ToHtmlString().Replace("src=\"/"
                    , string.Format("src=\"{0}/", AppSettingsReader.CDNPAth)));
            //}
        }
    }
}