using Impartial;
using Impartial.Enums;
using System;

namespace ImpartialUI.Models.PgModels
{
    public class PgUserModel
    {
        public Guid? id { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
    }

    public class PgCompetitorProfileModel
    {
        public Guid? id { get; set; }
        public Guid? user_id { get; set; }
        public int? wsdc_id { get; set; }

        public int? leader_rating { get; set; }
        public int? follower_rating { get; set; }

        public int? leader_newcomer_points { get; set; }
        public int? leader_novice_points { get; set; }
        public int? leader_intermediate_points { get; set; }
        public int? leader_advanced_points { get; set; }
        public int? leader_all_star_points { get; set; }
        public int? leader_champion_points { get; set; }

        public int? follower_newcomer_points { get; set; }
        public int? follower_novice_points { get; set; }
        public int? follower_intermediate_points { get; set; }
        public int? follower_advanced_points { get; set; }
        public int? follower_all_star_points { get; set; }
        public int? follower_champion_points { get; set; }
    }

    public class PgCompetitorRegistrationModel
    {
        public Guid? id { get; set; }
        public Guid? competitor_profile_id { get; set; }
        public Guid? dance_convention_id { get; set; }
        public string? bib_number { get; set; }
    }

    public class PgCompetitorRecordModel
    {
        public Guid? id { get; set; }
        public Guid? competition_id { get; set; }
        public Guid? competitor_registration_id { get; set; }
        public int? placement { get; set; }
        public int? points_earned { get; set; }
        public int? pre_rating { get; set; }
        public int? post_rating { get; set; }
    }

    public class PgJudgeProfileModel
    {
        public Guid? id { get; set; }
        public Guid? user_id { get; set; }
    }

    public class PgDanceConventionModel
    {
        public Guid? id { get; set; }
        public string? name { get; set; }
        public DateTime? date { get; set; }
    }

    public class PgCompetitionModel
    {
        public Guid? id { get; set; }
        public Guid? dance_convention_id { get; set; }
        public Division? division { get; set; }
        public Tier? leader_tier { get; set; }
        public Tier? follower_tier { get; set; }
    }

    public class PgPrelimCompetitionModel
    {
        public Guid? id { get; set; }
        public Guid? competition_id { get; set; }
        public DateTime? date_time { get; set; }
        public Role? role { get; set; }
        public Round? round { get; set; }
        public Guid? alternate1_id { get; set; }
        public Guid? alternate2_id { get; set; }
    }

    public class PgPromotedCompetitorModel
    {
        public Guid? id { get; set; }
        public Guid? prelim_competition_id { get; set; }
        public Guid? competitor_id { get; set;}
    }

    public class PgFinalCompetitionModel
    {
        public Guid? id { get; set; }
        public Guid? competition_id { get; set; }
        public DateTime? date_time { get; set; }
    }

    public class PgPlacementModel
    {
        public Guid? id { get; set; }
        public Guid? final_competition_id { get; set; }
        public Guid? leader_id { get; set; }
        public Guid? follower_id { get; set; }
        public int? placement { get; set; }
    }

    public class PgPrelimScoreModel
    {
        public Guid? id { get; set; }
        public Guid? prelim_competition_id { get; set; }
        public Guid? judge_id { get; set; }
        public Guid? competitor_id { get; set; }
        public CallbackScore? callback_score { get; set; }
    }

    public class PgFinalScoreModel
    {
        public Guid? id { get; set; }
        public Guid? final_competition_id { get; set; }
        public Guid? judge_id { get; set; }
        public Guid? leader_id { get; set; }
        public Guid? follower_id { get; set; }
        public int? score { get; set; }
    }
}