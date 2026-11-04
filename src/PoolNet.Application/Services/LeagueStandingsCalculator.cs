using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;

namespace PoolNet.Application.Services;

public class LeagueStandingsCalculator : ILeagueStandingsCalculator
{
    public List<LeagueStandingDto> CalculateStandings(
        IEnumerable<LeagueParticipant> participants,
        IEnumerable<Match> matches)
    {
        var completedMatches = matches
            .Where(m => m != null && m.Status == MatchStatus.Completed)
            .ToList();

        var standings = new List<LeagueStandingDto>();

        foreach (var participant in participants)
        {
            int pId = participant.PlayerId;
            string nickname = participant.Player?.Nickname ?? $"Gracz #{pId}";

            var playerMatches = completedMatches
                .Where(m => m.Player1Id == pId || m.Player2Id == pId)
                .ToList();

            int wins = playerMatches.Count(m => m.WinnerId == pId);
            int losses = playerMatches.Count - wins;
            int framesWon = playerMatches.Sum(m => m.Player1Id == pId ? m.Player1Score : m.Player2Score);
            int framesLost = playerMatches.Sum(m => m.Player1Id == pId ? m.Player2Score : m.Player1Score);
            int points = wins * 3;

            standings.Add(new LeagueStandingDto
            {
                PlayerId = pId,
                Nickname = nickname,
                MatchesPlayed = playerMatches.Count,
                Wins = wins,
                Losses = losses,
                FramesWon = framesWon,
                FramesLost = framesLost,
                Points = points
            });
        }

        // Sorting: Points DESC, Frame Difference DESC, Frames Won DESC, Nickname ASC
        var ordered = standings
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.FrameDifference)
            .ThenByDescending(s => s.FramesWon)
            .ThenBy(s => s.Nickname)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].Position = i + 1;
        }

        return ordered;
    }
}

