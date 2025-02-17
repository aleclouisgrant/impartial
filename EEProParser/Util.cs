using System;
using System.Collections.Generic;
using Impartial.Enums;
using System.Linq;

namespace Impartial
{
    public static class Util
    {
        public static Tier GetTier(int numberOfCompetitors)
        {
            if (numberOfCompetitors < 5 && numberOfCompetitors >= 1)
                return Tier.NoTier;
            else if (numberOfCompetitors >= 5 && numberOfCompetitors <= 10)
                return Tier.Tier1;
            else if (numberOfCompetitors >= 11 && numberOfCompetitors <= 19)
                return Tier.Tier2;
            else if (numberOfCompetitors >= 20 && numberOfCompetitors <= 39)
                return Tier.Tier3;
            else if (numberOfCompetitors >= 40 && numberOfCompetitors <= 79)
                return Tier.Tier4;
            else if (numberOfCompetitors >= 80 && numberOfCompetitors <= 129)
                return Tier.Tier5;
            else if (numberOfCompetitors >= 130)
                return Tier.Tier6;
            else
                return 0;
        }

        public static int GetAwardedPoints(int numberOfCompetitors, int placement)
        {
            if (numberOfCompetitors < 5 && numberOfCompetitors >= 1)
                return GetAwardedPoints(Tier.NoTier, placement);
            else if (numberOfCompetitors >= 5 && numberOfCompetitors <= 10)
                return GetAwardedPoints(Tier.Tier1, placement);
            else if (numberOfCompetitors >= 11 && numberOfCompetitors <= 19)
                return GetAwardedPoints(Tier.Tier2, placement);
            else if (numberOfCompetitors >= 20 && numberOfCompetitors <= 39)
                return GetAwardedPoints(Tier.Tier3, placement);
            else if (numberOfCompetitors >= 40 && numberOfCompetitors <= 79)
                return GetAwardedPoints(Tier.Tier4, placement);
            else if (numberOfCompetitors >= 80 && numberOfCompetitors <= 129)
                return GetAwardedPoints(Tier.Tier5, placement);
            else if (numberOfCompetitors >= 130)
                return GetAwardedPoints(Tier.Tier6, placement);
            else
                return 0;
        }

        public static int GetAwardedPoints(Tier tier, int placement, DateTime? date = null)
        {
            if (date == null)
                date = DateTime.Now;

            switch (tier)
            {
                case Tier.NoTier:
                default:
                    return 0;
                case Tier.Tier1:
                    if (placement == 1)
                        return 3;
                    else if (placement == 2)
                        return 2;
                    else if (placement == 3)
                        return 1;
                    else
                        return 0;
                case Tier.Tier2:
                    if (placement == 1)
                        return 6;
                    else if (placement == 2)
                        return 4;
                    else if (placement == 3)
                        return 3;
                    else if (placement == 4)
                        return 2;
                    else if (placement == 5)
                        return 1;
                    else
                        return 0;
                case Tier.Tier3:
                    if (placement == 1)
                        return 10;
                    else if (placement == 2)
                        return 8;
                    else if (placement == 3)
                        return 6;
                    else if (placement == 4)
                        return 4;
                    else if (placement == 5)
                        return 2;
                    else if (placement > 5 && placement <= 10 && date >= DateTime.Parse("2023-01-01"))
                        return 1;
                    else if (placement > 5 && placement <= 12 && date < DateTime.Parse("2023-01-01"))
                        return 1;
                    else
                        return 0;
                case Tier.Tier4:
                    if (placement == 1)
                        return 15;
                    else if (placement == 2)
                        return 12;
                    else if (placement == 3)
                        return 10;
                    else if (placement == 4)
                        return 8;
                    else if (placement == 5)
                        return 6;
                    else if (placement > 5 && placement <= 15)
                        return 1;
                    else
                        return 0;
                case Tier.Tier5:
                    if (placement == 1)
                        return 20;
                    else if (placement == 2)
                        return 16;
                    else if (placement == 3)
                        return 14;
                    else if (placement == 4)
                        return 12;
                    else if (placement == 5)
                        return 10;
                    else if (placement > 5 && placement <= 15)
                        return 2;
                    else
                        return 0;
                case Tier.Tier6:
                    if (placement == 1)
                        return 25;
                    else if (placement == 2)
                        return 22;
                    else if (placement == 3)
                        return 18;
                    else if (placement == 4)
                        return 15;
                    else if (placement == 5)
                        return 12;
                    else if (placement > 5 && placement <= 15)
                        return 2;
                    else
                        return 0;
            }
        }

