namespace MovieCatalogApp.Models;

public class Movie
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public Uri Poster { get; set; } = new("https://placehold.co/300x440?text=No+Poster");
    public string Description { get; set; } = string.Empty;
    public string Rating { get; set; } = string.Empty;
}
