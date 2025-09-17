namespace Esportify.Models.ViewModels
{
    public class GamesViewModel
    {
        public string Name { get; set; }
        public string Genre { get; set; }
        public string OfficialWebsite { get; set; }
        public IFormFile Image { get; set; }
    }
}
