using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Impartial
{
    public class LocalDatabaseProvider 
    {
        private List<DanceConvention> _events = new List<DanceConvention>();
        private List<Competition> _competitions = new List<Competition>();
        private List<Competitor> _competitors = new List<Competitor>();
        private List<Judge> _judges = new List<Judge>();

        public LocalDatabaseProvider() { }

        public void InsertSdcEvent(DanceConvention sdcEvent)
        {
            _events.Add(sdcEvent);
        }
        public void UpdateSdcEvent(Guid id, DanceConvention sdcEvent)
        {
            var evt = _events.Find(e => e.Id == id);
            if (evt != null)
                _events.Remove(evt);

            _events.Add(sdcEvent);
        }
        public DanceConvention GetSdcEventById(Guid id)
        {
            return _events.Find(e => id == e.Id);
        }
        public List<DanceConvention> GetAllSdcEvents()
        {
            return _events;
        }
        public void DeleteSdcEvent(DanceConvention sdcEvent)
        {
            _events.Remove(sdcEvent);
        }
        public void DeleteAllSdcEvents()
        {
            _events.Clear();
        }

        public void InsertCompetition(Competition competition)
        {
            _competitions.Add(competition);
        }
        public void UpdateCompetition(Guid id, Competition competition)
        {
            var comp = _competitions.Find(c => c.Id == id);
            if (comp != null)
                _competitions.Remove(comp);

            _competitions.Add(competition);
        }
        public Competition GetCompetitionById(Guid id)
        {
            return _competitions.Find(c => id == c.Id);
        }
        public List<Competition> GetAllCompetitions()
        {
            return _competitions;
        }
        public void DeleteCompetition(Competition competition)
        {
            _competitions.Remove(competition);
        }
        public void DeleteAllCompetitions()
        {
            _competitions.Clear();
        }

        public void InsertCompetitor(Competitor competitor)
        {
            _competitors.Add(competitor);
        }
        public void UpdateCompetitor(Guid id, Competitor competitor)
        {
            var comp = _competitors.Find(c => c.Id == id);
            if (comp != null)
                _competitors.Remove(comp);

            _competitors.Add(competitor);
        }
        public Competitor GetCompetitorById(Guid id)
        {
            return _competitors.Find(c => c.Id == id);
        }
        public List<Competitor> GetAllCompetitors()
        {
            return _competitors;
        }
        public void DeleteCompetitor(Competitor competitor)
        {
            _competitors.Remove(competitor);
        }
        public void DeleteAllCompetitors()
        {
            _competitors.Clear();
        }

        public void InsertJudge(Judge judge)
        {
            _judges.Add(judge);
        }
        public void UpdateJudge(Guid id, Judge judge)
        {
            var jud = _judges.Find(j => j.Id == id);
            if (jud != null)
                _judges.Remove(jud);

            _judges.Add(judge);
        }
        public Judge GetJudgeById(Guid id)
        {
            return _judges.Find(j => j.Id == id);
        }
        public Judge GetJudgeByName(string firstName, string lastName)
        {
            return _judges.Find(j => j.FirstName== firstName && j.LastName == lastName);
        }
        public List<Judge> GetAllJudges()
        {
            return _judges;
        }
        public void DeleteJudge(Judge judge)
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
