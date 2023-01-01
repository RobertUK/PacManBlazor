﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Canvas2D;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pacman.Shared.Models.Configuration;
using PacMan.GameComponents.Audio;
using PacMan.GameComponents.Canvas;
using PacMan.GameComponents.Events;
using PacMan.GameComponents.GameActs;
using PacMan.GameComponents.Ghosts;
using PacMan.GameComponents.Requests;

namespace PacMan.GameComponents
{
    public class Game : IGame
    {
        // ReSharper disable once UnusedMember.Local
        readonly DiagPanel _diagPanel = new();

        readonly IGameSoundPlayer _gameSoundPlayer;
        readonly IHumanInterfaceParser _input;
        readonly IPacMan _pacman;

        readonly IScorePanel _scorePanel;
        readonly IMediator _mediator;
        readonly IFruit _fruit;
        readonly IStatusPanel _statusPanel;

        CanvasTimingInformation? _canvasTimingInformation;

        IAct? _currentAct;
        TimedSpriteList? _tempSprites;
        EggTimer _pauser = EggTimer.Unset;

        CanvasWrapper? _scoreCanvas;
        CanvasWrapper? _mazeCanvas;
        CanvasWrapper? _statusCanvas;

        // ReSharper disable once NotAccessedField.Local
        CanvasWrapper? _diagCanvas;

        static bool _initialised;

        

        public Game(
            IMediator mediator,
            IFruit fruit,
            IStatusPanel statusPanel,
            IScorePanel scorePanel,
            IGameSoundPlayer gameSoundPlayer,
            IHumanInterfaceParser input,
            IPacMan pacman)
        {
            _mediator = mediator;
            _fruit = fruit;
            _statusPanel = statusPanel;
            _scorePanel = scorePanel;
            _gameSoundPlayer = gameSoundPlayer;
            _input = input;
            _pacman = pacman;
            this.Characters = new List<Character>();
        }

        [SuppressMessage("ReSharper", "HeapView.ObjectAllocation.Evident")]
        public async ValueTask Initialise(IJSRuntime jsRuntime, List<Character> characters)
        {
            _canvasTimingInformation = new();

            _tempSprites = new();

            _pauser = new(0.Milliseconds(), () => { });

            // POINTER: You can change the starting Act by using something like:
            // _currentAct = new TornGhostChaseAct(new AttractAct());

            // ReSharper disable once HeapView.BoxingAllocation

            //Characters = characters;
            this.Characters = characters;
            _currentAct = await _mediator.Send(new GetActRequest("AttractAct"));

            await _gameSoundPlayer.LoadAll(jsRuntime);

            _initialised = true;
        }

        async ValueTask update()
        {
            ensureInitialised();
            _input.Update(_canvasTimingInformation!);

            _scorePanel.Update(_canvasTimingInformation!);
            _statusPanel.Update(_canvasTimingInformation!);

            _tempSprites!.Update(_canvasTimingInformation!);

            _pauser.Run(_canvasTimingInformation!);

            await checkCheatKeys();

            if (_pauser.Finished)
            {
                await _currentAct!.Update(_canvasTimingInformation!);
            }
        }

        async ValueTask checkCheatKeys()
        {
            if (Cheats.AllowDebugKeys && _input.IsKeyCurrentlyDown(Keys.Three))
            {
                // ReSharper disable once HeapView.BoxingAllocation
                _currentAct = await _mediator.Send(new GetActRequest("LevelFinishedAct"));
            }

            if (Cheats.AllowDebugKeys && _input.IsKeyCurrentlyDown(Keys.Four))
            {
                await _mediator.Publish(new PacManEatenEvent());
            }

            if (Cheats.AllowDebugKeys && _input.WasKeyPressedAndReleased(Keys.Six))
            {
                await _mediator.Publish(new AllPillsEatenEvent());
            }
        }

