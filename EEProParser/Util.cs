using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Impartial.Enums;
using System.Linq;

namespace Impartial
{
    public enum Tier
    {
        Tier0,
        Tier1,
        Tier2,
        Tier3,
        Tier4,
        Tier5,
        Tier6
    }

    public static class Util
    {
        public static int GetAwardedPoints(int numberOfCompetitors, int placement)
        {
            if (numberOfCompetitors < 5 && numberOfCompetitors >= 1)
                return GetAwardedPoints(Tier.Tier0, placement);
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

        public static int GetAwardedPoints(Tier tier, int placement)
        {
            switch (tier)
            {
                case Tier.Tier0:
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
                    else if (placement > 5 && placement <= 12)
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
                    else if (placement > 5 && placement <= 12)
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
                    else if (placement > 5 && placement <= 12)
                        return 2;
                    else
                        return 0;
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
                default:
                case CallbackScore.No:
                    return "N";
            }
        }

        public static CallbackScore StringToCallbackScore(string score)
        {
            switch (score)
            {
                case "A1":
                case "Alt1":
                case "Alternate1":
                case "45":
                    return CallbackScore.Alt1;
                case "A2":
                case "Alt2":
                case "Alternate2":
                case "43":
                    return CallbackScore.Alt2;
                case "A3":
                case "Alt3":
                case "Alternate3":
                case "42":
                    return CallbackScore.Alt3;
                case "Y":
                case "Yes":
                case "100":
                    return CallbackScore.Yes;
                default:
                case "N":
                case "No":
                case "0":
                    return CallbackScore.No;
            }
        }

        public static Role StringToRole(string role)
        {
            switch (role)
            {
                case "Follower":
                    return Role.Follower;
                default:
                case "Leader":
                    return Role.Leader;
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

                default:
                case 0:
                    return CallbackScore.No;
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
        public static IPersonModel GetClosestPersonByFirstName(string input, IEnumerable<IPersonModel> list)
        {
            int leastDistance = 10000;
            IPersonModel match = null;

            foreach (IPersonModel p in list)
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
            if (s.IndexOf(from) == -1)
                throw new Exception("String \'" + from + "\' not found in searched string");

            int fromIndex = s.IndexOf(from) + from.Length;
            string choppedString = s.Substring(fromIndex, s.Length - fromIndex);

            int toIndex = choppedString.IndexOf(to) + fromIndex;
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

        public static IEnumerable<T> RemoveWhere<T>(this IEnumerable<T> query, Predicate<T> predicate)
        {
            return query.Where(e => !predicate(e));
        }

        public static Competitor FindCompetitorInCache(string firstName, string lastName, IEnumerable<Competitor> competitors)
        {
            //correcting some frequent misspellings
            switch (firstName + " " + lastName)
            {
                case "Aidan Keith-hynes":
                case "Aidan Keith Hynes":
                case "Aiden Keith Hynes":
                    return competitors.Where(c => c.FullName == "Aidan Keith-Hynes")?.FirstOrDefault();
                case "Jt Anderson":
                case "JT anderson":
                case "Jt anderson":
                    return competitors.Where(c => c.FullName == "JT Anderson")?.FirstOrDefault();
                case "Dimtri Hector":
                    return competitors.Where(c => c.FullName == "Dimitri Hector")?.FirstOrDefault();
                case "Conor mcclure":
                case "Conor Mcclure":
                    return competitors.Where(c => c.FullName == "Conor McClure")?.FirstOrDefault();
                case "Aris Demarco":
                    return competitors.Where(c => c.FullName == "Aris DeMarco")?.FirstOrDefault();
                case "J Khayree Jones":
                    return competitors.Where(c => c.FullName == "Khayree Jones")?.FirstOrDefault();
                case "JennAy Ferreira":
                    return competitors.Where(c => c.FullName == "Jen Ferreira")?.FirstOrDefault();
                case "Karla Anita Catana":
                    return competitors.Where(c => c.FullName == "Karla Catana")?.FirstOrDefault();
                case "Stephanie Risser":
                case "Stephanie Risser Loveira":
                    return competitors.Where(c => c.FullName == "Stephanie Loveira")?.FirstOrDefault();
                case "Samuel Vaden":
                    return competitors.Where(c => c.FullName == "Sam Vaden")?.FirstOrDefault();
                case "Casey Lane Margules":
                    return competitors.Where(c => c.FullName == "Casey Margules")?.FirstOrDefault();
                case "Margaret Tuttle":
                    return competitors.Where(c => c.FullName == "Margie Tuttle")?.FirstOrDefault();
                case "Alexander Glover":
                    return competitors.Where(c => c.FullName == "Alex Glover")?.FirstOrDefault();
                case "Frank Moda III":
                    return competitors.Where(c => c.FullName == "Frank Moda")?.FirstOrDefault();
                case "Kelly Ponce De Leon":
                    return competitors.Where(c => c.FullName == "Kelly Ponce de Leon")?.FirstOrDefault();
                case "Agustin Gutierrez":
                    return competitors.Where(c => c.FullName == "Augie Leija")?.FirstOrDefault();
                case "Saya Suzaki":
                    return competitors.Where(c => c.FullName == "Sayaka Suzaki")?.FirstOrDefault();
                case "Brittani Schiro":
                    return competitors.Where(c => c.FullName == "Brittany Schiro")?.FirstOrDefault();
                case "Xander Stavola":
                    return competitors.Where(c => c.FullName == "Alexander Stavola")?.FirstOrDefault();
                case "Skyler Pritchard":
                    return competitors.Where(c => c.FullName == "Skylar Pritchard")?.FirstOrDefault();
                case "Jade Ruiz":
                    return competitors.Where(c => c.FullName == "Jade Bryan")?.FirstOrDefault();
                case "Julie Edwards-Auclair":
                    return competitors.Where(c => c.FullName == "Julie Auclair")?.FirstOrDefault();
                case "Vincent van Mierlo":
                    return competitors.Where(c => c.FullName == "Vincent Van Mierlo")?.FirstOrDefault();
                case "Kyle Fitzgerald":
                    return competitors.Where(c => c.FullName == "Kyle FitzGerald")?.FirstOrDefault();
                case "Joel Jiminez":
                    return competitors.Where(c => c.FullName == "Joel Jimenez")?.FirstOrDefault();
                case "Talía Colón":
                    return competitors.Where(c => c.FullName == "Talia Colon")?.FirstOrDefault();
                case "Jess Ann Nail":
                    return competitors.Where(c => c.FullName == "Jes Ann Nail")?.FirstOrDefault();
                case "Carlie Obrien":
                    return competitors.Where(c => c.FullName == "Carlie O'Brien")?.FirstOrDefault();
                case "Katie Smiley":
                    return competitors.Where(c => c.FullName == "Katie Smiley-Oyen")?.FirstOrDefault();
                case "Fae Xenovia":
                    return competitors.Where(c => c.FullName == "Fae Ashley")?.FirstOrDefault();
                case "Jung Won Choe":
                case "Jung Won Cho":
                    return competitors.Where(c => c.FullName == "Jung Choe")?.FirstOrDefault();
                default:
                    return competitors.Where(c => c.FullName == firstName + " " + lastName)?.FirstOrDefault();
            }
        }

        public static Judge FindJudgeInCache(string firstName, string lastName, IEnumerable<Judge> judges)
        {
            //correcting some frequent misspellings
            switch ((firstName.Trim() + " " + lastName.Trim()).Trim())
            {
                case "Sean McKeaver":
                    return judges.Where(c => c.FullName == "Sean McKeever")?.FirstOrDefault();
                case "Tatiana Mollman":
                    return judges.Where(c => c.FullName == "Tatiana Mollmann")?.FirstOrDefault();
                case "Bonnie":
                    return judges.Where(c => c.FullName == "Bonnie Cannon")?.FirstOrDefault();
                case "Bryn":
                    return judges.Where(c => c.FullName == "Bryn Anderson")?.FirstOrDefault();
                case "Jerome":
                    return judges.Where(c => c.FullName == "Jerome Subey")?.FirstOrDefault();
                case "Annmarie":
                    return judges.Where(c => c.FullName == "Annmarie Marker")?.FirstOrDefault();
                case "PJ":
                    return judges.Where(c => c.FullName == "PJ Turner")?.FirstOrDefault();
                case "Patty":
                    return judges.Where(c => c.FullName == "Patty Vo")?.FirstOrDefault();
                case "Sharole Lashe":
                    return judges.Where(c => c.FullName == "Sharole Negrete")?.FirstOrDefault();

                default:
                    return judges.Where(c => c.FullName == firstName + " " + lastName)?.FirstOrDefault();
            }
        }
    }
}
