using System.Collections.Generic;

namespace PingDomIntegration
{
    public class CheckCreateDto
    {
        public string host { get; set; }
        public string name { get; set; }
        public string type { get; set; }

        public int port { get; set; }
        public bool sendtoemail { get; set; }
        public bool encryption { get; set; }
        //public string additionalurls { get; set; }
        public int resolution { get; set; }
        public string tags { get; set; }
        public List<KeyValuePair<string, string>> HeaderTags { get; set; }

    

    }
}