using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlogLife.Startup))]
namespace BlogLife
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