        public static double GetAccuracy(int placement, int actualPlacement)
        {
            return Math.Abs(placement - actualPlacement);
        }
        public static double GetWeightedAccuracy(int placement, int actualPlacement, int totalCouples)
        {
            return Math.Abs(
                placement * GetAwardedPoints(totalCouples, placement) - 
                actualPlacement * GetAwardedPoints(totalCouples, actualPlacement));
        }

        public static double GetCallbackScoreValue(CallbackScore callbackScore)
        {
            switch (callbackScore)
            {
                case CallbackScore.Yes:
                    return 10;
                case CallbackScore.Alt1:
                    return 4.5f;
                case CallbackScore.Alt2:
                    return 4.3;
                case CallbackScore.Alt3:
                    return 4.2;
                case CallbackScore.No:
                default:
                    return 0;
            }
        }

        public static int GetEditDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
        public static string GetClosestString(string input, IEnumerable<string> list)
        {
            int leastDistance = 10000;
            string match = "";

            foreach (string s in list)
            {
                int d = GetEditDistance(input, s);
                if (d == 0)
                    return s;

                if (d < leastDistance)
                {
                    leastDistance = d;
                    match = s;
                }
            }

            return match;
        }
        public static IUser GetClosestPersonByFirstName(string input, IEnumerable<IUser> list)
        {
            int leastDistance = 10000;
            IUser match = null;

            foreach (IUser p in list)
            {
                int d = GetEditDistance(input, p.FirstName);
                if (d == 0)
                    return p;

                if (d < leastDistance)
                {
                    leastDistance = d;
                    match = p;
                }
            }

            return match;
        }

        public static string GetSubString(string s, string from, string to)
        {
            if (s == null)
                return "";

            if (s.IndexOf(from) == -1)
                return "";

            int fromIndex = s.IndexOf(from) + from.Length;
            string choppedString = s.Substring(fromIndex, s.Length - fromIndex);

            int toIndex = choppedString.IndexOf(to) + fromIndex;
            string sub = "";

            try
            {
                sub = s.Substring(fromIndex, toIndex - fromIndex);
            }
            catch (ArgumentOutOfRangeException) { }

            return sub;
        }
        public static string GetSubStringN(string s, string from, string to, int n)
        {
            int fromIndex = IndexOfN(s, from, n) + from.Length;
            int toIndex = IndexOfN(s, to, n);
            string sub = "";

            try
            {
                sub = s.Substring(fromIndex, toIndex - fromIndex);
            }
            catch (ArgumentOutOfRangeException)
            {

            }

            return sub;
        }
        public static int IndexOfN(string s, string match, int n)
        {
            int i = 1;
            int index = -1;

            try
            {
                while (i <= n && (index = s.IndexOf(match, index + 1)) != -1)
                {
                    if (i == n)
                        return index;

                    i++;
                }
            }
            catch
            {
                return -1;
            }

            return -1;
        }

