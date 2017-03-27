using CommandLine;
using CommandLine.Text;

namespace PingDomIntegration.CommandLine
{
    public class CommandArgumentOptions
    {
        
        // Omitting long name, default --verbose
        [Option('v', HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        //--d
        [Option('d', "debug", Required = false, HelpText = "Turns debugging mode on.")]
        public bool IsDebug { get; set; }

        /*


                 [Option('c', "Child", Required = true, HelpText = "Child File + Path to use.")]
                 public string ChildFilePath { get; set; }

                 [Option('o', "OutputFile", Required = true, HelpText = "output file")]
                 public string OutFilePath { get; set; }
         */

        [Option('e', "Export", Required = false, HelpText = "Export Basic Info of Active only")]
        public bool IsExport { get; set; }



        [Option('u', "Hostname", Required = false, HelpText = "Specify Hostname")]
        public string HostName { get; set; }

        [Option('c', "Check", Required = false, HelpText = "Specify name for the check")]
        public string CheckName { get; set; }

        //[Option('t', "Tags", Required = false, HelpText = "Specify tags for the check")]
        //public string Tags { get; set; }

        [Option('a', "GetAll", Required = false, HelpText = "Should all checks be returned")]
        public bool GetAll { get; set; }

        //--f "C:\Users\chrish\Desktop\UserStory.txt" 
        [Option('o', "output", Required = false, HelpText = "output file path")]
        public string OutputFilePath { get; set; }

        //--f "C:\Users\chrish\Desktop\UserStory.txt" 
        [Option('i', "input", Required = false, HelpText = "Input file path")]
        public string InputFilePath { get; set; }

        [Option('p', "uptimePeriod", Required = false, HelpText = "Uptime Report")]
        public string UptimePeriod { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage()
        {
            
            var help = new HelpText
            {
                Heading = new HeadingInfo(
                    $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"),
                Copyright = new CopyrightInfo(" ", System.DateTime.Now.Year),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine(
                $"Usage: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} site action");
            help.AddOptions(this);
            return help;
          
        }
    }
}
