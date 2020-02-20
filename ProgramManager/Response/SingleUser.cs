using Newtonsoft.Json;

namespace ProgramManager.Response
{
    public class SingleUser
    {
        [JsonProperty]
        public string Username;

        [JsonProperty]
        public int Permissions;
    }
}