        public static CallbackScore StringToCallbackScore(string score)
        {
            switch (score)
            {
                case "a1":
                case "A1":
                case "alt1":
                case "Alt1":
                case "Alternate1":
                case "alternate1":
                case "alternate_1":
                case "45":
                case "4.5":
                    return CallbackScore.Alt1;
                case "a2":
                case "A2":
                case "alt2":
                case "Alt2":
                case "Alternate2":
                case "alternate2":
                case "alternate_2":
                case "43":
                case "4.3":
                    return CallbackScore.Alt2;
                case "a3":
                case "A3":
                case "alt3":
                case "Alt3":
                case "Alternate3":
                case "alternate3":
                case "alternate_3":
                case "42":
                case "4.2":
                    return CallbackScore.Alt3;
                case "y":
                case "Y":
                case "yes":
                case "Yes":
                case "100":
                case "10":
                    return CallbackScore.Yes;
                case "n":
                case "no":
                case "N":
                case "No":
                case "0":
                    return CallbackScore.No;
                default:
                    return CallbackScore.Unscored;
            }
        }
        public static CallbackScore NumberToCallbackScore(double value)
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
                case 0:
                    return CallbackScore.No;
                default:
                    return CallbackScore.Unscored;
            }
        }
        public static Division StringToDivision(string s)
        {
            switch (s)
            {
                case "Newcomer":
                case "newcomer":
                case "NEW":
                case "New":
                case "new":
                    return Division.Newcomer;
                case "Novice":
                case "novice":
                case "NOV":
                case "Nov":
                case "nov":
                    return Division.Novice;
                case "Intermediate":
                case "intermediate":
                case "INT":
                case "Int":
                case "int":
                    return Division.Intermediate;
                case "Advanced":
                case "advanced":
                case "ADV":
                case "Adv":
                case "adv":
                    return Division.Advanced;
                case "AllStar":
                case "Allstar":
                case "allStar":
                case "All Star":
                case "all_star":
                case "allstar":
                case "all star":
                case "All-Star":
                case "all-star":
                case "ALS":
                case "Als":
                case "als":
                    return Division.AllStar;
                case "Champion":
                case "champion":
                case "Champ":
                case "champ":
                case "CHM":
                case "CMP":
                    return Division.Champion;
                default:
                    return Division.Open;
            }
        }
        public static Role StringToRole(string s)
        {
            switch (s)
            {
                case "Follower":
                case "follower":
                    return Role.Follower;
                case "Leader":
                case "leader":
                default:
                    return Role.Leader;
            }
        }
        public static Round GetRoundFromString(string s)
        {
            switch (s)
            {
                case "prelims":
                case "Prelims":
                case "preliminaries":
                case "Preliminaries":
                    return Round.Prelims;
                case "quarters":
                case "Quarters":
                case "quarterfinals":
                case "Quarterfinals":
                    return Round.Quarterfinals;
                case "semis":
                case "Semis":
                case "semifinals":
                case "Semifinals":
                    return Round.Semifinals;
                default:
                case "finals":
                case "Finals":
                    return Round.Finals;
            }
        }

        public static string CallbackScoreToString(CallbackScore score)
        {
            switch (score)
            {
                case CallbackScore.Alt1:
                    return "A1";
                case CallbackScore.Alt2:
                    return "A2";
                case CallbackScore.Alt3:
                    return "A3";
                case CallbackScore.Yes:
                    return "Y";
                case CallbackScore.No:
                    return "N";
                default:
                case CallbackScore.Unscored:
                    return "";
            }
        }
        public static string DivisionToString(Division division)
        {
            switch (division)
            {
                case Division.Newcomer:
                    return "Newcomer";
                case Division.Novice:
                    return "Novice";
                case Division.Intermediate:
                    return "Intermediate";
                case Division.Advanced:
                    return "Advanced";
                case Division.AllStar:
                    return "All Star";
                case Division.Champion:
                    return "Champion";
                default:
                case Division.Open:
                    return "Open";
            }
        }
        public static string TierToString(Tier tier)
        {
            switch (tier) 
            {
                case Tier.Tier1:
                    return "Tier 1";
                case Tier.Tier2:
                    return "Tier 2";
                case Tier.Tier3:
                    return "Tier 3";
                case Tier.Tier4:
                    return "Tier 4";
                case Tier.Tier5:
                    return "Tier 5";
                case Tier.Tier6:
                    return "Tier 6";
                default:
                case Tier.NoTier:
                    return "No Tier";
            }
        }
        
        public static Division? ContainsDivisionString(string s)
        {
            if (s.Contains("Newcomer") ||
                s.Contains("newcomer") ||
                s.Contains("NEWCOMER") ||
                s.Contains("NEW") ||
                s.Contains("New") ||
                s.Contains("new"))
                    return Division.Newcomer;

            if (s.Contains("Novice") ||
                s.Contains("novice") ||
                s.Contains("NOV") ||
                s.Contains("Nov") ||
                s.Contains("nov"))
                    return Division.Novice;

            if (s.Contains("Intermediate") ||
                s.Contains("intermediate") ||
                s.Contains("INTERMEDIATE") ||
                s.Contains("INT") ||
                s.Contains("Int") ||
                s.Contains("int"))
                    return Division.Intermediate;

            if (s.Contains("Advanced") ||
                s.Contains("ADVANCED") ||
                s.Contains("advanced") ||
                s.Contains("ADV") ||
                s.Contains("Adv") ||
                s.Contains("adv"))
                    return Division.Advanced;
                
            if (s.Contains("AllStar") ||
                s.Contains("Allstar") ||
                s.Contains("ALLSTAR") ||
                s.Contains("ALL STAR") ||
                s.Contains("ALL-STAR") ||
                s.Contains("allStar") ||
                s.Contains("All Star") ||
                s.Contains("all_star") ||
                s.Contains("allstar") ||
                s.Contains("all star") ||
                s.Contains("All-Star") ||
                s.Contains("all-star"))
                    return Division.AllStar;
               
            if (s.Contains("Champion") ||
                s.Contains("champion") ||
                s.Contains("CHAMPION") ||
                s.Contains("CHAMP") ||
                s.Contains("Champ") ||
                s.Contains("champ")||
                s.Contains("CHM") ||
                s.Contains("CMP"))
                    return Division.Champion;

            if (s.Contains("Open") ||
                s.Contains("OPEN") ||
                s.Contains("open"))
                return Division.Open;

            return null;
        }
        public static Role? ContainsRoleString(string s)
        {
            if (s.Contains("LEADER") ||
                s.Contains("leader") ||
                s.Contains("Leader") ||
                s.Contains("LEAD") ||
                s.Contains("lead") ||
                s.Contains("Lead"))
                return Role.Leader;

            if (s.Contains("Follower") ||
                s.Contains("FOLLOWER") ||
                s.Contains("follower") ||
                s.Contains("follow") ||
                s.Contains("FOLLOW") ||
                s.Contains("Follow"))
                return Role.Follower;

            return null;
        }
        public static Round? ContainsRoundString(string s)
        {
            if (s.Contains("Prelims") ||
                s.Contains("PRELIMS") ||
                s.Contains("prelims") ||
                s.Contains("Prelim") ||
                s.Contains("PRELIM") ||
                s.Contains("prelim") ||
                s.Contains("Preliminaries") ||
                s.Contains("preliminaries") ||
                s.Contains("PRELIMINARIES") ||
                s.Contains("Preliminary") ||
                s.Contains("preliminary") ||
                s.Contains("PRELIMINARY"))
                return Round.Prelims;

            if (s.Contains("Quarterfinals") ||
                s.Contains("QUARTERFINALS") ||
                s.Contains("quarterfinals") ||
                s.Contains("Quarters") ||
                s.Contains("QUARTERS") ||
                s.Contains("quarters") ||
                s.Contains("Quarterfinal") ||
                s.Contains("QUARTERFINAL") ||
                s.Contains("quarterfinal") ||
                s.Contains("Quarter") ||
                s.Contains("QUARTER") ||
                s.Contains("quarter"))
                return Round.Quarterfinals;

            if (s.Contains("Semifinals") ||
                s.Contains("SEMIFINALS") ||
                s.Contains("semifinals") ||
                s.Contains("Semis") ||
                s.Contains("SEMIS") ||
                s.Contains("semis") ||
                s.Contains("Semifinal") ||
                s.Contains("SEMIFINAL") ||
                s.Contains("semifinal") ||
                s.Contains("Semi") ||
                s.Contains("SEMI") ||
                s.Contains("semi"))
                return Round.Semifinals;

            if (s.Contains("finals") ||
                s.Contains("FINALS") ||
                s.Contains("finals") ||
                s.Contains("final") ||
                s.Contains("FINAL") ||
                s.Contains("final"))
                return Round.Finals;

            return null;
        }

        public static IEnumerable<T> RemoveWhere<T>(this IEnumerable<T> query, Predicate<T> predicate)
        {
            return query.Where(e => !predicate(e));
        }

        public static ICompetitor FindCompetitorInCache(string firstName, string lastName, IEnumerable<ICompetitor> competitors)
        {
            //correcting some frequent misspellings
            switch (firstName + " " + lastName)
            {
                case "Aidan Keith-hynes":
                case "Aidan Keith Hynes":
                case "Aiden Keith Hynes":
                    return competitors.FirstOrDefault(c => c.FullName == "Aidan Keith-Hynes");
                case "Jt Anderson":
                case "JT anderson":
                case "Jt anderson":
                    return competitors.FirstOrDefault(c => c.FullName == "JT Anderson");
                case "Dimtri Hector":
                    return competitors.FirstOrDefault(c => c.FullName == "Dimitri Hector");
                case "Conor mcclure":
                case "Conor Mcclure":
                    return competitors.FirstOrDefault(c => c.FullName == "Conor McClure");
                case "Aris Demarco":
                    return competitors.FirstOrDefault(c => c.FullName == "Aris DeMarco");
                case "J Khayree Jones":
                    return competitors.FirstOrDefault(c => c.FullName == "Khayree Jones");
                case "JennAy Ferreira":
                    return competitors.FirstOrDefault(c => c.FullName == "Jen Ferreira");
                case "Karla Anita Catana":
                    return competitors.FirstOrDefault(c => c.FullName == "Karla Catana");
                case "Stephanie Risser":
                case "Stephanie Risser Loveira":
                    return competitors.FirstOrDefault(c => c.FullName == "Stephanie Loveira");
                case "Samuel Vaden":
                    return competitors.FirstOrDefault(c => c.FullName == "Sam Vaden");
                case "Casey Lane Margules":
                    return competitors.FirstOrDefault(c => c.FullName == "Casey Margules");
                case "Margaret Tuttle":
                    return competitors.FirstOrDefault(c => c.FullName == "Margie Tuttle");
                case "Alexander Glover":
                    return competitors.FirstOrDefault(c => c.FullName == "Alex Glover");
                case "Frank Moda III":
                    return competitors.FirstOrDefault(c => c.FullName == "Frank Moda");
                case "Kelly Ponce De Leon":
                    return competitors.FirstOrDefault(c => c.FullName == "Kelly Ponce de Leon");
                case "Agustin Gutierrez":
                    return competitors.FirstOrDefault(c => c.FullName == "Augie Leija");
                case "Saya Suzaki":
                    return competitors.FirstOrDefault(c => c.FullName == "Sayaka Suzaki");
                case "Brittani Schiro":
                    return competitors.FirstOrDefault(c => c.FullName == "Brittany Schiro");
                case "Xander Stavola":
                    return competitors.FirstOrDefault(c => c.FullName == "Alexander Stavola");
                case "Skyler Pritchard":
                    return competitors.FirstOrDefault(c => c.FullName == "Skylar Pritchard");
                case "Jade Ruiz":
                    return competitors.FirstOrDefault(c => c.FullName == "Jade Bryan");
                case "Julie Edwards-Auclair":
                case "Julie Edwards Auclair":
                    return competitors.FirstOrDefault(c => c.FullName == "Julie Auclair");
                case "Vincent van Mierlo":
                    return competitors.FirstOrDefault(c => c.FullName == "Vincent Van Mierlo");
                case "Kyle Fitzgerald":
                    return competitors.FirstOrDefault(c => c.FullName == "Kyle FitzGerald");
                case "Joel Jiminez":
                    return competitors.FirstOrDefault(c => c.FullName == "Joel Jimenez");
                case "Talía Colón":
                    return competitors.FirstOrDefault(c => c.FullName == "Talia Colon");
                case "Jess Ann Nail":
                    return competitors.FirstOrDefault(c => c.FullName == "Jes Ann Nail");
                case "Carlie Obrien":
                case "Carlie O'brien":
                    return competitors.FirstOrDefault(c => c.FullName == "Carlie O'Brien");
                case "Katie Smiley":
                case "Katie Smiley-oyen":
                    return competitors.FirstOrDefault(c => c.FullName == "Katie Smiley-Oyen");
                case "Fae Xenovia":
                case "Fae Ashley (Xenovia)":
                    return competitors.FirstOrDefault(c => c.FullName == "Fae Ashley");
                case "Jung Won Choe":
                case "Jung Won Cho":
                case "Jung choe":
                    return competitors.FirstOrDefault(c => c.FullName == "Jung Choe");
                case "Joanna Mienl":
                    return competitors.FirstOrDefault(c => c.FullName == "Joanna Meinl");
                case "Amber O'connell":
                    return competitors.FirstOrDefault(c => c.FullName == "Amber O'Connell");
                case "Savannah Harris-read":
                    return competitors.FirstOrDefault(c => c.FullName == "Savannah Harris-Read");
                case "Branden Stong":
                    return competitors.FirstOrDefault(c => c.FullName == "Branden Strong");
                case "Ashley Daniels":
                case "Ashley Snow (Daniels)":
                    return competitors.FirstOrDefault(c => c.FullName == "Ashley Snow");
                case "Jeff Wingo":
                    return competitors.FirstOrDefault(c => c.FullName == "Jeffrey Wingo");
                case "Aris ":
                    return competitors.FirstOrDefault(c => c.FullName == "Aris DeMarco");
                case "Estelle Bonaire":
                    return competitors.FirstOrDefault(c => c.FullName == "Estelle Bonnaire");
                case "Wee Tze Yi":
                    return competitors.FirstOrDefault(c => c.FullName == "Tze Yi Wee");
                case "Rob I":
                    return competitors.FirstOrDefault(c => c.FullName == "Rob Ingenthron");
                case "Ray Byun":
                    return competitors.FirstOrDefault(c => c.FullName == "Raymond Byun");
                case "Marie-pascale Cote":
                    return competitors.FirstOrDefault(c => c.FullName == "Marie-Pascale Cote");
                case "Mikaila Finley Pittman":
                case "Mikaila Finley":
                    return competitors.FirstOrDefault(c => c.FullName == "Mikaila Pittman");
                case "Lisa M Picard":
                    return competitors.FirstOrDefault(c => c.FullName == "Lisa Picard");
                case "Dominique Morin.":
                    return competitors.FirstOrDefault(c => c.FullName == "Dominique Morin");
                case "Lilly Auclair":
                    return competitors.FirstOrDefault(c => c.FullName == "Lily Auclair");
                case "Riley crozier":
                    return competitors.FirstOrDefault(c => c.FullName == "Riley Crozier");
                case "Christine M Medin":
                    return competitors.FirstOrDefault(c => c.FullName == "Christine Medin");
                case "tammy duke":
                    return competitors.FirstOrDefault(c => c.FullName == "Tammy Duke");
                case "Alexandra BRANCO":
                    return competitors.FirstOrDefault(c => c.FullName == "Alexandra Branco");
                case "Mathieu COMPAGNON":
                    return competitors.FirstOrDefault(c => c.FullName == "Mathieu Compagnon");
                case "Mélodie Paletta":
                    return competitors.FirstOrDefault(c => c.FullName == "Melodie Paletta");
                case "Larissa Frisbee":
                case "Larissa Thayane (Frisbee)":
                    return competitors.FirstOrDefault(c => c.FullName == "Larissa Thayane");
                case "Sebastien Cadet":
                    return competitors.FirstOrDefault(c => c.FullName == "Sebastian Cadet");
                case "Agnes Maslanka":
                    return competitors.FirstOrDefault(c => c.FullName == "Agnieszka Maslanka");
                case "Madelyn Finley":
                    return competitors.FirstOrDefault(c => c.FullName == "Madelyn Clark");
                case "Jason Phillips":
                    return competitors.FirstOrDefault(c => c.FullName == "Jasson Phillips");
                case "Alyx Mccarthey":
                case "Alexandra Mccarthey":
                    return competitors.FirstOrDefault(c => c.FullName == "Alyx McCarthey");
                case "Cj Wheelock":
                    return competitors.FirstOrDefault(c => c.FullName == "CJ Wheelock");
                case "Abigail Schneider":
                    return competitors.FirstOrDefault(c => c.FullName == "Abi Schneider");
                case "D'leene Deboer":
                    return competitors.FirstOrDefault(c => c.FullName == "D'Leene DeBoer");
                case "Marie Soldevilla":
                    return competitors.FirstOrDefault(c => c.FullName == "Marie Soldevila");

                default:
                    return competitors.FirstOrDefault(c => c.FullName == firstName + " " + lastName);
            }
        }

        public static IJudge FindJudgeInCache(string firstName, string lastName, IEnumerable<IJudge> judges)
        {
            //correcting some frequent misspellings
            switch ((firstName.Trim() + " " + lastName.Trim()).Trim())
            {
                case "Sean McKeaver":
                    return judges.FirstOrDefault(c => c.FullName == "Sean McKeever");
                case "Tatiana Mollman":
                    return judges.FirstOrDefault(c => c.FullName == "Tatiana Mollmann");
                case "Bonnie":
                    return judges.FirstOrDefault(c => c.FullName == "Bonnie Cannon");
                case "Bryn":
                    return judges.FirstOrDefault(c => c.FullName == "Bryn Anderson");
                case "Jerome":
                    return judges.FirstOrDefault(c => c.FullName == "Jerome Subey");
                case "Annmarie":
                    return judges.FirstOrDefault(c => c.FullName == "Annmarie Marker");
                case "PJ":
                    return judges.FirstOrDefault(c => c.FullName == "PJ Turner");
                case "Patty":
                    return judges.FirstOrDefault(c => c.FullName == "Patty Vo");
                case "Sharole Lashe":
                case "Sharole Lashe Negrete":
                    return judges.FirstOrDefault(c => c.FullName == "Sharole Negrete");
                case "Jessica Mccurdy":
                    return judges.FirstOrDefault(c => c.FullName == "Jessica McCurdy");
                case "Phillipe Berne":
                    return judges.FirstOrDefault(c => c.FullName == "Philippe Berne");
                case "Robin (Pro)":
                    return judges.FirstOrDefault(c => c.FullName == "Robin Smith");
                case "Tren":
                case "Trendlyon":
                case "Trendlyon Veal":
                    return judges.FirstOrDefault(c => c.FullName == "Tren Veal");
                case "Yvonne Antonnacci":
                    return judges.FirstOrDefault(c => c.FullName == "Yvonne Antonacci");
                case "Alyssa Marie Glanville":
                    return judges.FirstOrDefault(c => c.FullName == "Alyssa Glanville");
                case "KP":
                case "Ken Rutland":
                    return judges.FirstOrDefault(c => c.FullName == "KP Rutland");
                case "Jen Deluca":
                    return judges.FirstOrDefault(c => c.FullName == "Jen DeLuca");

                default:
                    return judges.FirstOrDefault(c => c.FullName == firstName + " " + lastName);
            }
        }
    }
}
