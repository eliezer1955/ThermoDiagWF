using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThermoDiagWF
{
    public class state
    {
        public state() { args = Environment.GetCommandLineArgs(); }
        public string[] args;
    }
    internal static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]


        static void Main( string[] args )
        {
            ComPortMap cm;
            cm = new ComPortMap();
            var configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));
            log4net.Config.XmlConfigurator.Configure(configFile);
            string serialNumber = cm.GetComPort( "SerialNumber" );
            log.Info( "SN" + serialNumber + " Thermo Diag is starting..." );

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            var myform = new Form1( args );
            myform.CmdLineArgs = args;
            myform.serialNumber = serialNumber;
 
            state mystate= new state();
            Application.Run( myform );
        }
    }
}
