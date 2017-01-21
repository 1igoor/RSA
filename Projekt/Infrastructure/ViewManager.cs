using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Projekt.Infrastructure
{
    class ViewManager
    {
        public ComboBox cbMyIP;
        public TextBox tbMyPort, tbTargetIP, tbTargetPort, tbConversation, tbMessage;
        public Label lConnectionStatus;
        public Button btConnectionInfo, btConnect, btDisconect, btSendMessage;

        public void SetView(ConnectionStatus connectionStatus)
        {
            if (connectionStatus == ConnectionStatus.connected)
                SetViewForConnected();
            else if (connectionStatus == ConnectionStatus.disconnected)
                SetViewForDisconnected();
            else
                SetViewForListening();
        }


        private void SetViewForConnected()
        {
            cbMyIP.IsEnabled = false;
            tbMyPort.IsEnabled = false;
            tbTargetIP.IsEnabled = false;
            tbTargetPort.IsEnabled = false;
            lConnectionStatus.Foreground = new SolidColorBrush(Colors.Green);
            lConnectionStatus.Content = "Połączono";
            btConnectionInfo.IsEnabled = true;
            btConnect.IsEnabled = false;
            btDisconect.IsEnabled = true;
            tbConversation.IsEnabled = true;
            tbMessage.IsEnabled = true;
            btSendMessage.IsEnabled = true;
        }

        private void SetViewForDisconnected()
        {
            cbMyIP.IsEnabled = true;
            tbMyPort.IsEnabled = true;
            tbTargetIP.IsEnabled = true;
            tbTargetPort.IsEnabled = true;
            lConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
            lConnectionStatus.Content = "Rozłączono";
            btConnectionInfo.IsEnabled = true;
            btConnect.IsEnabled = true;
            btDisconect.IsEnabled = false;
            tbConversation.IsEnabled = false;
            tbMessage.IsEnabled = false;
            btSendMessage.IsEnabled = false;
        }


        private void SetViewForListening()
        {
            cbMyIP.IsEnabled = false;
            tbMyPort.IsEnabled = false;
            tbTargetIP.IsEnabled = false;
            tbTargetPort.IsEnabled = false;
            lConnectionStatus.Foreground = new SolidColorBrush(Colors.MediumBlue);
            lConnectionStatus.Content = "Oczekiwanie";
            btConnectionInfo.IsEnabled = true;
            btConnect.IsEnabled = false;
            btDisconect.IsEnabled = true;
            tbConversation.IsEnabled = false;
            tbMessage.IsEnabled = false;
            btSendMessage.IsEnabled = false;
        }


        public void SetViewFromOtherThread(ConnectionStatus connectionStatus)
        {
            if (connectionStatus == ConnectionStatus.connected)
                SetViewForConnectedFromOtherThread();
            else if (connectionStatus == ConnectionStatus.disconnected)
                SetViewForDisconnectedFromOtherThread();
            //else
            //    SetViewForListeningFromOtherThread();
        }


        private void SetViewForConnectedFromOtherThread()
        {
            cbMyIP.Dispatcher.BeginInvoke((Action)(() => cbMyIP.IsEnabled = false));
            tbMyPort.Dispatcher.BeginInvoke((Action)(() => tbMyPort.IsEnabled = false));
            tbTargetIP.Dispatcher.BeginInvoke((Action)(() => tbTargetIP.IsEnabled = false));
            tbTargetPort.Dispatcher.BeginInvoke((Action)(() => tbTargetPort.IsEnabled = false));
            lConnectionStatus.Dispatcher.BeginInvoke((Action)(() => lConnectionStatus.Foreground = new SolidColorBrush(Colors.Green)));
            lConnectionStatus.Dispatcher.BeginInvoke((Action)(() => lConnectionStatus.Content = "Połączono"));
            btConnectionInfo.Dispatcher.BeginInvoke((Action)(() => btConnectionInfo.IsEnabled = true));
            btConnect.Dispatcher.BeginInvoke((Action)(() => btConnect.IsEnabled = false));
            btDisconect.Dispatcher.BeginInvoke((Action)(() => btDisconect.IsEnabled = true));
            tbConversation.Dispatcher.BeginInvoke((Action)(() => tbConversation.IsEnabled = true));
            tbMessage.Dispatcher.BeginInvoke((Action)(() => tbMessage.IsEnabled = true));
            btSendMessage.Dispatcher.BeginInvoke((Action)(() => btSendMessage.IsEnabled = true));
        }


        private void SetViewForDisconnectedFromOtherThread()
        {
            cbMyIP.Dispatcher.BeginInvoke((Action)(() => cbMyIP.IsEnabled = true));
            tbMyPort.Dispatcher.BeginInvoke((Action)(() => tbMyPort.IsEnabled = true));
            tbTargetIP.Dispatcher.BeginInvoke((Action)(() => tbTargetIP.IsEnabled = true));
            tbTargetPort.Dispatcher.BeginInvoke((Action)(() => tbTargetPort.IsEnabled = true));
            lConnectionStatus.Dispatcher.BeginInvoke((Action)(() => lConnectionStatus.Foreground = new SolidColorBrush(Colors.Red)));
            lConnectionStatus.Dispatcher.BeginInvoke((Action)(() => lConnectionStatus.Content = "Rozłączono"));
            btConnectionInfo.Dispatcher.BeginInvoke((Action)(() => btConnectionInfo.IsEnabled = true));
            btConnect.Dispatcher.BeginInvoke((Action)(() => btConnect.IsEnabled = true));
            btDisconect.Dispatcher.BeginInvoke((Action)(() => btDisconect.IsEnabled = false));
            tbConversation.Dispatcher.BeginInvoke((Action)(() => tbConversation.IsEnabled = false));
            tbMessage.Dispatcher.BeginInvoke((Action)(() => tbMessage.IsEnabled = false));
            btSendMessage.Dispatcher.BeginInvoke((Action)(() => btSendMessage.IsEnabled = false));
        }
    }
}
