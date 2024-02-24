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

        public WorldDanceRegistryParser(string prelimsPath = null, string finalsPath = null)
        {
            bool prelimPathFound = !(prelimsPath == null || prelimsPath == String.Empty || !File.Exists(prelimsPath));
            bool finalsPathFound = !(finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath));

            if (!prelimPathFound && !finalsPathFound)
                throw new FileNotFoundException();

            _prelimsSheetDoc = prelimPathFound ? File.ReadAllText(prelimsPath).Replace("\n", "").Replace("\r", "") : null;
            _finalsSheetDoc = finalsPathFound ? File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "") : null;
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
        
        public ICompetition GetCompetition(Division division)
        {
            var prelims = new Tuple<List<PrelimScore>, List<PrelimScore>>(null, null);
            if (_prelimsSheetDoc != null)
            {
                prelims = GetPrelimScores(division);
            }

            return new Competition(division)
            {
                Name = GetName(),
                Scores = _finalsSheetDoc != null ? GetFinalScores(division) : new(),
                LeaderPrelimScores = prelims.Item1 ?? new List<PrelimScore>(),
                FollowerPrelimScores = prelims.Item2 ?? new List<PrelimScore>(),
            };
        }

        private bool SemisExist(Division division)
        {
            return true;
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
            int rawScore = 1;
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
                    firstName = name.Substring(0, pos).Trim();
                    lastName = name.Substring(pos + 1).Trim();
                }

                var competitor = new ICompetitor(firstName, lastName);

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
                        callbackScore: callbackScore,
                        rawScore: rawScore,
                        round: 1);

                    leaderPrelimScores.Add(prelimScore);
                }
                rawScore++;
            }

            var followsPrelims = GetPrelimsDocByDivision(division, Role.Follower);
            HtmlDocument followsDoc = new HtmlDocument();
            followsDoc.LoadHtml(followsPrelims);
            var followerNodes = followsDoc.DocumentNode.SelectNodes("tr");
            var followerJudges = GetPrelimsJudgesByDivision(division, Role.Follower);

            int redactedFollows = 0;

            List<PrelimScore> followerPrelimScores = new List<PrelimScore>();
            rawScore = 1;
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
                    firstName = name.Substring(0, pos).Trim();
                    lastName = name.Substring(pos + 1).Trim();
                }

                var competitor = new ICompetitor(firstName, lastName);

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
                        callbackScore: callbackScore, 
                        rawScore: rawScore,
                        round: 1);

                    followerPrelimScores.Add(prelimScore);
                }
                rawScore++;
            }

            return new Tuple<List<PrelimScore>, List<PrelimScore>>(leaderPrelimScores, followerPrelimScores);
        }
        private List<IFinalScore> GetFinalScores(Division division)
        {
            var sub = GetFinalsDocByDivision(division);
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
                int actualPlacement = Int32.Parse(node[3].InnerText);

                string leaderName = node[1].InnerText;
                int leadPos = leaderName.IndexOf(' ');
                string leaderFirstName = leaderName.Substring(0, leadPos).Trim();
                string leaderLastName = leaderName.Substring(leadPos + 1).Trim();

                ICompetitor leader = leaders.Where(c => c.FullName == leaderFirstName + " " + leaderLastName)?.FirstOrDefault();
                if (leader == null)
                {
                    leader = new ICompetitor(leaderFirstName, leaderLastName);
                    leaders.Add(leader);
                }

                string followerName = node[2].InnerText;
                int followPos = followerName.IndexOf(' ');
                string followerFirstName = followerName.Substring(0, followPos).Trim();
                string followerLastName = followerName.Substring(followPos + 1).Trim();

                ICompetitor follower = followers.Where(c => c.FullName == followerFirstName + " " + followerLastName)?.FirstOrDefault();
                if (follower == null)
                {
                    follower = new ICompetitor(followerFirstName, followerLastName);
                    followers.Add(follower);
                }

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

                    var score = new FinalScore(judges[j - offset], placement, actualPlacement)
                    {
                        Leader = leader,
                        Follower = follower,
                    };

                    scores.Add(score);

                    if (judges[j - offset].Scores == null)
                        judges[j - offset].Scores = new List<IFinalScore>();

                    judges[j - offset].Scores.Add(score);
                }
            }
            return scores;
        }

        private List<IJudge> GetPrelimsJudgesByDivision(Division division, Role role)
        {
            var judges = new List<IJudge>();
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
                    judges.Add(new IJudge(name.Trim(), string.Empty));
                }
                else
                {
                    judges.Add(new IJudge(name.Trim().Substring(0, pos), name.Trim().Substring(pos + 1)));
                }
            }

            return judges;
        }
        private List<IJudge> GetFinalsJudgesByDivision(Division division)
        {
            var judges = new List<IJudge>();

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
                    judges.Add(new IJudge(name, string.Empty));
                }
                else
                {
                    judges.Add(new IJudge(name.Substring(0, pos), name.Substring(pos + 1)));
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

        private string GetName()
        {
            if (_finalsSheetDoc == null)
                return string.Empty;

            string name = Util.GetSubString(
                s: _finalsSheetDoc,
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
