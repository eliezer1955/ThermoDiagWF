using System;
using System.Collections.Generic;
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
        
        
        static void Main()
        {
            log.Info("Thermo Diag starting!");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );           
            state mystate= new state();
            Application.Run( new Form1( mystate.args ) );
        }
    }
}
