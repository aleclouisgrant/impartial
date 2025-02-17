using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Impartial;
using ImpartialUI.Models;

namespace ImpartialUI.Services.DatabaseProvider
{
    public class LocalDatabaseProvider : IDatabaseProvider
    {
        private List<IDanceConvention> _danceConventions = new List<IDanceConvention>();
        private List<ICompetition> _competitions = new List<ICompetition>();
        private List<ICompetitor> _competitors = new List<ICompetitor>();
        private List<IJudge> _judges = new List<IJudge>();

        public LocalDatabaseProvider(bool populateWithTestData = false)
        {
            if (populateWithTestData)
            {
                PopulateTestData();
            }
        }

        public Task DeleteAllCompetitionsAsync()
        {
            _competitions.Clear();
            return Task.CompletedTask;
        }

        public Task DeleteAllCompetitorsAsync()
        {
            _competitors.Clear();
            return Task.CompletedTask;
        }

        public Task DeleteAllDanceConventionsAsync()
        {
            _danceConventions.Clear();
            return Task.CompletedTask;
        }

        public Task DeleteAllJudgesAsync()
        {
            _judges.Clear();
            return Task.CompletedTask;
        }

        public Task DeleteCompetitionAsync(Guid id)
        {
            _competitions.RemoveWhere(c => c.Id == id);
            return Task.CompletedTask;
        }

        public Task DeleteCompetitorAsync(Guid id)
        {
            _competitors.RemoveWhere(c => c.CompetitorId == id);
            return Task.CompletedTask;
        }

        public Task DeleteDanceConventionAsync(Guid id)
        {
            _danceConventions.RemoveWhere(c => c.Id == id);
            return Task.CompletedTask;
        }