        async ValueTask draw()
        {
            ensureInitialised();

            var dim = Constants.UnscaledCanvasSize;

            if (_underlyingCanvasContext == null)
            {
                // ReSharper disable once HeapView.ObjectAllocation.Evident
                throw new InvalidOperationException($"{nameof(SetCanvasContextForOutput)} has not been called!");
            }

            await _underlyingCanvasContext.BeginBatchAsync();

            await _mazeCanvas!.Clear((int) dim.X, (int) dim.Y);

            await _scorePanel.Draw(_scoreCanvas!);

            await _statusPanel.Draw(_statusCanvas!);
            await _currentAct!.Draw(_mazeCanvas);
            await _tempSprites!.Draw(_mazeCanvas);

            if (DiagInfo.ShouldShow)
            {
                await _diagPanel.Draw(_diagCanvas!);
            }

            await _underlyingCanvasContext.EndBatchAsync();
        }

        static void ensureInitialised()
        {
            if (!_initialised)
            {
                throw new InvalidOperationException("Not initialised!");
            }
        }

        public ValueTask FruitEaten(int points)
        {
            ensureInitialised();
            _tempSprites!.Add(new(3000, new ScoreSprite(_fruit.Position, points)));

            return default;
        }

        public ValueTask GhostEaten(IGhost ghost, int points)
        {
            ensureInitialised();
            _tempSprites!.Add(new(900, new ScoreSprite(_pacman.Position, points)));

            ghost.Visible = false;
            _pacman.Visible = false;

            _pauser = new(1000.Milliseconds(), () => {
                ghost.Visible = true;
                _pacman.Visible = true;
            });

            return default;
        }

        public void SetCanvasContextForOutput(Canvas2DContext context)
        {
            _underlyingCanvasContext = context ?? throw new ArgumentNullException(nameof(context));

            _scoreCanvas = new(context, new(0, 0));
            _mazeCanvas = new(context, new(0, 26));
            _diagCanvas = new(context, new(0, 220));
            _statusCanvas = new(context, new(0, 274));
        }

        public void SetCanvasesForPlayerMazes(Canvas2DContext player1MazeCanvas, Canvas2DContext player2MazeCanvas)
        {
            _ = player1MazeCanvas ?? throw new InvalidOperationException("null canvas!");
            _ = player2MazeCanvas ?? throw new InvalidOperationException("null canvas!");

            MazeCanvases.Populate(new(player1MazeCanvas), new(player2MazeCanvas));
        }

        static readonly Stopwatch _stopWatch = new();

        static float getTimestep() => 1000 / (float) Constants.FramesPerSecond;

        static float _delta;

        float _lastTimestamp;
        Canvas2DContext? _underlyingCanvasContext;

        static int _frameCount;
        bool _postRenderInitialised;

   

        public List<Character> Characters { get; set; }
  

        public async ValueTask RunGameLoop(float timestamp)
        {
            _stopWatch.Restart();

            if (!_initialised || !_postRenderInitialised)
            {
                return;
            }

            _delta += timestamp - _lastTimestamp;
            _lastTimestamp = timestamp;

            // timestep would normally be fixed (at 60FPS), but we have
            // the ability to slow down and speed up the game (via the A and S keys)
            var timestep = getTimestep();

            while (_delta >= timestep)
            {
                Pnrg.Update();
                _canvasTimingInformation!.Update(timestep);

                DiagInfo.IncrementUpdateCount();
                await DiagInfo.Update(_canvasTimingInformation, _input);
                await update();

                _delta -= timestep;
                if (Debugger.IsAttached)
                {
                    _delta = timestep;
                }
            }

            await draw();

            DiagInfo.IncrementDrawCount(timestamp);

            ++_frameCount;

            var fps = (int) (_canvasTimingInformation!.TotalTime.TotalMilliseconds / _frameCount);
            DiagInfo.Fps = fps;

            DiagInfo.UpdateTimeLoopTaken(_stopWatch.ElapsedMilliseconds);
        }

        public void PostRenderInitialize(
            Canvas2DContext outputCanvasContext,
            Canvas2DContext player1MazeCanvas,
            Canvas2DContext player2MazeCanvas,
            in ElementReference spritesheetReference)
        {
            SetCanvasContextForOutput(outputCanvasContext);
            SetCanvasesForPlayerMazes(player1MazeCanvas, player2MazeCanvas);

            Spritesheet.SetReference(spritesheetReference);

            _postRenderInitialised = true;
        }

        public void SetAct(IAct act) => _currentAct = act ?? throw new ArgumentNullException(nameof(act));
    }
}