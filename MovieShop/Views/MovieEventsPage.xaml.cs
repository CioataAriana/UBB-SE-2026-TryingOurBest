using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Views;

public sealed partial class MovieEventsPage : Page
{
    private Movie? _movie;

    public MovieEventsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MovieEventsNavArgs args)
            return;

        _movie = args.Movie;
        TitleBlock.Text = _movie == null ? "Events" : $"Events - {_movie.Title}";

        if (_movie == null)
            return;

        var events = new EventRepo().GetEventsForMovie(_movie.ID);
        EventsList.ItemsSource = events;
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
            Frame.GoBack();
    }
}

