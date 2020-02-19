using Nancy;

namespace ProgramManager.Endpoint
{
    public class UserAdd : NancyModule
    {
        public UserAdd() : base("/")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(args, "token", "username", "password", "email");
                if (error.HasValue)
                    return (Response.AsJson(new Response.Information()
                    {
                        Message = error.Value.message
                    }, error.Value.code));

                if (Program.P.ProgDb.DoesUserExists(args["username"]))
                    return (Response.AsJson(new Response.Information()
                    {
                        Message = "User already exists"
                    }, HttpStatusCode.BadRequest));

                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }
    }
}