        public Task DeleteJudgeAsync(Guid id)
        {
            _judges.RemoveWhere(j => j.JudgeId == id);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ICompetition>> GetAllCompetitionsAsync()
        {
            var tcs = new TaskCompletionSource<IEnumerable<ICompetition>>();
            tcs.SetResult(_competitions);

            return tcs.Task;
        }

        public Task<IEnumerable<ICompetitor>> GetAllCompetitorsAsync()
        {
            var tcs = new TaskCompletionSource<IEnumerable<ICompetitor>>();
            tcs.SetResult(_competitors);

            return tcs.Task;
        }

        public Task<IEnumerable<IDanceConvention>> GetAllDanceConventionsAsync()
        {
            var tcs = new TaskCompletionSource<IEnumerable<IDanceConvention>>();
            tcs.SetResult(_danceConventions);

            return tcs.Task;
        }

        public Task<IEnumerable<IJudge>> GetAllJudgesAsync()
        {
            var tcs = new TaskCompletionSource<IEnumerable<IJudge>>();
            tcs.SetResult(_judges);

            return tcs.Task;
        }

        public Task<ICompetition> GetCompetitionAsync(Guid id)
        {
            var tcs = new TaskCompletionSource<ICompetition>();
            tcs.SetResult(_competitions.Where(c => c.Id == id).FirstOrDefault());

            return tcs.Task;
        }

        public Task<ICompetitor> GetCompetitorAsync(Guid id)
        {
            var tcs = new TaskCompletionSource<ICompetitor>();
            tcs.SetResult(_competitors.Where(c => c.CompetitorId == id).FirstOrDefault());

            return tcs.Task;
        }

        public Task<ICompetitor> GetCompetitorAsync(string firstName, string lastName)
        {
            var tcs = new TaskCompletionSource<ICompetitor>();
            tcs.SetResult(_competitors.Where(c => c.FirstName == firstName && c.LastName == lastName).FirstOrDefault());

            return tcs.Task;
        }

        public Task<IDanceConvention> GetDanceConventionAsync(Guid id)
        {
            var tcs = new TaskCompletionSource<IDanceConvention>();
            tcs.SetResult(_danceConventions.Where(c => c.Id == id).FirstOrDefault());

            return tcs.Task;
        }

        public Task<IJudge> GetJudgeAsync(Guid id)
        {
            var tcs = new TaskCompletionSource<IJudge>();
            tcs.SetResult(_judges.Where(j => j.JudgeId == id).FirstOrDefault());

            return tcs.Task;
        }

        public Task<IJudge> GetJudgeAsync(string firstName, string lastName)
        {
            var tcs = new TaskCompletionSource<IJudge>();
            tcs.SetResult(_judges.Where(j => j.FirstName == firstName && j.LastName == lastName).FirstOrDefault());

            return tcs.Task;
        }

        public Task UpsertCompetitionAsync(ICompetition competition, Guid danceConventionId)
        {
            _competitions.Add(competition);
            return Task.CompletedTask;
        }

        public Task UpsertCompetitorAsync(ICompetitor competitor)
        {
            _competitors.Add(competitor);
            return Task.CompletedTask;
        }

        public Task UpsertDanceConventionAsync(IDanceConvention convention)
        {
            _danceConventions.Add(convention);
            return Task.CompletedTask;
        }

        public Task UpsertJudgeAsync(IJudge judge)
        {
            _judges.Add(judge);
            return Task.CompletedTask;
        }

        private void PopulateTestData()
        {
            var danceConvention = new DanceConvention(
                name: "Countdown Swing Boston", 
                date: DateTime.Parse("12-31-2022"));

            var competition = new Competition(
                danceConventionId: danceConvention.Id, 
                name: danceConvention.Name, 
                date: DateTime.Parse("12-31-2022"), 
                division: Division.AllStar);

            IJudge anne = new Judge("Anne", "Fleming");
            IJudge arjay = new Judge("Arjay", "Centeno");
            IJudge bryn = new Judge("Bryn", "Anderson");
            IJudge john = new Judge("John", "Lindo");
            IJudge lemery = new Judge("Lemery", "Rollinscott");
            IJudge greg = new Judge("Greg", "Rollinscott");
            IJudge victoria = new Judge("Victoria", "Henk");

            ICompetitorRegistration brandon  = new CompetitorRegistration(new Competitor("Brandon", "Rasmussen"), "320");
            ICompetitorRegistration melodie = new CompetitorRegistration(new Competitor("Melodie", "Paletta"), "367");
            ICompetitorRegistration neil = new CompetitorRegistration(new Competitor("Neil", "Joshi"), "131");
            ICompetitorRegistration kristen = new CompetitorRegistration(new Competitor("Kristen", "Wallace"), "396");
            ICompetitorRegistration lucky = new CompetitorRegistration(new Competitor("Lucky", "Sipin"), "159");
            ICompetitorRegistration dimitri = new CompetitorRegistration(new Competitor("Dimitri", "Hector"), "197");
            ICompetitorRegistration maxwell = new CompetitorRegistration(new Competitor("Maxwell", "Thew"), "275");
            ICompetitorRegistration shanna = new CompetitorRegistration(new Competitor("Shanna", "Porcari"), "179");
            ICompetitorRegistration oscar = new CompetitorRegistration(new Competitor("Oscar", "Hampton"), "370");
            ICompetitorRegistration saya = new CompetitorRegistration(new Competitor("Sayaka", "Suzaki"), "102");
            ICompetitorRegistration joshu = new CompetitorRegistration(new Competitor("Joshu", "Creel"), "316");
            ICompetitorRegistration jen = new CompetitorRegistration(new Competitor("Jen", "Ferreira"), "447");
            ICompetitorRegistration edem = new CompetitorRegistration(new Competitor("Edem", "Attikese"), "105");
            ICompetitorRegistration jia = new CompetitorRegistration(new Competitor("Jia", "Lu"), "364");
            ICompetitorRegistration kyle = new CompetitorRegistration(new Competitor("Kyle", "FitzGerald"), "103");
            ICompetitorRegistration rachel = new CompetitorRegistration(new Competitor("Rachel", "Shook"), "428");
            ICompetitorRegistration sam = new CompetitorRegistration(new Competitor("Sam", "Vaden"), "336");
            ICompetitorRegistration elli = new CompetitorRegistration(new Competitor("Elli", "Warner"), "330");
            ICompetitorRegistration kaiano = new CompetitorRegistration(new Competitor("Kaiano", "Levine"), "107");
            ICompetitorRegistration liz = new CompetitorRegistration(new Competitor("Liz", "Ravdin"), "312");
            ICompetitorRegistration alec = new CompetitorRegistration(new Competitor("Alec", "Grant"), "390");
            ICompetitorRegistration olivia = new CompetitorRegistration(new Competitor("Olivia", "Burnsed"), "287");
            ICompetitorRegistration david = new CompetitorRegistration(new Competitor("David", "Carrington"), "303");
            ICompetitorRegistration jesann = new CompetitorRegistration(new Competitor("Jes Ann", "Nail"), "375");

            ICompetitorRegistration roberto = new CompetitorRegistration(new Competitor("Roberto", "Corporan"), "115");
            ICompetitorRegistration glen = new CompetitorRegistration(new Competitor("Glen", "Acheampong"), "383");
            ICompetitorRegistration chris = new CompetitorRegistration(new Competitor("Chris", "Lo"), "171");
            ICompetitorRegistration simon = new CompetitorRegistration(new Competitor("Simon", "Girard"), "323");
            ICompetitorRegistration kevin = new CompetitorRegistration(new Competitor("Kevin", "Balcom"), "416");
            ICompetitorRegistration vincent = new CompetitorRegistration(new Competitor("Vincent", "Van Mierlo"), "112");
            ICompetitorRegistration alex = new CompetitorRegistration(new Competitor("Alex", "Glover"), "258");
            ICompetitorRegistration matt = new CompetitorRegistration(new Competitor("Matt", "Davis"), "121");
            ICompetitorRegistration frank = new CompetitorRegistration(new Competitor("Frank", "Moda"), "296");
            ICompetitorRegistration christopher = new CompetitorRegistration(new Competitor("Christopher", "Muise"), "368");

            ICompetitorRegistration alyx = new CompetitorRegistration(new Competitor("Alyx", "McCarthey"), "329");
            ICompetitorRegistration jacqueline = new CompetitorRegistration(new Competitor("Jacqueline", "Lo"), "168");
            ICompetitorRegistration maya = new CompetitorRegistration(new Competitor("Maya", "Tydykov"), "415");
            ICompetitorRegistration kendra = new CompetitorRegistration(new Competitor("Kendra", "Zara"), "385");
            ICompetitorRegistration isabelle = new CompetitorRegistration(new Competitor("Isabelle", "Roy"), "348");
            ICompetitorRegistration brianna = new CompetitorRegistration(new Competitor("Brianna", "Miller"), "283");


            competition.FinalCompetition = new FinalCompetition(
                dateTime: competition.Date, 
                division: competition.Division, 
                finalScores: new List<IFinalScore>());

            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, brandon, melodie, 1, 1));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, neil, kristen, 4, 2));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, lucky, dimitri, 3, 3));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, maxwell, shanna, 6, 4));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, oscar, saya, 9, 5));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, joshu, jen, 8, 6));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, edem, jia, 5, 7));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, kyle, rachel, 2, 8));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, sam, elli, 12, 9));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, kaiano, liz, 7, 10));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, alec, olivia, 10, 11));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(anne, david, jesann, 11, 12));

            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, brandon, melodie, 2, 1));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, neil, kristen, 4, 2));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, lucky, dimitri, 3, 3));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, maxwell, shanna, 1, 4));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, oscar, saya, 5, 5));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, joshu, jen, 8, 6));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, edem, jia, 12, 7));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, kyle, rachel, 6, 8));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, sam, elli, 7, 9));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, kaiano, liz, 9, 10));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, alec, olivia, 10, 11));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(arjay, david, jesann, 11, 12));

            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, brandon, melodie, 11, 1));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, neil, kristen, 1, 2));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, lucky, dimitri, 3, 3));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, maxwell, shanna, 4, 4));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, oscar, saya, 2, 5));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, joshu, jen, 6, 6));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, edem, jia, 7, 7));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, kyle, rachel, 10, 8));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, sam, elli, 5, 9));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, kaiano, liz, 9, 10));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, alec, olivia, 8, 11));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(bryn, david, jesann, 12, 12));

            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, brandon, melodie, 1, 1));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, neil, kristen, 2, 2));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, lucky, dimitri, 5, 3));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, maxwell, shanna, 3, 4));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, oscar, saya, 9, 5));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, joshu, jen, 4, 6));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, edem, jia, 6, 7));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, kyle, rachel, 7, 8));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, sam, elli, 8, 9));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, kaiano, liz, 10, 10));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, alec, olivia, 11, 11));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(john, david, jesann, 12, 12));

            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, brandon, melodie, 4, 1));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, neil, kristen, 2, 2));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, lucky, dimitri, 1, 3));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, maxwell, shanna, 10, 4));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, oscar, saya, 3, 5));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, joshu, jen, 5, 6));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, edem, jia, 7, 7));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, kyle, rachel, 12, 8));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, sam, elli, 9, 9));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, kaiano, liz, 6, 10));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, alec, olivia, 8, 11));
            competition.FinalCompetition.FinalScores.Add(new FinalScore(lemery, david, jesann, 11, 12));

            var leaderPrelimComp = new PrelimCompetition(
                dateTime: competition.Date, 
                division: competition.Division,
                round: Round.Prelims, 
                role: Role.Leader, 
                prelimScores: new List<IPrelimScore>(), 
                promotedCompetitors: new List<ICompetitor>(), 
                alternate1: maxwell.Competitor, 
                alternate2: lucky.Competitor);

            var followerPrelimComp = new PrelimCompetition(
                dateTime: competition.Date, 
                division: competition.Division, 
                round: Round.Prelims, 
                role: Role.Follower, 
                prelimScores: new List<IPrelimScore>(), 
                promotedCompetitors: new List<ICompetitor>(), 
                alternate1: alyx.Competitor, 
                alternate2: jacqueline.Competitor);

            competition.PairedPrelimCompetitions.Add(new PairedPrelimCompetition(Round.Prelims, leaderPrelimComp, followerPrelimComp));

            followerPrelimComp.PromotedCompetitors.Add(saya.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(shanna.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(olivia.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(liz.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(elli.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(jia.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(melodie.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(kristen.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(rachel.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(jen.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(jesann.Competitor);
            followerPrelimComp.PromotedCompetitors.Add(dimitri.Competitor);

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, saya, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, saya, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, saya, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, shanna, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, shanna, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, shanna, Impartial.Enums.CallbackScore.Yes));
            
            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, olivia, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, olivia, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, olivia, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, liz, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, liz, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, liz, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, elli, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, elli, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, elli, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, jia, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, jia, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, jia, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, melodie, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, melodie, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, melodie, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, kristen, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, kristen, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, kristen, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, rachel, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, rachel, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, rachel, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, jen, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, jen, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, jen, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, jesann, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, jesann, Impartial.Enums.CallbackScore.Alt1));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, jesann, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, dimitri, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, dimitri, Impartial.Enums.CallbackScore.No));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, dimitri, Impartial.Enums.CallbackScore.Yes));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, alyx, Impartial.Enums.CallbackScore.Alt1));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, alyx, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, alyx, Impartial.Enums.CallbackScore.No));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, jacqueline, Impartial.Enums.CallbackScore.Alt2));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, jacqueline, Impartial.Enums.CallbackScore.Yes));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, jacqueline, Impartial.Enums.CallbackScore.No));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, maya, Impartial.Enums.CallbackScore.Alt3));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, maya, Impartial.Enums.CallbackScore.Alt3));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, maya, Impartial.Enums.CallbackScore.Alt3));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, kendra, Impartial.Enums.CallbackScore.No));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, kendra, Impartial.Enums.CallbackScore.Alt2));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, kendra, Impartial.Enums.CallbackScore.Alt1));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, isabelle, Impartial.Enums.CallbackScore.No));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, isabelle, Impartial.Enums.CallbackScore.No));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, isabelle, Impartial.Enums.CallbackScore.Alt2));

            followerPrelimComp.PrelimScores.Add(new PrelimScore(bryn, brianna, Impartial.Enums.CallbackScore.No));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(greg, brianna, Impartial.Enums.CallbackScore.No));
            followerPrelimComp.PrelimScores.Add(new PrelimScore(victoria, brianna, Impartial.Enums.CallbackScore.No));

            leaderPrelimComp.PromotedCompetitors.Add(kyle.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(kaiano.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(roberto.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(neil.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(david.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(brandon.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(glen.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(sam.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(alec.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(edem.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(joshu.Competitor);
            leaderPrelimComp.PromotedCompetitors.Add(oscar.Competitor);

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, kyle, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, kyle, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, kyle, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, kaiano, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, kaiano, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, kaiano, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, roberto, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, roberto, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, roberto, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, neil, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, neil, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, neil, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, david, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, david, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, david, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, brandon, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, brandon, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, brandon, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, glen, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, glen, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, glen, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, sam, Impartial.Enums.CallbackScore.Alt2));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, sam, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, sam, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, alec, Impartial.Enums.CallbackScore.Alt3));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, alec, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, alec, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, edem, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, edem, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, edem, Impartial.Enums.CallbackScore.No));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, joshu, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, joshu, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, joshu, Impartial.Enums.CallbackScore.No));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, oscar, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, oscar, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, oscar, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, maxwell, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, maxwell, Impartial.Enums.CallbackScore.Alt3));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, maxwell, Impartial.Enums.CallbackScore.Alt1));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, lucky, Impartial.Enums.CallbackScore.Alt1));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, lucky, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, lucky, Impartial.Enums.CallbackScore.No));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, chris, Impartial.Enums.CallbackScore.Yes));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, chris, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, chris, Impartial.Enums.CallbackScore.No));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, simon, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, simon, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, simon, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, kevin, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, kevin, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, kevin, Impartial.Enums.CallbackScore.Yes));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, vincent, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, vincent, Impartial.Enums.CallbackScore.Alt2));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, vincent, Impartial.Enums.CallbackScore.Alt2));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, alex, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, alex, Impartial.Enums.CallbackScore.Alt1));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, alex, Impartial.Enums.CallbackScore.No));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, matt, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, matt, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, matt, Impartial.Enums.CallbackScore.Alt3));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, frank, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, frank, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, frank, Impartial.Enums.CallbackScore.No));

            leaderPrelimComp.PrelimScores.Add(new PrelimScore(arjay, christopher, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(john, christopher, Impartial.Enums.CallbackScore.No));
            leaderPrelimComp.PrelimScores.Add(new PrelimScore(lemery, christopher, Impartial.Enums.CallbackScore.No));

            _danceConventions.Add(danceConvention);

            _competitions.Add(competition);

            _judges.Add(anne);
            _judges.Add(arjay);
            _judges.Add(bryn);
            _judges.Add(john);
            _judges.Add(lemery);
            _judges.Add(victoria);
            _judges.Add(greg);

            _competitors.Add(brandon.Competitor);
            _competitors.Add(melodie.Competitor);
            _competitors.Add(neil.Competitor);
            _competitors.Add(kristen.Competitor);
            _competitors.Add(lucky.Competitor);
            _competitors.Add(dimitri.Competitor);
            _competitors.Add(maxwell.Competitor);
            _competitors.Add(shanna.Competitor);
            _competitors.Add(oscar.Competitor);
            _competitors.Add(saya.Competitor);
            _competitors.Add(joshu.Competitor);
            _competitors.Add(jen.Competitor);
            _competitors.Add(edem.Competitor);
            _competitors.Add(jia.Competitor);
            _competitors.Add(kyle.Competitor);
            _competitors.Add(rachel.Competitor);
            _competitors.Add(sam.Competitor);
            _competitors.Add(elli.Competitor);
            _competitors.Add(kaiano.Competitor);
            _competitors.Add(liz.Competitor);
            _competitors.Add(alec.Competitor);
            _competitors.Add(olivia.Competitor);
            _competitors.Add(david.Competitor);
            _competitors.Add(jesann.Competitor);
        }
    }
}
