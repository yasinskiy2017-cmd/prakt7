using MovieCatalogApp.Models;
using MovieCatalogApp.Views;

namespace MovieCatalogApp.Services;

public class NavigationService
{
    public Task ShowMovieDetailsAsync(Movie movie)
    {
        var navigation = GetNavigation();
        return navigation.PushModalAsync(new NavigationPage(new DetailsPage(movie)));
    }

    public Task GoBackAsync()
    {
        var navigation = GetNavigation();
        return navigation.PopModalAsync();
    }

    private static INavigation GetNavigation()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page
            ?? throw new InvalidOperationException("Головну сторінку застосунку не знайдено.");
        return page.Navigation;
    }
}
