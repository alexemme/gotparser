using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using Parser.Websites;
using Parser.Interfaces;
using Parser.DAO;

namespace ArbitrageForm
{
    public partial class Form1 : Form
    {
        //private const string url = "https://www.wsb.co.za/allevents.php?tournamentId=8&sortby=1";
        Dictionary<string, List<Results>> results = new Dictionary<string, List<Results>>();
        private DateTime start;

        public Form1()
        {
            InitializeComponent();
            start = DateTime.Now;
            dgr.Text = "processing";
            ParseSite();
        }

        private void ParseSite()
        {
            Wsb wsb = new Wsb();
            wsb.DataAvailable += new EventHandler(DataIsReady);
            wsb.Parse();

            LmBookmaker lmb = new LmBookmaker();
            lmb.DataAvailable += new EventHandler(DataIsReady);
            lmb.Parse();
        }

        void DataIsReady(object sender, EventArgs e)
        {
            IParser parser = sender as IParser;
            results.Add(parser.Website, parser.RetrieveData());
            dgr.Text += string.Format(" Receieved data for {0} \r\n", parser.Website);
            if (results.Keys.Count > 1)
            {
                MapData();
                dgr.Text += string.Format("Timestamp {0}", DateTime.Now.Subtract(start).ToString());
            }

            List<string> str = new List<string>();
            //foreach (Results res in results)
            //{
            //    str.Add(res.Sport + " - " + res.Tournament + " - " + res.Description + " - " + res.Name + " - " + res.Odds + " - closes at " + res.Closes);
            //}
            //dgr.Lines = str.ToArray();
        }

        private void MapData()
        {
            var firstSet = results.First();
            List<Results> baseSet = firstSet.Value;
            var diffSets = results.Values.Where(p => p != baseSet);
            foreach (Results baseResult in baseSet)
            {
                foreach (var set in diffSets)
                {
                    var hope = set.Where(p => p.Sport.ToLower() == baseResult.Sport.ToLower()
                        && p.Tournament.ToLower() == baseResult.Tournament.ToLower());
                    if (hope.Count() > 0)
                    {
                        dgr.Text += string.Format("Match Sport = {0}, Tournament = {1}\rr\n", baseResult.Sport,baseResult.Tournament);
                    }
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void balloon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
    }
}
