using Windows.Devices.Geolocation;

namespace Nawigacja
{
    internal class DaneGeograficzne
    {
        public static BasicGeoposition pktStartowy { get; set; }
        public static BasicGeoposition pktDocelowy { get; set; }
        public static string opisCelu { get; set; } = null;
    }
}
