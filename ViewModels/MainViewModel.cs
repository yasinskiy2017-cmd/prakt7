using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MovieCatalogApp.Models;
using MovieCatalogApp.Services;

namespace MovieCatalogApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private const int VisibleBatchSize = 10;
    private readonly MovieService _movieService;
    private readonly NavigationService _navigationService;
    private readonly List<Movie> _titlesLoaded = new();
    private bool _isLoading;
    private Movie? _selectedMovie;
    private bool _hasMoreFromApi = true;

    // CollectionView відображає лише обмежену кількість карток одночасно.
    public ObservableCollection<Movie> TitlesVisible { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set { if (_isLoading == value) return; _isLoading = value; OnPropertyChanged(); }
    }

    public Movie? SelectedMovie
    {
        get => _selectedMovie;
        set { if (_selectedMovie == value) return; _selectedMovie = value; OnPropertyChanged(); }
    }

    public ICommand LoadMoreCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand OpenMovieCommand { get; }

    public MainViewModel(MovieService movieService, NavigationService navigationService)
    {
        _movieService = movieService;
        _navigationService = navigationService;
        LoadMoreCommand = new Command(async () => await LoadMoreAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());
        OpenMovieCommand = new Command<Movie>(async movie => await OpenMovieAsync(movie));
        _ = RefreshAsync();
    }

    private async Task OpenMovieAsync(Movie? movie)
    {
        if (movie is null) return;
        try { await _navigationService.ShowMovieDetailsAsync(movie); }
        finally { SelectedMovie = null; }
    }

    private async Task RefreshAsync()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            _titlesLoaded.Clear();
            TitlesVisible.Clear();
            _hasMoreFromApi = true;
            var batch = await _movieService.GetMoviesAsync(true);
            _titlesLoaded.AddRange(batch);
            _hasMoreFromApi = batch.Count > 0 && _movieService.HasMorePages;
            AddNextVisibleBatch();
        }
        finally { IsLoading = false; }
    }

    private async Task LoadMoreAsync()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            if (TitlesVisible.Count < _titlesLoaded.Count)
            {
                AddNextVisibleBatch();
                return;
            }

            if (!_hasMoreFromApi) return;
            var batch = await _movieService.GetMoviesAsync(false);
            if (batch.Count == 0) { _hasMoreFromApi = false; return; }
            _titlesLoaded.AddRange(batch);
            _hasMoreFromApi = _movieService.HasMorePages;
            AddNextVisibleBatch();
        }
        finally { IsLoading = false; }
    }

    private void AddNextVisibleBatch()
    {
        var count = Math.Min(VisibleBatchSize, _titlesLoaded.Count - TitlesVisible.Count);
        for (var i = 0; i < count; i++)
            TitlesVisible.Add(_titlesLoaded[TitlesVisible.Count]);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
