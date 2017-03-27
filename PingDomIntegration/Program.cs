using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PingDomIntegration.CommandLine;
using PingDomIntegration.Logging.Interfaces;
using PingDomIntegration.Logging.Services;

namespace PingDomIntegration
{
    internal class Program
    {
        private static readonly ILogger Logger = LoggingService.GetLoggingService();
        private static CommandLineProcessor _clp;

        public static void Main(string[] args)
        {
            Logger.Info("Program startup");
            ConfigureCommandLIne(args);
            


            var pingdomConfiguration = new PingdomConfiguration
            {
                BaseUrl = System.Configuration.ConfigurationManager.AppSettings["pingdom:BaseUrl"],
                AccountApiKey = System.Configuration.ConfigurationManager.AppSettings["pingdom:AppKey"],
                Accountemail = System.Configuration.ConfigurationManager.AppSettings["pingdom:UserName"],
                Accountpw = System.Configuration.ConfigurationManager.AppSettings["pingdom:Password"]
            };

            // --a all checks --u "Hudson.com" --c "TestMe" --t "Azure,API" --o output.md
            //command line --h --a --o
            var allchecks = GetAllChecks(pingdomConfiguration);

            if (_clp.Options.GetAll)
            {
                Logger.Info("Write out list of all checks");
                WriteOutListOfChecks(allchecks);
            }

            if (!string.IsNullOrEmpty(_clp.Options.HostName) && !string.IsNullOrEmpty(_clp.Options.CheckName))
            {
                Logger.Info("Create new check");
                CreateNewCheck(pingdomConfiguration, _clp.Options.HostName, _clp.Options.CheckName, string.Empty, allchecks);
            }

            if (!string.IsNullOrEmpty(_clp.Options.InputFilePath))
            {
                Logger.Info("Create new check from inputfile");
                ProcessInputBatchFile(pingdomConfiguration, _clp.Options.InputFilePath, allchecks);

            }

            if (_clp.Options.IsExport)
            {
                Logger.Info("Export existing checks");
                WriteOutListOfCheckIdAndCheckName(allchecks);
            }

            if (!string.IsNullOrEmpty(_clp.Options.UptimePeriod))
            {
                Logger.Info("Get Uptime Report");
                WriteUptimeReport(pingdomConfiguration, allchecks, GetTimePeriodFromCommandLineArgs());
            }
            
            Logger.Info("Program End");
            //Console.ReadLine();
        }

        private static int GetTimePeriodFromCommandLineArgs()
        {
            var timePeriod = DateTime.Now.Month;

            if (_clp.Options.UptimePeriod.ToLower() == "month")
            {
                timePeriod = DateTime.Now.Month;
            }
            if (_clp.Options.UptimePeriod.ToLower() == "year")
            {
                timePeriod = DateTime.Now.Year;
            }

            return timePeriod;
        }

        private static void WriteUptimeReport(PingdomConfiguration pingdomConfiguration, IEnumerable<Check> allchecks, int timePeriod)
        {
            if (allchecks == null)
            {
                throw new ArgumentNullException(nameof(allchecks));
            }

            var sb = new StringBuilder();
            var pp = new PingdomProcesser(pingdomConfiguration);
            var firstDay = new DateTime(timePeriod, 1, 1);
            var lastDay = new DateTime(timePeriod, 12, 31);
            IEnumerable<Check> checks = allchecks.ToList();
            var groupsNoReportOn = new List<string> { "SB3 ", "TEST"};

            foreach (var check in checks.OrderBy(x => x.name))
            {
                if (check.status == "paused")
                {
                    continue;
                }

                Logger.Info($"Processing:{check.name}");
                
                var res = pp.GetSummaryUptime(check, firstDay, lastDay);
                if (res == null)
                {
                    continue;
                }

                check.UpTime = res.status.CalculateUptime();
                sb.AppendLine($"|{check.id}|{check.name}|{res.status.totaldown}|{res.status.totalup}|{check.UpTime:P2}|");
            }

            sb.AppendLine(GroupCheckSummaries(checks.OrderBy(x => x.name), groupsNoReportOn));

            WriteOutputFile(string.Join(Environment.NewLine, sb.ToString()), $"PingdomUpTime-{timePeriod}");
        }

        private static string GroupCheckSummaries(IEnumerable<Check> allchecks, List<string> groupsNoReportOn)
        {
            var summaryHeader = new StringBuilder();
            summaryHeader.AppendLine("");
            summaryHeader.AppendLine("**Uptime Summary**");
            summaryHeader.AppendLine("");
            summaryHeader.Append($"|Date|");

            var summarydata = new StringBuilder();
            summarydata.Append($"|{DateTime.Now.Year} {DateTime.Now:MMM}|");

            
            var checks = allchecks as IList<Check> ?? allchecks.Where(i => !groupsNoReportOn.Any(e => i.name.Contains(e))).ToList();
            var summaryForGroups = checks.OrderBy(x => x.name).GroupBy(g => g.name.Substring(0, 4))
                 .Select(gn => new
                 {
                     GroupName = gn.Key,
                     CountOfSitesInGrp = gn.Count(),
                     UpTime = gn.Sum(c => c.UpTime / gn.Count())
                 });
            
            foreach (var result in summaryForGroups)
            {
                summaryHeader.Append($"{result.GroupName.Replace("-", "")}|");
                summarydata.Append($"({result.CountOfSitesInGrp:D2}) {result.UpTime:P2}|");
            }

            

            //|{DateTime.Now.Year} {DateTime.Now:MMM}|
            summaryHeader.Append($"Overall|{Environment.NewLine}");
            summaryHeader.AppendLine("|:--------| ------------ | ------------ |");

            summarydata.Append($"({checks.Count()}) {checks.Average(t => t.UpTime):P2}|");
            var retval = summaryHeader.Append(summarydata);
            return retval.ToString();
        }

