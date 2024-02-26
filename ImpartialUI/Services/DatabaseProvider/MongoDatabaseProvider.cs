using System;
using System.Collections.Generic;
using Impartial;
using ImpartialUI.Services.DatabaseProvider.Helpers;

namespace ImpartialUI.Services.DatabaseProvider
{
    public class MongoDatabaseProvider
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

        public void InsertSdcEvent(IDanceConvention sdcEvent)
        {
            _helper.Insert(SDCEVENTS_TABLE_STRING, sdcEvent);
        }
        public void UpdateSdcEvent(Guid id, IDanceConvention sdcEvent)
        {
            _helper.UpsertById(SDCEVENTS_TABLE_STRING, id, sdcEvent);
        }
        public IDanceConvention GetSdcEventById(Guid id)
        {
            return _helper.LoadById<IDanceConvention>(SDCEVENTS_TABLE_STRING, id);
        }
        public List<IDanceConvention> GetAllSdcEvents()
        {
            return _helper.LoadAll<IDanceConvention>(SDCEVENTS_TABLE_STRING);
        }
        public void DeleteSdcEvent(IDanceConvention sdcEvent)
        {
            _helper.DeleteById<IDanceConvention>(SDCEVENTS_TABLE_STRING, sdcEvent.Id);
        }
        public void DeleteAllSdcEvents()
        {
            _helper.DeleteAll<IDanceConvention>(SDCEVENTS_TABLE_STRING);
        }

        public void InsertCompetition(ICompetition competition)
        {
            _helper.Insert(COMPETITIONS_TABLE_STRING, competition);
        }
        public void UpdateCompetition(Guid id, ICompetition competition)
        {
            _helper.UpsertById(COMPETITIONS_TABLE_STRING, id, competition);
        }
        public ICompetition GetCompetitionById(Guid id)
        {
            return _helper.LoadById<ICompetition>(COMPETITIONS_TABLE_STRING, id);
        }
        public List<ICompetition> GetAllCompetitions()
        {
            return _helper.LoadAll<ICompetition>(COMPETITIONS_TABLE_STRING);
        }
        public void DeleteCompetition(ICompetition competition)
        {
            _helper.DeleteById<ICompetition>(COMPETITIONS_TABLE_STRING, competition.Id);
        }
        public void DeleteAllCompetitions()
        {
            _helper.DeleteAll<ICompetition>(COMPETITIONS_TABLE_STRING);
        }

        public void InsertCompetitor(ICompetitor competitor)
        {
            _helper.Insert(COMPETITORS_TABLE_STRING, competitor);
        }
        public void UpdateCompetitor(Guid id, ICompetitor competitor)
        {
            _helper.UpsertById(COMPETITORS_TABLE_STRING, id, competitor);
        }
        public ICompetitor GetCompetitorById(Guid id)
        {
            return _helper.LoadById<ICompetitor>(COMPETITORS_TABLE_STRING, id);
        }
        public List<ICompetitor> GetAllCompetitors()
        {
            return _helper.LoadAll<ICompetitor>(COMPETITORS_TABLE_STRING);
        }
        public void DeleteCompetitor(ICompetitor competitor)
        {
            _helper.DeleteById<ICompetitor>(COMPETITORS_TABLE_STRING, competitor.CompetitorId);
        }
        public void DeleteAllCompetitors()
        {
            _helper.DeleteAll<ICompetitor>(COMPETITORS_TABLE_STRING);
        }

        public void InsertJudge(IJudge judge)
        {
            _helper.Insert(JUDGES_TABLE_STRING, judge);
        }
        public void UpdateJudge(Guid id, IJudge judge)
        {
            _helper.UpsertById(JUDGES_TABLE_STRING, id, judge);
        }
        public IJudge GetJudgeById(Guid id)
        {
            return _helper.LoadById<IJudge>(JUDGES_TABLE_STRING, id);
        }
        public IJudge GetJudgeByName(string firstName, string lastName)
        {
            return _helper.LoadByString<IJudge>(JUDGES_TABLE_STRING, firstName + " " + lastName);
        }
        public List<IJudge> GetAllJudges()
        {
            return _helper.LoadAll<IJudge>(JUDGES_TABLE_STRING);
        }
        public void DeleteJudge(IJudge judge)
        {
            _helper.DeleteById<IJudge>(JUDGES_TABLE_STRING, judge.JudgeId);
        }
        public void DeleteAllJudges()
        {
            _helper.DeleteAll<IJudge>(JUDGES_TABLE_STRING);
        }
    }
}
