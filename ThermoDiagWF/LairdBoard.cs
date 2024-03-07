using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThermoDiagWF
{
    public class LairdBoard
    {
        private Dictionary<string, string> ports;
        public string  portName = null;
        public SerialPort thermoPort=new SerialPort();


        public LairdBoard()
        {
            string text = File.ReadAllText(@"C:\\ProgramData\LabScript\\Data\\comports.json");
            ports = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            string[] portsNow = SerialPort.GetPortNames();
            foreach (string p in portsNow) //find port not in ports (Json file)
            {
                if (ports.ContainsValue(p)) continue;
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
                    Thread.Sleep(5);
                    string response = thermoPort.ReadLine();
                    response = thermoPort.ReadLine();
                    if (response.Contains("SC_v"))
                    {
                        portName = p;
                        break;
                    }
                    else
                    {
                        thermoPort.Close();
                        portName = null;
                    }
                }
                catch (Exception ex)
                {
                    thermoPort.Close();
                    portName = null;
                }

            }
            if (thermoPort.IsOpen)
            {
                thermoPort.Write("$A0\r\n");
                Thread.Sleep(5);
                try
                {
                    while (thermoPort.ReadLine() != null)
                        thermoPort.ReadLine();
                }
                catch (Exception ex) //wait for timeout
                {

                }
            }
            else
                portName = null;
        }

        public string ReadTemp()
        { 
            if (portName == null) return "999.99";
            thermoPort.Write("$R100?\r\n");
            Thread.Sleep(5);
            string response = thermoPort.ReadLine();
            response = thermoPort.ReadLine();
            response = response.Trim();
            response = response.TrimEnd('\n','\r' );
            return response;
        }
    }
}
