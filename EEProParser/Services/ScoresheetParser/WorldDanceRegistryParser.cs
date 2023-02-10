using HtmlAgilityPack;
using Impartial.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Impartial.Services.ScoresheetParser
{
    public class WorldDanceRegistryParser : IScoresheetParser
    {
        private string _prelimsSheetDoc { get; set; }
        private string _finalsSheetDoc { get; set; }

        public WorldDanceRegistryParser(string prelimsPath, string finalsPath)
        {
            if (prelimsPath == null || prelimsPath == String.Empty || !File.Exists(prelimsPath))
                throw new FileNotFoundException();
            if (finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath))
                throw new FileNotFoundException();

            _prelimsSheetDoc = File.ReadAllText(prelimsPath).Replace("\n", "").Replace("\r", "");
            _finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
        }

        public WorldDanceRegistryParser(string finalsPath)
        {
            if (finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath))
                throw new FileNotFoundException();

            _finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
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
            var leaderJudges = GetPrelimsJudgesByDivision(division, Role.Leader);

            int redactedLeads = 0;

            List<PrelimScore> leaderPrelimScores = new List<PrelimScore>();
            foreach (var lead in leadNodes)
            {
                var node = lead.SelectNodes("td");
                
                bool finaled = node[0].InnerText == "Y";
                string name = node[3].InnerText;
                int pos = name.IndexOf(' ');

                string firstName, lastName = string.Empty;

                if (name == "***")
                {
                    firstName = "REDACTED";
                    lastName = (++redactedLeads).ToString();
                }
                else
                {
                    firstName = name.Substring(0, pos);
                    lastName = name.Substring(pos + 1);
                }

                var competitor = new Competitor(firstName, lastName);

                CallbackScore callbackScore;
                int offset = 4;
                for (int i = offset; i < leaderJudges.Count + offset; i++)
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
                        judge: leaderJudges[i - offset],
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

            int redactedFollows = 0;

            List<PrelimScore> followerPrelimScores = new List<PrelimScore>();
            foreach (var follow in followerNodes)
            {
                var node = follow.SelectNodes("td");

                bool finaled = node[0].InnerText == "Y";
                string name = node[3].InnerText;
                int pos = name.IndexOf(' ');

                string firstName, lastName = string.Empty;

                if (name == "***")
                {
                    firstName = "REDACTED";
                    lastName = (++redactedFollows).ToString();
                }
                else
                {
                    firstName = name.Substring(0, pos);
                    lastName = name.Substring(pos + 1);
                }

                var competitor = new Competitor(firstName, lastName);

                CallbackScore callbackScore;
                int offset = 4;
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
                    from: "</td>",
                    to: "<td class=\"fw-bold td-grey bg-darken-md\">Scores Sum");
            }
            else if (role == Role.Follower)
            {
                sub = Util.GetSubString(
                    s: _prelimsSheetDoc.Substring(_prelimsSheetDoc.IndexOf("<td class=\"fw-bold td-grey bg-darken-md text-start\">Followers")),
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
        private List<Judge> GetFinalsJudgesByDivision(Division division)
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

        private string GetPrelimsDocByDivision(Division division, Role role)
        {
            Division div;
            var doc = Util.GetSubString(
                s: _prelimsSheetDoc,
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
                    s: _prelimsSheetDoc,
                    from: "<td class=\"fw-bold td-grey bg-darken-md text-start\">Leaders",
                    to: "</tbody></table>");
            }
            else if (role == Role.Follower)
            {
                prelims = Util.GetSubString(
                    s: _prelimsSheetDoc,
                    from: "<td class=\"fw-bold td-grey bg-darken-md text-start\">Followers",
                    to: "</tbody></table></div></div>");
            }

            return prelims.Substring(prelims.IndexOf("</tr></thead><tbody>") + new string("</tr></thead><tbody>").Length - 1);
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
