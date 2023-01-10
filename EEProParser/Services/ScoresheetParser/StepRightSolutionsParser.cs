using HtmlAgilityPack;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Impartial.Services.ScoresheetParser
{
    public class StepRightSolutionsParser : IScoresheetParser
    {
        private string _finalsSheetDoc { get; set; }

        public List<Judge> Judges { get; set; }
        public List<Score> Scores { get; set; }

        public StepRightSolutionsParser(string finalsPath)
        {
            //if (prelimsSheetPath == null || prelimsSheetPath == String.Empty)
            //    return;
            if (finalsPath == null || finalsPath == String.Empty)
                return;

            //_prelimsSheetDoc = File.ReadAllText(prelimsSheetPath).Replace("\n", "").Replace("\r", "");
            _finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
        }

        public Competition GetCompetition(Division division)
        {
            var sub = GetFinalsDocByDivision(division);
            var judges = GetJudgesByDivision(division);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("tr");

            var scores = new List<Score>();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i].SelectNodes("td");

                string actualPlacementString = node.Where(n => n.GetClasses().Contains("r")).FirstOrDefault().InnerText;
                int actualPlacement = Int32.Parse(actualPlacementString.Remove(actualPlacementString.Length - 2));

                string leaderName = node.Where(n => n.GetClasses().Contains("nolines") && n.GetClasses().Contains("name")).FirstOrDefault().InnerText;
                int leadPos = leaderName.IndexOf(' ');
                string leaderFirstName = leaderName.Substring(0, leadPos);
                string leaderLastName = leaderName.Substring(leadPos + 1);

                string followerName = node.Where(n => n.GetClasses().Contains("nolines") && n.GetClasses().Contains("name") && n.GetClasses().Contains("heavyborder")).FirstOrDefault().InnerText;
                int followPos = followerName.IndexOf(' ');
                string followerFirstName = followerName.Substring(0, followPos);
                string followerLastName = followerName.Substring(followPos + 1);

                for (int j = 3; j < judges.Count + 3; j++)
                {
                    int placement;

                    try
                    {
                        placement = Int32.Parse(node[j].InnerText);
                    }
                    catch
                    {
                        placement = Int32.Parse(node[j].InnerText.Substring(0, 1));
                    }

                    var score = new Score(judges[j - 3], placement, actualPlacement)
                    {
                        Leader = new Competitor(leaderFirstName, leaderLastName),
                        Follower = new Competitor(followerFirstName, followerLastName),
                    };

                    scores.Add(score);

                    if (judges[j - 3].Scores == null)
                        judges[j - 3].Scores = new List<Score>();

                    judges[j - 3].Scores.Add(score);
                }
            }

            #region Output
            Console.WriteLine("Division: " + division);
            Console.WriteLine("Judges: ");
            foreach (var j in judges)
            {
                Console.WriteLine(j.FirstName);
            }
            Console.WriteLine("");
            Console.WriteLine("Scores: ");
            foreach (var score in scores)
            {
                Console.WriteLine("{0}: {1} & {2} ({3}: {4} ({5}%))",
                    score.ActualPlacement,
                    score.Leader.FirstName,
                    score.Follower.FirstName,
                    score.Judge.FullName,
                    score.Placement,
                    score.Accuracy * 100);
            }
            Console.WriteLine("");
            #endregion

            return new Competition(division)
            {
                Scores = scores
            };
        }

        public List<Division> GetDivisions()
        {
            var divisions = new List<Division>();

            var finals = Util.GetSubString(
                s: _finalsSheetDoc,
                from: "<div class=\"span9\"><div class=\"well clearfix\">",
                to: "</tbody></table></div>");

            var divisionString = Util.GetSubString(finals, "<h3>", "</h3>");

            if (divisionString.Contains("Masters"))
                return divisions;

            if (divisionString.Contains("Newcomer"))
                divisions.Add(Division.Newcomer);
            else if (divisionString.Contains("Novice"))
                divisions.Add(Division.Novice);
            else if (divisionString.Contains("Intermediate"))
                divisions.Add(Division.Intermediate);
            else if (divisionString.Contains("Advanced"))
                divisions.Add(Division.Advanced);
            else if (divisionString.Contains("All-Star"))
                divisions.Add(Division.AllStar);
            else if (divisionString.Contains("All Star"))
                divisions.Add(Division.AllStar);
            else if (divisionString.Contains("Champion"))
                divisions.Add(Division.Champion);
            else if (divisionString.Contains("Invitational"))
                divisions.Add(Division.Open);
            else
                divisions.Add(Division.Open);

            return divisions;
        }

        public List<Judge> GetJudgesByDivision(Division division)
        {
            var judges = new List<Judge>();

            string sub = Util.GetSubString(
                s: _finalsSheetDoc,
                from: "<div><b>Judges: </b>",
                to: "<div class=\"judge_note\">");

            List<string> names = sub.Split(',').ToList();
            
            // because step right solutions anonymizes judges, we are going to return anonymous judges
            for (int i = 1; i <= names.Count; i++)
            {
                judges.Add(new Judge("Anonymous", i.ToString()));
            }

            //foreach (var name in names)
            //{
            //    int pos = name.Trim().IndexOf(' ');

            //    //no last name was recorded
            //    if (pos == -1)
            //    {
            //        judges.Add(new Judge(name.Trim(), string.Empty));
            //    }
            //    else
            //    {
            //        judges.Add(new Judge(name.Trim().Substring(0, pos), name.Trim().Substring(pos + 1)));
            //    }
            //}

            return judges;
        }

        private string GetFinalsDocByDivision(Division division)
        {
            Division div;
            var finals = Util.GetSubString(
                s: _finalsSheetDoc,
                from: "<div class=\"span9\"><div class=\"well clearfix\">",
                to: "</tbody></table></div>");

            var divisionString = Util.GetSubString(finals, "<h3>", "</h3>");

            finals = finals.Substring(finals.IndexOf("<th rowspan=\"1\">Placement</th></tr></thead><tbody>") + new string("<th rowspan =\"1\">Placement</th></tr></thead><tbody>").Length - 1);

            if (divisionString.Contains("Newcomer"))
                div = Division.Newcomer;
            else if (divisionString.Contains("Novice"))
                div = Division.Novice;
            else if (divisionString.Contains("Intermediate"))
                div = Division.Intermediate;
            else if (divisionString.Contains("Advanced"))
                div = Division.Advanced;
            else if (divisionString.Contains("All-Star"))
                div = Division.AllStar;
            else if (divisionString.Contains("All Star"))
                div = Division.AllStar;
            else if (divisionString.Contains("Champion"))
                div = Division.Champion;
            else if (divisionString.Contains("Invitational"))
                div = Division.Open;
            else
                div = Division.Open;

            if (division == div)
                return finals;

            if (divisionString.Contains("Masters"))
                return "";

            return "";
        }
    }
}
