using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace ClientServer
{
   public delegate void WriteMsg(string s);
   public class Client
   {
      protected Socket _Socket { get; set; }
      protected IPEndPoint _ClientEndPoint { get; set; }
      public int Id { set; get; }

      public bool Shutdown { get; protected set; }
      public bool Connected { get; protected set; }
      public IPAddress IPAddress { get; protected set; }
      public int Port { get; protected set; }

      public event WriteMsg OnWrite;


      public Client(IPAddress address, int port) {
         IPAddress = address;
         Port = port;
         _ClientEndPoint = new IPEndPoint(address, port);
         _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
         Shutdown = true;
      }

      public Client(string address, int port)
         : this(IPAddress.Parse(address), port) {
      }

      public virtual void Connect() {
         Shutdown = false;
         StartConnecting(new ConnectionInfo(_Socket, _ClientEndPoint));
      }

      public virtual void SendData(string msg) {
         StartSending(_Socket, msg);
      }

      protected void StartConnecting(ConnectionInfo info) {
          if(OnWrite != null)
            OnWrite(string.Format("[CLIENT {1}] - Attempting to connect to {0}", info.RemoteEndPoint, Id));
         info.Socket.BeginConnect(info.RemoteEndPoint, new AsyncCallback(_OnConnectingCallback), info);
      }

      protected void _OnConnectingCallback(IAsyncResult result) {
         ConnectionInfo info = (ConnectionInfo)result.AsyncState;
         try
         {
            info.Socket.EndConnect(result);
            if (OnWrite != null)
                OnWrite(string.Format("[CLIENT {1}] - Connection established to {0}", info.RemoteEndPoint, Id));
            StartRecieving(info.Socket);
         }
         catch (SocketException ex)
         {
             if(OnWrite != null)
            OnWrite(string.Format("[CLIENT {1}] - {0}", ex.Message, Id));
         }
         if (!info.Socket.Connected)
            StartConnecting(info);
      }

      private void StartRecieving(Socket socket) {
         byte[] buffer = new byte[8192];
         var container = new SocketContainer(socket, buffer);
         IAsyncResult result = socket.BeginReceive(buffer, 0, 8192, SocketFlags.None, new AsyncCallback(_OnRecievingCallback), container);
      }
      public  void _OnRecievingCallback(IAsyncResult result) {
         SocketContainer container = (SocketContainer)result.AsyncState;
         int bytesRecieved = 0;
         try
         {
            bytesRecieved = container.ConnectionSocket.EndReceive(result);
         }
         catch (SocketException ex)
         {
            OnWrite(string.Format("[CLIENT {1}] - {0}", ex.Message, Id));
         }
         OnWrite(string.Format("[SERVER] - {0}", Encoding.UTF8.GetString(container.Buffer, 0, bytesRecieved)));
         StartRecieving(container.ConnectionSocket);

      }

      protected  void StartSending(Socket socket, string msg) {
         byte[] buffer = Encoding.ASCII.GetBytes(msg);
         SocketContainer container = new SocketContainer(socket, buffer);
         socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(_OnSendingCallback), container);
      }

      protected  void _OnSendingCallback(IAsyncResult result) {
         SocketContainer container = (SocketContainer)result.AsyncState;
         int bytesSent = 0;
         try
         {
            bytesSent = container.ConnectionSocket.EndSend(result);
         }
         catch (SocketException ex)
         {
            //Console.WriteLine(string.Format("[CLIENT {1}] - {0}", ex.Message, Id));
         }
      }

      protected class ConnectionInfo
      {
         public virtual Socket Socket { get; protected set; }
         public virtual IPEndPoint RemoteEndPoint { get; protected set; }

         public ConnectionInfo(Socket socket, IPEndPoint endPoint) {
            Socket = socket;
            RemoteEndPoint = endPoint;
         }
      }
   }
}
