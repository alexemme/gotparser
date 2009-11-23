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
        Thread pinnacle, stanJames;

        public Form1()
        {
            InitializeComponent();
            start = DateTime.Now;
            dgr.Text = "Processing";

            pinnacle = new Thread(new ThreadStart(ParsePinnacle));
            stanJames = new Thread(new ThreadStart(ParseStanJames));
            pinnacle.Start();
            stanJames.Start();
            ParseSite();
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            balloon.Dispose();
            if (pinnacle != null && pinnacle.IsAlive)
            {
                pinnacle.Abort();
            }
            if (stanJames != null && stanJames.IsAlive)
            {
                stanJames.Abort();
            }
        }

        private void ParsePinnacle()
        {
            Pinnacle pinnacle = new Pinnacle();
            pinnacle.DataAvailable += new EventHandler(DataIsReady);
            pinnacle.Parse();
        }

        private void ParseStanJames()
        {            
            StanJames stanJames = new StanJames();
            stanJames.DataAvailable += new EventHandler(DataIsReady);
            stanJames.Parse();
        }

        private void ParseSite(){
            //Wsb wsb = new Wsb();
            //wsb.DataAvailable += new EventHandler(DataIsReady);
            //wsb.Parse();

            //LmBookmaker lmb = new LmBookmaker();
            //lmb.DataAvailable += new EventHandler(DataIsReady);
            //lmb.Parse();
        }

        void DataIsReady(object sender, EventArgs e)
        {
            IParser parser = sender as IParser;
            results.Add(parser.Website, parser.RetrieveData());
            dgr.Text += string.Format(" Receieved data for {0} Timestamp {1}\r\n", parser.Website, DateTime.Now.Subtract(start).ToString());
            if (results.Keys.Count > 1)
            {
                //MapData();
                dgr.Text += string.Format("Timestamp {0}\r\n", DateTime.Now.Subtract(start).ToString());
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
                    var hope = set.Where(p => (p.Sport.Contains(baseResult.Sport) || baseResult.Sport.Contains(p.Sport))
                        && (p.Name.Contains(baseResult.Name) || baseResult.Name.Contains(p.Name) ||
                        ((p.Name1 == baseResult.Name1) && (p.Name2 == baseResult.Name2)) ||
                        ((p.Name1 == baseResult.Name2) && (p.Name2 == baseResult.Name1))
                        ));
                    if (hope.Count() > 0)
                    {
                        var arb = hope.Where(p => !p.Odds.Contains(baseResult.Odds));
                        if (arb.Count() > 0)
                        {
                            dgr.Text += string.Format("Match Sport = {0}, Tournament = {1}, Name = {2}, Odd = {3} \r\n", baseResult.Sport, baseResult.Tournament, baseResult.Name, baseResult.Odds);
                            foreach (var sub in hope)
                            {
                                dgr.Text += string.Format("\tSport = {0}, Tournament = {1}, Name = {2}, Odd = {3}\r\n", sub.Sport, sub.Tournament, sub.Name, sub.Odds);
                            }
                        }
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
