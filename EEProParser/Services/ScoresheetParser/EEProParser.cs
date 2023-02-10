using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using Impartial.Enums;

namespace Impartial
{
    public class EEProParser : IScoresheetParser
    {
        private string _prelimsSheetDoc { get; set; }
        private string _finalsSheetDoc { get; set; }

        public EEProParser(string prelimsPath, string finalsPath)
        {
            if (prelimsPath == null || prelimsPath == String.Empty || !File.Exists(prelimsPath))
                throw new FileNotFoundException();
            if (finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath))
                throw new FileNotFoundException();

            _prelimsSheetDoc = File.ReadAllText(prelimsPath).Replace("\n", "").Replace("\r", "");
            _finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
        }

        public EEProParser(string finalsPath)
        {
            if (finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath))
                throw new FileNotFoundException();

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
                else if (divisionString.Contains("Open"))
                    div = Division.Open;
                else if (divisionString.Contains("Invitational"))
                    div = Division.Open;
                else
                    continue;

                divisions.Add(div);
            }

            return divisions;
        }

        public Competition GetCompetition(Division division)
        {
            var prelims = new Tuple<List<PrelimScore>, List<PrelimScore>>(null, null);
            if (_prelimsSheetDoc != null)
            {
                prelims = GetPrelimScores(division);
            }

            var sub = GetFinalsDocByDivision(division);
            var judges = GetFinalsJudgesByDivision(division);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("tr");

            var scores = new List<Score>();
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
                Scores = scores,
                LeaderPrelimScores = prelims.Item1 ?? new List<PrelimScore>(),
                FollowerPrelimScores = prelims.Item2 ?? new List<PrelimScore>(),
            };
        }
        
        private Tuple<List<PrelimScore>, List<PrelimScore>> GetPrelimScores(Division division)
        {
            var leadsPrelims = GetPrelimsDocByDivision(division, Role.Leader);
            HtmlDocument leadsDoc = new HtmlDocument();
            leadsDoc.LoadHtml(leadsPrelims);
            var leadNodes = leadsDoc.DocumentNode.SelectNodes("tr");
            var leadJudges = GetPrelimsJudgesByDivision(division, Role.Leader);

            List<PrelimScore> leaderPrelimScores = new List<PrelimScore>();
            foreach (var lead in leadNodes)
            {
                var node = lead.SelectNodes("td");
                bool finaled = lead.GetClasses().Contains("adv");
                string name = node[1].InnerText;
                int pos = name.IndexOf(' ');
                var competitor = new Competitor(name.Substring(0, pos), name.Substring(pos + 1));

                CallbackScore callbackScore;
                int offset = 2;
                for (int i = offset; i < leadJudges.Count + offset; i++)
                {
                    try
                    {
                        callbackScore = Util.NumberToCallbackScore(Double.Parse(node[i].InnerText));
                    }
                    catch
                    {
                        callbackScore = Util.NumberToCallbackScore(Double.Parse(node[i].InnerText.Substring(0, 1)));
                    }

                    var prelimScore = new PrelimScore(
                        role: Role.Leader,
                        judge: leadJudges[i - offset],
                        competitor: competitor,
                        finaled: finaled,
                        callbackScore: callbackScore);

                    leaderPrelimScores.Add(prelimScore);
                }
            }

            var followsPrelims = GetPrelimsDocByDivision(division, Role.Follower);
            HtmlDocument followsDoc = new HtmlDocument();
            followsDoc.LoadHtml(followsPrelims);
            var followerNodes = followsDoc.DocumentNode.SelectNodes("tr");
            var followerJudges = GetPrelimsJudgesByDivision(division, Role.Follower);

            List<PrelimScore> followerPrelimScores = new List<PrelimScore>();
            foreach (var follow in followerNodes)
            {
                var node = follow.SelectNodes("td");
                bool finaled = follow.GetClasses().Contains("adv");
                string name = node[1].InnerText;
                int pos = name.IndexOf(' ');
                var competitor = new Competitor(name.Substring(0, pos), name.Substring(pos + 1));

                CallbackScore callbackScore;
                int offset = 2;
                for (int i = offset; i < followerJudges.Count + offset; i++)
                {
                    try
                    {
                        callbackScore = Util.NumberToCallbackScore(Double.Parse(node[i].InnerText));
                    }
                    catch
                    {
                        callbackScore = Util.NumberToCallbackScore(Double.Parse(node[i].InnerText.Substring(0, 1)));
                    }

                    var prelimScore = new PrelimScore(
                        role: Role.Follower,
                        judge: followerJudges[i - offset],
                        competitor: competitor,
                        finaled: finaled,
                        callbackScore: callbackScore);

                    followerPrelimScores.Add(prelimScore);
                }
            }

            return new Tuple<List<PrelimScore>, List<PrelimScore>>(leaderPrelimScores, followerPrelimScores);
        }

        private List<Judge> GetPrelimsJudgesByDivision(Division division, Role role)
        {
            var judges = new List<Judge>();
            string sub = string.Empty;

            if (role == Role.Leader)
            {
                sub = Util.GetSubString(
                    s: _prelimsSheetDoc,
                    from: "<div><b>Judges: </b>",
                    to: "</div><div class=\"judge_note\">");
            }
            else if (role == Role.Follower)
            {
                sub = Util.GetSubString(
                    s: _prelimsSheetDoc.Substring(_prelimsSheetDoc.IndexOf("<h3 id=\"followers\">Followers")),
                    from: "<div><b>Judges: </b>",
                    to: "<div class=\"judge_note\">");
            }

            List<string> names = sub.Split(',').ToList();

            foreach (var name in names)
            {
                int pos = name.Trim().IndexOf(' ');

                //no last name was recorded
                if (pos == -1)
                {
                    judges.Add(new Judge(name.Trim(), string.Empty));
                }
                else
                {
                    judges.Add(new Judge(name.Trim().Substring(0, pos), name.Trim().Substring(pos + 1)));
                }
            }

            return judges;
        }
        private List<Judge> GetFinalsJudgesByDivision(Division division)
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
