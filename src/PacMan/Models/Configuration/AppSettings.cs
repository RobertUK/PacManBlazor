using Pacman.Models.Configuration;
using PacMan.GameComponents.Ghosts;

namespace Pacman.Models.Configuration
{
    public class AppSettings
    {
        public int CanvasHeight { get; set; }

        public int CanvasWidth { get; set; }

        public List<Character> Characters { get; set; }

        public int FontSize { get; set; }


    }




}