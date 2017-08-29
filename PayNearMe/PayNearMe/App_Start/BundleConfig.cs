using System.Web;
using System.Web.Optimization;

namespace PayNearMe
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js",
                                                                        "~/Scripts/Register.js",
                                                                        "~/Scripts/respond.js",
                                                                        "~/Scripts/Home.js",
                                                                        "~/Scripts/addBeneficiary.js",
                                                                        "~/Scripts/ChangePassword.js",
                                                                        "~/Scripts/addBeneficiary2.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-3.1.1.js",
                                                                     "~/Scripts/jquery.validate.js",
                                                                     "~/Scripts/jquery.validate.unobtrusive.js",
                                                                     "~/Scripts/jquery.freezeheader.js",
                                                                     "~/Scripts/jspdf.min.js",
                                                                     "~/Scripts/Transactions.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizrRr").Include( "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Styles/cssBundle").Include("~/Content/Register.css",
                                                                      "~/Content/modbootstrap.css",
                                                                      "~/Content/Site.css",
                                                                      "~/Content/normalize.css",
                                                                      "~/Content/navigation.css",
                                                                      "~/Content/font-awesome.css",
                                                                      "~/Content/Profile.css",
                                                                      "~/Content/media.css",
                                                                      "~/Content/style_responsive.css",
                                                                      "~/Content/color_responsive.css",
                                                                      "~/Content/_SVGImageLoad.css",
                                                                      "~/Content/iBeneficiary.css"
                                                                      ));
        }
    }
}
