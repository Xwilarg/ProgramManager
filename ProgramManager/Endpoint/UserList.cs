using Nancy;
using System.Linq;

namespace ProgramManager.Endpoint
{
    public class UserList : NancyModule
    {
        /// <summary>
        /// List all users (username and permissions) given a session token
        /// </summary>
        public UserList() : base("/userList")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(true, args, "token");
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

                return Response.AsJson(new Response.User()
                {
                    Users = Program.P.ProgDb.GetAllUsers().ToArray()
                });
            });
        }
    }
}
