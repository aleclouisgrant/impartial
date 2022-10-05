using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
