using MCTG.Dal;
using MCTG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.PresentationLayer
{
    class Program
    {
        static void main(string[] args) 
        {
            TcpListener server = new TcpListener(IPAddress.Any, 8080);
            server.Start();

            var userRepository = new UserRepository();
            var authService = new AuthService(userRepository);
            var router = new Router(authService);

            Console.WriteLine("Server is running...");

            while (true)
            {
                var client = server.AcceptTcpClient();
                router.RouteRequest(client);
                client.Close();
            }
        }
    }
}
