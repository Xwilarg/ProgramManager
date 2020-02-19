using Nancy;
using Nancy.Helpers;
using System.Collections.Specialized;
using System.IO;

namespace ProgramManager.Endpoint
{
    class Common
    {
        /// <summary>
        /// Parse all args of the request body and return them in a NameValueCollection
        /// </summary>
        public static NameValueCollection ParseArgs(Stream bodyStream)
        {
            string body;
            using (var reader = new StreamReader(bodyStream)) // x-www-form-urlencoded
                body = reader.ReadToEnd();
            return HttpUtility.ParseQueryString(body);
        }

        /// <summary>
        /// Basic check for all endpoints
        /// </summary>
        /// <param name="args">All args (given by ParseArgs)</param>
        /// <param name="mandatory">List of mandatory parameters</param>
        /// <returns>Error is something wrong happened, else null</returns>
        public static Answer? BasicCheck(bool checkToken, NameValueCollection args, params string[] mandatory)
        {
            if (checkToken)
            {
                string token = args.Get("token");
                if (string.IsNullOrEmpty(token))
                    return new Answer(HttpStatusCode.BadRequest, "Missing arguments");
                if (!Program.P.ProgDb.IsTokenValid(token))
                    return new Answer(HttpStatusCode.Unauthorized, "Bad token");
            }
            foreach (string s in mandatory)
            {
                if (string.IsNullOrWhiteSpace(args.Get(s)))
                    return new Answer(HttpStatusCode.BadRequest, "Missing arguments");
            }
            return null;
        }

        public struct Answer
        {
            public Answer(HttpStatusCode mcode, string mmessage)
            {
                code = mcode;
                message = mmessage;
            }
            public HttpStatusCode code;
            public string message;
        }
    }
}