        private static void WriteOutListOfCheckIdAndCheckName(List<Check> allchecks)
        {
            if (allchecks == null)
            {
                throw new ArgumentNullException(nameof(allchecks));
            }

            var sb = new StringBuilder();
            foreach (var check in allchecks.OrderBy(x => x.name))
            {
                if (check.status == "paused")
                {
                    continue;
                }

                Logger.Info($"Processing:{check.name}");
                sb.AppendLine($"{check.id}|{check.name}");
            }

            WriteOutputFile(string.Join(Environment.NewLine, sb.ToString()), "PingdomExport");

            
        }

        private static void ProcessInputBatchFile(PingdomConfiguration pingdomConfiguration, string optionsInputFilePath, List<Check> allchecks)
        {
            if (File.Exists(_clp.Options.InputFilePath))
            {
                var logFile = File.ReadAllLines(optionsInputFilePath);
                var logList = new List<string>(logFile);
                foreach (var checkAndUrl in logList)
                {
                    if (string.IsNullOrEmpty(checkAndUrl)) { continue; }

                    var splitval = checkAndUrl.Split('|');
                    var checkname = splitval[0];
                    var hostUrl = splitval[1];
                    var tags = splitval[2];
                    CreateNewCheck(pingdomConfiguration, hostUrl, checkname, tags, allchecks);
                }

                return;
            }

            Logger.Error("Input file does not exist");
            throw new ArgumentNullException($"InputFilePath", "Missing input file");
        }

        private static void WriteOutListOfChecks(IEnumerable<Check> allchecks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("|Id|Created|Name|HostName|LegacyNotifications|Resolution|Type|IPV6|LastErrorTime|LastTestTime|LastResponseTimne|Status|Policy|PolicyName|AckTimeout|AutoResolve|");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
            foreach (var check in allchecks)
            {
                var fd = (from x in check.GetType().GetProperties()
                          select x).ToDictionary(x => x.Name, x => (x.GetGetMethod().Invoke(check, null) == null ? "" : x.GetGetMethod().Invoke(check, null).ToString()));

                foreach (var kvp in fd)
                {
                    sb.Append($"|{kvp.Value}|");
                }

                sb.Append(Environment.NewLine);
            }

            WriteOutputFile(string.Join(Environment.NewLine, sb.ToString()), "PingdomChecks");
        }

        private static void CreateNewCheck(PingdomConfiguration pingdomConfiguration, string hostName, string checkName, string tags, IEnumerable<Check> allChecks)
        {
           // var allchecks = GetAllChecks(pingdomConfiguration);
            var alreadyExists = allChecks.Any(x => x.hostname == hostName);
            if (!alreadyExists)
            {
                Logger.Info("Adding Check to Pingdom");
                var check = new CheckCreateDto
                {
                    host = hostName,
                    name = checkName,
                    type = "http",
                    encryption = true,
                    port = 443,
                    sendtoemail = true,
                    resolution = 1,
                    tags = tags

                };
                var pp = new PingdomProcesser(pingdomConfiguration);
                var jsonOutput = pp.CreateNewCheck(check);

                Logger.Info($"{Environment.NewLine}{jsonOutput}");
            }
            else
            {
                Logger.Info("Check already exists");
            }
        }

        private static List<Check> GetAllChecks(PingdomConfiguration pingdomConfiguration)
        {
            var pp = new PingdomProcesser(pingdomConfiguration);

            var jsonOutput = pp.GetChecks();
            //var rootElementObj = JsonConvert.DeserializeObject<Rootobject>(jsonOutput);
            //foreach (var check in rootElementObj.checks)
            //{
            //    Logger.Info($"Host: {check.hostname} Status: {check.status} ");
            //}

            return jsonOutput;
        }

        private static void WriteOutputFile(string results, string reportTitle)
        {
            //var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            Logger.Info(Environment.NewLine + results);
            string filename = $"{reportTitle}-{DateTime.Now:yyyy - MM - dd_hh - mm - ss - tt}.md";
            string path;

            path = string.IsNullOrEmpty(_clp.Options.OutputFilePath) ? filename : _clp.Options.OutputFilePath;

            File.WriteAllText(path, results);
            Logger.Info($"File Created: {path}");

        }

        private static void ConfigureCommandLIne(string[] args)
        {
            Logger.Info("Configure Command Line Settings");
            _clp = new CommandLineProcessor(args, Logger);
            Logger.Info("Debug Mode:{0}", _clp.Options.IsDebug);
          
        }


    }
}
