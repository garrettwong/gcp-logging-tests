﻿using gcp_logging_tests.API;
using gcp_logging_tests.Utilities;
using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace gcp_logging_tests.FirewallLogTests
{
    public class FirewallLog_Tests
    {
        private RandomGenerator _randomGenerator;
        private GCPLogQueryGenerator _gcpLogQueryGenerator;
        private string _knownVmIp;

        private const string PROJECT_ID = "gwc-sandbox";
        private const string ZONE = "us-central1-a";
        private const string INSTANCE = "apache";
        private const string DEFAULT_IP = "34.134.129.29";

        public FirewallLog_Tests()
        {
            Access.Initiailize();

            _randomGenerator = new RandomGenerator();
            _gcpLogQueryGenerator = new GCPLogQueryGenerator();

            // dynamically grab the IP
            var computeService = new Compute();
            var res = computeService.GetInstance(PROJECT_ID, ZONE, INSTANCE);
            try
            {
                _knownVmIp = res.NetworkInterfaces[0].AccessConfigs[0].NatIP;
            }
            catch (Exception ex)
            {
                _knownVmIp = DEFAULT_IP;
            }
        }

        private IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        [Fact]
        public void FirewallLog_TestGetRequest()
        {
            var ip = LocalIPAddress();
            Console.WriteLine(ip.ToString());

            var client = new RestClient($"http://{_knownVmIp}/");
            client.Timeout = 500;
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = client.Execute(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
        }

        [Fact]
        public void FirewallLog_TestOpenPorts()
        {
            for (var i = 78; i < 82; i++)
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

        SELECT * 
FROM `gwc-sandbox.vpc_flows.compute_googleapis_com_vpc_flows_2021*` 
WHERE jsonPayload.connection.src_ip = '107.139.105.69'
ORDER BY timestamp DESC


         */



        [Fact]
        public void PostContentToApache()
        {
            var client = new RestClient($"http://{_knownVmIp}/");
            var request = new RestRequest(Method.POST);
            var text = File.ReadAllText("fviz-package.json");
            request.AddParameter("undefined", text, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Assert.NotNull(response.Content);
        }
    }
}
