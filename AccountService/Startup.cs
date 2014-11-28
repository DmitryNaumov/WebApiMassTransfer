using System.Web.Http;
using Owin;

namespace AccountService
{
    public class Startup
    {
        public virtual void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();
            configuration.MapHttpAttributeRoutes();

            app.UseWebApi(configuration);
        }
    }
}
