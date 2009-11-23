using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser.Interfaces;
using System.Xml.Linq;

namespace Parser.Websites
{
    public class Pinnacle : IParser
    {
        #region IParser Members

        public string Website
        {
            get { return "www.pinnaclesports.com"; }
        }

        public void Parse()
        {
            XDocument xml = XDocument.Load(@"C:\WorkHome\gotparser\xml\pinnacleFeed.asp.xml");

            var info = from feed in xml.Descendants("pinnacle_line_feed")
                       select new
                       {
                           PinnacleFeedTime = feed.Element("PinnacleFeedTime").Value,
                           lastContest = feed.Element("lastContest").Value,
                           lastGame = feed.Element("lastGame").Value
                       };
            var events = from feed in xml.Descendants("event")
                         
                         select new
                         {
                             eventTime = feed.Element("event_datetimeGMT").Value,
                             gamenumber = feed.Element("gamenumber").Value,
                             sporttype = feed.Element("sporttype").Value,
                             league = feed.Element("league").Value,
                             contest_maximum = feed.Element("contest_maximum") == null ? null : feed.Element("contest_maximum"),
                             participants = feed.Element("participants").Descendants("participant").Select(p => new
                             {
                                 participant_name = p.Element("participant_name").Value,
                                 contestantnum = p.Element("contestantnum").Value,
                                 rotnum = p.Element("rotnum").Value,
                                 visiting_home_draw = p.Element("visiting_home_draw") == null ? null : p.Element("visiting_home_draw").Value,
                                 odds = p.Element("odds") == null ? null : p.Descendants("odds").Select(t => new
                                 {
                                     to_base = t.Element("to_base").Value,
                                     moneyline_value = t.Element("moneyline_value").Value
                                 })
                             }).ToList(),
                             periods = feed.Element("periods") == null ? null : feed.Element("periods").Descendants("period").Select(p => new
                             {
                                 period_number = p.Element("period_number").Value,
                                 period_description = p.Element("period_description").Value,
                                 periodcutoff_datetimeGMT = p.Element("periodcutoff_datetimeGMT").Value,
                                 period_status = p.Element("period_status").Value,
                                 period_update = p.Element("period_update").Value,
                                 spread_maximum = p.Element("spread_maximum").Value,
                                 moneyline_maximum = p.Element("moneyline_maximum").Value,
                                 total_maximum = p.Element("total_maximum").Value,
                                 moneylineHome = p.Element("moneyline") == null ? null : p.Element("moneyline").Element("moneyline_home").Value,
                                 moneyline_visiting = p.Element("moneyline") == null ? null : p.Element("moneyline").Element("moneyline_visiting").Value,
                                 spread_home = p.Element("spread") == null ? null : p.Element("spread").Element("spread_home").Value,
                                 spread_visiting = p.Element("spread") == null ? null : p.Element("spread").Element("spread_visiting").Value,
                                 spread_adjust_home = p.Element("spread") == null ? null : p.Element("spread").Element("spread_adjust_home").Value,
                                 spread_adjust_visiting = p.Element("spread") == null ? null : p.Element("spread").Element("spread_adjust_visiting").Value,
                                 total_points = p.Element("total") == null ? null : p.Element("total").Element("total_points").Value,
                                 over_adjust = p.Element("total") == null ? null : p.Element("total").Element("over_adjust").Value,
                                 under_adjust = p.Element("total") == null ? null : p.Element("total").Element("under_adjust").Value
                             }).ToList(),
                             total = feed.Element("total") == null ? null : feed.Descendants("total").Select(p => new
                             {
                                 total_points = p.Element("total_points").Value,
                                 units = p.Element("units").Value
                             }).ToList()
                         };
            var resultSet = events.ToList();
            Console.WriteLine("Pinnacle");
        }



        public event EventHandler DataAvailable;

        public List<Parser.DAO.Results> RetrieveData()
        {
            List<Parser.DAO.Results> results = new List<Parser.DAO.Results>();
            results.Add(new Parser.DAO.Results()
            {
                Name = "b"
            });
            return results;
        }

        #endregion
    }
}
