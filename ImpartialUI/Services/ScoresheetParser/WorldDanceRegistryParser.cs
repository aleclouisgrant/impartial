using HtmlAgilityPack;
using Impartial;
using Impartial.Enums;
using ImpartialUI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ImpartialUI.Services.ScoresheetParser
{
    public class WorldDanceRegistryParser : ScoresheetParserBase
    {
        //TODO: add semis and quarters sheets
        public WorldDanceRegistryParser(string prelimsPath = null, string finalsPath = null) : base(prelimsPath, finalsPath) { }

        public override List<Division> GetDivisions()
        {
            var divisions = new List<Division>();

            var finals = Util.GetSubString(
                s: FinalsSheetDoc,
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

        public override ICompetition GetCompetition(Division division)
        {
            var competition = new Competition(danceConventionId: Guid.Empty, name: GetName(), date: DateTime.MinValue, division: division)
            {
                FinalCompetition = GetFinalCompetition(division),
                PairedPrelimCompetitions = new List<IPairedPrelimCompetition>()
            };

            var prelims = GetPairedPrelimCompetition(division, Round.Prelims);

            if (prelims.LeaderPrelimCompetition != null)
                competition.PairedPrelimCompetitions.Add(prelims);

            return competition;
        }

        public override IPrelimCompetition GetPrelimCompetition(Division division, Round round, Role role)
        {
            var prelims = GetPrelimsDocByDivision(division, role, round);
            if (prelims == string.Empty)
                return null;

            var prelimCompetition = new PrelimCompetition(
                dateTime: DateTime.MinValue,
                division: division,
                round: round,
                role: role,
                prelimScores: new List<IPrelimScore>(),
                promotedCompetitors: new List<ICompetitor>());

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(prelims);
            var nodes = doc.DocumentNode.SelectNodes("tr");
            var judges = GetPrelimsJudgesByDivision(division, role, round);

            int redactedCompetitors = 0;

            foreach (var node in nodes)
            {
                var nodeCollection = node.SelectNodes("td");
                
                bool finaled = nodeCollection[0].InnerText == "Y";
                string name = nodeCollection[3].InnerText;
                int pos = name.IndexOf(' ');

                string firstName, lastName = string.Empty;

                if (name == "***")
                {
                    firstName = "REDACTED";
                    lastName = (++redactedCompetitors).ToString();
                }
                else
                {
                    firstName = name.Substring(0, pos).Trim();
                    lastName = name.Substring(pos + 1).Trim();
                }

                var competitor = new Competitor(firstName, lastName);

                CallbackScore callbackScore;
                int offset = 4;
                for (int i = offset; i < judges.Count + offset; i++)
                {
                    try
                    {
                        callbackScore = Util.NumberToCallbackScore(Double.Parse(nodeCollection[i].InnerText));
                    }
                    catch
                    {
                        callbackScore = Util.NumberToCallbackScore(Double.Parse(nodeCollection[i].InnerText.Substring(0, 1)));
                    }

                    var prelimScore = new PrelimScore(
                        judge: judges[i - offset],
                        competitor: competitor,
                        callbackScore: callbackScore);

                    prelimCompetition.PrelimScores.Add(prelimScore);
                }

                if (finaled)
                    prelimCompetition.PromotedCompetitors.Add(competitor);
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
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i].SelectNodes("td");
                int placement = Int32.Parse(node[3].InnerText);

                string leaderName = node[1].InnerText;
                int leadPos = leaderName.IndexOf(' ');
                string leaderFirstName = leaderName.Substring(0, leadPos).Trim();
                string leaderLastName = leaderName.Substring(leadPos + 1).Trim();

                ICompetitor leader = leaders.Where(c => c.FullName == leaderFirstName + " " + leaderLastName)?.FirstOrDefault();
                if (leader == null)
                {
                    leader = new Competitor(leaderFirstName, leaderLastName);
                    leaders.Add(leader);
                }

                string followerName = node[2].InnerText;
                int followPos = followerName.IndexOf(' ');
                string followerFirstName = followerName.Substring(0, followPos).Trim();
                string followerLastName = followerName.Substring(followPos + 1).Trim();

                ICompetitor follower = followers.Where(c => c.FullName == followerFirstName + " " + followerLastName)?.FirstOrDefault();
                if (follower == null)
                {
                    follower = new Competitor(followerFirstName, followerLastName);
                    followers.Add(follower);
                }

                // scores start on the 5th node
                int offset = 4;
                for (int j = offset; j < judges.Count + offset; j++)
                {
                    int score;

                    try
                    {
                        score = Int32.Parse(node[j].InnerText);
                    }
                    catch
                    {
                        score = Int32.Parse(node[j].InnerText.Substring(0, 1));
                    }

                    var finalScore = new FinalScore(
                        judge:judges[j - offset],
                        leader: leader,
                        follower: follower,
                        score: score, 
                        placement: placement);

                    scores.Add(finalScore);

                    if (judges[j - offset].Scores == null)
                        judges[j - offset].Scores = new List<IFinalScore>();

                    judges[j - offset].Scores.Add(finalScore);
                }
            }
            return new FinalCompetition(dateTime: DateTime.MinValue, division: division, finalScores: scores);
        }

        private List<IJudge> GetPrelimsJudgesByDivision(Division division, Role role, Round round)
        {
            var judges = new List<IJudge>();
            string sub = string.Empty;

            if (role == Role.Leader)
            {
                sub = Util.GetSubString(
                    s: PrelimsSheetDoc,
                    from: "</td>",
                    to: "<td class=\"fw-bold td-grey bg-darken-md\">Scores Sum");
            }
            else if (role == Role.Follower)
            {
                sub = Util.GetSubString(
                    s: PrelimsSheetDoc.Substring(PrelimsSheetDoc.IndexOf("<td class=\"fw-bold td-grey bg-darken-md text-start\">Followers")),
                    from: "</td>",
                    to: "<td class=\"fw-bold td-grey bg-darken-md\">Scores Sum");
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("td");

            if (nodes == null) //division may not exist
                return judges;

            if (role == Role.Leader)
            {
                //delete unnecessary first 3 nodes
                nodes.RemoveAt(0);
                nodes.RemoveAt(0);
                nodes.RemoveAt(0);
            }

            foreach (var node in nodes)
            {
                string name = node.InnerText.Trim();
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

            string sub = Util.GetSubString(
                s: FinalsSheetDoc,
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

        private string GetPrelimsDocByDivision(Division division, Role role, Round round)
        {
            Division div;
            var doc = Util.GetSubString(
                s: PrelimsSheetDoc,
                from: "<div class=\"pb-4\"><br>",
                to: "</tbody></table></div>");

            var divisionString = Util.GetSubString(doc, "<h3 class=\"text-center\">", "</h3>");

            doc = doc.Substring(doc.IndexOf("</tr></thead><tbody>") + new string("</tr></thead><tbody>").Length);

            if (divisionString.Contains("Masters"))
                return string.Empty;

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

            if (division != div)
                return string.Empty;

            string prelims = string.Empty;

            if (role == Role.Leader)
            {
                prelims = Util.GetSubString(
                    s: PrelimsSheetDoc,
                    from: "<td class=\"fw-bold td-grey bg-darken-md text-start\">Leaders",
                    to: "</tbody></table>");
            }
            else if (role == Role.Follower)
            {
                prelims = Util.GetSubString(
                    s: PrelimsSheetDoc,
                    from: "<td class=\"fw-bold td-grey bg-darken-md text-start\">Followers",
                    to: "</tbody></table></div></div>");
            }

            return prelims.Substring(prelims.IndexOf("</tr></thead><tbody>") + new string("</tr></thead><tbody>").Length - 1);
        }
        private string GetFinalsDocByDivision(Division division)
        {
            Division div;
            var finals = Util.GetSubString(
                s: FinalsSheetDoc,
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

        public override string GetName()
        {
            if (FinalsSheetDoc == null)
                return string.Empty;

            string name = Util.GetSubString(
                s: FinalsSheetDoc,
                from: "<title>",
                to: "</title>").Trim();

            if (name.EndsWith(" - Scores"))
            {
                name = name.Substring(0, name.LastIndexOf(" - Scores"));
            }

            return name;
        }
    }
}
