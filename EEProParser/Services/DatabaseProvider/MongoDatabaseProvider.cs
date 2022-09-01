using System;
using System.Collections.Generic;


namespace Impartial
{
    public class MongoDatabaseProvider : IDatabaseProvider
    {
        private MongoHelper _helper;

        static string COMPETITIONS_TABLE_STRING = "Competitions";
        static string COMPETITORS_TABLE_STRING = "Competitors";
        static string JUDGES_TABLE_STRING = "Judges";
        static string SDCEVENTS_TABLE_STRING = "SdcEvents";

        public MongoDatabaseProvider(string databaseString)
        {
            _helper = new MongoHelper(databaseString);
        }

        public void InsertSdcEvent(SdcEvent sdcEvent)
        {
            _helper.Insert(SDCEVENTS_TABLE_STRING, sdcEvent);
        }
        public void UpdateSdcEvent(Guid id, SdcEvent sdcEvent)
        {
            _helper.UpsertById(SDCEVENTS_TABLE_STRING, id, sdcEvent);
        }
        public SdcEvent GetSdcEventById(Guid id)
        {
            return _helper.LoadById<SdcEvent>(SDCEVENTS_TABLE_STRING, id);
        }
        public List<SdcEvent> GetAllSdcEvents()
        {
            return _helper.LoadAll<SdcEvent>(SDCEVENTS_TABLE_STRING);
        }
        public void DeleteSdcEvent(SdcEvent sdcEvent)
        {
            _helper.DeleteById<SdcEvent>(SDCEVENTS_TABLE_STRING, sdcEvent.Id);
        }
        public void DeleteAllSdcEvents()
        {
            _helper.DeleteAll<SdcEvent>(SDCEVENTS_TABLE_STRING);
        }

        public void InsertCompetition(Competition competition)
        {
            _helper.Insert(COMPETITIONS_TABLE_STRING, competition);
        }
        public void UpdateCompetition(Guid id, Competition competition)
        {
            _helper.UpsertById(COMPETITIONS_TABLE_STRING, id, competition);
        }
        public Competition GetCompetitionById(Guid id)
        {
            return _helper.LoadById<Competition>(COMPETITIONS_TABLE_STRING, id);
        }
        public List<Competition> GetAllCompetitions()
        {
            return _helper.LoadAll<Competition>(COMPETITIONS_TABLE_STRING);
        }
        public void DeleteCompetition(Competition competition)
        {
            _helper.DeleteById<Competition>(COMPETITIONS_TABLE_STRING, competition.Id);
        }
        public void DeleteAllCompetitions()
        {
            _helper.DeleteAll<Competition>(COMPETITIONS_TABLE_STRING);
        }

        public void InsertCompetitor(Competitor competitor)
        {
            _helper.Insert(COMPETITORS_TABLE_STRING, competitor);
        }
        public void UpdateCompetitor(Guid id, Competitor competitor)
        {
            _helper.UpsertById(COMPETITORS_TABLE_STRING, id, competitor);
        }
        public Competitor GetCompetitorById(Guid id)
        {
            return _helper.LoadById<Competitor>(COMPETITORS_TABLE_STRING, id);
        }
        public List<Competitor> GetAllCompetitors()
        {
            return _helper.LoadAll<Competitor>(COMPETITORS_TABLE_STRING);
        }
        public void DeleteCompetitor(Competitor competitor)
        {
            _helper.DeleteById<Competitor>(COMPETITORS_TABLE_STRING, competitor.Id);
        }
        public void DeleteAllCompetitors()
        {
            _helper.DeleteAll<Competitor>(COMPETITORS_TABLE_STRING);
        }

        public void InsertJudge(Judge judge)
        {
            _helper.Insert(JUDGES_TABLE_STRING, judge);
        }
        public void UpdateJudge(Guid id, Judge judge)
        {
            _helper.UpsertById(JUDGES_TABLE_STRING, id, judge);
        }
        public Judge GetJudgeById(Guid id)
        {
            return _helper.LoadById<Judge>(JUDGES_TABLE_STRING, id);
        }
        public List<Judge> GetAllJudges()
        {
            return _helper.LoadAll<Judge>(JUDGES_TABLE_STRING);
        }
        public void DeleteJudge(Judge judge)
        {
            _helper.DeleteById<Judge>(JUDGES_TABLE_STRING, judge.Id);
        }
        public void DeleteAllJudges()
        {
            _helper.DeleteAll<Judge>(JUDGES_TABLE_STRING);
        }
    }
}
