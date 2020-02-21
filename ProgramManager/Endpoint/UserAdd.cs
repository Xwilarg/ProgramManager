using Nancy;

namespace ProgramManager.Endpoint
{
    public class UserAdd : NancyModule
    {
        public UserAdd() : base("/userAdd")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(true, args, "token", "urlToken", "password");
                if (error.HasValue)
                    return Response.AsJson(new Response.Information()
                    {
                        Message = error.Value.message
                    }, error.Value.code);

                if (Program.P.UserRequests.ContainsKey(args["urlToken"]))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "Invalid urlToken"
                    }, HttpStatusCode.BadRequest);

                if (Program.P.ProgDb.DoesUserExists(args["username"]))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "User already exists"
                    }, HttpStatusCode.BadRequest);

                var user = Program.P.UserRequests[args["urlToken"]];
                Program.P.ProgDb.AddUser(user.Username, args["password"], user.Permissions);
                Program.P.UserRequests.Remove(args["urlToken"]);

                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }
    }
}
