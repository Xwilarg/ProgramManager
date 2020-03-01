using Nancy;

namespace ProgramManager.Endpoint
{
    public class ProgramStop : NancyModule
    {
        public ProgramStop() : base("/programStop")
        {
            Post("/", x =>
            {
                // Getting args
                var args = Common.ParseArgs(Request.Body);

                // Error handling
                Common.Answer? error = Common.BasicCheck(true, args, "token", "name");
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

                if (!Program.P.ProgDb.Processes.ContainsKey(args.Get("name")))
                    return Response.AsJson(new Response.Information()
                    {
                        Message = "Program doesn't exists"
                    }, HttpStatusCode.BadRequest);

                Program.P.ProgDb.Processes[args.Get("name")].Stop();

                return Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent);
            });
        }
    }
}
