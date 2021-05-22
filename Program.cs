using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LumiSoft.Net.STUN.Client;

namespace CopyCopy
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Press 1 to send, 2 to listen local, 3 to listen stun");
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.D1) SendText();
            if (key.Key == ConsoleKey.D2) ReceiveLocal();
            if (key.Key == ConsoleKey.D3) ReceiveSTUN();

        }

        static void SendText()
        {
            Console.WriteLine("IP:Port");
            var endPoint = IPEndPoint.Parse(Console.ReadLine());
            byte[] byData = System.Text.Encoding.ASCII.GetBytes("TEST1234");

            /*Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soc.Connect(endPoint);
            soc.Send(byData);
            soc.Close();*/

            UdpClient client = new UdpClient();
            client.Connect(endPoint);
            client.Send(byData, byData.Length);
        }

        static void ReceiveLocal()
        {
            Console.WriteLine("Listening on 127.0.0.1:4711");
            var endPoint = new IPEndPoint(IPAddress.Loopback, 4711);

            /*Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            soc.Bind(endPoint); */

            Listen(endPoint);
        }
   
        static void ReceiveSTUN()
        {
            var strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            var localIPAddress = ipEntry.AddressList.First();
            Console.WriteLine($"Local IP: {localIPAddress}");

            var localEndPoint = new IPEndPoint(localIPAddress, 4711);
            
            // Create new socket for STUN client.
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(localEndPoint);
            
            Console.WriteLine("Querying STUN server...");
            STUN_Result result = STUN_Client.Query("stun.sipgate.net", 3478, socket);

            if (result.NetType == STUN_NetType.UdpBlocked)
            {
                Console.WriteLine("Error: UDP blocked or bad STUN server");
            }
            else
            {
                Console.WriteLine($"Success: Public Endpoint is {result.PublicEndPoint.ToString()} Type: {result.NetType}");
     
                Listen(new IPEndPoint(localIPAddress, result.PublicEndPoint.Port));
            }
        }


        private static void Listen(IPEndPoint endPoint)
        {
            Console.WriteLine($"Listening on {endPoint.ToString()}");

            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(endPoint);

            IPEndPoint remoteEndpoint = null;
            var bytes = client.Receive(ref remoteEndpoint);
            var text = System.Text.Encoding.ASCII.GetString(bytes);
            Console.WriteLine($"Received {text}");
        }
    }
}
