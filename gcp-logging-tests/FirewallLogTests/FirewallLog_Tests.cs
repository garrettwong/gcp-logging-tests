using gcp_logging_tests.API;
using gcp_logging_tests.Utilities;
using RestSharp;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace gcp_logging_tests.FirewallLogTests
{
    public class FirewallLog_Tests
    {
        private RandomGenerator _randomGenerator;
        private GCPLogQueryGenerator _gcpLogQueryGenerator;
        private string _knownVmIp;

        public FirewallLog_Tests()
        {
            Access.Initiailize();

            _randomGenerator = new RandomGenerator();
            _gcpLogQueryGenerator = new GCPLogQueryGenerator();

            _knownVmIp = "34.134.129.29"; // IP HERE
        }

        [Fact]
        public void FirewallLog_Test()
        {
            var client = new RestClient($"http://{_knownVmIp}/");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);


            for(var i = 75; i < 85; i++)
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    tcpClient.SendTimeout = 500;
                    tcpClient.ReceiveTimeout = 500;

                    try
                    {
                        tcpClient.Connect(_knownVmIp, i);
                        Console.WriteLine($"Port {i} open");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Port closed");
                    }
                }
            }
        }

        /*
         SELECT * 
FROM `gwc-sandbox.firewall.compute_googleapis_com_firewall_20210802` ORDER BY timestamp desc 
LIMIT 1000
         */
    }
}
