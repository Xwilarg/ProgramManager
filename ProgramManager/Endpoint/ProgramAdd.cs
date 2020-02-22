using Nancy;
using System.IO;

namespace ProgramManager.Endpoint
{
    public class ProgramAdd : NancyModule
    {
        /// <summary>
        /// Add a program given a path
        /// </summary>
        public ProgramAdd() : base("/programAdd")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(true, args, "token", "path");
                if (error.HasValue)
                    return Response.AsJson(new Response.Information()
                    {
                        Message = error.Value.message
                    }, error.Value.code);

                if (!Program.P.ProgDb.HavePermission(args["token"], UserPermission.ManageProgram))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "Missing permissions"
                    }, HttpStatusCode.Unauthorized);

                if (!File.Exists(args["path"]))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "Invalid path"
                    }, HttpStatusCode.BadRequest);

                FileInfo fi = new FileInfo(args["path"]);
                if (Program.P.ProgDb.Processes.ContainsKey(fi.Name))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "File already added"
                    }, HttpStatusCode.BadRequest);

                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }
    }
}
