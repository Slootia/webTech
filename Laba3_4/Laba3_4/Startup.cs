using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Laba3_4.Startup))]
namespace Laba3_4
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
