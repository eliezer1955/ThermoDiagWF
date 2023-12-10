﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ThermoDiagWF
{
    public partial class Form1 : Form
    {
        public int valve;
        public int position;
        ThermoController thermoController;
        public string CurrentMacro = "stepper.tst.txt";
        public string[] CmdLineArgs; 

        public Form1( string[] args)
        {
            //System.Diagnostics.Debugger.Launch();   
            InitializeComponent();
            CmdLineArgs = args;
            thermoController = new ThermoController(CurrentMacro, this);
            button3.Text = thermoController.CurrentMacro;
            if (CmdLineArgs.Length > 0)
            {
                Thread runner = new Thread( () => thermoController.SocketMode( CmdLineArgs ) );
                runner.Start();
            }
        }

        private void Form1_Load( object sender, EventArgs e )
        {

        }

        private void progressBar1_Click( object sender, EventArgs e )
        {

        }

        private void button1_Click( object sender, EventArgs e )
        {
            Control[] macro = this.Controls.Find( "button3", true );
            string CurrentMacro = macro[0].Text;
            MacroRunner macroRunner = new MacroRunner( thermoController,null, CurrentMacro );
            macroRunner.RunMacro();
        }

        private void label3_Click( object sender, EventArgs e )
        {
            button1_Click( sender, e );
        }

        private void Pos12_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 0;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );

        }

        private void radioButton43_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.LeftRightChoice = 0;
            Control[] found = this.Controls.Find( "groupBox1", true );
            if (found.Length > 0)
                found[0].Visible = false;
        }

        private void radioButton44_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.LeftRightChoice = 1;
            Control[] found = this.Controls.Find( "groupBox1", true );
            if (found.Length > 0)
                found[0].Visible = true;
        }

        private void Pos11_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 1;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos10_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 2;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos9_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 3;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos8_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 4;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos7_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 5;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos1_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = 1;
            thermoController.valvepos = 0;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos2_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = 1;
            thermoController.valvepos = 1;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos3_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = 1;
            thermoController.valvepos = 2;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos4_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = 1;
            thermoController.valvepos = 3;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos5_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 0;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos6_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 2 : 3;
            thermoController.valvepos = 0;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos18_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 0;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos17_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 1;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos16_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 2;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos15_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 3;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos14_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 4;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void Pos13_CheckedChanged( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 5;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void button2_Click( object sender, EventArgs e )
        {
            thermoController.valve = thermoController.LeftRightChoice == 0 ? 5 : 1;
            thermoController.valvepos = 6;
            thermoController.MoveValve( thermoController.valve, thermoController.valvepos );
        }

        private void button3_Click( object sender, EventArgs e )
        {
            var picker = new OpenFileDialog();
            picker.FileName = thermoController.CurrentMacro;
            picker.DefaultExt = "txt";
            picker.InitialDirectory= Environment.CurrentDirectory;
            picker.Filter = "txt files (*.txt)|*.txt";
            if (picker.ShowDialog() == DialogResult.OK)
            {
                thermoController.CurrentMacro = picker.FileName;
                button3.Text = thermoController.CurrentMacro;
            }
        }
       
    }
}