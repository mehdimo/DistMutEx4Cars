using System;
using System.Collections.Generic;
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
    public partial class MainForm : Form
    {
        List<Car> cars;
        List<Thread> carThreads;
        Road r1, r2, r3, r4, r5, r6, r_bridge;

        public MainForm()
        {
            InitializeComponent();
            Init();
            MakeCars();
            carThreads = new List<Thread>();
        }

        private void MakeCars()
        {
            cars = new List<Car>();
            for (int i = 1; i <= 4; i++)
            {
                string filename = "car_Blue.jpg";
                if (i % 2 == 0)
                    filename = "car_Red.jpg";
                Car car = new Car(i);
                car.Name = "Car" + i;
                car.Size = new System.Drawing.Size(60, 40);
                car.Text = car.Name;
                car.Location = new System.Drawing.Point(10 + (i * 35), 17 + (i % 2 * 50));
                TakePosition(car);
                car.BackgroundImage = Image.FromFile(filename);
                car.BackgroundImageLayout = ImageLayout.Stretch;
                car.Gear = int.Parse(GetSpeed(i));
                car.OnWriteForm += car_OnWriteForm;
                this.Controls.Add(car);
                cars.Add(car);
            }
        }

        private void StartCars()
        {
            foreach (Car car in cars)
            {
                car.Enabled = true;
                Thread thread = new Thread(new ParameterizedThreadStart(car_Click));
                carThreads.Add(thread);
                thread.Start(car);
            }
        }

        void car_OnWriteForm(string s)
        {
            WriteOnBoard(s);
        }

        private string GetSpeed(int id)
        {
            switch (id)
            {
                case 1:
                    return comboBox1.Text;
                case 2:
                    return comboBox2.Text;
                case 3:
                    return comboBox3.Text;
                case 4:
                    return comboBox4.Text;
                default:
                    return comboBox1.Text;
            }
        }

        private void GetSpeeds()
        {
            foreach(Car car in cars)
                car.Gear = int.Parse(GetSpeed(car.Id));
        }

        void car_Click(object ca)
        {
            Car car = (Car)ca;
            switch (car.Id)
            {
                case 1:
                    car.Tour(r1, r2, r3, r_bridge, r4, r5, r6);
                    break;
                case 2:
                    car.Tour(r2, r3, r_bridge, r4, r5, r6, r_bridge, r1);
                    break;
                case 3:
                    car.Tour(r5, r6, r_bridge, r1, r2, r3, r_bridge, r4);
                    break;
                case 4:
                    car.Tour(r6, r_bridge, r1, r2, r3, r_bridge, r4, r5);
                    break;
            }
        }

        void TakePosition(Car car)
        {
            switch (car.Id)
            {
                case 1:
                    car.Location = r1.Start;
                    break;
                case 2:
                    car.Location = r2.Start;
                    break;
                case 3:
                    car.Location = r5.Start;
                    break;
                case 4:
                    car.Location = r6.Start;
                    break;
            }
        }

        public void Run()
        {
            foreach(Car car in cars)
                car.StartServer();

            foreach (Car car in cars)
                car.RunClients();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetSpeeds();
            Run();
            StartCars();
        }

        private void WriteOnBoard(string str)
        {
            textBox1.Text = str + Environment.NewLine + textBox1.Text;
        }

        private void Init()
        {
            r1 = new Road();
            r2 = new Road();
            r3 = new Road();
            r4 = new Road();
            r5 = new Road();
            r6 = new Road();
            r_bridge = new Road();
            r_bridge.IsBridge = true;

            r1.Form = this;
            r2.Form = r3.Form = r4.Form = r5.Form = r6.Form = r_bridge.Form = r1.Form;

            r1.Start = new Point(200, 200);
            r1.End = new Point(50, 50);

            r2.Start = r1.End;
            r2.End = new Point(50, 350);

            r3.Start = r2.End;
            r3.End = r1.Start;

            r4.Start = new Point(600, 200);
            r4.End = new Point(750, 350);

            r5.Start = r4.End;
            r5.End = new Point(750, 50);

            r6.Start = r5.End;
            r6.End = r4.Start;

            r_bridge.Start = r1.Start;
            r_bridge.End = r4.Start;
        }

        private void CreateRoads()
        {
            //this.Invoke((MethodInvoker)delegate
            //{
                r1.Create();
                r2.Create();
                r3.Create();

                r4.Create();
                r5.Create();
                r6.Create();

                r_bridge.Create();

                //this.Refresh();
            //});
        }

        public void Form1_Paint(object sender, PaintEventArgs e)
        {
            CreateRoads();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MainForm.CheckForIllegalCrossThreadCalls = false;
        }
    }
}
