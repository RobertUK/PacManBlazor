

namespace Pacman.Shared.Models.Configuration
{
    public class AppConfig
    {

        public int CanvasHeight { get; set; }
        public int CanvasWidth { get; set; }
        public List<Character> Characters { get; set; }

        public int FontSize { get; set; }
        public string Font { get; set; }


    }

    public class BlogifierConfiguration
    {

        public string PathBase { get; set; }
        public string DbProvider { get; set; }
        public string ConnString { get; set; }
        public string Salt { get; set; }
        public int DemoMode { get; set; }
        public string FileExtensions { get; set; }

        public bool DBDebug { get; set; }

    }


    public class PacmanConfiguration43534
    {

       

    }




}