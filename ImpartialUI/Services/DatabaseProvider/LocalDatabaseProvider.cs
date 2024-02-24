using System;
using System.Collections.Generic;
using Impartial;

namespace ImpartialUI.Services.DatabaseProvider
{
    public class LocalDatabaseProvider
    {
        private List<IDanceConvention> _events = new List<IDanceConvention>();
        private List<ICompetition> _competitions = new List<ICompetition>();
        private List<ICompetitor> _competitors = new List<ICompetitor>();
        private List<IJudge> _judges = new List<IJudge>();

        public LocalDatabaseProvider() { }

        public void InsertSdcEvent(IDanceConvention sdcEvent)
        {
            _events.Add(sdcEvent);
        }
        public void UpdateSdcEvent(Guid id, IDanceConvention sdcEvent)
        {
            var evt = _events.Find(e => e.Id == id);
            if (evt != null)
                _events.Remove(evt);

            _events.Add(sdcEvent);
        }
        public IDanceConvention GetSdcEventById(Guid id)
        {
            return _events.Find(e => id == e.Id);
        }
        public List<IDanceConvention> GetAllSdcEvents()
        {
            return _events;
        }
        public void DeleteSdcEvent(IDanceConvention sdcEvent)
        {
            _events.Remove(sdcEvent);
        }
        public void DeleteAllSdcEvents()
        {
            _events.Clear();
        }

        public void InsertCompetition(ICompetition competition)
        {
            _competitions.Add(competition);
        }
        public void UpdateCompetition(Guid id, ICompetition competition)
        {
            var comp = _competitions.Find(c => c.Id == id);
            if (comp != null)
                _competitions.Remove(comp);

            _competitions.Add(competition);
        }
        public ICompetition GetCompetitionById(Guid id)
        {
            return _competitions.Find(c => id == c.Id);
        }
        public List<ICompetition> GetAllCompetitions()
        {
            return _competitions;
        }
        public void DeleteCompetition(ICompetition competition)
        {
            _competitions.Remove(competition);
        }
        public void DeleteAllCompetitions()
        {
            _competitions.Clear();
        }

        public void InsertCompetitor(ICompetitor competitor)
        {
            _competitors.Add(competitor);
        }
        public void UpdateCompetitor(Guid id, ICompetitor competitor)
        {
            var comp = _competitors.Find(c => c.Id == id);
            if (comp != null)
                _competitors.Remove(comp);

            _competitors.Add(competitor);
        }
        public ICompetitor GetCompetitorById(Guid id)
        {
            return _competitors.Find(c => c.Id == id);
        }
        public List<ICompetitor> GetAllCompetitors()
        {
            return _competitors;
        }
        public void DeleteCompetitor(ICompetitor competitor)
        {
            _competitors.Remove(competitor);
        }
        public void DeleteAllCompetitors()
        {
            _competitors.Clear();
        }

        public void InsertJudge(IJudge judge)
        {
            _judges.Add(judge);
        }
        public void UpdateJudge(Guid id, IJudge judge)
        {
            var jud = _judges.Find(j => j.Id == id);
            if (jud != null)
                _judges.Remove(jud);

            _judges.Add(judge);
        }
        public IJudge GetJudgeById(Guid id)
        {
            return _judges.Find(j => j.Id == id);
        }
        public IJudge GetJudgeByName(string firstName, string lastName)
        {
            return _judges.Find(j => j.FirstName == firstName && j.LastName == lastName);
        }
        public List<IJudge> GetAllJudges()
        {
            return _judges;
        }
        public void DeleteJudge(IJudge judge)
        {
            _judges.Remove(judge);
        }
        public void DeleteAllJudges()
        {
            _judges.Clear();
        }

        //public Task UpsertCompetitionAsync(Competition competition)
        //{

        //}

        //public Task<Competition> GetCompetitionByIdAsync(Guid id)
        //{
        //    return Task.FromResult(0);
        //}

        //public Task DeleteCompetitionAsync(Competition competition)
        //{
        //    return Task.FromResult(0);
        //}

        //public Task<IEnumerable<Competition>> GetAllCompetitionsAsync()
        //{
        //    return Task.FromResult(0);
        //}

        //public Task DeleteAllCompetitionsAsync()
        //{
        //    return Task.FromResult(0);
        //}

        //public Task UpsertCompetitorAsync(Competitor competitor)
        //{
        //    return Task.FromResult(0);
        //}

        //public Task<Competitor> GetCompetitorByIdAsync(Guid id)
        //{
        //}

        //public Task<Competitor> GetCompetitorByNameAsync(string firstName, string lastName)
        //{
        //}

        //public Task DeleteCompetitorAsync(Competitor competitor)
        //{
        //}

        //public Task<IEnumerable<Competitor>> GetAllCompetitorsAsync()
        //{
        //}

        //public Task DeleteAllCompetitorsAsync()
        //{
        //}

        //public Task UpsertJudgeAsync(Judge judge)
        //{
        //}

        //public Task<Judge> GetJudgeByIdAsync(Guid id)
        //{
        //}

        //public Task<Judge> GetJudgeByNameAsync(string firstName, string lastName)
        //{
        //}

        //public Task DeleteJudgeAsync(Judge judge)
        //{
        //}

        //public Task<IEnumerable<Judge>> GetAllJudgesAsync()
        //{
        //}

        //public Task DeleteAllJudgesAsync()
        //{
        //}

        //public Task<IEnumerable<CompetitorDataModel>> GetCompetitorDataModelsAsync()
        //{
        //}

        //public Task UpsertCompetitorDataModelAsync(CompetitorDataModel competitorDataModel)
        //{
        //}
    }
}
