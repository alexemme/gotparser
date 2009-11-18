using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser.Interfaces;
using System.Net;
using Parser.DAO;
using System.Globalization;

namespace Parser.Websites
{
    public class Wsb : IParser
    {
        private const string url = "https://www.wsb.co.za/";
        private const string versusSplit = " vs ";

        List<Results> results = new List<Results>();
        WebClient wc = new WebClient();

        #region IParser Members

        public string Website
        {
            get { return url; }
        }

        public void Parse()
        {

            ParseSports();
            DataAvailable(this, null);
        }

        private void ParseSports()
        {
            string data = wc.DownloadString(url + "new_browser.php");
            string[] splits = data.Split(new string[] { @"eventselect.php?", "tournamentselect.php?" }, 100, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in splits)
            {
                if (item.StartsWith("sportId"))
                {
                    string a = item.Replace("sportId=", "");
                    int i = a.IndexOf("&");
                    string strId = a.Substring(0, i);
                    a = a.Remove(0, i + "sportName=".Length + 1);
                    i = a.IndexOf("\'");
                    string strName = a.Substring(0, i);
                    ParseEventSelect(int.Parse(strId), strName, "No Tournament");
                }
                else
                {
                    if (item.StartsWith("sportName"))
                    {
                        string a = item.Replace("sportName=", "");
                        int i = a.IndexOf("\'");
                        string strName = a.Substring(0, i);
                        ParseTournamentSelect(strName);
                    }
                }
            }
        }

        private void ParseEventSelect(int id, string name, string tournament)
        {
            string data = wc.DownloadString(url + string.Format("eventselect.php?sportId={0}&sportName={1}", id, name));
            string[] splits = data.Split(new string[] { @"updateMainContentallevents" }, 100, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in splits)
            {
                if (item.StartsWith("("))
                {
                    string a = item.Replace("(", "");
                    int i = a.IndexOf(",");
                    string strId = a.Substring(0, i);
                    ParseEvents(int.Parse(strId), name, tournament);
                }
            }
        }

        private void ParseTournamentSelect(string name)
        {
            string data = wc.DownloadString(url + string.Format("tournamentselect.php?sportName={0}", name));
            string[] splits = data.Split(new string[] { @"eventselect.php?" }, 100, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in splits)
            {
                if (item.StartsWith("sportId"))
                {
                    string a = item.Replace("sportId=", "");
                    int i = a.IndexOf("&");
                    string strId = a.Substring(0, i);
                    a = a.Substring(i);
                    a = a.Replace("&sportName=" + name + "+%3A+", "");
                    i = a.IndexOf('\'');
                    string tournament = a.Substring(0, i);
                    ParseEventSelect(int.Parse(strId), name, tournament);
                }
            }
        }

        private void ParseEvents(int id, string sport, string tournament)
        {


            string data = wc.DownloadString(url + string.Format("allevents.php?tournamentId={0}&sortby=1", id));

            string[] splits = data.Split(new string[] { @"showEventStatus" }, 100, StringSplitOptions.RemoveEmptyEntries);


            foreach (string item in splits)
            {
                int a = item.IndexOf("betslip_alt\"", 0);
                if (a > 0)
                {
                    int t1 = item.IndexOf("<td");
                    t1 = item.IndexOf(">", t1);
                    int t2 = item.IndexOf("<", t1);
                    string desc = item.Substring(t1 + 1, t2 - t1 - 1).Trim();
                    t1 = item.IndexOf("<td", t2);
                    t1 = item.IndexOf(">", t1);
                    t2 = item.IndexOf("<", t1);
                    string close = item.Substring(t1 + 1, t2 - t1 - 1).Replace("Book Closes at", "").Trim();

                    DateTime closeTime = DateTime.ParseExact(close, "HH:mm dd-MM-yyyy", CultureInfo.InvariantCulture);

                    string[] secondSplit = item.Split(new string[] { "betslip_alt\"" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sItem in secondSplit)
                    {
                        a = sItem.IndexOf(">");
                        if (a == 0)
                        {
                            int b = sItem.IndexOf(">", a);
                            int c = sItem.IndexOf("<", b);
                            string name = sItem.Substring(b + 1, c - b - 1);

                            int i = sItem.IndexOf("betslip_alt2", 0);
                            if (i > 0)
                            {
                                int j = sItem.IndexOf("center", i) + 6;
                                int k = sItem.IndexOf(">", j);
                                int l = sItem.IndexOf("<", k);
                                string bet = sItem.Substring(k + 1, l - k - 1).Trim();
                                name = name.Replace('+', ' ').ToLower();
                                string[] nameSplit = name.Split(new string[] { versusSplit }, StringSplitOptions.RemoveEmptyEntries);                                
                                Results res = new Results()
                                {
                                    Name = name,
                                    Sport = sport.Replace('+', ' ').ToLower(),
                                    Tournament = tournament.Replace('+', ' ').ToLower(),
                                    Odds = bet.ToLower(),
                                    TimeStamp = DateTime.Now,
                                    Closes = closeTime,
                                    Description = desc.Replace('+', ' ').ToLower(),
                                    Site = Website,
                                    Name1 = nameSplit[0].Trim(),
                                    Name2 = (nameSplit.Length > 1 ? nameSplit[1].Trim() : string.Empty)
                                };
                                results.Add(res);
                            }
                        }
                    }
                }
            }
        }

        public event EventHandler DataAvailable;

        public List<Parser.DAO.Results> RetrieveData()
        {
            return results;
        }

        #endregion


    }
}
