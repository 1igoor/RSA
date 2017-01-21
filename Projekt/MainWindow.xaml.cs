using Projekt.Exceptions;
using Projekt.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Projekt
{

    enum ConnectionStatus
    {
        connected,
        disconnected,
        listening
    }


    public partial class MainWindow : Window
    {

        ViewManager viewManager;

        IPAddress MyIP, TargetIP;
        int MyPort, TargetPort;

        RSA rsa;      

        TcpClient targetPC;

        Thread listenThread;
        bool stopListen = false;

        ConnectionStatus connectionStatus;

        BinaryReader reader;

        Thread readFromTargetPCThread;


        public MainWindow()
        {
            try
            {
                InitializeComponent();

                viewManager = new ViewManager();
                AddElementsToViewManager();

                rsa = new RSA();

                targetPC = new TcpClient();

                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                List<string> cbMyIPItemsList = new List<string>();
                cbMyIPItemsList.Add("127.0.0.1");
                foreach (IPAddress ip in host.AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        cbMyIPItemsList.Add(ip.ToString());
                cbMyIP.ItemsSource = cbMyIPItemsList;
                cbMyIP.SelectedIndex = 0;

                connectionStatus = ConnectionStatus.disconnected;
                viewManager.SetView(connectionStatus);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\nAplikacja zostanie zamknięta.", "Błąd podczas próby uruchomienia aplikacji");
                Application.Current.Shutdown();
            }
        }


        private void AddElementsToViewManager()
        {
            viewManager.cbMyIP = cbMyIP;
            viewManager.tbMyPort = tbMyPort;
            viewManager.tbTargetIP = tbTargetIP;
            viewManager.tbTargetPort = tbTargetPort;
            viewManager.lConnectionStatus = lConnectionStatus;
            viewManager.btConnectionInfo = btConnectionInfo;
            viewManager.btConnect = btConnect;
            viewManager.btDisconect = btDisconect;
            viewManager.tbConversation = tbConversation;
            viewManager.tbMessage = tbMessage;
            viewManager.btSendMessage = btSendMessage;
        }


        private void cbMyIP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbMyIP.SelectedIndex == 0)
            {
                tbMyPort.Text = "80";
                tbTargetIP.Text = "127.0.0.1";
                tbTargetPort.Text = "81";
            }
            else
            {
                tbMyPort.Text = "80";
                tbTargetIP.Text = "";
                tbTargetPort.Text = "";
            }
        }


        private void btConnectionInfo_Click(object sender, RoutedEventArgs e)
        {
            if (connectionStatus == ConnectionStatus.connected)
                MessageBox.Show("Mój klucz publiczny: {e: " + rsa.myPublicKey.e + ", n: " + rsa.myPublicKey.n + "}\n" +
                                "Mój klucz prywatny: {d: " + rsa.myPrivateKey.d + ", n: " + rsa.myPrivateKey.n + "}\n" +
                                "Docelowy klucz publiczny: {e: " + rsa.targetPublicKey.e + ", n: " + rsa.targetPublicKey.n + "}\n");
            else if ((connectionStatus == ConnectionStatus.disconnected) || (connectionStatus == ConnectionStatus.listening))
                MessageBox.Show("Mój klucz publiczny: {e: " + rsa.myPublicKey.e + ", n: " + rsa.myPublicKey.n + "}\n" +
                                "Mój klucz prywatny: {d: " + rsa.myPrivateKey.d + ", n: " + rsa.myPrivateKey.n + "}\n");
        }

        private void btConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MyIP = ConnectionPropertiesMethods.ReturnIPAddress(cbMyIP);
                MyPort = ConnectionPropertiesMethods.ReturnPort(tbMyPort, 1);
                TargetIP = ConnectionPropertiesMethods.ReturnIPAddress(tbTargetIP);
                TargetPort = ConnectionPropertiesMethods.ReturnPort(tbTargetPort, 2);
            }
            catch (ConnectionPropertyException cpe)
            {
                MessageBox.Show(cpe.Message, "Błąd");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd");
                return;
            }

            if ((MyIP.ToString() == TargetIP.ToString()) && (MyPort == TargetPort))
            {
                MessageBox.Show("Wartości Port nie mogą być takie same", "Błąd");
                return;
            }

            // sprawdzamy czy docelowy komputer nie otworzył wcześniej połączenia (próbujemy się połączyć)
            bool targetPCOpenedConnection = false;
            try
            {
                targetPC = new TcpClient();

                var result = targetPC.BeginConnect(TargetIP, TargetPort, null, null);
                result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                if (targetPC.Connected)
                {
                    targetPC.EndConnect(result);
                    targetPCOpenedConnection = true;                    
                    BinaryWriter writer = new BinaryWriter(targetPC.GetStream());
                    writer.Write(rsa.MakeMyPublicKeyString());
                    reader = new BinaryReader(targetPC.GetStream());
                    rsa.ReadTargetPublicKeyString(reader.ReadString());

                    connectionStatus = ConnectionStatus.connected;
                    viewManager.SetView(connectionStatus);
                    readFromTargetPCThread = new Thread(ReadFromTargetPC);
                    readFromTargetPCThread.IsBackground = true;
                    readFromTargetPCThread.Start();
                }
            }
            catch (Exception ex)
            {
                targetPC.Close();
                connectionStatus = ConnectionStatus.disconnected;
                viewManager.SetView(connectionStatus);
                MessageBox.Show(ex.Message, "Błąd podczas próby nawiązania połączenia");
                return;
            }

            if (targetPCOpenedConnection == false) // jeśli nie udało się podłczyć do istniejącego połączenia to sami otwieramy połączenie
            {
                try
                {
                    targetPC = new TcpClient();

                    listenThread = new Thread(Listen);
                    listenThread.IsBackground = true;
                    listenThread.Start();
                    connectionStatus = ConnectionStatus.listening;
                    viewManager.SetView(connectionStatus);
                }
                catch (Exception ex)
                {
                    targetPC.Close();
                    connectionStatus = ConnectionStatus.disconnected;
                    viewManager.SetView(connectionStatus);
                    MessageBox.Show(ex.Message, "Błąd podczas próby nawiązania połączenia");
                    return;
                }
            }
        }


        private void Listen()
        {
            try
            {
                TcpListener listener = new TcpListener(MyIP, MyPort);
                listener.Start();

                while (stopListen == false)
                {
                    if (!listener.Pending()) // jeśli nie ma oczekujących połączeń
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    targetPC = listener.AcceptTcpClient(); 
                    reader = new BinaryReader(targetPC.GetStream());
                    rsa.ReadTargetPublicKeyString(reader.ReadString());
                    BinaryWriter writer = new BinaryWriter(targetPC.GetStream());
                    writer.Write(rsa.MakeMyPublicKeyString());

                    connectionStatus = ConnectionStatus.connected;
                    viewManager.SetViewFromOtherThread(connectionStatus);
                    readFromTargetPCThread = new Thread(ReadFromTargetPC);
                    readFromTargetPCThread.IsBackground = true;
                    readFromTargetPCThread.Start();
                    break;
                }

                listener.Stop();                
            }
            catch (Exception ex)
            {
                targetPC.Close();
                connectionStatus = ConnectionStatus.disconnected;
                viewManager.SetViewFromOtherThread(connectionStatus);
                MessageBox.Show(ex.Message, "Błąd podczas próby nawiązania połączenia");
                return;
            }
        }


        private void ReadFromTargetPC()
        {
            while(true)
            {
                try
                {
                    reader = new BinaryReader(targetPC.GetStream());
                    string text = reader.ReadString();
                    string decodedMessage = rsa.DecodeMessage(text);
                    tbConversation.Dispatcher.BeginInvoke((Action)(() => tbConversation.AppendText("odebrano (zakodowana) : " + text + "\n")));
                    tbConversation.Dispatcher.BeginInvoke((Action)(() => tbConversation.AppendText("odebrano wiadomość: " + decodedMessage + "\n")));
                    tbConversation.Dispatcher.BeginInvoke((Action)(() => tbConversation.ScrollToEnd()));
                }
                catch (Exception ex)
                {
                    connectionStatus = ConnectionStatus.disconnected;
                    viewManager.SetViewFromOtherThread(connectionStatus);
                    break;
                }
            }
        }


        private void btDisconect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (connectionStatus == ConnectionStatus.connected)
                {
                    targetPC.Close();
                    connectionStatus = ConnectionStatus.disconnected;
                    viewManager.SetView(connectionStatus);
                }
                else if (connectionStatus == ConnectionStatus.listening)
                {
                    stopListen = true;
                    listenThread.Join();
                    stopListen = false;
                    connectionStatus = ConnectionStatus.disconnected;
                    viewManager.SetView(connectionStatus);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd podczas próby zakończenia połączenia");
                return;
            }
        }


        private void btSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string encodedMessage = rsa.EnodeMessage(tbMessage.Text);
            BinaryWriter writer = new BinaryWriter(targetPC.GetStream());
            writer.Write(encodedMessage);
            tbConversation.AppendText("wyslano wiadomosc: " + tbMessage.Text + "\n");
            tbConversation.AppendText("wyslano (zakodowana): " + encodedMessage + "\n");
            tbConversation.ScrollToEnd();
            tbMessage.Text = "";
            tbMessage.Focus();           
        }


    }
}
