using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PayNearMe.Startup))]
namespace PayNearMe
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
