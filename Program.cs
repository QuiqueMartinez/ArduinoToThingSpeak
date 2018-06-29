using System;
using System.Net;
using System.IO.Ports;
using System.Management;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            ArduinoReader ar = new ArduinoReader();
            for (;;)
            {
                System.Threading.Thread.Sleep(5000);
            }
       }

        static void SaveMeasurementData(double data)
        {
            string apiKey;
          
            string server = "https://api.thingspeak.com/";
            string webMethod;
            string uri;
            var webclient = new WebClient();
            apiKey = "3FQDOCMWRTX0YMFK";
            
            webMethod = "update?api_key=" + apiKey + "&field1=" + data;
            uri = server + webMethod;
            try
            {
                webclient.UploadString(uri, "POST", "");
                Console.WriteLine("Sent " + data);
            }
            catch
            {
                Console.WriteLine("Server Error");
            }
        }

        class ArduinoReader
        {
            enum ERROR_STATES
            {
                NOT_INITIALIZED,
                NOT_FOUND,
                RUNNING,
            }
            ERROR_STATES status = ERROR_STATES.NOT_INITIALIZED;


            private SerialPort port;

            public ArduinoReader()
            {
                AutodetectArduinoPort();
            }


            void AutodetectArduinoPort()
            {
                string ComPortName = "";
                ManagementScope connectionScope = new ManagementScope();
                SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

                try
                {
                    foreach (ManagementObject item in searcher.Get())
                    {
                        string desc = item["Description"].ToString();
                        string deviceId = item["DeviceID"].ToString();

                        if (desc.Contains("Arduino"))
                        {
                            ComPortName = deviceId;
                            status = ERROR_STATES.RUNNING;
                        }
                    }
                }
                catch (ManagementException ex)
                {
                    ComPortName = "";
                    status = ERROR_STATES.NOT_FOUND;
                }

                if (status == ERROR_STATES.RUNNING)
                {
                    port = new SerialPort(ComPortName, 9600, Parity.None, 8, StopBits.One);
                    port.Open();
                    port.DataReceived += OnDataAvailable;
                }
            }

            void OnDataAvailable(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
            {
                string line = port.ReadLine();
                double entry = (double.Parse(line, System.Globalization.CultureInfo.InvariantCulture));
                SaveMeasurementData(entry * 100);
               
            }
        }

    }
}
