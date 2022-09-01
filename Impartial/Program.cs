using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;

namespace Impartial
{
    class Program
    {
        static void Main(string[] args)
        {
            var psc2018_prelims = File.ReadAllText("Scoresheets/psc2018/psc2018_prelims.html");
            var psc2018_finals = File.ReadAllText("Scoresheets/psc2018/psc2018_finals.html");

            var psc2018 = new SdcEvent("Philly Swing Classic", new DateTime(2018, 9, 21))
            {
                Competitions = new List<Competition>()
                {
                    //GetCompetition(psc2018_prelims, psc2018_finals, Division.Newcomer),
                    //GetCompetition(psc2018_prelims, psc2018_finals, Division.Novice),
                    //GetCompetition(psc2018_prelims, psc2018_finals, Division.Intermediate),
                    //GetCompetition(psc2018_prelims, psc2018_finals, Division.Advanced),
                    //GetCompetition(psc2018_prelims, psc2018_finals, Division.AllStar)
                }
            };

            //var asc2018_prelims = File.ReadAllText("Scoresheets/asc2018/asc2018_prelims.html");
            //var asc2018_finals = File.ReadAllText("Scoresheets/asc2018/asc2018_finals.html");

            //var asc2018 = new SdcEvent("Atlanta Swing Classic", new DateTime(2018, 9, 27))
            //{
            //    Competitions = new List<Competition>()
            //    {
            //        GetCompetition(asc2018_prelims, asc2018_finals, Division.Newcomer),
            //        GetCompetition(asc2018_prelims, asc2018_finals, Division.Novice),
            //        GetCompetition(asc2018_prelims, asc2018_finals, Division.Intermediate),
            //        GetCompetition(asc2018_prelims, asc2018_finals, Division.Advanced),
            //        GetCompetition(asc2018_prelims, asc2018_finals, Division.AllStar)
            //    }
            //};
        }


    }
}
