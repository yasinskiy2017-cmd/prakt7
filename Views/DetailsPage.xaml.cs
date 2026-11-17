using MovieCatalogApp.Models;

namespace MovieCatalogApp.Views;

public partial class DetailsPage : ContentPage
{
    public DetailsPage(Movie movie)
    {
        InitializeComponent();
        BindingContext = movie;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        var rootNavigation = Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;
        if (rootNavigation is not null && rootNavigation.ModalStack.Count > 0)
            await rootNavigation.PopModalAsync();
    }
}
