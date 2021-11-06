namespace TarbikMap
{
    using System;

    internal static class CalculationUtils
    {
        public static double DistanceBetween(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // km
            var x1 = lat2 - lat1;
            var dLat = ToRad(x1);
            var x2 = lon2 - lon1;
            var dLon = ToRad(x2);
            var a =
              (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) +
              (Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2));
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        private static double ToRad(double x)
        {
            return (x * Math.PI) / 180;
        }
    }
}