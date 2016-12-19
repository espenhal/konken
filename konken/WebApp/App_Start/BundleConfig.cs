using System.Web.Optimization;

namespace WebApp
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;
            bundles.FileExtensionReplacementList.Clear();
            
            bundles.Add(new ScriptBundle("~/app/appfiles").IncludeDirectory("~/app/", "*.js", true));
        }
    }
}