using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Nawigacja
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            MapService.ServiceToken = "qEhmC4LZobGPcNmZeE0m~A3JPEthh4rhqcVWh6p4b1g~AkUzZM9oOcDGZeJdwqD_7FRGaRqap3wfGoQpycOh-0YtYaImGjQ9jCYgG7KYZv6j";

        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (DaneGeograficzne.pktStartowy.Longitude != default(double) && DaneGeograficzne.pktStartowy.Latitude != default(double) &&
                DaneGeograficzne.pktDocelowy.Longitude != default(double) && DaneGeograficzne.pktDocelowy.Latitude != default(double))
            {
                Geopoint startowyGeopoint = new Geopoint(DaneGeograficzne.pktStartowy);

                Geopoint docelowyGeopoint = new Geopoint(DaneGeograficzne.pktDocelowy);

                MapPolyline trasaLotem = new MapPolyline
                {
                    StrokeColor = Windows.UI.Colors.Black,
                    StrokeThickness = 3,
                    StrokeDashed = true,
                    Path = new Geopath(new List<BasicGeoposition> {
                DaneGeograficzne.pktStartowy,
                DaneGeograficzne.pktDocelowy
            })
                };

                mojaMapa.MapElements.Add(trasaLotem);
                MapIcon znacznikStart = new MapIcon
                {
                    Location = startowyGeopoint,
                    Title = "Tu jestem!"
                };

                MapIcon znacznikDocelowy = new MapIcon
                {
                    Location = docelowyGeopoint,
                    Title = "Cel podróży"
                };

                mojaMapa.MapElements.Add(znacznikStart);
                mojaMapa.MapElements.Add(znacznikDocelowy);

                await mojaMapa.TrySetViewAsync(startowyGeopoint, 8);
                await ObliczOdleglosc();
            }
            Trasa();
            

        }
        private async Task ObliczOdleglosc()
        {
            var geolocator = new Geolocator();

            var aktualnaPozycja = await geolocator.GetGeopositionAsync();

            var startowy = aktualnaPozycja.Coordinate.Point.Position;
            var docelowy = new BasicGeoposition
            {
                Latitude = DaneGeograficzne.pktDocelowy.Latitude,
                Longitude = DaneGeograficzne.pktDocelowy.Longitude
            };

            var odleglosc = ObliczOdleglosc(startowy, docelowy);

            var odlegloscWKm = odleglosc / 1000;
            var komunikat = $"Odległość między punktami: {odlegloscWKm:F2} km";
            var dialog = new Windows.UI.Popups.MessageDialog(komunikat);
            await dialog.ShowAsync();
        }

        private double ObliczOdleglosc(BasicGeoposition punktA, BasicGeoposition punktB)
        {
            const double earthRadius = 6371;

            var lat1 = ToRadians(punktA.Latitude);
            var lon1 = ToRadians(punktA.Longitude);
            var lat2 = ToRadians(punktB.Latitude);
            var lon2 = ToRadians(punktB.Longitude);

            var dlon = lon2 - lon1;
            var dlat = lat2 - lat1;

            var a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c * 1000;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        private async void Trasa()
        {
            if (DaneGeograficzne.pktStartowy.Longitude != default(double) && DaneGeograficzne.pktStartowy.Latitude != default(double) &&
                DaneGeograficzne.pktDocelowy.Longitude != default(double) && DaneGeograficzne.pktDocelowy.Latitude != default(double))
            {
                BasicGeoposition startowyPos = DaneGeograficzne.pktStartowy;
                BasicGeoposition docelowyPos = DaneGeograficzne.pktDocelowy;

                Geopoint startowyGeopoint = new Geopoint(startowyPos);
                Geopoint docelowyGeopoint = new Geopoint(docelowyPos);

                MapRouteFinderResult wynik = await MapRouteFinder.GetDrivingRouteAsync(startowyGeopoint, docelowyGeopoint);

                if (wynik.Status == MapRouteFinderStatus.Success)
                {
                    MapRoute trasa = wynik.Route;

                    MapRouteView trasaWidok = new MapRouteView(trasa);

                    mojaMapa.Routes.Add(trasaWidok);

                    await mojaMapa.TrySetViewBoundsAsync(trasa.BoundingBox, null, Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Nie udało się wyznaczyć trasy.");
                    await dialog.ShowAsync();
                }
            }

        }
        private void powMapa(object sender, RoutedEventArgs e)
        {
            if (mojaMapa.ZoomLevel < mojaMapa.MaxZoomLevel)
            {
                mojaMapa.ZoomLevel++;
            }
        }

        private void pomMapa(object sender, RoutedEventArgs e)
        {
            if (mojaMapa.ZoomLevel > mojaMapa.MinZoomLevel)
            {
                mojaMapa.ZoomLevel--;
            }
        }

        private void trybMapy(object sender, RoutedEventArgs e)
        {
            AppBarButton ab = sender as AppBarButton;
            if (ab != null)
            {
                FontIcon fIcon = new FontIcon();
                fIcon.FontFamily = new FontFamily("Segoe UI");

                if (mojaMapa.Style == MapStyle.Road)
                {
                    mojaMapa.Style = MapStyle.AerialWithRoads;
                    ab.Label = "mapa";
                    fIcon.Glyph = "M"; 
                }
                else
                {
                    mojaMapa.Style = MapStyle.Road;
                    ab.Label = "satelita";
                    fIcon.Glyph = "S"; 
                }

                ab.Icon = fIcon;
            }
        }

        private void Koordynaty(object sender, RoutedEventArgs e)
        {
            {
                this.Frame.Navigate(typeof(Koordynaty));
            }
        }
    }
}
