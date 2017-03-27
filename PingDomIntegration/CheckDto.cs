namespace PingDomIntegration
{
    public class Rootobject
    {
        public Check[] checks { get; set; }
        public Counts counts { get; set; }
    }

    public class Counts
    {
        public int total { get; set; }
        public int limited { get; set; }
        public int filtered { get; set; }
    }

    public class Check
    {
        //private readonly CheckCreateDto _checkCreateDto = new CheckCreateDto();
        public int id { get; set; }
        public int created { get; set; }
        public string name { get; set; }
        public string hostname { get; set; }
        public bool use_legacy_notifications { get; set; }
        public int resolution { get; set; }
        public string type { get; set; }
        public bool ipv6 { get; set; }
        public int lasterrortime { get; set; }
        public int lasttesttime { get; set; }
        public int lastresponsetime { get; set; }
        public string status { get; set; }
        public int alert_policy { get; set; }
        public string alert_policy_name { get; set; }
        public int acktimeout { get; set; }
        public int autoresolve { get; set; }

        public double UpTime { get; set; }

       
    }
}
