using PoolNet.Application.DTOs;
using PoolNet.Domain.Entities;

namespace PoolNet.Application.Interfaces;

public interface ILeagueStandingsCalculator
{
    List<LeagueStandingDto> CalculateStandings(
        IEnumerable<LeagueParticipant> participants,
        IEnumerable<Match> matches);
}

