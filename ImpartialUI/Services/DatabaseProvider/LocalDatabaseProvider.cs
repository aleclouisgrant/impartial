using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Impartial;
using ImpartialUI.Models;
using Org.BouncyCastle.Math.EC.Endo;

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
            _competitors.RemoveWhere(c => c.Id == id);
            return Task.CompletedTask;
        }

        public Task DeleteDanceConventionAsync(Guid id)
        {
            _danceConventions.RemoveWhere(c => c.Id == id);
            return Task.CompletedTask;
        }

        public Task DeleteJudgeAsync(Guid id)
        {
            _judges.RemoveWhere(c => c.Id == id);
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
            tcs.SetResult(_competitors.Where(c => c.Id == id).FirstOrDefault());

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
            tcs.SetResult(_judges.Where(c => c.Id == id).FirstOrDefault());

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

            ICompetitor brandon = new Competitor("Brandon", "Rasmussen");
            ICompetitor melodie = new Competitor("Melodie", "Paletta");
            ICompetitor neil = new Competitor("Neil", "Joshi");
            ICompetitor kristen = new Competitor("Kristen", "Wallace");
            ICompetitor lucky = new Competitor("Lucky", "Sipin");
            ICompetitor dimitri = new Competitor("Dimitri", "Hector");
            ICompetitor maxwell = new Competitor("Maxwell", "Thew");
            ICompetitor shanna = new Competitor("Shanna", "Porcari");
            ICompetitor oscar = new Competitor("Oscar", "Hampton");
            ICompetitor saya = new Competitor("Sayaka", "Suzaki");
            ICompetitor joshu = new Competitor("Joshu", "Creel");
            ICompetitor jen = new Competitor("Jen", "Ferreira");
            ICompetitor edem = new Competitor("Edem", "Attikese");
            ICompetitor jia = new Competitor("Jia", "Lu");
            ICompetitor kyle = new Competitor("Kyle", "FitzGerald");
            ICompetitor rachel = new Competitor("Rachel", "Shook");
            ICompetitor sam = new Competitor("Sam", "Vaden");
            ICompetitor elli = new Competitor("Elli", "Warner");
            ICompetitor kaiano = new Competitor("Kaiano", "Levine");
            ICompetitor liz = new Competitor("Liz", "Ravdin");
            ICompetitor alec = new Competitor("Alec", "Grant");
            ICompetitor olivia = new Competitor("Olivia", "Burnsed");
            ICompetitor david = new Competitor("David", "Carrington");
            ICompetitor jesann = new Competitor("Jes Ann", "Nail");

            ICompetitor roberto = new Competitor("Roberto", "Corporan");
            ICompetitor glen = new Competitor("Glen", "Acheampong");
            ICompetitor chris = new Competitor("Chris", "Lo");
            ICompetitor simon = new Competitor("Simon", "Girard");
            ICompetitor kevin = new Competitor("Kevin", "Balcom");
            ICompetitor vincent = new Competitor("Vincent", "Van Mierlo");
            ICompetitor alex = new Competitor("Alex", "Glover");
            ICompetitor matt = new Competitor("Matt", "Davis");
            ICompetitor frank = new Competitor("Frank", "Moda");
            ICompetitor christopher = new Competitor("Christopher", "Muise");

            ICompetitor alyx = new Competitor("Alyx", "McCarthey");
            ICompetitor jacqueline = new Competitor("Jacqueline", "Lo");
            ICompetitor maya = new Competitor("Maya", "Tydykov");
            ICompetitor kendra = new Competitor("Kendra", "Zara");
            ICompetitor isabelle = new Competitor("Isabelle", "Roy");
            ICompetitor brianna = new Competitor("Brianna", "Miller");

            competition.FinalCompetition = new FinalCompetition(
                dateTime: competition.Date, 
                division: competition.Division, 
                finalScores: new());

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

            var leaderPrelimComp = new PrelimCompetition(dateTime: competition.Date, division: competition.Division, round: Round.Prelims, role: Role.Leader, prelimScores: new List<IPrelimScore>(), promotedCompetitors: new List<ICompetitor>());
            var followerPrelimComp = new PrelimCompetition(dateTime: competition.Date, division: competition.Division, round: Round.Prelims, role: Role.Follower, prelimScores: new List<IPrelimScore>(), promotedCompetitors: new List<ICompetitor>());
            competition.PairedPrelimCompetitions.Add(new PairedPrelimCompetition(Round.Prelims, leaderPrelimComp, followerPrelimComp));

            followerPrelimComp.PromotedCompetitors.Add(saya);
            followerPrelimComp.PromotedCompetitors.Add(shanna);
            followerPrelimComp.PromotedCompetitors.Add(olivia);
            followerPrelimComp.PromotedCompetitors.Add(liz);
            followerPrelimComp.PromotedCompetitors.Add(elli);
            followerPrelimComp.PromotedCompetitors.Add(jia);
            followerPrelimComp.PromotedCompetitors.Add(melodie);
            followerPrelimComp.PromotedCompetitors.Add(kristen);
            followerPrelimComp.PromotedCompetitors.Add(rachel);
            followerPrelimComp.PromotedCompetitors.Add(jen);
            followerPrelimComp.PromotedCompetitors.Add(jesann);
            followerPrelimComp.PromotedCompetitors.Add(dimitri);

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

            leaderPrelimComp.PromotedCompetitors.Add(kyle);
            leaderPrelimComp.PromotedCompetitors.Add(kaiano);
            leaderPrelimComp.PromotedCompetitors.Add(roberto);
            leaderPrelimComp.PromotedCompetitors.Add(neil);
            leaderPrelimComp.PromotedCompetitors.Add(david);
            leaderPrelimComp.PromotedCompetitors.Add(brandon);
            leaderPrelimComp.PromotedCompetitors.Add(glen);
            leaderPrelimComp.PromotedCompetitors.Add(sam);
            leaderPrelimComp.PromotedCompetitors.Add(alec);
            leaderPrelimComp.PromotedCompetitors.Add(edem);
            leaderPrelimComp.PromotedCompetitors.Add(joshu);
            leaderPrelimComp.PromotedCompetitors.Add(oscar);

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

            _competitors.Add(brandon);
            _competitors.Add(melodie);
            _competitors.Add(neil);
            _competitors.Add(kristen);
            _competitors.Add(lucky);
            _competitors.Add(dimitri);
            _competitors.Add(maxwell);
            _competitors.Add(shanna);
            _competitors.Add(oscar);
            _competitors.Add(saya);
            _competitors.Add(joshu);
            _competitors.Add(jen);
            _competitors.Add(edem);
            _competitors.Add(jia);
            _competitors.Add(kyle);
            _competitors.Add(rachel);
            _competitors.Add(sam);
            _competitors.Add(elli);
            _competitors.Add(kaiano);
            _competitors.Add(liz);
            _competitors.Add(alec);
            _competitors.Add(olivia);
            _competitors.Add(david);
            _competitors.Add(jesann);
        }
    }
}
