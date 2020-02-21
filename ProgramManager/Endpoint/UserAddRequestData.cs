using Nancy;

namespace ProgramManager.Endpoint
{
    public class UserAddRequestData : NancyModule
    {
        /// <summary>
        /// Get user data from an URL
        /// </summary>
        public UserAddRequestData() : base("/userAddRequestData")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(false, args, "urlToken");
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

                return Response.AsJson(new Response.SingleUser()
                {
                    Username = user.Username,
                    Permissions = user.Permissions
                });
            });
        }
    }
}
