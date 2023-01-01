// ReSharper disable HeapView.ObjectAllocation.Evident

using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PacMan.GameComponents;
using PacMan.GameComponents.Audio;
using PacMan.GameComponents.GameActs;
using PacMan.GameComponents.Ghosts;
using PacMan.GameComponents.Requests;
using System.Configuration;
using Pacman.Shared.Models.Configuration;
using System.Net.Http.Json;

namespace PacMan
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebAssemblyHostBuilder.CreateDefault(args);

           // builder.Configuration.AddJsonFile("/appsettings.json");


            builder.RootComponents.Add<App>("app");
            IServiceCollection services = builder.Services;

            //BlogifierConfiguration exchangeOptfions = new BlogifierConfiguration(); ;

            //builder.Configuration.GetSection("Blogifier").Bind(exchangeOptfions);

            //AppConfig exchangeOptfions2 = new AppConfig();
            //builder.Configuration.GetSection("Pacman").Bind(exchangeOptfions);

            //AppConfig exchangeOptfions211 = new AppConfig();
            //builder.Configuration.GetSection("AppConfig").Bind(exchangeOptfions211);

            services.Configure<AppConfig>(options => builder.Configuration.GetSection("AppConfig").Bind(options));

            services.AddSingleton<IGame, Game>();
            services.AddSingleton<IGameStorage, GameStorage>();
            services.AddSingleton<IHumanInterfaceParser, HumanInterfaceParser>();
            services.AddSingleton<ISoundLoader, SoundLoader>();
            services.AddSingleton<IGameSoundPlayer, GameSoundPlayer>();

            services.AddSingleton<IAct, AttractAct>();
            services.AddSingleton<IAct, GameAct>();

            services.AddSingleton<IAct, BigPacChaseAct>();
            services.AddSingleton<IAct, GameOverAct>();
            services.AddSingleton<IAct, GhostTearAct>();
            services.AddSingleton<IAct, LevelFinishedAct>();
            services.AddSingleton<IAct, NullAct>();
            services.AddSingleton<IAct, PacManDyingAct>();
            services.AddSingleton<IAct, PlayerIntroAct>();
            services.AddSingleton<IAct, DemoPlayerIntroAct>();
            services.AddSingleton<IAct, StartButtonAct>();
            services.AddSingleton<IAct, TornGhostChaseAct>();

            services.AddSingleton<IAct, PlayerGameOverAct>();

            services.AddSingleton<IActs, Acts>();

            services.AddSingleton<IGameStats, GameStats>();

            services.AddSingleton<IHaveTheMazeCanvases, MazeCanvases>();

            services.AddSingleton<IGhost, Blinky>();
            services.AddSingleton<IGhost, Pinky>();
            services.AddSingleton<IGhost, Inky>();
            services.AddSingleton<IGhost, Clyde>();

            services.AddSingleton<IPacMan, PacMan.GameComponents.PacMan>();

            services.AddSingleton<ICoinBox, CoinBox>();

            services.AddSingleton<IFruit, Fruit>();

            services.AddSingleton<IGhostCollection, GhostCollection>();

            services.AddSingleton<IStatusPanel, StatusPanel>();
            services.AddSingleton<IScorePanel, ScorePanel>();

            services.AddSingleton<IMaze, Maze>();

            services.AddLogging();

            services.Add(
                new(
                    typeof(IExceptionNotificationService),
                    typeof(ExceptionNotificationService),
                    ServiceLifetime.Singleton));

            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            Assembly componentsAssembly =
                typeof(ClassThatLivesInGameComponentsActsAsAMarkerForThisAssemblyForReflection).Assembly;

            services.AddMediatR(
                c =>
                    c.AsSingleton(),
                thisAssembly,
                componentsAssembly);

            //builder.Services.AddHttpClient<IPacMan, EmployeeService>(client =>
            //{
            //    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            //});

            services.AddSingleton(new HttpClient {BaseAddress = new(builder.HostEnvironment.BaseAddress)});

            var host = builder.Build();

            await host.RunAsync();

        }
    }
}