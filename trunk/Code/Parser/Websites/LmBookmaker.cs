using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser.Interfaces;
using Parser.DAO;
using System.Net;
using System.Globalization;

namespace Parser.Websites
{
    public class LmBookmaker : IParser
    {
        private const string url = "https://www.lmbookmaker.com/";

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
            string data = wc.DownloadString(url + "EventBrowser.aspx");
            string[] splits = data.Split(new string[] { "TemplatePage.aspx?pageNo=2&" }, 100, StringSplitOptions.RemoveEmptyEntries);

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
                    ParseEventSelect(int.Parse(strId), strName);
                }
            }
        }

        private void ParseEventSelect(int id, string name)
        {
            string data = wc.DownloadString(url + string.Format("TemplatePage.aspx?pageNo=2&sportId={0}&sportName={1}", id, name));

            string[] secs = data.Split(new string[] { "tblHeadingTitle" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string section in secs)
            {
                if (section.StartsWith("\">"))
                {
                    string tournament = section.Replace("\">", "");
                    int k = tournament.IndexOf("<");
                    tournament = tournament.Substring(0, k);

                    string[] splits = section.Split(new string[] { @"Betting.aspx?pageNo=2" }, 100, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string item in splits)
                    {
                        if (item.StartsWith("&"))
                        {
                            string a = item.Replace("&headId=", "");
                            int i = a.IndexOf("&", 0);
                            string headId = a.Substring(0, i);
                            a = a.Replace(headId + "&subHeadId=", "");
                            i = a.IndexOf("&", 0);
                            string subheadid = a.Substring(0, i);
                            a = a.Replace(subheadid + "&eventsLinkId=", "");
                            i = a.IndexOf("\"");
                            string eventsLinkId = a.Substring(0, i);
                            i = a.IndexOf("&", i);
                            int b;
                            if (i > -1)
                            {
                                b = a.IndexOf("<", i);
                            }
                            else
                            {
                                i = a.IndexOf('>') + 1;
                                b = a.IndexOf("<", i);
                            }
                            string desc = a.Substring(i, b - i).Replace("&gt;", "").Replace("&nbsp;","").Trim();

                            ParseEvents(name, tournament, desc, "", headId, subheadid, eventsLinkId);
                        }
                    }
                }
            }
        }

        private void ParseEvents(string sport, string tournament, string desc, string eventId, string headId, string subHeadId, string eventLinksId)
        {
            string data = wc.DownloadString(url + string.Format("MarketList.aspx?eventId={0}&pageNo=2&headId={1}&subHeadId={2}&eventsLinkId={3}",
                eventId, headId, subHeadId, eventLinksId));

            string[] splits = data.Split(new string[] { @"FixedOddsBetList_" }, StringSplitOptions.RemoveEmptyEntries);

            DateTime closeTime = DateTime.MinValue;

            foreach (string item in splits)
            {
                if (item.Contains("FixedOddsBetList"))
                {
                    string[] secSplits = item.Split(new string[] { "<tr onmouseover" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sItem in secSplits)
                    {
                        if (sItem.Contains("JavaScript"))
                        {
                            int i = sItem.IndexOf("<td");
                            i = sItem.IndexOf("<td", i);
                            string a = sItem.Substring(i, sItem.Length - i);//.Replace("<td>", "");
                            i = a.IndexOf("<td>", 1);
                            a = a.Substring(i + 4, a.Length - i - 4);
                            string name = a.Substring(0, a.IndexOf("<"));
                            i = a.IndexOf("<td");
                            a = a.Substring(i, a.Length - i).Replace("<td align=\"center\">", "");
                            i = a.IndexOf("<");
                            string odds = a.Substring(0, i);
                            Results res = new Results()
                            {
                                Name = name.ToLower(),
                                Sport = sport.ToLower(),
                                Tournament = tournament.ToLower(),
                                Odds = odds,
                                TimeStamp = DateTime.Now,
                                Closes = closeTime,
                                Description = desc.ToLower()
                            };
                            results.Add(res);
                        }
                    }
                }
                else
                {
                    int i = item.IndexOf("Bet until");
                    if (i > 0)
                    {
                        string a = item.Remove(0, i).Replace("Bet until", "");
                        i = a.IndexOf("<");
                        a = a.Substring(0, i).Trim();
                        closeTime = DateTime.ParseExact(a, "dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
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
