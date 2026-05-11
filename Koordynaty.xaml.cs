using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Services.Maps;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Nawigacja
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Koordynaty : Page
    {
        public Koordynaty()
        {
            this.InitializeComponent();
            GdzieJaNaMapie();
        }

        private void Powrot_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
        private async void GdzieJaNaMapie()
        {
            Geolocator mójGPS = new Geolocator { DesiredAccuracyInMeters = 1 };

            Geoposition mojeZGPS = await mójGPS.GetGeopositionAsync();
            tbGPS.Text = $"Moje położenie: {mojeZGPS.Coordinate.Point.Position.Latitude}, {mojeZGPS.Coordinate.Point.Position.Longitude}";

            DaneGeograficzne.pktStartowy = new BasicGeoposition
            {
                Latitude = mojeZGPS.Coordinate.Point.Position.Latitude,
                Longitude = mojeZGPS.Coordinate.Point.Position.Longitude
            };
        }


        private async void Szukaj_Click(object sender, RoutedEventArgs e)
        {
            string szukanyAdres = txAdres.Text;

            if (string.IsNullOrWhiteSpace(szukanyAdres))
            {
                return;
            }

            MapLocationFinderResult wynik = await MapLocationFinder.FindLocationsAsync(szukanyAdres, new Geopoint(DaneGeograficzne.pktStartowy), 3);

            if (wynik.Status == MapLocationFinderStatus.Success && wynik.Locations.Count > 0)
            {
                DaneGeograficzne.pktDocelowy = wynik.Locations[0].Point.Position;
                DaneGeograficzne.opisCelu = szukanyAdres;

                tbDlg.Text = $"Długość geograficzna: {DaneGeograficzne.pktDocelowy.Longitude}";
                tbSzer.Text = $"Szerokość geograficzna: {DaneGeograficzne.pktDocelowy.Latitude}";
            }
            else
            {
                tbDlg.Text = "Nie można znaleźć lokalizacji.";
                tbSzer.Text = "";
            }
        }
    }
}
