using System;
using System.Collections.Generic;
using System.Text;

namespace Impartial
{
    public interface IDatabaseProvider
    {
        void InsertSdcEvent(SdcEvent sdcEvent);
        void UpdateSdcEvent(Guid id, SdcEvent sdcEvent);
        SdcEvent GetSdcEventById(Guid id);
        void DeleteSdcEvent(SdcEvent sdcEvent);
        List<SdcEvent> GetAllSdcEvents();
        void DeleteAllSdcEvents();

        void InsertCompetition(Competition competition);
        void UpdateCompetition(Guid id, Competition competition);
        Competition GetCompetitionById(Guid id);
        void DeleteCompetition(Competition competition);
        List<Competition> GetAllCompetitions();
        void DeleteAllCompetitions();

        void InsertCompetitor(Competitor competitor);
        void UpdateCompetitor(Guid id, Competitor competitor);
        Competitor GetCompetitorById(Guid id);
        void DeleteCompetitor(Competitor competitor);
        List<Competitor> GetAllCompetitors();
        void DeleteAllCompetitors();

        void InsertJudge(Judge judge);
        void UpdateJudge(Guid id, Judge judge);
        Judge GetJudgeById(Guid id);
        void DeleteJudge(Judge judge);
        List<Judge> GetAllJudges();
        void DeleteAllJudges();
    }
}
