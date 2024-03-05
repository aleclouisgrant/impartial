using HtmlAgilityPack;
using Impartial;
using Impartial.Enums;
using ImpartialUI.Models;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImpartialUI.Services.ScoresheetParser
{
    public class StepRightSolutionsParser : ScoresheetParserBase
    {
        public StepRightSolutionsParser(string prelimsPath = null, string quartersPath = null, string semisPath = null, string finalsPath = null) : base(prelimsPath, quartersPath, semisPath, finalsPath) { }
        public override List<Division> GetDivisions()
        {
            var divisions = new List<Division>();
            var divisionString = Util.GetSubString(FinalsSheetDoc, "<h3>", "</h3>");

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

        public override IPrelimCompetition? GetPrelimCompetition(Division division, Round round, Role role)
        {
            string sheet = "";
            switch (round)
            {
                case Round.Quarterfinals:
                    sheet = QuartersSheetDoc;
                    break;
                case Round.Semifinals:
                    sheet = SemisSheetDoc;
                    break;
                case Round.Prelims:
                    sheet = PrelimsSheetDoc; 
                    break;
                default:
                    return null;
            }

            var prelimsHtml = GetPrelimsDocByDivision(division, role, sheet);
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
            var judges = GetPrelimsJudgesByDivision(division, role, sheet);

            foreach (var node in nodes)
            {
                var nodeCollection = node.SelectNodes("td");
                bool finaled = node.GetClasses().Contains("adv");
                string name = nodeCollection[1].InnerText;
                int pos = name.IndexOf(' ');
                var competitor = new Competitor(name.Substring(0, pos).Trim(), name.Substring(pos + 1).Trim());

                CallbackScore callbackScore;
                int offset = 2;
                for (int i = offset; i < judges.Count + offset; i++)
                {
                    try
                    {
                        callbackScore = Util.NumberToCallbackScore(double.Parse(nodeCollection[i].InnerText));
                    }
                    catch
                    {
                        callbackScore = Util.NumberToCallbackScore(double.Parse(nodeCollection[i].InnerText.Substring(0, 1)));
                    }

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

            var finalsJudges = GetFinalsJudgesByDivision(division);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sub);

            var nodes = doc.DocumentNode.SelectNodes("tr");

            var leaders = new List<ICompetitor>();
            var followers = new List<ICompetitor>();

            var scores = new List<IFinalScore>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i].SelectNodes("td");

                string actualPlacementString = node.Where(n => n.GetClasses().Contains("r")).FirstOrDefault().InnerText;
                int actualPlacement = int.Parse(actualPlacementString.Remove(actualPlacementString.Length - 2));

                string leaderName = node.Where(n => n.GetClasses().Contains("nolines") && n.GetClasses().Contains("name")).FirstOrDefault().InnerText;
                int leadPos = leaderName.IndexOf(' ');
                string leaderFirstName = leaderName.Substring(0, leadPos).Trim();
                string leaderLastName = leaderName.Substring(leadPos + 1).Trim();

                ICompetitor leader = leaders.Where(c => c.FullName == leaderFirstName + " " + leaderLastName)?.FirstOrDefault();
                if (leader == null)
                {
                    leader = new Competitor(leaderFirstName, leaderLastName);
                    leaders.Add(leader);
                }

                string followerName = node.Where(n => n.GetClasses().Contains("nolines") && n.GetClasses().Contains("name") && n.GetClasses().Contains("heavyborder")).FirstOrDefault().InnerText;
                int followPos = followerName.IndexOf(' ');
                string followerFirstName = followerName.Substring(0, followPos).Trim();
                string followerLastName = followerName.Substring(followPos + 1).Trim();

                ICompetitor follower = followers.Where(c => c.FullName == followerFirstName + " " + followerLastName)?.FirstOrDefault();
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
                        placement = int.Parse(node[j].InnerText);
                    }
                    catch
                    {
                        placement = int.Parse(node[j].InnerText.Substring(0, 1));
                    }

                    var finalScore = new FinalScore(
                        judge: finalsJudges[j - offset],
                        leader: leader,
                        follower: follower,
                        score: placement,
                        placement: actualPlacement);

                    scores.Add(finalScore);

                    if (finalsJudges[j - offset].Scores == null)
                        finalsJudges[j - offset].Scores = new List<IFinalScore>();

                    finalsJudges[j - offset].Scores.Add(finalScore);
                }
            }

            return new FinalCompetition(dateTime: DateTime.MinValue, division: division, finalScores: scores);
        }

        private List<IJudge> GetPrelimsJudgesByDivision(Division division, Role role, string doc)
        {
            var judges = new List<IJudge>();
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
        private List<IJudge> GetFinalsJudgesByDivision(Division division)
        {
            var judges = new List<IJudge>();

            string sub = Util.GetSubString(
                s: FinalsSheetDoc,
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
            if (doc == null || doc == string.Empty)
                return string.Empty;

            var titleString = Util.GetSubString(
                s: doc,
                from: "<h3>",
                to: "</h3>");

            if (titleString.Contains("Masters"))
                return string.Empty;

            Division? div;
            div = Util.ContainsDivisionString(titleString);

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
            var finals = Util.GetSubString(
                s: FinalsSheetDoc,
                from: "<div class=\"well clearfix\">",
                to: "</tbody></table></div>");

            var titleString = Util.GetSubString(finals, "<h3>", "</h3>");

            finals = finals.Substring(finals.IndexOf("<tbody>") + new string("<tbody>").Length - 1);

            if (titleString.Contains("Masters"))
                return string.Empty;

            Division? div;
            div = Util.ContainsDivisionString(titleString);

            if (division == div)
                return finals;

            return string.Empty;
        }

        public override string GetName()
        {
            if (FinalsSheetDoc == null)
                return string.Empty;

            var nameDoc = new HtmlDocument();

            nameDoc.LoadHtml(
                Util.GetSubString(
                    s: FinalsSheetDoc,
                    from: "<h2>",
                    to: "</h2>"));
            string name = nameDoc.DocumentNode.SelectNodes("a")?.FirstOrDefault().InnerText;

            return name;
        }
    }
}
