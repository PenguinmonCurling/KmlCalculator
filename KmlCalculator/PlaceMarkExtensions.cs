using System;
using System.Collections.Generic;
using System.Linq;
using SharpKml.Dom;

namespace KmlCalculator
{
    public static class PlaceMarkExtensions
    {
        public static double GetDistanceBetweenPlacemarks(this Placemark placemark, Placemark placemarkToCompare)
        {
            var minDistance = 999999d;
            Console.WriteLine("Comparing {0} and {1}", placemark.Name, placemarkToCompare.Name);
            foreach (var coordinate in ((MultipleGeometry)placemark.Geometry).Geometry.SelectMany(x => ((Polygon)x).OuterBoundary.LinearRing.Coordinates))
            {
                foreach (var comparableCoordinate in ((MultipleGeometry)placemarkToCompare.Geometry).Geometry.SelectMany(x => ((Polygon)x).OuterBoundary.LinearRing.Coordinates))
                {
                    var distance = CompareLatitudeLongitude(coordinate.Latitude, comparableCoordinate.Latitude, coordinate.Longitude, comparableCoordinate.Longitude);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }
            Console.WriteLine("Distance between {0} and {1} is {2}", placemark.Name, placemarkToCompare.Name, minDistance);
            return minDistance;
        }

        public static bool IsDistanceBetweenPlacemarksBelowTarget(this Placemark placemark, Placemark placemarkToCompare, double targetDistance, out double minDistance)
        {
            minDistance = 999999d;
            Console.WriteLine("Comparing {0} and {1}", placemark.Name, placemarkToCompare.Name);
            foreach (var coordinate in ((MultipleGeometry)placemark.Geometry).Geometry.SelectMany(x => ((Polygon)x).OuterBoundary.LinearRing.Coordinates))
            {
                foreach (var comparableCoordinate in ((MultipleGeometry)placemarkToCompare.Geometry).Geometry.SelectMany(x => ((Polygon)x).OuterBoundary.LinearRing.Coordinates))
                {
                    var distance = CompareLatitudeLongitude(coordinate.Latitude, comparableCoordinate.Latitude, coordinate.Longitude, comparableCoordinate.Longitude);
                    if (distance < targetDistance)
                    {
                        Console.WriteLine("Distance between {0} and {1} is below {2}", placemark.Name, placemarkToCompare.Name, targetDistance);
                        return true;
                    }
                    else if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }
            Console.WriteLine("Distance between {0} and {1} is {2}", placemark.Name, placemarkToCompare.Name, minDistance);
            return false;
        }

        private static double CompareLatitudeLongitude(double lat1, double lat2, double lon1, double lon2)
        {
            var R = 6378.137; // Radius of earth in KM
            var dLat = lat2 * Math.PI / 180 - lat1 * Math.PI / 180;
            var dLon = lon2 * Math.PI / 180 - lon1 * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d * 1000; // metres
        }

        public static Placemark ClonePlacemark(this Placemark placemark)
        {
            var clonedPlacemark = new Placemark();
            clonedPlacemark.Id = placemark.Id;
            clonedPlacemark.Name = placemark.Name;
            var clonedGeometry = new MultipleGeometry();
            if (placemark.Geometry is MultipleGeometry)
            {
                foreach (var geometry in ((MultipleGeometry)placemark.Geometry).Geometry)
                {
                    clonedGeometry.AddGeometry(new Polygon { OuterBoundary = new OuterBoundary { LinearRing = new LinearRing { Coordinates = new CoordinateCollection(((Polygon)geometry).OuterBoundary.LinearRing.Coordinates) } } });
                }
            }
            else if (placemark.Geometry is Polygon geometry)
            {
                clonedGeometry.AddGeometry(new Polygon { OuterBoundary = new OuterBoundary { LinearRing = new LinearRing { Coordinates = new CoordinateCollection(((Polygon)geometry).OuterBoundary.LinearRing.Coordinates) } } });

            }
            clonedPlacemark.Geometry = clonedGeometry;
            return clonedPlacemark;
        }

        public static IEnumerable<Polygon> ClonePolygons(this Placemark placemark)
        {
            var clonedGeometry = new List<Polygon>();
            foreach (var geometry in ((MultipleGeometry)placemark.Geometry).Geometry)
            {
                clonedGeometry.Add(new Polygon { OuterBoundary = new OuterBoundary { LinearRing = new LinearRing { Coordinates = new CoordinateCollection(((Polygon)geometry).OuterBoundary.LinearRing.Coordinates) } } });
            }
            return clonedGeometry;
        }
    }
}
