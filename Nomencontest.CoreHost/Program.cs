using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using Nomencontest.Base;
using Nomencontest.Core;

namespace Nomencontest.Core.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new Settings();
            var ip = settings.ServerIPAddress;

            var address = new Uri(ip + ":" + settings.GamePort + "/Nomencontest/");


            //WebRemote.WebRemote remoteHost = null;
            //try
            //{
            //    remoteHost = new WebRemote.WebRemote(settings.IPAddress, settings.RemoteReceiverPort, settings.SenderPort);
            //    Console.WriteLine("Web remote running on port " + settings.RemoteReceiverPort);
            //}
            //catch (Exception e)
            //{
            //    remoteHost?.Shutdown();
            //}


            using (ServiceHost host = new ServiceHost(typeof(Nomencontest.Core.Core), address))
            {
                NetHttpBinding binding = new NetHttpBinding();
                binding.Security.Mode = BasicHttpSecurityMode.None;
                binding.MaxBufferSize = 2147483647;
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                host.AddServiceEndpoint(typeof(ICore), binding, address);
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                host.Description.Behaviors.Add(smb);
                host.Open();
                Console.WriteLine("Service host running at " + ip + ":" + settings.GamePort);


                //ForturnaHub.SetupServer(settings.ServerIPAddress + ":" + settings.GamePort);
                Console.ReadLine();

                host.Close();
            }
        }
    }
}
