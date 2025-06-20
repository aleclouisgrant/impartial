namespace Impartial.PgTableSchema
{
    public static class PgDanceConventionsTableSchema
    {
        public static string TableName => "dance_conventions";

        public static string Id => "id";
        public static string Name => "name";
        public static string Date => "date";
        //public static string Location => "location";
        //public static string Timezone => "timezone";
    }

    public static class PgUsersTableSchema
    {
        public static string TableName => "users";

        public static string Id => "id";
        public static string FirstName => "first_name";
        public static string LastName => "last_name";
    }

    public static class PgJudgeProfilesTableSchema
    {
        public static string TableName => "judge_profiles";

        public static string Id => "id";
        public static string UserId => "user_id";
    }

    public static class PgCompetitorProfilesTableSchema
    {
        public static string TableName => "competitor_profiles";

        public static string Id => "id";
        public static string UserId => "user_id";
        public static string WsdcId => "wsdc_id";
        public static string LeaderRating => "leader_rating";
        public static string FollowerRating => "follower_rating";
        public static string LeaderNewcomerPoints => "leader_newcomer_points";
        public static string LeaderNovicePoints => "leader_novice_points";
        public static string LeaderIntermediatePoints => "leader_intermediate_points";
        public static string LeaderAdvancedPoints => "leader_advanced_points";
        public static string LeaderAllStarPoints => "leader_all_star_points";
        public static string LeaderChampionPoints => "leader_champion_points";
        public static string FollowerNewcomerPoints => "follower_newcomer_points";
        public static string FollowerNovicePoints => "follower_novice_points";
        public static string FollowerIntermediatePoints => "follower_intermediate_points";
        public static string FollowerAdvancedPoints => "follower_advanced_points";
        public static string FollowerAllStarPoints => "follower_all_star_points";
        public static string FollowerChampionPoints => "follower_champion_points";
    }

    public static class PgCompetitorRegistrationsTableSchema
    {
        public static string TableName => "competitor_registrations";

        public static string Id => "id";
        public static string CompetitorProfileId => "competitor_profile_id";
        public static string DanceConventionId => "dance_convention_id";
        public static string BibNumber => "bib_number";
    }

    public static class PgCompetitorRecordsTableSchema
    {
        public static string TableName => "competitor_records";

        public static string Id => "id";
        public static string CompetitionId => "competition_id";
        public static string CompetitorRegistrationId => "competitor_registration_id";
        public static string Placement => "placement";
        public static string PointsEarned => "points_earned";
        public static string PreCompetitionRating => "pre_rating";
        public static string PostCompetitionEventRating => "post_rating";
    }

    public static class PgCompetitionsTableSchema
    {
        public static string TableName => "competitions";

        public static string Id => "id";
        public static string DanceConventionId => "dance_convention_id";
        public static string Division => "division";
        public static string LeaderTier => "leader_tier";
        public static string FollowerTier => "follower_tier";
    }

    public static class PgPrelimCompetitionsTableSchema
    {
        public static string TableName => "prelim_competitions";

        public static string Id => "id";
        public static string CompetitionId => "competition_id";
        public static string DateTime => "date_time";
        public static string Role => "role";
        public static string Round => "round";
        public static string Alternate1Id => "alternate_1_id";
        public static string Alternate2Id => "alternate_2_id";
    }

    public static class PgFinalCompetitionsTableSchema
    {
        public static string TableName => "final_competitions";

        public static string Id => "id";
        public static string CompetitionId => "competition_id";
        public static string DateTime => "date_time";
    }

    public static class PgPrelimScoresTableSchema
    {
        public static string TableName => "prelim_scores";

        public static string Id => "id";
        public static string PrelimCompetitionId => "prelim_competition_id";
        public static string JudgeId => "judge_id";
        public static string CompetitorId => "competitor_id";
        public static string CallbackScore => "callback_score";
    }

    public static class PgFinalScoresTableSchema
    {
        public static string TableName => "final_scores";

        public static string Id => "id";
        public static string PrelimCompetitionId => "final_competition_id";
        public static string JudgeId => "judge_id";
        public static string LeaderId => "leader_id";
        public static string FollowerId => "follower_id";
        public static string Score => "score";
    }

    public static class PgPromotedCompetitorsTableSchema
    {
        public static string TableName => "promoted_competitors";

        public static string Id => "id";
        public static string PrelimCompetitionId => "prelim_competition_id";
        public static string CompetitorId => "competitor_id";
    }

    public static class PgPlacementsTableSchema
    {
        public static string TableName => "placements";

        public static string Id => "id";
        public static string FinalCompetitionId => "final_competition_id";
        public static string LeaderId => "leader_id";
        public static string FollowerId => "follower_id";
        public static string Placement => "placement";
    }
}