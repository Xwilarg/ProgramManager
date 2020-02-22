using Newtonsoft.Json;

namespace ProgramManager.Response
{
    public class Program
    {
        [JsonProperty]
        public SingleProgram[] Processes;
    }
}
