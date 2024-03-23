using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace ThermoDiagWF
{
    public class MacroRunner
    {
        public class MyRef<T>
        {
            public T Ref { get; set; }
        }


        public void refreshGUI()
        {
            this.controller.parent.Invalidate();
            this.controller.parent.Update();
            this.controller.parent.Refresh();
            Application.DoEvents();
        }



        public void AddVar( string key, object v ) //storing the ref to a string
        {
            if (null == v)
            {
                v = new MyRef<string> { Ref = " " };
            }
            variables.Add( key, v );
        }



        public void changeVar( string key, object newValue ) //changing any of them
        {
            var ref2 = variables[key] as MyRef<string>;
            if (ref2 == null)
            {
                ref2 = new MyRef<string> { Ref = " " };
            }
            ref2.Ref = newValue.ToString();
        }
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger( typeof( MacroRunner ) );
        public string CurrentMacro;
        public SerialPort thermoPort;
        StreamReader fs = null;
        ThermoController controller = null;
        bool socketMode = false;
        public PipeClient pipeClient = null;
        private string[] Macro;
        private int currentline = 0;
        private System.Collections.Generic.Dictionary<string, int> label = new System.Collections.Generic.Dictionary<string, int>();
        private String response;
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        public LairdBoard laird;
        public String fluidMeasurement = " ";
        public string elapsedTime = "0";
        public string serialNumber="";

        private string ExpandVariables( string instring )
        {
            StringBuilder sb = new StringBuilder();
            int start = 0;
            int i;

            var val = new MyRef<string> { Ref = "" };
            for (i = start ; i < instring.Length ; i++)
                if (instring[i] == '%')
                    for (int j = 1 ; j < instring.Length - i ; j++)
                        if (instring[i + j] == '%')
                        {
                            sb.Append( instring.Substring( start, i - start ) );
                            string key = instring.Substring( i + 1, j - 1 );
                            if (variables.ContainsKey( key ))
                            {
                                val = (MyRef<string>)variables[key];
                                sb.Append( val.Ref );
                                start = i = i + j + 1;
                            }
                            else _logger.Error( "Unknown variable:" + val );
                            continue;
                        }
            if ((i - start > 0) && (start < instring.Length))
                sb.Append( instring.Substring( start, i - start ) );
            return sb.ToString();
        }

        private string Evaluate( string instring )
        {
            try
            {
                instring = ExpandVariables( instring );
                DataTable dt = new DataTable();
                var v = dt.Compute( instring, "" );
                return v.ToString();
            }
            catch (Exception ex)
            {
                return instring;
            }
        }

        public MacroRunner( ThermoController sc, PipeClient pipeClientin, string filename = null )
        {
            thermoPort = sc.thermoPort;
            CurrentMacro = filename;
            pipeClient = pipeClientin;
            controller = sc;
            socketMode = (CurrentMacro == null);
            AddVar( "response", response );
            AddVar( "fluidMeasurement", fluidMeasurement.ToString() );
            AddVar( "elapsedTime", elapsedTime );
            serialNumber=controller.parent.serialNumber;
            AddVar( "serialNumber", serialNumber );
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
            laird = new LairdBoard();
            if (laird.portName != null)
            {
                controller.SetControlPropertyThreadSafe( controller.parent.textBox9, "Text", laird.portName );
                refreshGUI();
            }

        }

        string[] sendCmd( string tosend )
        {
            thermoPort.Write( "/3" + tosend + "R\r\n" );
            StringBuilder response1 = new StringBuilder();
            do
            {
                int RxBuffer = thermoPort.ReadByte();
                response1.Append( (char)RxBuffer );
                if (RxBuffer == '\n') break;
            } while (true);
            var temps = response1.ToString();
            temps = temps.Substring( 3 ).Trim();
            temps = temps.Replace( "\r", string.Empty );
            temps = temps.Replace( "\n", string.Empty );
            temps = temps.Replace( "\u0003", string.Empty );
            var tempa = temps.Split( ' ' );
            return tempa;
        }

        public List<string> GetIRs()
        {
            var tempa = sendCmd( "`0" ); //select IR channel 0
            tempa = sendCmd( "^" );
            var tempb = sendCmd( "`1" ); //select IR channel 1
            tempb = sendCmd( "^" );
            controller.SetControlPropertyThreadSafe( controller.parent.textBox7, "Text", tempa[0] );
            controller.SetControlPropertyThreadSafe( controller.parent.textBox8, "Text", tempb[0] );
            if (laird.portName != null)
            {
                string s = float.Parse( laird.ReadTemp() ).ToString();
                if (s == "999.99)") s = "";
                controller.SetControlPropertyThreadSafe( controller.parent.textBox10, "Text", s );
            }
            refreshGUI();
            List<string> temps = new List<string>();
            temps.Add( tempa[0] );
            temps.Add( tempb[0] );

            return temps;

        }

        private List<string> GetTemps()
        {
            List<string> temps = new List<string>();
            try
            {
                var tempa = sendCmd( "a" );
                controller.SetControlPropertyThreadSafe( controller.parent.textBox1, "Text", tempa[0] );
                controller.SetControlPropertyThreadSafe( controller.parent.textBox2, "Text", tempa[1] );
                controller.SetControlPropertyThreadSafe( controller.parent.textBox3, "Text", tempa[2] );
                controller.SetControlPropertyThreadSafe( controller.parent.textBox4, "Text", tempa[3] );
                controller.SetControlPropertyThreadSafe( controller.parent.textBox5, "Text", tempa[4] );
                controller.SetControlPropertyThreadSafe( controller.parent.textBox6, "Text", tempa[5] );
                List<string> tb = GetIRs();
                refreshGUI();

                foreach (string s in tempa) temps.Add( s );
                foreach (string s in tb) temps.Add( s );
                temps.Add( float.Parse( laird.ReadTemp() ).ToString() );
                return temps;
            }
            catch (Exception ex)
            {
                return temps;
            }

        }


        public long MonitorTemps( long period, long logperiod = -1, float tempTarget = 100000.0F, int sensorNumber = 8 )
        {
            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long currentTime = startTime;
            this.controller.SetControlPropertyThreadSafe( controller.parent.button2, "Visible", true );
            this.controller.SetControlPropertyThreadSafe( controller.parent.textBox14, "Visible", true );
            refreshGUI();
            bool rampup;
            if (tempTarget > 0) { rampup = true; }
            else { rampup = false; tempTarget = -tempTarget; }
            if (logperiod > 0)
                _logger.Info( "Starting Temperature monitoring " );
            long lastlog = currentTime; ;
            while (currentTime - startTime < period)
            {
                currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                this.controller.SetControlPropertyThreadSafe( controller.parent.textBox14, "Text",
                    ((period - (currentTime - startTime)) / 1000).ToString() );
                refreshGUI();
                List<string> tempa = GetTemps();
                if (rampup)
                {
                    if (tempTarget < 100.0 && float.Parse( tempa[sensorNumber] ) > tempTarget) break;
                }
                else
                {
                    if (tempTarget < 100.0 && float.Parse( tempa[sensorNumber] ) < tempTarget) break;
                }
                if (logperiod > 0) //log to logfile
                {
                    if (currentTime - lastlog > logperiod)
                    {
                        lastlog = currentTime;
                        StringBuilder sb = new StringBuilder();
                        foreach (string t in tempa)
                        {
                            sb.Append( t );
                            sb.Append( ' ' );
                        }

                        _logger.Info( "Temperatures= " + sb );
                    };
                }
                if (controller.parent.stopMonitoring)
                {
                    controller.parent.stopMonitoring = false;
                    break;
                }
            }
            this.controller.SetControlPropertyThreadSafe( controller.parent.button2, "Visible", false );
            this.controller.SetControlPropertyThreadSafe( controller.parent.textBox14, "Visible", false );
            refreshGUI();
            if (logperiod > 0)
                _logger.Info( "Temperature monitoring completed, elapsed time="+ 
                    (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime).ToString());
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
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
            System.Text.UTF8Encoding temp = new System.Text.UTF8Encoding( true );
            string line;
            string response = "";
            while (true)
            {
                line = await readLine();
                GetTemps();
                if (line == null) break;
                if (line.StartsWith( "\0" )) continue;
                if (line.StartsWith( "#" )) continue;
                if (line.StartsWith( ":" )) continue;
                if (string.IsNullOrEmpty( line )) continue;
                if (string.IsNullOrWhiteSpace( line )) continue;
                if (line.StartsWith( "END" )) //Terminate program
                {
                    string expr = "";
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        expr = parsedLine[1]; //isolate expression
                    Environment.Exit( Int32.Parse( Evaluate( expr ) ) );
                }
                if (line.StartsWith( "IFRETURNISNOT" )) //conditional execution based on last return
                {
                    string value = "";
                    string expr = "";
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        expr = parsedLine[1]; //isolate expression
                    if (parsedLine[2] != null)
                        value = parsedLine[2]; //isolate target value
                    value = Evaluate( value );
                    expr = Evaluate( expr );

                    if (value == expr) //last return matches value
                        continue; //do nothing, go to read next command
                                  //value is not equal to last response, execute conditional command
                    line = ""; //reassemble rest of conditional command
                    for (int i = 3 ; i < parsedLine.Length ; i++)
                    {
                        line += parsedLine[i];
                        if (i < parsedLine.Length - 1) line += ",";
                    }
                    //continue execution as if it was non-conditional
                }
                if (line.StartsWith( "IFRETURNIS" )) //conditional execution based on last return
                {
                    string value = "";
                    string expr = "";
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        expr = parsedLine[1]; //isolate expression
                    if (parsedLine[2] != null)
                        value = parsedLine[2]; //isolate target value
                    value = Evaluate( value );
                    expr = Evaluate( expr );
                    if (value != Evaluate( expr )) //last return does not match value
                        continue; //do nothing, go to read next command
                                  //value is equal to last response
                    line = ""; //reassemble rest of command
                    for (int i = 3 ; i < parsedLine.Length ; i++)
                    {
                        line += parsedLine[i];
                        if (i < parsedLine.Length - 1) line += ",";
                    }
                    //continue execution as if it was non-conditional
                }
                if (line.StartsWith( "EVALUATE" )) //Set response to evaluation of expression
                {
                    string value = "";
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        value = parsedLine[1]; //isolate target value

                    response = Evaluate( parsedLine[1] );
                    changeVar( "response", response );
                    continue;

                }
                if (line.StartsWith( "SET" )) //set value of global var; create it if needed
                {
                    string variable = "";
                    string value = "";
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        variable = parsedLine[1];
                    if (parsedLine[2] != null)
                        value = Evaluate( parsedLine[2] );
                    if (!variables.ContainsKey( variable ))
                        AddVar( variable, null );
                    changeVar( variable, value );
                    continue;
                }
                if (line.StartsWith( "EXIT" )) //stop macro
                {
                    break;
                }
                if (line.StartsWith( "EXECUTE" )) //stop macro
                {
                    string value = "";

                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        value = ExpandVariables( parsedLine[1]);
                    System.Diagnostics.Process.Start( "CMD.exe", "/C " + value );
                    continue;
                }

                if (line.StartsWith( "LOGERROR" )) //write log entry
                {
                    string value = "";
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        value = ExpandVariables( parsedLine[1] );

                    _logger.Error( value );
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
                if (line.StartsWith( "@" ))
                {
                    MacroRunner macroRunner = new MacroRunner( controller, pipeClient, line.Substring( 1 ) );
                    macroRunner.RunMacro();
                    continue;
                }
                // Wait for fixed time
                if (line.StartsWith( "SLEEP" ))
                {
                    int delay = 0;
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        delay = Int32.Parse( parsedLine[1] );
                    Thread.Sleep( delay );
                    continue;
                }
                // Wait until status is idle
                if (line.StartsWith( "WAIT" ))
                {
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                    {
                        bool motionDone = false;
                        do
                        {
                            Int32.Parse( parsedLine[1] );
                            thermoPort.Write( "/Q" + parsedLine[1] + "R\r\n" );
                            Thread.Sleep( 100 );
                            byte c1;
                            do
                            {
                                c1 = (byte)thermoPort.ReadByte();
                                response += c1;
                            } while (c1 != '\n');
                            if ((response.TrimEnd( '\r', '\n' )[2] & 0x40) != 0) continue; //isolate status byte, busy bit
                            motionDone = true;
                        } while (!motionDone);

                    }
                    continue;
                }
                // Pop up MessageBox
                if (line.StartsWith( "ALERT" ))
                {
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                    {
                        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                        DialogResult result;
                        result = MessageBox.Show( parsedLine[1], "Thermo Alert!", buttons );
                        response = result.ToString();
                        continue;
                    }
                }

                // Read switches continuously for period of time, update GUI display
                if (line.StartsWith( "MONITORTEMPS" ))
                {
                    long logperiod = -1;
                    float tempTarget = 100000.0F;
                    int sensorNumber = 8;
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (parsedLine.Length > 2) logperiod = long.Parse( parsedLine[2] );
                    if (parsedLine.Length > 3) tempTarget = float.Parse( Evaluate(parsedLine[3]));
                    if (parsedLine.Length > 4) sensorNumber = int.Parse( parsedLine[4] );
                    elapsedTime = MonitorTemps( long.Parse( parsedLine[1] ), logperiod, tempTarget, sensorNumber ).ToString();
                    changeVar( "elapsedTime", elapsedTime );
                    continue;
                }
                // Read switches continuously for period of time, update GUI display
                if (line.StartsWith( "GETTEMP" ))
                {
                    List<string> temps = GetTemps();
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    int tempNumber = 0;
                    if (parsedLine.Length > 2) tempNumber = int.Parse( parsedLine[2] );
                    response = temps[tempNumber];
                    changeVar( "response", response );
                    continue;
                }
                // Read switches continuously for period of time, update GUI display, log results
                if (line.StartsWith( "THERMOTEST" ))
                {
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    this.controller.SetControlPropertyThreadSafe( controller.parent.label12, "Visible", true );
                    refreshGUI();
                    elapsedTime = MonitorTemps( long.Parse( parsedLine[1] ),
                        long.Parse( parsedLine[2] ) ).ToString();
                    changeVar( "elapsedTime", elapsedTime );
                    this.controller.SetControlPropertyThreadSafe( controller.parent.label12, "Visible", false );
                    refreshGUI();
                    continue;
                }
                // PRINT STATUS ON gui
                if (line.StartsWith( "STATUS" ))
                {
                    string status = "";
                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;
                    if (parsedLine[1] != null)
                        status = parsedLine[1];
                    controller.SetControlPropertyThreadSafe( controller.parent.textBox11, "Text", status );
                    refreshGUI();
                    continue;
                }

                if (line.StartsWith( "REPORT" ))
                {

                    string[] line1 = line.Split( '#' ); //Disregard comments
                    string[] parsedLine = line1[0].Split( ',' );
                    if (string.IsNullOrWhiteSpace( parsedLine[0] )) //Disregard blanks lines
                        continue;

                    if (parsedLine[1] != null)
                    {
                        var i = line.IndexOf( ',' );
                        if (i > -1)
                        {
                            await pipeClient.client.Send( "Thermo:" + line.Substring( i + 1 ) );
                            continue;
                        }
                    }
                }


                //Actual command
                string[] lin1 = line.Split( '#' );
                lin1[0] = lin1[0].TrimEnd( new char[] { ' ', '\r', '\n', '\t' } );
                lin1[0] = lin1[0].Trim();
                if (!string.IsNullOrWhiteSpace( lin1[0] ))
                {
                    lastCommand = lin1;
                    thermoPort.Write( lin1[0] + "\r\n" );
                    StringBuilder response1 = new StringBuilder();
                    do
                    {
                        int RxBuffer = thermoPort.ReadByte();
                        response1.Append( (char)RxBuffer );
                        if (RxBuffer == '\n') break;
                    } while (true);
                    if (lin1[0].StartsWith( "/3&R" )) //command to read firmware version
                    {
                        var temps = response1.ToString();
                        temps = temps.Replace( "\r", string.Empty );
                        temps = temps.Replace( "\n", string.Empty );
                        temps = temps.Replace( "\u0003", string.Empty );
                        controller.parent.label1.Text = temps.Substring( 3 ).Trim();
                    }
                    changeVar( "response", response1 );
                    response = response1.ToString();
                }


            }
        }

    }
}