using System.Collections.Generic;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PacMan.GameComponents.GameActs;
using PacMan.GameComponents.Ghosts;
using Pacman.Shared.Models.Configuration;

namespace PacMan.GameComponents
{
    public interface IGame
    {
        ValueTask Initialise(IJSRuntime jsRuntime, List<Character> characters);

        ValueTask FruitEaten(int points);

        public List<Character> Characters { get; set; }

        ValueTask RunGameLoop(float timestamp);

        void PostRenderInitialize(Canvas2DContext outputCanvasContext, Canvas2DContext player1MazeCanvas, Canvas2DContext player2MazeCanvas, in ElementReference spritesheetReference);

        void SetAct(IAct act);

        ValueTask GhostEaten(IGhost ghost, int points);
    }
}