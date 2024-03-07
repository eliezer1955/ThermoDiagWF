using System;
using System.Threading;
using System.Windows.Forms;

namespace ThermoDiagWF
{
    public partial class Form1 : Form
    {
        public int valve;
        public int position;
        ThermoController thermoController;
        public string CurrentMacro = "thermosetup.txt";
        public string[] CmdLineArgs;
        public bool stopMonitoring = false;

        public Form1(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();   
            InitializeComponent();
            CmdLineArgs = args;
            if (args.Length > 1 && args[1] != "Slave")
                CurrentMacro = args[1];
            thermoController = new ThermoController(CurrentMacro, this);
            button3.Text = thermoController.CurrentMacro;

            if (CmdLineArgs.Length > 1)
                if (CmdLineArgs[1] == "Slave")
                {
                    Thread runner = new Thread(() => thermoController.SocketMode(CmdLineArgs));
                    runner.Start();
                }
                else
                {
                    MacroRunner macroRunner = new MacroRunner(thermoController, null, CurrentMacro);
                    macroRunner.RunMacro();
                }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Control[] macro = this.Controls.Find("button3", true);
            string CurrentMacro = macro[0].Text;
            MacroRunner macroRunner = new MacroRunner(thermoController, null, CurrentMacro);
            macroRunner.RunMacro();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void Pos12_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 0;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);

        }

        private void radioButton43_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.LeftRightChoice = 0;
            Control[] found = this.Controls.Find("groupBox1", true);
            if (found.Length > 0)
                found[0].Visible = false;
        }

        private void radioButton44_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.LeftRightChoice = 1;
            Control[] found = this.Controls.Find("groupBox1", true);
            if (found.Length > 0)
                found[0].Visible = true;
        }

        private void Pos11_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 1;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos10_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 2;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos9_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 3;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos8_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 4;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos7_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 5;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos1_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = 1;
            thermoController.valvepos = 0;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos2_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = 1;
            thermoController.valvepos = 1;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos3_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = 1;
            thermoController.valvepos = 2;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos4_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = 1;
            thermoController.valvepos = 3;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos5_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 0;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos6_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 0;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos18_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 0;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos17_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 1;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos16_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 2;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos15_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 3;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos14_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 4;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void Pos13_CheckedChanged(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 5;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 6;
            thermoController.MoveValve(thermoController.valve, thermoController.valvepos);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var picker = new OpenFileDialog();
            picker.FileName = thermoController.CurrentMacro;
            picker.DefaultExt = "txt";
            picker.InitialDirectory = Environment.CurrentDirectory;
            picker.Filter = "txt files (*.txt)|*.txt";
            if (picker.ShowDialog() == DialogResult.OK)
            {
                thermoController.CurrentMacro = picker.FileName;
                button3.Text = thermoController.CurrentMacro;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        stopMonitoring = true;
    }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void textBox11_TextChanged( object sender, EventArgs e )
        {

        }
    }
}
