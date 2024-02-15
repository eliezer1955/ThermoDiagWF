using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThermoDiagWF
{
    public class LairdBoard
    {
        private Dictionary<string, string> ports;
        public string  portName = null;
        public SerialPort thermoPort;

        private bool notin(string n)
        {
            return !(ports.ContainsKey(n));
        }

        public LairdBoard()
        {
            string text = File.ReadAllText(@"C:\\ProgramData\LabScript\\Data\\comports.json");
            ports = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            string[] portsNow = SerialPort.GetPortNames();
            string[] portsFiltered = Array.FindAll(portsNow, notin); //isolate all com ports NOT in json dictionary
            foreach (string p in portsFiltered)
            {
                try
                {
                    thermoPort.PortName = p;
                    thermoPort.BaudRate = 115200;
                    thermoPort.DataBits = 8;
                    thermoPort.StopBits = StopBits.One;
                    thermoPort.Parity = Parity.None;
                    thermoPort.ReadTimeout = 1500;
                    thermoPort.WriteTimeout = 1500;
                    thermoPort.Open();
                    thermoPort.Write("$v\r\n");
                    string response = thermoPort.ReadLine();
                    if (response.Contains("Laird"))
                    {
                        portName = p;
                        break;
                    }
                    else
                    {
                        thermoPort.Close();
                        thermoPort = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(thermoPort + " Exception: " + ex.Message);
                }

            }
            thermoPort.Write("$A2\r\n");
            try
            {
                while (thermoPort.ReadLine()!=null);
            }
            catch(Exception ex) //wait for timeout
            {

            }
        }

        string ReadTemp()
        {
            if (thermoPort.PortName == null) return "";
            thermoPort.Write("$R100?\r\n");
            string response = thermoPort.ReadLine();
            response = response.Trim();
            response = response.TrimEnd('\n','\r' );
            return response;
        }
    }
}
