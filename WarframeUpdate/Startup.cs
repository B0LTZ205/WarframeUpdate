using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WarframeUpdate.Startup))]
namespace WarframeUpdate
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
