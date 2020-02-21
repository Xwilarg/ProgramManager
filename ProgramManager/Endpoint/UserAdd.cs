using Nancy;

namespace ProgramManager.Endpoint
{
    public class UserAdd : NancyModule
    {
        /// <summary>
        /// Add an user given an URL token and a password.
        /// Users datas are stored in Program.P.UserRequests
        /// </summary>
        public UserAdd() : base("/userAdd")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(false, args, "urlToken", "password");
                if (error.HasValue)
                    return Response.AsJson(new Response.Information()
                    {
                        Message = error.Value.message
                    }, error.Value.code);

                if (!Program.P.UserRequests.ContainsKey(args["urlToken"]))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "Invalid urlToken"
                    }, HttpStatusCode.BadRequest);

                var user = Program.P.UserRequests[args["urlToken"]];
                if (Program.P.ProgDb.DoesUserExists(user.Username))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "User already exists"
                    }, HttpStatusCode.BadRequest);

                Program.P.ProgDb.AddUser(user.Username, args["password"], user.Permissions);
                Program.P.UserRequests.Remove(args["urlToken"]);

                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }
    }
}
