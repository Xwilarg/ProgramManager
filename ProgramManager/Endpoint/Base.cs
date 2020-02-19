using Nancy;

namespace ProgramManager.Endpoint
{
    public class Base : NancyModule
    {
        public Base() : base("/")
        {
            Get("/", x =>
            {
                return Response.AsJson(new Response.Information()
                {
                    Message = "Please go to https://admin.zirk.eu to use this service"
                });
            });
        }
    }
}
