using Newtonsoft.Json;

namespace ProgramManager.Response
{
    public class SingleProgram
    {
        [JsonProperty]
        public string Name;

        [JsonProperty]
        public string Stdout;
    }
}
