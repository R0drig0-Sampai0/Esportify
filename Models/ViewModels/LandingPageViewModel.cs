namespace Esportify.Models
{
    public class LandingPageViewModel
    {
        public List<Game> FeaturedGames { get; set; }
        public List<PlayerViewModel> TopPlayers { get; set; }
        public int PlayerCount { get; set; }
        public int TournamentCount { get; set; }
    }

    public class PlayerViewModel
    {
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public decimal Earnings { get; set; }
    }
}