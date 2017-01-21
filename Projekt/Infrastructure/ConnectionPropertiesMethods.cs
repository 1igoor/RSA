using Projekt.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Projekt.Infrastructure
{
    class ConnectionPropertiesMethods
    {

        public static IPAddress ReturnIPAddress(ComboBox cbMyIP)
        {
            IPAddress ipAddressToReturn = null;

            try
            {
                ipAddressToReturn = IPAddress.Parse(cbMyIP.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                throw new ConnectionPropertyException("Wystąpił błąd podczas pobierania adresu IP z listy rozwijanej w \"Mój komputer\":" + Environment.NewLine + ex.Message);
            }

            return ipAddressToReturn;
        }


        public static IPAddress ReturnIPAddress(TextBox tbTargetIP)
        {
            IPAddress ipAddressToReturn = null;

            if (String.IsNullOrEmpty(tbTargetIP.Text) || String.IsNullOrWhiteSpace(tbTargetIP.Text))
                throw new ConnectionPropertyException("Parametr IP dla \"Komputer docelowy\" nie może być pusty.");

            try
            {
                ipAddressToReturn = IPAddress.Parse(tbTargetIP.Text);
            }
            catch (Exception ex)
            {
                throw new ConnectionPropertyException("Wystąpił błąd podczas pobierania adresu IP dla \"Komputer docelowy\":" + Environment.NewLine + ex.Message);
            }

            return ipAddressToReturn;
        }


        public static int ReturnPort(TextBox tbWithPort, int type)
        {
            int portToReturn = 0;

            if (String.IsNullOrEmpty(tbWithPort.Text) || String.IsNullOrWhiteSpace(tbWithPort.Text))
            {
                if (type == 1)
                    throw new ConnectionPropertyException("Parametr Port dla \"Mój komputer\" nie może być pusty.");
                else if (type == 2)
                    throw new ConnectionPropertyException("Parametr Port dla \"Komputer docelowy\" nie może być pusty.");
            }

            try
            {
                portToReturn = int.Parse(tbWithPort.Text);
            }
            catch (Exception ex)
            {
                if (type == 1)
                    throw new ConnectionPropertyException("Wystąpił błąd podczas pobierania portu dla \"Mój komputer\":" + Environment.NewLine + ex.Message);
                else if (type == 2)
                    throw new ConnectionPropertyException("Wystąpił błąd podczas pobierania portu dla \"Komputer docelowy\":" + Environment.NewLine + ex.Message);
            }

            return portToReturn;

        }
    }
}
