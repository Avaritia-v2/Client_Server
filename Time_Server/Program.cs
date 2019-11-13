using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private static byte[] buffer = new byte[1234]; 
        private static List<Socket> clietnSockets = new List<Socket>(); 
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args)
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadKey();
        }
        private static void SetupServer() 
        {
            Message("Setting up server");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any,8888)); 
            serverSocket.Listen(8); 
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),null);
        }
        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = serverSocket.EndAccept(ar);
            clietnSockets.Add(socket);
            Message("Клиент подключен");
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                int received = socket.EndReceive(ar);
                byte[] dataBuf = new byte[received];
                Array.Copy(buffer, dataBuf, received);
                string text = Encoding.UTF8.GetString(dataBuf);
                string accepted = "";

                Message("Полученное сообщение: " + text);

                Thread.Sleep(5500);
                for (int i = 0; i<text.Length; i++)
                {
                    if (text[i] == 'C')
                    {
                        accepted = text.Remove(i - 1);
                        break;
                    }
                }
                string toSend = "Умножение на 1,23: "+Convert.ToDouble(accepted)*1.23; //Я сделал увиличение значения в 1,23 раза, потому что не знал, что делать

                byte[] data = Encoding.UTF8.GetBytes(toSend); 
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket); 
            }
            catch (SocketException)
            {
                Message("Клиент отключился");
            }
            
        }
        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }
        private static void Message(string str)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff ") + str);
        }
    }
}
