using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace ClientServer
{
   public class Server
   {
      protected Socket _ConnectionSocket { get; set; }
      protected IPEndPoint _ServerEndPoint { get; set; }
      protected HashSet<Socket> _ConnectedClientSockets { get; set; }

      public IPAddress IPAddress { get; protected set; }
      public int Port { get; protected set; }
      public bool Connected { get; protected set; }
      public bool Shutdown { get; protected set; }

      public event WriteMsg OnWrite;
      public ProcessMsg OnProcessMsg;

      public Server(IPAddress ipAddress, int port) {
         IPAddress = ipAddress;
         Port = port;
         _ServerEndPoint = new IPEndPoint(IPAddress, Port);
         _ConnectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
         Shutdown = true;
         _ConnectedClientSockets = new HashSet<Socket>();
      }

      public Server(string ipAddress, int port)
         : this(IPAddress.Parse(ipAddress), port) {
      }

      public virtual void Start() {
          if(OnWrite!= null)
            OnWrite(string.Format("[SERVER {0}] - Server Starting", Port));
         Shutdown = false;
         _ConnectionSocket.Bind(_ServerEndPoint);
         _ConnectionSocket.Listen(1);
         AcceptConnections();
      }

      public virtual void SendData(ref byte[] dataBuffer, int currentPosition) {
         foreach (Socket s in _ConnectedClientSockets)
         {
            SendingData(new SocketContainer(s, ref dataBuffer), currentPosition);
         }
      }

      private void SendingData(SocketContainer container, int currentPosition) {

         IAsyncResult result =
            container
            .ConnectionSocket
            .BeginSend(container.Buffer, 0, currentPosition + 1, SocketFlags.None, new AsyncCallback(OnSendingCallback), container);
      }

      private void OnSendingCallback(IAsyncResult result) 
      {
         SocketContainer container = (SocketContainer)result.AsyncState;
         int bytesSent = 0;
         try
         {
            bytesSent = container.ConnectionSocket.EndSend(result);
            if (OnWrite != null)
                OnWrite(string.Format("[SERVER {1}] - Sent {0} bytes\n", bytesSent, Port));
         }
         catch (SocketException ex)
         {
             if (OnWrite != null)
                OnWrite(string.Format("[SERVER] - {0}", ex.Message));
         }
      }
      protected virtual void AcceptConnections() 
      {
          if(OnWrite != null)
            OnWrite(string.Format( "[SERVER {0}] - Listening for connections", Port));
         IAsyncResult result = _ConnectionSocket.BeginAccept(new AsyncCallback(OnAcceptCallback), null);
      }

      protected virtual void OnAcceptCallback(IAsyncResult result) 
      {
         Socket soc = null;
         try
         {
            soc = _ConnectionSocket.EndAccept(result);
            _ConnectedClientSockets.Add(soc);
            if (OnWrite != null)
                OnWrite(string.Format("[SERVER {1}] - Connection from {0}", soc.RemoteEndPoint, Port));
         }
         catch (SocketException ex)
         {
             if (OnWrite != null)
                OnWrite(string.Format("[SERVER] - {0}", ex.Message));
         }
         RecieveData(soc);
         AcceptConnections();

      }

      protected virtual void RecieveData(Socket connectedSocket) 
      {
         byte[] buffer = new byte[8192];
         SocketContainer container = new SocketContainer(connectedSocket, buffer);
         IAsyncResult result = connectedSocket.BeginReceive(container.Buffer, 0, container.Buffer.Length, SocketFlags.None, new AsyncCallback(OnRecievDataCallback), container);
      }

      protected virtual void OnRecievDataCallback(IAsyncResult result) 
      {
         SocketContainer container = (SocketContainer)result.AsyncState;
         int bytesRecieved = 0;
         try
         {
            bytesRecieved = container.ConnectionSocket.EndReceive(result);
         }
         catch (SocketException ex)
         {
             if (OnWrite != null)
                OnWrite(string.Format("[SERVER] - {0}", ex.Message));
         }

         string buff = Encoding.UTF8.GetString(container.Buffer, 0, bytesRecieved);
         string cont = string.Format("[SERVER {0}] - Received message: ", Port) + buff;
         ProcessMsg(buff);
         OnWrite(cont);

         RecieveData(container.ConnectionSocket);
      }

      private void ProcessMsg(string msg)
      {
          if (OnProcessMsg != null)
              OnProcessMsg(msg);
      }
   }
}
