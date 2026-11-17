using MovieCatalogApp.ViewModels;

namespace MovieCatalogApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0 || MoviesGridLayout is null)
            return;

        // У горизонтальній орієнтації використовуємо дві колонки,
        // у вертикальній — одну.
        MoviesGridLayout.Span = width > height ? 2 : 1;
    }
}
