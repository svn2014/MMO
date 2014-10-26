using Quant;
using Quant.Entrust;
using Quant.Strategy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;


namespace OMM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        StrategyMM1 _SMM1 = null;
        private void btnRun_Click(object sender, EventArgs e)
        {
            //Option.GetOptions("510050.SH");
            //Option.GetOptions("510180.SH");
            //Option.GetOptions("600104.SH");
            //Option.GetOptions("601318.SH");

            this._SMM1 = new StrategyMM1();
            //this._SMM1.Add(new Option("90000454", Exchange.SH));
            //this._SMM1.Add(new Option("90000462", Exchange.SH));
            this._SMM1.Add("510050");
            this._SMM1.Run();
        }
    }
}
