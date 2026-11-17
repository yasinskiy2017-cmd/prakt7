using MovieCatalogApp.Models;

namespace MovieCatalogApp.Controls;

public partial class MovieCardView : ContentView
{
    private const uint AnimationDuration = 420;
    private static string? _lastAnimatedMovieId;
    private static readonly object AnimationLock = new();

    public static readonly BindableProperty ModelProperty = BindableProperty.Create(
        nameof(Model), typeof(Movie), typeof(MovieCardView), default(Movie), BindingMode.OneWay);

    public Movie? Model
    {
        get => (Movie?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public MovieCardView()
    {
        InitializeComponent();
        Opacity = 0;
        TranslationY = 40;
        BindingContextChanged += OnBindingContextChanged;
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (BindingContext is not Movie movie || string.IsNullOrWhiteSpace(movie.Id))
            return;

        lock (AnimationLock)
        {
            if (_lastAnimatedMovieId == movie.Id)
                return;
            _lastAnimatedMovieId = movie.Id;
        }

        // Fire-and-forget: не блокуємо рендеринг інших елементів CollectionView.
        _ = AnimateAppearingAsync();
    }

    private async Task AnimateAppearingAsync()
    {
        try
        {
            this.CancelAnimations();
            Opacity = 0;
            TranslationY = 40;
            await Task.Delay(50);
            await Task.WhenAll(
                this.FadeTo(1, AnimationDuration, Easing.CubicOut),
                this.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка анімації картки: {ex}");
            Opacity = 1;
            TranslationY = 0;
        }
    }
}
