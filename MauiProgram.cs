using Microsoft.Extensions.Logging;
using MovieCatalogApp.Services;
using MovieCatalogApp.ViewModels;
using MovieCatalogApp.Views;

namespace MovieCatalogApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<MovieService>();
            builder.Services.AddSingleton<NavigationService>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
