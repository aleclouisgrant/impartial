using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;

namespace Impartial
{
    public class EEProParser : IScoresheetParser
    {
        private string _prelimsSheetDoc { get; set; }
        private string _finalsSheetDoc { get; set; }

        public List<Judge> Judges { get; set; }
        public List<Score> Scores { get; set; }

        public EEProParser() { }

        public EEProParser(string prelimsSheetPath, string finalsPath)
        {
            //if (prelimsSheetPath == null || prelimsSheetPath == String.Empty)
            //    return;
            if (finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath))
                return;

            //_prelimsSheetDoc = File.ReadAllText(prelimsSheetPath).Replace("\n", "").Replace("\r", "");
            _finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
        }

        public List<Division> GetDivisions()
        {
            var divisions = new List<Division>();

            int c = 1;

            while (c < 21)
            {
                Division div;
                var finals = Util.GetSubStringN(
                    s: _finalsSheetDoc,
                    from: "<tr bgcolor=\"#ffae5e\">",
                    to: "</tr></tbody></table><p>",
                    n: c++);

                var divisionString = Util.GetSubString(finals, "<td colspan=\"13\">Division", "</td></tr><tr>");

                if (divisionString.Contains("Masters"))
                    continue;

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
                    continue;

                divisions.Add(div);
            }

            return divisions;
        }

        public List<Judge> GetJudgesByDivision(Division division)
        {
            var judges = new List<Judge>();
            
            string sheet = GetFinalsDocByDivision(division);
            string sub = Util.GetSubString(
                s: sheet,
                from: "<td><em><strong>Competitor</strong></em></td>",
                to: "<td><em><strong>BIB</strong></em></td>");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("td/em/strong");

            if (nodes == null) //division may not exist
                return judges;

            foreach (var node in nodes)
            {
                string name = node.InnerText.Replace("&nbsp;", "");
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
        public Competition GetCompetition(Division division)
        {
            //var leadsPrelims = GetPrelimsDocByDivision(division, Role.Leader);
            //HtmlDocument leadsDoc = new HtmlDocument();
            //leadsDoc.LoadHtml(leadsPrelims);
            //var leadNodes = leadsDoc.DocumentNode.SelectNodes("tr");
            //leadNodes.RemoveAt(0);

            //var followsPrelims = GetPrelimsDocByDivision(division, Role.Follower);
            //HtmlDocument followsDoc = new HtmlDocument();
            //followsDoc.LoadHtml(followsPrelims);
            //var followNodes = followsDoc.DocumentNode.SelectNodes("tr");
            //followNodes.RemoveAt(0);

            var sub = GetFinalsDocByDivision(division);
            var judges = GetJudgesByDivision(division);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("tr");

            var scores = new List<Score>();

            //used for weighted ranking with points
            //int totalCouples = leadNodes.Count > followNodes.Count ? leadNodes.Count : followNodes.Count;

            for (int i = 1; i < nodes.Count; i++)
            {
                var node = nodes[i].SelectNodes("td");
                int actualPlacement = Int32.Parse(node[0].InnerText);

                for (int j = 2; j < judges.Count + 2; j++)
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

                    string leaderName = node[1].InnerText.Substring(0, node[1].InnerText.IndexOf(" and "));
                    int leadPos = leaderName.IndexOf(' ');

                    string followerName = node[1].InnerText.Substring(node[1].InnerText.IndexOf(" and ") + " and ".Length);
                    int followPos = followerName.IndexOf(' ');

                    var score = new Score(judges[j - 2], placement, actualPlacement)
                    {
                        Leader = new Competitor(leaderName.Substring(0, leadPos), leaderName.Substring(leadPos + 1)),
                        Follower = new Competitor(followerName.Substring(0, followPos), followerName.Substring(followPos + 1)),
                    };

                    scores.Add(score);
                    
                    if (judges[j - 2].Scores == null)
                        judges[j - 2].Scores = new List<Score>();

                    judges[j - 2].Scores.Add(score);
                }
            }

            return new Competition(division)
            {
                Scores = scores
            };
        }

        private string GetPrelimsDocByDivision(Division division, Role role)
        {
            int c = 1;

            while (true)
            {
                Division div;
                var prelims = Util.GetSubStringN(
                    s: _prelimsSheetDoc,
                    from: "<tr bgcolor=\"#ffae5e\">",
                    to: "</tr></tbody></table><p>",
                    n: c++);

                var divisionString = Util.GetSubString(prelims, "<td colspan=\"11\">", "</td></tr><tr>");

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
                else
                    div = Division.Open;

                Role r;
                if (divisionString.Contains("Leader"))
                    r = Role.Leader;
                else if (divisionString.Contains("Follower"))
                    r = Role.Follower;
                else
                    r = Role.None;

                if (division == div && r == role)
                    return prelims;

                if (c > 20)
                    return "";
            }
        }
        private string GetFinalsDocByDivision(Division division)
        {
            int c = 1;

            while (true)
            {
                Division div;
                var finals = Util.GetSubStringN(
                    s: _finalsSheetDoc,
                    from: "<tr bgcolor=\"#ffae5e\">",
                    to: "</tr></tbody></table><p>",
                    n: c++);

                var divisionString = Util.GetSubString(finals, "<td colspan=\"13\">Division", "</td></tr><tr>");

                if (divisionString.Contains("Masters"))
                    continue;

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

                if (c > 20)
                    return "";
            }
        }
    }
}
