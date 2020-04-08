using Microsoft.EntityFrameworkCore;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.Entities;
using PremierLeague.Core.DataTransferObjects;
using System.Collections.Generic;
using System.Linq;
using System;


namespace PremierLeague.Persistence
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly int rank = 1;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<Team> GetAllWithGames()
        {
            return _dbContext.Teams.Include(t => t.HomeGames).Include(t => t.AwayGames).ToList();
        }

        public IEnumerable<Team> GetAll()
        {
            return _dbContext.Teams.OrderBy(t => t.Name).ToList();
        }

        public void AddRange(IEnumerable<Team> teams)
        {
            _dbContext.Teams.AddRange(teams);
        }

        public Team Get(int teamId)
        {
            return _dbContext.Teams.Find(teamId);
        }

        public void Add(Team team)
        {
            _dbContext.Teams.Add(team);
        }

        public (Team Team, int Goals) GetTeamWithMostGoals() =>
            _dbContext.Teams.Select(t => new
            {
                Team = t,
                Goals = t.AwayGames.Sum(g => g.GuestGoals) + t.HomeGames.Sum(g => g.HomeGoals)
            }).AsEnumerable().Select(t => (t.Team, t.Goals)).OrderByDescending(t => t.Goals).ToArray().First();


        public (Team Team, int Goals) GetTeamWithMostAwayGoals() =>
            _dbContext.Teams.Select(t => new
                    {  Team = t,
                    Goals = t.AwayGames.Sum(g => g.GuestGoals)
                })
               .AsEnumerable().Select(t => (t.Team, t.Goals)).OrderByDescending(t => t.Goals).ToArray().First();

 
        public (Team Team, int Goals) GetTeamWithMostHomeGoals() =>
            _dbContext.Teams.Select(t => new
            {
                Team = t,
                Goals = t.HomeGames.Sum(g=> g.HomeGoals)
            })
            .AsEnumerable().Select(t => (t.Team, t.Goals)).OrderByDescending(t => t.Goals).ToArray().First();

        public (Team Team, int GoalRatio) GetTeamWithBestGoalRatio() =>
            _dbContext.Teams.Select(t =>new 
            {
                Team = t,
                           GoalRatio = t.HomeGames.Sum(g => g.HomeGoals) - t.HomeGames.Sum(g => g.GuestGoals)
                                     + t.AwayGames.Sum(g => g.GuestGoals) - t.AwayGames.Sum(g => g.HomeGoals)

            })
            .AsEnumerable()
            .Select(t => (t.Team, t.GoalRatio))
            .OrderByDescending(t => t.GoalRatio)
            .ToArray()
            .First();

        public TeamStatisticDto[] GetTeamStatistics() =>
                _dbContext.Teams
                .Select(t => new TeamStatisticDto
                {
                    Name = t.Name,
                    AvgGoalsShotAtHome = t.HomeGames.Average(g => g.HomeGoals),
                    AvgGoalsShotOutwards = t.AwayGames.Average(g => g.GuestGoals),
                    AvgGoalsShotInTotal = t.HomeGames.Select(g => new { GoalsShot = g.HomeGoals })
                                          .Concat(t.AwayGames.Select(g => new { GoalsShot = g.GuestGoals }))
                                          .Average(g => g.GoalsShot),
                    AvgGoalsGotAtHome = t.HomeGames.Average(g => g.GuestGoals),
                    AvgGoalsGotOutwards = t.AwayGames.Average(g => g.HomeGoals),
                    AvgGoalsGotInTotal = t.HomeGames.Select(g => new { GoalsGot = g.GuestGoals })
                                          .Concat(t.AwayGames.Select(g => new { GoalsGot = g.HomeGoals }))
                                          .Average(g => g.GoalsGot)
                })
            .OrderByDescending(t => t.AvgGoalsShotInTotal)
            .ToArray();
        

        public TeamTableRowDto[] GetTeamTableRow()
        {
            var teams = _dbContext.Teams
                .Select(t => new TeamTableRowDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Matches = t.AwayGames.Count() + t.HomeGames.Count(),
                    Won = t.HomeGames.Where(match => match.HomeGoals > match.GuestGoals).Count() + t.AwayGames.Where(match => match.HomeGoals < match.GuestGoals).Count(),
                    Lost = t.HomeGames.Where(match => match.HomeGoals < match.GuestGoals).Count() + t.AwayGames.Where(match => match.HomeGoals > match.GuestGoals).Count(),
                    GoalsFor = t.HomeGames.Sum(match => match.HomeGoals) + t.AwayGames.Sum(match => match.GuestGoals),
                    GoalsAgainst = t.HomeGames.Sum(match => match.GuestGoals) + t.AwayGames.Sum(match => match.HomeGoals)
                })
                .AsEnumerable()
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalDifference)
                .ToArray();
            

            foreach (var team in teams )
            {
                team.Rank = Array.IndexOf(teams, team ) + rank;
            }
            return teams;
        }

    }
}