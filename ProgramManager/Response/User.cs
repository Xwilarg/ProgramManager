using Newtonsoft.Json;

namespace ProgramManager.Response
{
    public class User
    {
        [JsonProperty]
        public SingleUser[] Users;
    }
}
