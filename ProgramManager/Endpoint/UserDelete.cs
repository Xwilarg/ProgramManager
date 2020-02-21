using Nancy;

namespace ProgramManager.Endpoint
{
    public class UserDelete : NancyModule
    {
        /// <summary>
        /// Delete an user given it username
        /// </summary>
        public UserDelete() : base("/userDelete")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(true, args, "token", "username");
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

                if (!Program.P.ProgDb.DoesUserExists(args["username"]))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "User don't exists"
                    }, HttpStatusCode.BadRequest);

                Program.P.ProgDb.DeleteUser(args["username"]);

                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }
    }
}