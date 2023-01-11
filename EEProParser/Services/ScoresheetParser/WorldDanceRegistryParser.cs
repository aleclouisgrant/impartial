using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Impartial.Services.ScoresheetParser
{
    public class WorldDanceRegistryParser : IScoresheetParser
    {
        private string _finalsSheetDoc { get; set; }

        public List<Judge> Judges { get; set; }
        public List<Score> Scores { get; set; }

        public WorldDanceRegistryParser(string finalsPath)
        {
            if (finalsPath == null || finalsPath == String.Empty)
                return;

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
                int actualPlacement = Int32.Parse(node[3].InnerText);

                string leaderName = node[1].InnerText;
                int leadPos = leaderName.IndexOf(' ');
                string leaderFirstName = leaderName.Substring(0, leadPos);
                string leaderLastName = leaderName.Substring(leadPos + 1);

                string followerName = node[2].InnerText;
                int followPos = followerName.IndexOf(' ');
                string followerFirstName = followerName.Substring(0, followPos);
                string followerLastName = followerName.Substring(followPos + 1);

                // scores start on the 5th node
                int offset = 4;
                for (int j = offset; j < judges.Count + offset; j++)
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

                    var score = new Score(judges[j - offset], placement, actualPlacement)
                    {
                        Leader = new Competitor(leaderFirstName, leaderLastName),
                        Follower = new Competitor(followerFirstName, followerLastName),
                    };

                    scores.Add(score);

                    if (judges[j - offset].Scores == null)
                        judges[j - offset].Scores = new List<Score>();

                    judges[j - offset].Scores.Add(score);
                }
            }

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
                from: "<div class=\"pb-4\"><br>",
                to: "</tbody></table></div>");

            var divisionString = Util.GetSubString(finals, "<h3 class=\"text-center\">", "</h3>");

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
                from: "</a></caption><thead><tr>",
                to: "</tr>");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("td");

            if (nodes == null) //division may not exist
                return judges;

            // get rid of first 4 unnecessary nodes
            nodes.RemoveAt(0);
            nodes.RemoveAt(0);
            nodes.RemoveAt(0);
            nodes.RemoveAt(0);

            foreach (var node in nodes)
            {
                string name = node.InnerText.Replace("&nbsp;", "");

                // reached the end of the judges names
                if (name == "1--1")
                    break;

                int pos = name.IndexOf(' ');

                //no last name was recorded
                if (pos == -1)
                {
                    judges.Add(new Judge(name, string.Empty));
                }
                else
                {
                    judges.Add(new Judge(name.Substring(0, pos), name.Substring(pos + 1)));
                }
            }

            return judges;
        }

        private string GetFinalsDocByDivision(Division division)
        {
            Division div;
            var finals = Util.GetSubString(
                s: _finalsSheetDoc,
                from: "<div class=\"pb-4\"><br>",
                to: "</tbody></table></div>");

            var divisionString = Util.GetSubString(finals, "<h3 class=\"text-center\">", "</h3>");

            finals = finals.Substring(finals.IndexOf("</tr></thead><tbody>") + new string("</tr></thead><tbody>").Length);

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
