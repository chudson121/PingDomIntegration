using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace PingDomIntegration
{
    public class PingdomProcesser
    {
        public PingdomConfiguration Config;

        public PingdomProcesser(PingdomConfiguration config)
        {
            Config = config;
        }

        public List<Check> GetChecks()
        {
            var response = ConfigClientForChecks(Method.GET, "checks");
            var rootElementObj = JsonConvert.DeserializeObject<Rootobject>(response.Content);
            //foreach (var check in rootElementObj.checks)
            //{
            //    Logger.Info($"Host: {check.hostname} Status: {check.status} ");
            //}

            return rootElementObj.checks.ToList();
            //return response.Content;
        }

        public string CreateNewCheck(CheckCreateDto check)
        {
            var fd = (from x in check.GetType().GetProperties() select x).ToDictionary(x => x.Name, x => (x.GetGetMethod().Invoke(check, null) == null 
            ? "" 
            : x.GetGetMethod().Invoke(check, null).ToString()));

            var response = ConfigClientForChecks(Method.POST, "checks", fd);

            var content = response.Content; // raw content as string
            return content;


            //var newCheck = new { name = check.name, type = check.type, host = check.hostname };
            //var createNewCheckResponse = await Pingdom.Client.Checks.CreateNewCheck(newCheck);
            
        }

        private IRestResponse ConfigClientForChecks(Method httpVerb, string resourceMethod, Dictionary<string, string> payload = null)
        {
            var client = new RestClient(Config.BaseUrl)
            {
                Authenticator = new HttpBasicAuthenticator(Config.Accountemail, Config.Accountpw)
            };

            var request = new RestRequest(resourceMethod, httpVerb);
            request.AddHeader("App-Key", Config.AccountApiKey);
            if (payload != null)
            {
                foreach (var kvp in payload)
                {
                    request.AddParameter(kvp.Key, kvp.Value);
                }


            }
            //

            var response = client.Execute(request);
            return response;
            
        }

        public Summary GetSummaryUptime(Check check, DateTime firstDay, DateTime lastDay)
        {
            var payload = new Dictionary<string, string> {{"includeuptime", "true"}};

            var response = ConfigClientForChecks(Method.GET, $"summary.average/{check.id}", payload);

            var rootElementObj = JsonConvert.DeserializeObject<SummaryRootobject>(response.Content);
            return rootElementObj?.summary;
        }
    }
}
