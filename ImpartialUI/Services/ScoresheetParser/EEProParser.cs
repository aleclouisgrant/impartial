using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Impartial;
using Impartial.Enums;
using ImpartialUI.Models;

namespace ImpartialUI.Services.ScoresheetParser
{
    public class EEProParser : ScoresheetParserBase
    {
        public EEProParser(string prelimsPath = null, string finalsPath = null) : base(prelimsPath, finalsPath) { }

        public override List<Division> GetDivisions()
        {
            var divisions = new List<Division>();

            int c = 1;

            while (c < 30)
            {
                Division div;
                var finals = Util.GetSubStringN(
                    s: FinalsSheetDoc,
                    from: "<tr bgcolor=\"#ffae5e\">",
                    to: "</tr></tbody></table><p>",
                    n: c++);

                string divisionString = "";

                try
                {
                    divisionString = Util.GetSubString(finals, "<td colspan=\"13\">Division", "</td></tr><tr>");
                }
                catch
                {
                    try
                    {
                        divisionString = Util.GetSubString(finals, "<td colspan=\"15\">Division", "</td></tr><tr>");
                    }
                    catch
                    {
                        return divisions;
                    }
                }

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
                else if (divisionString.Contains("Allstar"))
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

        public override IPrelimCompetition? GetPrelimCompetition(Division division, Round round, Role role)
        {
            string prelimsHtml = GetPrelimsDocByDivision(division, role, round);
            if (prelimsHtml == string.Empty)
            {
                return null;
            }

            var prelimCompetition = new PrelimCompetition(
                dateTime: DateTime.MinValue,
                division: division,
                round: round,
                role: role,
                prelimScores: new List<IPrelimScore>(),
                promotedCompetitors: new List<ICompetitor>(),
                alternate1: null,
                alternate2: null);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(prelimsHtml);
            var nodes = doc.DocumentNode.SelectNodes("tr");
            var judges = GetPrelimsJudgesByDivision(division, role, round);

            if (nodes != null)
                nodes.RemoveAt(0);

            foreach (var node in nodes)
            {
                var nodeCollection = node.SelectNodes("td");

                bool finaled = nodeCollection[5 + judges.Count].InnerText == "X" && nodeCollection[6 + judges.Count].InnerText == "";
                string name = nodeCollection[1].InnerText;
                int pos = name.IndexOf(' ');

                string firstName = "";
                string lastName = "";

                if (pos == -1)
                {
                    firstName = name;
                }
                else
                {
                    firstName = name.Substring(0, pos).Trim();
                    lastName = name.Substring(pos + 1).Trim();
                }

                var competitor = new Competitor(firstName, lastName);

                CallbackScore callbackScore;
                int offset = 2;
                for (int i = offset; i < judges.Count + offset; i++)
                {
                    callbackScore = Util.StringToCallbackScore(nodeCollection[i].InnerText);

                    var prelimScore = new PrelimScore(
                        judge: judges[i - offset],
                        competitor: competitor,
                        callbackScore: callbackScore);

                    prelimCompetition.PrelimScores.Add(prelimScore);
                }

                if (finaled)
                {
                    prelimCompetition.PromotedCompetitors.Add(competitor);
                }
            }

            return prelimCompetition;
        }

        public override IFinalCompetition? GetFinalCompetition(Division division) 
        {
            var sub = GetFinalsDocByDivision(division);
            if (sub == string.Empty)
                return null;

            var judges = GetFinalsJudgesByDivision(division);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("tr");

            var leaders = new List<ICompetitor>();
            var followers = new List<ICompetitor>();

            var scores = new List<IFinalScore>();
            for (int i = 1; i < nodes.Count; i++)
            {
                var node = nodes[i].SelectNodes("td");
                int placement = int.Parse(node[0].InnerText);

                for (int j = 2; j < judges.Count + 2; j++)
                {
                    int score;

                    try
                    {
                        score = int.Parse(node[j].InnerText);
                    }
                    catch
                    {
                        score = int.Parse(node[j].InnerText.Substring(0, 1));
                    }

                    string leaderName = node[1].InnerText.Substring(0, node[1].InnerText.IndexOf(" and "));
                    int leadPos = leaderName.IndexOf(' ');
                    string leaderFirstName = leaderName.Substring(0, leadPos).Trim();
                    string leaderLastName = leaderName.Substring(leadPos + 1).Trim();

                    ICompetitor leader = leaders.Where(c => c.FullName == leaderFirstName + " " + leaderLastName)?.FirstOrDefault();
                    if (leader == null)
                    {
                        leader = new Competitor(leaderFirstName, leaderLastName);
                        leaders.Add(leader);
                    }

                    string followerName = node[1].InnerText.Substring(node[1].InnerText.IndexOf(" and ") + " and ".Length);
                    int followPos = followerName.IndexOf(' ');
                    string followerFirstName = followerName.Substring(0, followPos).Trim();
                    string followerLastName = followerName.Substring(followPos + 1).Trim();

                    ICompetitor follower = followers.Where(c => c.FullName == followerFirstName + " " + followerLastName)?.FirstOrDefault();
                    if (follower == null)
                    {
                        follower = new Competitor(followerFirstName, followerLastName);
                        followers.Add(follower);
                    }

                    var finalScore = new FinalScore(
                        judge: judges[j - 2],
                        leader: leader,
                        follower: follower,
                        score: score, 
                        placement: placement);

                    scores.Add(finalScore);

                    if (judges[j - 2].Scores == null)
                        judges[j - 2].Scores = new List<IFinalScore>();

                    judges[j - 2].Scores.Add(finalScore);
                }
            }

            return new FinalCompetition(dateTime: DateTime.MinValue, division: division, finalScores: scores);
        }

        private List<IJudge> GetPrelimsJudgesByDivision(Division division, Role role, Round round)
        {
            var judges = new List<IJudge>();
            string sub = Util.GetSubString(
                    s: GetPrelimsDocByDivision(division, role, round),
                    from: "<td><em><strong>Count</strong></em></td>",
                    to: "<td><em><strong>BIB</strong></em></td>");

            if (sub == string.Empty)
                throw new DivisionNotFoundException(division);

            var doc = new HtmlDocument();
            doc.LoadHtml(sub);
            var nodes = doc.DocumentNode.SelectNodes("td");

            if (nodes != null)
                nodes.RemoveAt(0);

            foreach (var node in nodes)
            {
                var name = node.InnerText.Trim();
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
        private List<IJudge> GetFinalsJudgesByDivision(Division division)
        {
            var judges = new List<IJudge>();

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

        private string GetPrelimsDocByDivision(Division division, Role role, Round round)
        {
            for (int c = 1; c < 30; c++)
            {
                var sheet = Util.GetSubStringN(
                    s: PrelimsSheetDoc,
                    from: "<tr bgcolor=\"#ffae5e\">",
                    to: "</tr></tbody></table><p>",
                    n: c);

                string titleString = string.Empty;

                titleString = Util.GetSubString(sheet, "Jack &amp; Jill", "</td></tr><tr>");

                Trace.WriteLine("TITLE: " + titleString);

                if (titleString == string.Empty)
                    return string.Empty;

                Division? div;
                Role? rl;
                Round? rnd;

                rnd = Util.ContainsRoundString(titleString);

                if (round == Round.Prelims)
                {
                    if (rnd == Round.Semifinals || rnd == Round.Quarterfinals)
                        continue;
                }
                else if (round == Round.Semifinals)
                {
                    if (rnd == Round.Prelims || rnd == Round.Quarterfinals)
                        continue;
                }
                else if (round == Round.Quarterfinals)
                {
                    if (rnd == Round.Prelims || rnd == Round.Semifinals)
                        continue;
                }

                div = Util.ContainsDivisionString(titleString);
                rl = Util.ContainsRoleString(titleString);

                if (div == division && rl == role)
                    return sheet;
            }
            return string.Empty;
        }
        private string GetFinalsDocByDivision(Division division)
        {
            for (int c = 1; c < 30; c++)
            {
                var finals = Util.GetSubStringN(
                    s: FinalsSheetDoc,
                    from: "<tr bgcolor=\"#ffae5e\">",
                    to: "</tr></tbody></table><p>",
                    n: c);

                string titleString = string.Empty;

                titleString = Util.GetSubString(finals, "Division", "</td></tr><tr>");
                
                if (titleString.Contains("Masters"))
                    continue;

                Division? div;

                div = Util.ContainsDivisionString(titleString);

                if (division == div)
                    return finals;
            }

            return string.Empty;
        }

        public override string GetName()
        {
            if (FinalsSheetDoc == null)
                return string.Empty;

            string name = Util.GetSubString(
                s: FinalsSheetDoc,
                from: "<h2><center>",
                to: "</center></h2>");

            if (name.Contains("- "))
            {
                name = name.Replace("- ", "");
            }

            return name;
        }
    }
}
