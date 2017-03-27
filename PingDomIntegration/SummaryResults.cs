namespace PingDomIntegration
{

    public class SummaryRootobject
    {
        public Summary summary { get; set; }
    }

    public class Summary
    {
        public Responsetime responsetime { get; set; }
        public Status status { get; set; }
    }

    public class Responsetime
    {
        public int from { get; set; }
        public int to { get; set; }
        public int avgresponse { get; set; }
    }

    public class Status
    {
        public int totalup { get; set; }
        public int totaldown { get; set; }
        public int totalunknown { get; set; }
        
        private const double Convert100Todecimal = 100 / 100d;
        public double CalculateUptime()
        {
 
            var uptime = (totaldown == 0) ? Convert100Todecimal : Convert100Todecimal - ((totaldown / 100d) / ((totalup / 100d) + (totaldown / 100d)));
            return uptime;

        }
    }

}
