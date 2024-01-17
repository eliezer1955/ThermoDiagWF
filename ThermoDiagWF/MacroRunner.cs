﻿using System;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace ThermoDiagWF
{
    public class MacroRunner
    {
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(MacroRunner));
        public string CurrentMacro;
        public SerialPort thermoPort;
        StreamReader fs = null;
        ThermoController controller = null;
        bool socketMode = false;
        public PipeClient pipeClient = null;
        private string[] Macro;
        private int currentline = 0;
        private System.Collections.Generic.Dictionary<string, int> label = new System.Collections.Generic.Dictionary<string, int>();



        public MacroRunner(ThermoController sc, PipeClient pipeClientin, string filename = null)
        {
            thermoPort = sc.thermoPort;
            CurrentMacro = filename;
            pipeClient = pipeClientin;
            controller = sc;
            socketMode = (CurrentMacro == null);
            int currentline = 0;
            if (CurrentMacro != null)
            {
                //Load full macro into memory as array of strings
                Macro = System.IO.File.ReadAllLines( CurrentMacro );
                //Scan macro array for labels, record their line number in Dictionary
                currentline = 0;
                foreach (string line in Macro)
                {
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    if (line1[0].StartsWith( ":" ))
                        label.Add( line1[0].Substring( 1 ).TrimEnd( '\r', '\n', ' ', '\t' ), currentline + 1 );
                    ++currentline;
                }
            }
        }


        public long MonitorTemps(long period)
        {
            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long currentTime = startTime;
            while (currentTime - startTime < period)
            {
                currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                thermoPort.Write("/3aR\r\n");
                StringBuilder response1 = new StringBuilder();
                do
                {
                    int RxBuffer = thermoPort.ReadByte();
                    response1.Append((char)RxBuffer);
                    if (RxBuffer == '\n') break;
                } while (true);
                var temps = response1.ToString();
                temps = temps.Substring(3).Trim();
                temps = temps.Replace("\r", string.Empty);
                temps = temps.Replace("\n", string.Empty);
                temps = temps.Replace("\u0003", string.Empty);
                var tempa = temps.Split(' ');
                controller.SetControlPropertyThreadSafe(controller.parent.textBox1, "Text", tempa[0]);
                controller.SetControlPropertyThreadSafe(controller.parent.textBox2, "Text", tempa[1]);
                controller.SetControlPropertyThreadSafe(controller.parent.textBox3, "Text", tempa[2]);
                controller.SetControlPropertyThreadSafe(controller.parent.textBox4, "Text", tempa[3]);
                controller.SetControlPropertyThreadSafe(controller.parent.textBox5, "Text", tempa[4]);
                controller.SetControlPropertyThreadSafe(controller.parent.textBox6, "Text", tempa[5]);
                controller.parent.Update();
                Thread.Yield();
            }

            return period;
        }


        public async Task<string> readLine()
        {
            //System.Diagnostics.Debugger.Launch();
            string s;
            if (socketMode)
            {
                await pipeClient.receive(); //block until string is received
                s = pipeClient.lastReceive; //retrieve string received
                lock (pipeClient._writerSemaphore)
                {
                    pipeClient.lastReceive = null; //reset lastreceive for next read
                }
            }
            else
            {
                s = currentline >= Macro.Length ? null : Macro[currentline++];
            }
            return s;
        }


        public async void RunMacro()
        {

            //  Read in macro stream

            byte[] b = new byte[1024];
            string[] lastCommand;
            string lastCommandReturnTypes;
            System.Text.UTF8Encoding temp = new System.Text.UTF8Encoding(true);
            string line;
            string response = "";
            while (true)
            {
                line = await readLine();

                if (line == null) break;
                if (line.StartsWith("\0")) continue;
                if (line.StartsWith("#")) continue;
                if (line.StartsWith( ":" )) continue;
                if (string.IsNullOrEmpty(line)) continue;
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("IFRETURNISNOT")) //conditional execution based on last return
                {
                    string value = "";
                    string[] line1 = line.Split('#'); //Disregard comments
                    string[] parsedLine = line1[0].Split(',');
                    if (string.IsNullOrWhiteSpace(parsedLine[0])) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        value = parsedLine[1]; //isolate target value

                    if (value == response) //last return matches value
                        continue; //do nothing, go to read next command
                    //value is not equal to last response, execute conditional command
                    line = ""; //reassemble rest of conditional command
                    for (int i = 2; i < parsedLine.Length; i++)
                    {
                        line += parsedLine[i];
                        if (i < parsedLine.Length - 1) line += ",";
                    }
                    //coninue execution as if it was non-conditional
                }
                if (line.StartsWith("IFRETURNIS")) //conditional execution based on last return
                {
                    string value = "";
                    string[] line1 = line.Split('#'); //Disregard comments
                    string[] parsedLine = line1[0].Split(',');
                    if (string.IsNullOrWhiteSpace(parsedLine[0])) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        value = parsedLine[1]; //isolate target value

                    if (value != response) //last return does not match value
                        continue; //do nothing, go to read next command
                    //value is equal to last response
                    line = ""; //reassemble rest of command
                    for (int i = 2; i < parsedLine.Length; i++)
                    {
                        line += parsedLine[i];
                        if (i < parsedLine.Length - 1) line += ",";
                    }
                    //coninue execution as if it was non-conditional
                }
                if (line.StartsWith("LOGERROR")) //write log entry
                {
                    string value = "";
                    string[] line1 = line.Split('#'); //Disregard comments
                    string[] parsedLine = line1[0].Split(',');
                    if (string.IsNullOrWhiteSpace(parsedLine[0])) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        value = parsedLine[1];

                    _logger.Error(value);
                    continue;
                }
                if (line.StartsWith( "GOTO" ))
                {
                    string value = "";
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        value = parsedLine[1].TrimEnd( '\r', '\n', ' ', '\t' );
                    if (!label.ContainsKey( value ))
                        _logger.Error( "Unknown label " + value );
                    else
                    {

                        currentline = label[value];
                        continue;
                    }

                }
                // "Nested" macro calling
                if (line.StartsWith("@"))
                {
                    MacroRunner macroRunner = new MacroRunner(controller, pipeClient, line.Substring(1));
                    macroRunner.RunMacro();
                    continue;
                }
                // Wait for fixed time
                if (line.StartsWith("SLEEP"))
                {
                    int delay = 0;
                    string[] line1 = line.Split('#'); //Disregard comments
                    string[] parsedLine = line1[0].Split(',');
                    if (string.IsNullOrWhiteSpace(parsedLine[0])) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        delay = Int32.Parse(parsedLine[1]);
                    Thread.Sleep(delay);
                    continue;
                }
                // Wait until status is idle
                if (line.StartsWith("WAIT"))
                {
                    string[] line1 = line.Split('#'); //Disregard comments
                    string[] parsedLine = line1[0].Split(',');
                    if (string.IsNullOrWhiteSpace(parsedLine[0])) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                    {
                        bool motionDone = false;
                        do
                        {
                            Int32.Parse(parsedLine[1]);
                            thermoPort.Write("/Q" + parsedLine[1] + "R\r\n");
                            Thread.Sleep(100);
                            byte c1;
                            do
                            {
                                c1 = (byte)thermoPort.ReadByte();
                                response += c1;
                            } while (c1 != '\n');
                            if ((response.TrimEnd('\r', '\n')[2] & 0x40) != 0) continue; //isolate status byte, busy bit
                            motionDone = true;
                        } while (!motionDone);

                    }
                    continue;
                }
                // Pop up MessageBox
                if (line.StartsWith("ALERT"))
                {
                    string[] line1 = line.Split('#'); //Disregard comments
                    string[] parsedLine = line1[0].Split(',');
                    if (string.IsNullOrWhiteSpace(parsedLine[0])) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                    {
                        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                        DialogResult result;
                        result = MessageBox.Show(parsedLine[1], "Stepper Alert!", buttons);
                        response = result.ToString();
                        continue;
                    }
                }

                // Read switches continuously for period of time, update GUI display
                if (line.StartsWith("MONITORTEMPS"))
                {
                    string[] line1 = line.Split('#'); //Disregard comments
                    string[] parsedLine = line1[0].Split(',');
                    MonitorTemps(long.Parse(parsedLine[1]));
                    continue;
                }

                if (line.StartsWith("REPORT"))
                {

                    string[] line1 = line.Split('#'); //Disregard comments
                    string[] parsedLine = line1[0].Split(',');
                    if (string.IsNullOrWhiteSpace(parsedLine[0])) //Disregard blanks lines
                        continue;

                    if (parsedLine[1] != null)
                    {
                        var i = line.IndexOf(',');
                        if (i > -1)
                        {
                            await pipeClient.client.Send("Thermo:" + line.Substring(i + 1));
                            continue;
                        }
                    }
                }


                //Actual command
                string[] lin1 = line.Split('#');
                lin1[0] = lin1[0].TrimEnd(new char[] { ' ', '\r', '\n', '\t' });
                lin1[0] = lin1[0].Trim();
                if (!string.IsNullOrWhiteSpace(lin1[0]))
                {
                    lastCommand = lin1;
                    thermoPort.Write(lin1[0] + "\r\n");
                    StringBuilder response1 = new StringBuilder();
                    do
                    {
                        int RxBuffer = thermoPort.ReadByte();
                        response1.Append((char)RxBuffer);
                        if (RxBuffer == '\n') break;
                    } while (true);
                    if (lin1[0].StartsWith("/3&R"))
                    {
                        var temps = response1.ToString();
                        temps = temps.Replace("\r", string.Empty);
                        temps = temps.Replace("\n", string.Empty);
                        temps = temps.Replace("\u0003", string.Empty);
                        controller.parent.label1.Text = temps.Substring(3).Trim();
                    }
                }


            }
        }

    }
}