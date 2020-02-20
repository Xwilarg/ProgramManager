﻿using Nancy;

namespace ProgramManager.Endpoint
{
    public class TokenCheck : NancyModule
    {
        public TokenCheck() : base("/tokenCheck")
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

                return Response.AsJson(new Response.Empty()
                { }, HttpStatusCode.NoContent);
            });
        }
    }
}