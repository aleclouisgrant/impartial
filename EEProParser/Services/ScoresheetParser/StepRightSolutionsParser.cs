﻿using HtmlAgilityPack;
using Impartial.Enums;
using iText.Barcodes.Dmcode;
using iText.Layout.Element;
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
        private string _finalsSheetDoc { get; set; }

        public StepRightSolutionsParser(string prelimsPath, string finalsPath)
        {
            if (prelimsPath == null || prelimsPath == String.Empty || !File.Exists(prelimsPath))
                throw new FileNotFoundException();
            if (finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath))
                throw new FileNotFoundException();

            _prelimsSheetDoc = File.ReadAllText(prelimsPath).Replace("\n", "").Replace("\r", "");
            _finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
        }

        public StepRightSolutionsParser(string finalsPath)
        {
            if (finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath))
                throw new FileNotFoundException();

            _finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
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
                prelims = GetPrelimScores(division);
            }

            var sub = GetFinalsDoc(division);
            var finalsJudges = GetFinalsJudgesByDivision(division);

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

                Competitor leader = new Competitor(leaderFirstName, leaderLastName);
                Competitor follower = new Competitor(followerFirstName, followerLastName);

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

            return new Competition(division)
            {
                Scores = scores,
                LeaderPrelimScores = prelims.Item1 ?? new List<PrelimScore>(),
                FollowerPrelimScores = prelims.Item2 ?? new List<PrelimScore>(), 
            };
        }

        private Tuple<List<PrelimScore>, List<PrelimScore>> GetPrelimScores(Division division)
        {
            var leadsPrelims = GetPrelimsDoc(division, Role.Leader);
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
                        callbackScore = NumberToCallbackScore(Double.Parse(node[i].InnerText));
                    }
                    catch
                    {
                        callbackScore = NumberToCallbackScore(Double.Parse(node[i].InnerText.Substring(0, 1)));
                    }

                    var prelimScore = new PrelimScore()
                    {
                        Role = Role.Leader,
                        Judge = leadJudges[i - offset],
                        Competitor = competitor,
                        Finaled = finaled,
                        CallbackScore = callbackScore
                    };

                    leaderPrelimScores.Add(prelimScore);
                }
            }

            var followsPrelims = GetPrelimsDoc(division, Role.Follower);
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
                        callbackScore = NumberToCallbackScore(Double.Parse(node[i].InnerText));
                    }
                    catch
                    {
                        callbackScore = NumberToCallbackScore(Double.Parse(node[i].InnerText.Substring(0, 1)));
                    }

                    var prelimScore = new PrelimScore()
                    {
                        Role = Role.Follower,
                        Judge = followerJudges[i - offset],
                        Competitor = competitor,
                        Finaled = finaled,
                        CallbackScore = callbackScore
                    };

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
        private List<Judge> GetFinalsJudgesByDivision(Division division)
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

        private string GetPrelimsDoc(Division division, Role role)
        {
            Division div;

            var divisionString = Util.GetSubString(
                s: _prelimsSheetDoc, 
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
                    s: _prelimsSheetDoc,
                    from: "<h3 id=\"leaders\">Leaders",
                    to: "</tbody></table><br>");
            }
            else if (role == Role.Follower)
            {
                prelims = Util.GetSubString(
                    s: _prelimsSheetDoc,
                    from: "<h3 id=\"followers\">Followers",
                    to: "</tbody></table></div>                                <div class=\"text-center\">");
            }

            return prelims.Substring(prelims.IndexOf("</tr></thead><tbody>") + new string("</tr></thead><tbody>").Length - 1);
        }
        private string GetFinalsDoc(Division division)
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

        private CallbackScore NumberToCallbackScore(double value)
        {
            switch (value)
            {
                case 10:
                    return CallbackScore.Yes;
                case 4.5:
                    return CallbackScore.Alt1;
                case 4.3:
                    return CallbackScore.Alt2;
                case 4.2:
                    return CallbackScore.Alt3;

                default:
                case 0:
                    return CallbackScore.No;
            }
        }
    }
}
