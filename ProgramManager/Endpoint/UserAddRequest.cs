using Nancy;

namespace ProgramManager.Endpoint
{
    public class UserAddRequest : NancyModule
    {
        /// <summary>
        /// Request the creation of an URL allowing to create a new user
        /// Given a session token, the new user username and permissions
        /// </summary>
        public UserAddRequest() : base("/userAddRequest")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(true, args, "token", "username", "permissions");
                if (error.HasValue)
                    return Response.AsJson(new Response.Information()
                    {
                        Message = error.Value.message
                    }, error.Value.code);

                if (!Program.P.ProgDb.HavePermission(args["token"], UserPermission.ManageUser))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "Missing permissions"
                    }, HttpStatusCode.Unauthorized);

                if (Program.P.ProgDb.DoesUserExists(args["username"]))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "User already exists"
                    }, HttpStatusCode.BadRequest);

                int perms;
                if (!int.TryParse(args["permissions"], out perms))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "Invalid permission"
                    }, HttpStatusCode.BadRequest);

                string token = Program.P.ProgDb.GetRandomToken();
                Program.P.UserRequests.Add(token, new Response.SingleUser()
                {
                    Username = args["username"],
                    Permissions = perms
                });

                return Response.AsJson(new Response.Login()
                {
                    Token = token
                });
            });
        }
    }
}
