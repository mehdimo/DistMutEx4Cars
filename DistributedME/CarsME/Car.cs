using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ClientServer
{
    public delegate void ProcessMsg(string s);

    public class Car : Button
    {
        string ipAddress = "127.0.0.1";

        public int Id { get; private set; }
        public Car(int id)
        {
            this.Id = id;
            this.BackColor = System.Drawing.Color.Blue;
            requestQueue = new List<CarMessage>();
            clients = new List<Client>();
            deferedRequests = new List<DifferedMessage>();
            this.ForeColor = Color.Gold;
            this.Font = new Font(this.Font, FontStyle.Bold);
        }

        // The request set for each car
        int[,] ids = new int[4,3] { { 2, 3, 4 }, { 1, 3, 4 }, { 1, 2, 4 }, { 1, 2, 3 } };

        public Road InRoad { set; get; }
        public int Gear = 5;
        int step = 5;
        bool moving = false;

        bool onTheBridge = false;
        List<DifferedMessage> deferedRequests; // To do what needed for requests received during CS execution

        private List<int> RequestSet
        {
            get
            {
                List<int> requestSet = new List<int>();
                for (int i = 0; i < 3; i++)
                    requestSet.Add(ids[Id - 1, i]);

                return requestSet;
            }
        }

        List<CarMessage> requestQueue;
        int repliesNo = 0;

        Server server { get; set; }
        List<Client> clients;

        public delegate void RepaintRoad();
        public RepaintRoad ReshapeRoad;

        public WriteMsg OnWriteForm2;


        public event WriteMsg OnWriteForm;

        public void StartServer()
        {
            Thread threadServer = new Thread(new ThreadStart(startServer));
            threadServer.IsBackground = true;
            threadServer.Start();
        }

        public void RunClients()
        {
            for (int i = 0; i < 3; i++)
            {
                Thread threadClient = new Thread(new ParameterizedThreadStart(startClient));
                threadClient.IsBackground = true;
                threadClient.Start(i);
            }
        }

        private void startClient(object obj)
        {
            startClient1((int)obj);
        }

        private void startClient1(int id2)
        {
            int id3 = GetPeerId(Id - 1, id2);
            Client client = new Client(ipAddress, id3);
            clients.Add(client);
            client.Id = Id;
            if (OnWriteForm != null)
                client.OnWrite += client_OnWrite;
            client.Connect();
        }

        void client_OnWrite(string s)
        {
            if (OnWriteForm != null)
                OnWriteForm(s);
        }

        private void startServer()
        {
            server = new Server(ipAddress, Id);
            if (OnWriteForm != null)
                server.OnWrite += server_OnWrite;
            server.OnProcessMsg = new ProcessMsg(ProcessCarMessage);
            server.Start();
        }

        void server_OnWrite(string s)
        {
            if (OnWriteForm != null)
                OnWriteForm(s);
        }

        private void ProcessCarMessage(string str)
        {
            CarMessage msg = CarMessage.Create(str);
            if (msg.Type == MessageType.Request)
            {
                requestQueue.Add(msg);
                CarMessage reply = GenerateMessage(MessageType.Reply);
                
                if (!onTheBridge) // check not in CS
                {
                    SendMessage(reply, msg.Id);
                }
                else
                {
                    deferedRequests.Add(new DifferedMessage(reply, msg.Id));
                }
            }
            else if (msg.Type == MessageType.Reply)
            {
                repliesNo++;
            }
            else if (msg.Type == MessageType.Release)
            {
                CarMessage m = requestQueue.Find(t => t.Id == msg.Id);
                if(m!= null)
                    requestQueue.Remove(m);
            }
        }

        private void sendBufferFromServer()
        {
            byte[] buffer = new byte[8192];
            int currentPosition = 0;
            string s = string.Format("SERVER {0} REPLY", Id);
            int bytesWriten = 0;
            while (bytesWriten < s.Length)
            {
                bytesWriten = Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, currentPosition);
                currentPosition += bytesWriten;
            }
            server.SendData(ref buffer, currentPosition);
            currentPosition = 0;
        }

        private Client GetClientOf(int serverPort)
        {
            foreach (Client cl in clients)
                if (cl.Port == serverPort)
                    return cl;
            return null;
        }

        public void SendMessage(CarMessage message, int serverPort)
        {
            try
            {
                Client client = GetClientOf(serverPort);
                if (client != null)
                    client.SendData(message.ToString());
            }
            catch
            {
                //WriteMessage(String.Format("Exception sending {0} msg from {1} to {2}", type, id, serverPort));
            }
        }

        private CarMessage GenerateMessage(char type)
        {
            DateTime now = DateTime.Now;
            long span = now.Ticks;
            CarMessage message = new CarMessage(type, this.Id, span);
            return message;
        }

        public void SendFromServer()
        {
            sendBufferFromServer();
        }

        private int GetPeerId(int currentId, int pearId)
        {
            return ids[currentId, pearId];
        }

        public void TakeRoad(Road road)
        {
            this.InRoad = road;
            if (road.IsBridge)
            {
                if (this.Location == road.End)
                {
                    road.ChangeDirection();
                    road.ChangedDirection = true;
                }
            }
            this.Location = road.Start;
            moving = true;
            BringToFront();
        }

        private void WantPermissionToCS()
        {
            CarMessage request = GenerateMessage(MessageType.Request);
            requestQueue.Add(request);
            foreach (int carId in RequestSet)
                SendMessage(request, carId);
        }

        private void AnnounceRelease()
        {
            CarMessage releaseMsg = GenerateMessage(MessageType.Release);
            foreach (int carId in RequestSet)
                SendMessage(releaseMsg, carId);
        }

        public void Move1()
        {
            int x = this.Location.X;
            int y = this.Location.Y;

            int distX = InRoad.End.X;
            int distY = InRoad.End.Y;
            int stepX = step;
            int stepY = step;
            if (distX < x)
                stepX *= -1;
            if (distY < y)
                stepY *= -1;

            int tX = distX - x;
            int tY = distY - y;

            if (tX != 0)
                x += stepX;
            if (tY != 0)
                y += stepY;

            this.Location = new Point(x, y);
            if (tX == 0 && tY == 0)
                moving = false;

            if (ReshapeRoad != null)
                ReshapeRoad();
        }

        public void Move()
        {
            if (InRoad != null)
            {
                if (InRoad.IsBridge)
                {
                    WantPermissionToCS();
                    while (true)
                    {
                        if (repliesNo == RequestSet.Count && requestQueue.Count > 0 && requestQueue[0].Id == this.Id)
                        {
                            repliesNo = 0;
                            onTheBridge = true;
                            break;
                        }
                    }
                }
                while (moving)
                {
                    AdjustSpeed();
                    Move1();
                }
                if (InRoad.IsBridge)
                {
                    if (InRoad.ChangedDirection)
                    {
                        InRoad.ChangeDirection();
                        InRoad.ChangedDirection = false;
                    }
                    onTheBridge = false;
                    foreach (DifferedMessage msg in deferedRequests)
                    {
                        SendMessage(msg.Message, msg.Receiver);
                    }
                    deferedRequests.Clear();

                    AnnounceRelease();
                    
                    CarMessage ownRequest = requestQueue.Find(m => m.Id == this.Id);
                    if (ownRequest != null)
                        requestQueue.Remove(ownRequest);
                }
            }
        }

        private void AdjustSpeed()
        {
            int t = 200 / Gear;
            Thread.Sleep(t);
        }

        public void Tour(params Road[] roads)
        {
            foreach (Road road in roads)
            {
                TakeRoad(road);
                Move();
            }
        }

        class DifferedMessage
        {
            public DifferedMessage(CarMessage m, int s)
            {
                this.Message = m;
                this.Receiver = s;
            }
            public CarMessage Message;
            public int Receiver;
        }
    }
}
