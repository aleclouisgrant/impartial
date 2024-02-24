﻿using Impartial;
using System;
using System.Linq;

namespace ImpartialUI.Models
{
    public class FinalScore : IFinalScore
    {
        private Guid _judgeId;
        private Guid _leaderId;
        private Guid _followerId;

        public Guid Id { get; set; }
        public IFinalCompetition FinalCompetition { get; set; }
        public IJudge Judge { get; set; }
        public ICompetitor Leader { get; set; }
        public ICompetitor Follower { get; set; }

        public int Score { get; set; }
        public int Placement { get; set; }

        public double Accuracy => Util.GetAccuracy(Score, Placement);

        public FinalScore(IFinalCompetition finalCompetition, Guid judgeId, Guid leaderId, Guid followerId, int score, int placement, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            FinalCompetition = finalCompetition;
            Score = score; 
            Placement = placement;

            SetJudge(judgeId);
            SetLeader(leaderId);
            SetFollower(followerId);
        }

        public void SetJudge(Guid id)
        {
            _judgeId = id;
            Judge = App.JudgesDb.Where(j => j.Id == _judgeId).FirstOrDefault();
        }

        public void SetLeader(Guid id)
        {
            _leaderId = id;
            Leader = App.CompetitorsDb.Where(j => j.Id == _leaderId).FirstOrDefault();
        }

        public void SetFollower(Guid id)
        {
            _followerId = id;
            Follower = App.CompetitorsDb.Where(j => j.Id == _followerId).FirstOrDefault();
        }
    }
}
