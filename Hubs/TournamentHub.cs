using Microsoft.AspNetCore.SignalR;

namespace Esportify.Hubs
{
    public class TournamentHub : Hub
    {
        /// <summary>
        /// Adiciona a conexão do cliente ao grupo de um torneio específico.
        /// Permite receber notificações em tempo real sobre eventos desse torneio.
        /// </summary>
        /// <param name="tournamentId">ID do torneio para o qual o cliente quer receber notificações</param>
        public async Task JoinTournamentGroup(string tournamentId)
        {
            if (string.IsNullOrWhiteSpace(tournamentId))
                throw new ArgumentException("Tournament ID cannot be empty", nameof(tournamentId));

            await Groups.AddToGroupAsync(Context.ConnectionId, $"tournament-{tournamentId}");
        }

        /// <summary>
        /// Remove a conexão do cliente do grupo de um torneio específico.
        /// O cliente deixará de receber notificações sobre esse torneio.
        /// </summary>
        /// <param name="tournamentId">ID do torneio</param>
        public async Task LeaveTournamentGroup(string tournamentId)
        {
            if (string.IsNullOrWhiteSpace(tournamentId))
                throw new ArgumentException("Tournament ID cannot be empty", nameof(tournamentId));

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tournament-{tournamentId}");
        }
    }
}
