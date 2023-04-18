using HtmlAgilityPack;
using Impartial.Enums;
using iText.Barcodes.Dmcode;
using iText.Layout.Element;
using iText.StyledXmlParser.Jsoup.Nodes;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Impartial.Services.ScoresheetParser
{
    public class StepRightSolutionsParser : IScoresheetParser
    {
        private string _prelimsSheetDoc { get; set; }
        private string _semisSheetDoc { get; set; }
        private string _finalsSheetDoc { get; set; }

        public StepRightSolutionsParser(string prelimsPath = null, string semisPath = null, string finalsPath = null)
        {
            bool prelimPathFound = !(prelimsPath == null || prelimsPath == String.Empty || !File.Exists(prelimsPath));
            bool semisPathFound = !(semisPath == null || semisPath == String.Empty || !File.Exists(semisPath));
            bool finalsPathFound = !(finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath));

            if (!prelimPathFound && !semisPathFound && !finalsPathFound)
                throw new FileNotFoundException();

            _prelimsSheetDoc = prelimPathFound ? File.ReadAllText(prelimsPath).Replace("\n", "").Replace("\r", "") : null;
            _semisSheetDoc = semisPathFound ? File.ReadAllText(semisPath).Replace("\n", "").Replace("\r", "") : null;
            _finalsSheetDoc = finalsPathFound ? File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "") : null;
        }

        public List<Division> GetDivisions()
        {
            var divisions = new List<Division>();
            var divisionString = Util.GetSubString(_finalsSheetDoc, "<h3>", "</h3>");

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
                prelims = GetPrelimScores(division, _prelimsSheetDoc, 1);
            }

            if (_semisSheetDoc != null)
            {
                var semis = GetPrelimScores(division, _semisSheetDoc, 2);
                prelims.Item1.AddRange(semis.Item1);
                prelims.Item2.AddRange(semis.Item2);
            }

            return new Competition(division)
            {
                Name = GetName(),
                Scores = _finalsSheetDoc != null ? GetFinalScores(division) : new(),
                LeaderPrelimScores = prelims.Item1 ?? new List<PrelimScore>(),
                FollowerPrelimScores = prelims.Item2 ?? new List<PrelimScore>(),
            };
        }

        private Tuple<List<PrelimScore>, List<PrelimScore>> GetPrelimScores(Division division, string doc, int round)
        {
            var leadsPrelims = GetPrelimsDocByDivision(division, Role.Leader, doc);
            HtmlDocument leadsDoc = new HtmlDocument();
            leadsDoc.LoadHtml(leadsPrelims);
            var leadNodes = leadsDoc.DocumentNode.SelectNodes("tr");
            var leaderJudges = GetPrelimsJudgesByDivision(division, Role.Leader, doc);

            List<PrelimScore> leaderPrelimScores = new List<PrelimScore>();
            int rawScore = 1;
            foreach (var lead in leadNodes)
            {
                var node = lead.SelectNodes("td");
                bool finaled = lead.GetClasses().Contains("adv");
                string name = node[1].InnerText;
                int pos = name.IndexOf(' ');
                var competitor = new Competitor(name.Substring(0, pos).Trim(), name.Substring(pos + 1).Trim());

                CallbackScore callbackScore;
                int offset = 2;
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
                        round: round);

                    leaderPrelimScores.Add(prelimScore);
                }
                rawScore++;
            }

            var followsPrelims = GetPrelimsDocByDivision(division, Role.Follower, doc);
            HtmlDocument followsDoc = new HtmlDocument();
            followsDoc.LoadHtml(followsPrelims);
            var followerNodes = followsDoc.DocumentNode.SelectNodes("tr");
            var followerJudges = GetPrelimsJudgesByDivision(division, Role.Follower, doc);

            List<PrelimScore> followerPrelimScores = new List<PrelimScore>();
            rawScore = 1;
            foreach (var follow in followerNodes)
            {
                var node = follow.SelectNodes("td");
                bool finaled = follow.GetClasses().Contains("adv");
                string name = node[1].InnerText;
                int pos = name.IndexOf(' ');
                var competitor = new Competitor(name.Substring(0, pos).Trim(), name.Substring(pos + 1).Trim());

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
                        callbackScore: callbackScore,
                        rawScore: rawScore,
                        round: round);

                    followerPrelimScores.Add(prelimScore);
                }
                rawScore++;
            }

            return new Tuple<List<PrelimScore>, List<PrelimScore>>(leaderPrelimScores, followerPrelimScores);
        }

        private List<Score> GetFinalScores(Division division)
        {
            var sub = GetFinalsDocByDivision(division);
            var finalsJudges = GetFinalsJudgesByDivision(division);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("tr");

            var leaders = new List<Competitor>();
            var followers = new List<Competitor>();

            var scores = new List<Score>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i].SelectNodes("td");

                string actualPlacementString = node.Where(n => n.GetClasses().Contains("r")).FirstOrDefault().InnerText;
                int actualPlacement = Int32.Parse(actualPlacementString.Remove(actualPlacementString.Length - 2));

                string leaderName = node.Where(n => n.GetClasses().Contains("nolines") && n.GetClasses().Contains("name")).FirstOrDefault().InnerText;
                int leadPos = leaderName.IndexOf(' ');
                string leaderFirstName = leaderName.Substring(0, leadPos).Trim();
                string leaderLastName = leaderName.Substring(leadPos + 1).Trim();

                Competitor leader = leaders.Where(c => c.FullName == leaderFirstName + " " + leaderLastName)?.FirstOrDefault();
                if (leader == null)
                {
                    leader = new Competitor(leaderFirstName, leaderLastName);
                    leaders.Add(leader);
                }

                string followerName = node.Where(n => n.GetClasses().Contains("nolines") && n.GetClasses().Contains("name") && n.GetClasses().Contains("heavyborder")).FirstOrDefault().InnerText;
                int followPos = followerName.IndexOf(' ');
                string followerFirstName = followerName.Substring(0, followPos).Trim();
                string followerLastName = followerName.Substring(followPos + 1).Trim();

                Competitor follower = followers.Where(c => c.FullName == followerFirstName + " " + followerLastName)?.FirstOrDefault();
                if (follower == null)
                {
                    follower = new Competitor(followerFirstName, followerLastName);
                    followers.Add(follower);
                }

                int offset = 3;
                for (int j = offset; j < finalsJudges.Count + offset; j++)
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

                    var score = new Score(finalsJudges[j - offset], placement, actualPlacement)
                    {
                        Leader = leader,
                        Follower = follower,
                    };

                    scores.Add(score);

                    if (finalsJudges[j - offset].Scores == null)
                        finalsJudges[j - offset].Scores = new List<Score>();

                    finalsJudges[j - offset].Scores.Add(score);
                }
            }

            return scores;
        }

        private List<Judge> GetPrelimsJudgesByDivision(Division division, Role role, string doc)
        {
            var judges = new List<Judge>();
            string sub = string.Empty;

            if (role == Role.Leader)
            {
                sub = Util.GetSubString(
                    s: doc,
                    from: "<div><b>Judges: </b>",
                    to: "</div>");
            }
            else if (role == Role.Follower)
            {
                sub = Util.GetSubString(
                    s: doc.Substring(doc.IndexOf("<h3 id=\"followers\">Followers")),
                    from: "<div><b>Judges: </b>",
                    to: "</div>");
            }

            List<string> names = sub.Split(',').ToList();

            // step right solutions usuallys anonymizes judges, so we test for that
            if (names.First().Contains("J1"))
            {
                foreach (var nameString in names)
                {
                    string name = nameString.Substring(0, nameString.IndexOf("<em>"));
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

            // otherwise, we are going to return anonymous judges
            for (int i = 1; i <= names.Count; i++)
            {
                judges.Add(new Judge("Anonymous", i.ToString()));
            }

            return judges;
        }
        private List<Judge> GetFinalsJudgesByDivision(Division division)
        {
            var judges = new List<Judge>();

            string sub = Util.GetSubString(
                s: _finalsSheetDoc,
                from: "<div><b>Judges: </b>",
                to: "</div>");

            List<string> names = sub.Split(',').ToList();

            // step right solutions usuallys anonymizes judges, so we test for that
            if (names.First().Contains("J1"))
            {
                foreach (var nameString in names)
                {
                    string name = nameString.Substring(0, nameString.IndexOf("<em>"));
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

            // otherwise, we are going to return anonymous judges
            for (int i = 1; i <= names.Count; i++)
            {
                judges.Add(new Judge("Anonymous", i.ToString()));
            }

            return judges;
        }

        private string GetPrelimsDocByDivision(Division division, Role role, string doc)
        {
            Division div;

            var divisionString = Util.GetSubString(
                s: doc, 
                from: "<h3>", 
                to: "</h3>");

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
                    s: doc,
                    from: "<h3 id=\"leaders\">Leaders",
                    to: "</tbody></table><br>");
            }
            else if (role == Role.Follower)
            {
                prelims = Util.GetSubString(
                    s: doc,
                    from: "<h3 id=\"followers\">Followers",
                    to: "</tbody></table></div>                                <div class=\"text-center\">");
            }

            return prelims.Substring(prelims.IndexOf("</tr></thead><tbody>") + new string("</tr></thead><tbody>").Length - 1);
        }
        private string GetFinalsDocByDivision(Division division)
        {
            Division div;
            var finals = Util.GetSubString(
                s: _finalsSheetDoc,
                from: "<div class=\"well clearfix\">",
                to: "</tbody></table></div>");

            var divisionString = Util.GetSubString(finals, "<h3>", "</h3>");

            finals = finals.Substring(finals.IndexOf("<tbody>") + new string("<tbody>").Length - 1);

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

            var nameDoc = new HtmlDocument();
            
            nameDoc.LoadHtml(
                Util.GetSubString(
                    s: _finalsSheetDoc,
                    from: "<h2>",
                    to: "</h2>"));
            string name = nameDoc.DocumentNode.SelectNodes("a")?.FirstOrDefault().InnerText;

            return name;
        }
    }
}
