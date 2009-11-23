using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser.Interfaces;
using System.Xml.Linq;
using System.Net;
using System.IO;

namespace Parser.Websites
{
    public class StanJames : IParser
    {
        #region IParser Members

        public string Website
        {
            get { return "www.stanjames.com"; }
        }

        public void Parse()
        {
            
            string xml = "http://xml.stanjames.com/";
            WebClient wc = new WebClient();
            string data = wc.DownloadString(xml);
            string[] split = data.Split(new string[]{"</head>"}, StringSplitOptions.None);
            XDocument html = XDocument.Load(new StringReader(split[1].Replace("<br>", "<br />").Replace("<hr>","<hr />").Replace("</html>","")));
            var links = (from feed in html.Descendants("body").Descendants("pre").Elements("A")
                         where !feed.Attribute("HREF").Value.StartsWith("/@") && feed.Attribute("HREF").Value.ToLower().EndsWith("xml")
                       select feed.Attribute("HREF").Value).ToList();
            foreach (var link in links)
            {
                ParseSport(xml + link);
            }                       
        }

        private void ParseSport(string url)
        {
            XDocument xml = XDocument.Load(url);//@"C:\WorkHome\gotparser\xml\stanjamestennis-mens.xml");

            var info = from feed in xml.Descendants("category")
                       select new
                       {
                           name = feed.Attribute("name").Value,
                           timeGenerated = feed.Attribute("timeGenerated").Value,
                           Bookmaker = feed.Attribute("Bookmaker").Value
                       };
            var events = from feed in xml.Descendants("category").Descendants("event")

                         select new
                         {
                             name = feed.Attribute("name").Value,
                             eventid = feed.Attribute("eventid").Value,
                             date = feed.Attribute("date").Value,
                             time = feed.Attribute("time").Value,
                             meeting = feed.Attribute("meeting").Value,
                             venue = feed.Attribute("venue").Value,
                             sport = feed.Attribute("sport").Value,
                             sporttype = feed.Attribute("sporttype").Value,
                             betUrl = feed.Attribute("sb-url").Value,
                             bettype = feed.Descendants("bettype") == null? null :feed.Descendants("bettype").Select(p =>  new 
                             {
                                 betStartDate = p.Attribute("bet-start-date") == null ? null :p.Attribute("bet-start-date").Value,
                                 betStartTime = p.Attribute("bet-start-time") == null ? null :p.Attribute("bet-start-time").Value,
                                 ewreduction = p.Attribute("ewreduction") == null ? null : p.Attribute("ewreduction").Value,
                                 ewplaceterms = p.Attribute("ewplaceterms") == null ? null : p.Attribute("ewplaceterms").Value,
                                 eachway = p.Attribute("eachway") == null ? null : p.Attribute("eachway").Value,
                                 suspended = p.Attribute("suspended").Value,
                                 name = p.Attribute("name").Value,
                                 inrunning = p.Attribute("inrunning").Value,
                                 bettypeid = p.Attribute("bettypeid").Value,
                                 betUrl = p.Attribute("sb-url").Value,
                                 bet = p.Descendants("bet") == null ? null : p.Descendants("bet").Select(t => new
                                 {
                                     name = t.Attribute("name").Value,
                                     had = t.Attribute("had-value") == null ? null : t.Attribute("had-value").Value,
                                     id = t.Attribute("id").Value,
                                     price = t.Attribute("price").Value,
                                     priceDecimal = t.Attribute("priceDecimal").Value,
                                     activePriceTypes = t.Attribute("active-price-types").Value
                                 }).ToList()
                             }).ToList()
                         };
            
            var resultSet = events.ToList();
            Console.WriteLine("StanJames");
        }

        public event EventHandler DataAvailable;

        public List<Parser.DAO.Results> RetrieveData()
        {
            List<Parser.DAO.Results> results = new List<Parser.DAO.Results>();
            results.Add(new Parser.DAO.Results()
            {
                Name = "a"
            });
            return results;
        }

        #endregion
    }
}
