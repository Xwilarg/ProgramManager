using Nancy;

namespace ProgramManager.Endpoint
{
    public class Login : NancyModule
    {
        public Login() : base("/login")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(false, args, "username", "password");
                if (error.HasValue)
                    return (Response.AsJson(new Response.Information()
                    {
                        Message = error.Value.message
                    }, error.Value.code));

                string userToken = Program.P.ProgDb.GetTokenFromLogin(args["username"], args["password"]);
                if (userToken == null)
                    return (Response.AsJson(new Response.Information()
                    {
                        Message = "Invalid username/password combinaison"
                    }, HttpStatusCode.BadRequest));

                return (Response.AsJson(new Response.Login()
                {
                    Token = userToken
                }));
            });
        }
    }
}
