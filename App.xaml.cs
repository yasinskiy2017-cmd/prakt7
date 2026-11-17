using MovieCatalogApp.Views;

namespace MovieCatalogApp
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {
            InitializeComponent();
            MainPage = mainPage;
        }
    }
}
