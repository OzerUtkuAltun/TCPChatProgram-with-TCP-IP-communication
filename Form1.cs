using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;



namespace TCPChatProgram
{
    public partial class Form1 : Form
    {
        private static TextBox _newText;
        private static ListBox _results;
        private static Socket _client;
        private static readonly byte[] Data = new byte[1024];


        public Form1()
        {
            InitializeComponent();
            Text = "TCP Chat Program";
            Size = new Size(400, 380);

            Label label1 = new Label();
            label1.Parent = this;
            label1.Text = "Enter text string:";
            label1.AutoSize = true;
            label1.Location = new Point(10, 30);

            _newText = new TextBox();
            _newText.Parent = this;
            _newText.Size = new Size(200, 2 * Font.Height);
            _newText.Location = new Point(10, 55);

            _results = new ListBox();
            _results.Parent = this;
            _results.Location = new Point(10, 85);
            _results.Size = new Size(360, 18 * Font.Height);

            Button sendit = new Button();
            sendit.Parent = this;
            sendit.Text = "Send";
            sendit.Location = new Point(220, 52);
            sendit.Size = new Size(5 * Font.Height, 2 * Font.Height);
            sendit.Click += new EventHandler(ButtonSendOnClick);

            Button connect = new Button();
            connect.Parent = this;
            connect.Text = "Connect";
            connect.Location = new Point(295, 20);
            connect.Size = new Size(6 * Font.Height, 2 * Font.Height);
            connect.Click += new EventHandler(ButtonConnectOnClick);

            Button listen = new Button();
            listen.Parent = this;
            listen.Text = "Listen";
            listen.Location = new Point(295, 52);
            listen.Size = new Size(6 * Font.Height, 2 * Font.Height);
            listen.Click += new EventHandler(ButtonListenOnClick);

        }

        void ButtonListenOnClick(object obj, EventArgs ea)
        {
            _results.Items.Add("Listening for a _client...");
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 9050);
            newsock.Bind(iep);
            newsock.Listen(5);
            newsock.BeginAccept(new AsyncCallback(AcceptConn), newsock);
        }

        void ButtonConnectOnClick(object obj, EventArgs ea)
        {
            _results.Items.Add("Connecting...");
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
            _client.BeginConnect(iep, new AsyncCallback(Connected), _client);
        }

        void ButtonSendOnClick(object obj, EventArgs ea)
        {
            byte[] message = Encoding.ASCII.GetBytes(_newText.Text);
            _newText.Clear();
            _client.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendData), _client);
        }

        void AcceptConn(IAsyncResult iar)
        {
            Socket oldserver = (Socket)iar.AsyncState;
            _client = oldserver.EndAccept(iar);
            _results.Items.Add("Connection from: " + _client.RemoteEndPoint.ToString());
            Thread receiver = new Thread(new ThreadStart(ReceiveData));
            receiver.Start();
        }

        void Connected(IAsyncResult iar)
        {
            try
            {
                _client.EndConnect(iar);
                _results.Items.Add("Connected to: " + _client.RemoteEndPoint.ToString());
                Thread receiver = new Thread(new ThreadStart(ReceiveData));
                receiver.Start();

            }
            catch (SocketException)
            {
                _results.Items.Add("Error connecting");
            }
        }

        void SendData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
        }

        void ReceiveData()
        {
            int recv;
            string stringData;
            while (true)
            {
                recv = _client.Receive(Data);
                stringData = Encoding.ASCII.GetString(Data, 0, recv);
                if (stringData == "bye")
                    break;
                _results.Items.Add(stringData);
            }
            stringData = "bye";
            byte[] message = Encoding.ASCII.GetBytes(stringData);
            _client.Send(message);
            _client.Close();
            _results.Items.Add("Connection stopped");
            return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}