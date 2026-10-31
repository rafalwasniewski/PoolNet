using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;

namespace PoolNet.Application.Services;

public class PlayerStatisticsCalculator : IPlayerStatisticsCalculator
{
    public OverallStatisticsDto CalculateOverall(int playerId, IEnumerable<Match> matches)
    {
        var completedMatches = matches.Where(m => m.Status == MatchStatus.Completed && (m.Player1Id == playerId || m.Player2Id == playerId)).ToList();

        int wins = 0;
        int losses = 0;
        int framesWon = 0;
        int framesLost = 0;

        foreach (var match in completedMatches)
        {
            bool isPlayer1 = match.Player1Id == playerId;
            int playerFrames = isPlayer1 ? match.Player1Score : match.Player2Score;
            int opponentFrames = isPlayer1 ? match.Player2Score : match.Player1Score;

            framesWon += playerFrames;
            framesLost += opponentFrames;

            if (match.WinnerId == playerId)
            {
                wins++;
            }
            else
            {
                losses++;
            }
        }

        int total = wins + losses;
        double winRate = total > 0 ? Math.Round((double)wins / total * 100, 1) : 0;

        return new OverallStatisticsDto
        {
            TotalMatches = total,
            Wins = wins,
            Losses = losses,
            WinRate = winRate,
            FramesWon = framesWon,
            FramesLost = framesLost
        };
    }

    public List<GameTypeStatisticsDto> CalculatePerGameType(int playerId, IEnumerable<Match> matches)
    {
        var completedMatches = matches.Where(m => m.Status == MatchStatus.Completed && (m.Player1Id == playerId || m.Player2Id == playerId)).ToList();

        var result = new List<GameTypeStatisticsDto>();
        var gameTypes = new[] { BilliardGameType.EightBall, BilliardGameType.NineBall, BilliardGameType.TenBall };

        foreach (var gameType in gameTypes)
        {
            var typeMatches = completedMatches.Where(m => m.GameType == gameType).ToList();
            int wins = 0;
            int losses = 0;
            int framesWon = 0;
            int framesLost = 0;

            foreach (var match in typeMatches)
            {
                bool isPlayer1 = match.Player1Id == playerId;
                int playerFrames = isPlayer1 ? match.Player1Score : match.Player2Score;
                int opponentFrames = isPlayer1 ? match.Player2Score : match.Player1Score;

                framesWon += playerFrames;
                framesLost += opponentFrames;

                if (match.WinnerId == playerId)
                {
                    wins++;
                }
                else
                {
                    losses++;
                }
            }

            int total = wins + losses;

            result.Add(new GameTypeStatisticsDto
            {
                GameType = gameType,
                TotalMatches = total,
                Wins = wins,
                Losses = losses,
                FramesWon = framesWon,
                FramesLost = framesLost
            });
        }

        return result;
    }
}
