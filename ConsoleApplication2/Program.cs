using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace ConsoleApplication2
{
  class Program
  {
    const string basePage = "http://dotabuff.com";
    static string playerId;
    static bool openFoundPages = false;

    static void Main(string[] args)
    {
        playerId = ReadID().Length == 0 ? "26424147" : ReadID();
      string boolean = ReadLaunchPages();
      Console.Clear();

      if (boolean == "y")
        openFoundPages = true;

      HtmlWeb playerPage = new HtmlWeb();
      HtmlDocument playerDocument = playerPage.Load(basePage + "/players/" + playerId + "/matches");
      XPathNavigator playerDocumentNavigator = playerDocument.CreateNavigator();

      int lastPage = 1; ;

      try
      {
        lastPage = int.Parse(playerDocumentNavigator.SelectSingleNode("//*[@id='page-content']/section/article/nav/span[@class='last']/a/@href").Value.Split('=').Last());
      }
      catch (NullReferenceException)
      {
        Console.WriteLine("ID not found");
        Console.Read();
      }

      for (int i = 1; i != lastPage; i++)
      {
        Console.WriteLine("Reading page " + i + "/" + lastPage);

        HtmlWeb matchesPage = new HtmlWeb();
        HtmlDocument matchesDocument = matchesPage.Load(basePage + "/players/" + playerId + "/matches.html?page=" + i);
        XPathNavigator matchesDocumentNavigator = matchesDocument.CreateNavigator();

        XPathNodeIterator iterator = matchesDocumentNavigator.Select("//*[@id='page-content']/section/article/table/tbody/tr/td[@class='cell-xlarge']/a/@href");
        while (iterator.MoveNext())
        {
          string matchUrl = basePage + iterator.Current.Value;

          new Thread(() =>
          {
            HtmlWeb matchDetailPage = new HtmlWeb();
            HtmlDocument matchDetailDocument = matchDetailPage.Load(matchUrl, "GET");
            XPathNavigator matchDetailDocumentNavigator = matchDetailDocument.CreateNavigator();

            string matchId = matchDetailDocumentNavigator.SelectSingleNode("//*[@id='content-header-primary']/div/h1").Value;
            //Console.WriteLine("Checking " + matchId);

            XPathNodeIterator verifiedPlayers = matchDetailDocumentNavigator.Select("//*[@id='page-content']/div/div/section/article/table/tbody/tr/td/i");

            if (verifiedPlayers.Count > 0)
            {
              Console.WriteLine("Found: " + matchId);
              while (verifiedPlayers.MoveNext())
              {
                Console.WriteLine(verifiedPlayers.Current.SelectSingleNode("@title").Value);
              }

              if (openFoundPages)
                System.Diagnostics.Process.Start(matchUrl);
            }
          }).Start();
        }
      }


     // XPathNavigator aaaaa = aa.SelectSingleNode("//span[@class='next']");

    }

    static string ReadID()
    {
      Console.Write("Dotabuff ID: ");
      return Console.ReadLine();
    }

    static string ReadLaunchPages()
    {
      Console.Write("Launch found pages y/n: ");
      return Console.ReadLine();
    }
  }
}
