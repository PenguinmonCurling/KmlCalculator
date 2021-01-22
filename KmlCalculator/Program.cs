using SharpKml.Base;
using SharpKml.Dom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KmlCalculator
{
    class Program
    {
        private readonly static string _folderPath = "C:\\Users\\Jansuine\\Documents\\Stupid images i find mildly amusing\\Factory\\QGIS\\GIS";

        static void Main(string[] args)
        {
            var areaToCompare = "Greater London";
            var comparisonValue = 1500;
            var distanceToIgnore = 50000;
            var builtUpAreas = PopulateBuiltUpAreas();
            var areaToComparePlacemark = ((Placemark)builtUpAreas.Where(x => x.Name.Contains(areaToCompare)).FirstOrDefault()).ClonePlacemark();
            var closeAreas = new List<Feature>();
            var population = 0;

            var newOneFound = false;
            do
            {
                newOneFound = false;
                var builtUpAreasTemporary = builtUpAreas.ToList();
                var clonedLondon = areaToComparePlacemark.ClonePlacemark();
                Parallel.ForEach(builtUpAreasTemporary, builtUpArea =>
                {
                    double distance;
                    if (clonedLondon.IsDistanceBetweenPlacemarksBelowTarget((Placemark)builtUpArea, comparisonValue, out distance))
                    {
                        newOneFound = true;
                        Console.WriteLine("{0} is within {1} of {2}, adding to List", builtUpArea.Name, comparisonValue, areaToCompare);
                        closeAreas.Add(builtUpArea);
                        builtUpAreas.Remove(builtUpArea);
                        var clonedGeometry = ((Placemark)builtUpArea).ClonePolygons();
                        if (int.TryParse(builtUpArea.Description.Text.Split(' ')[0], out int parsedPopulation))
                        {
                            population += parsedPopulation;
                        }
                        foreach (var polygon in clonedGeometry)
                        {
                            ((MultipleGeometry)areaToComparePlacemark.Geometry).AddGeometry(polygon);
                        }
                    }
                    else if (distance > distanceToIgnore)
                    {
                        builtUpAreas.Remove(builtUpArea);
                    }
                });
            }
            while (newOneFound == true);

            areaToComparePlacemark.Description = new Description { Text = population.ToString() };
            var outPutStream = new FileStream(Path.Combine(_folderPath, string.Format("{0}{1}m.kml", areaToCompare, comparisonValue)), FileMode.Create);
            var serialiser = new Serializer();
            serialiser.Serialize(areaToComparePlacemark, outPutStream);
        }

        private static List<Feature> PopulateBuiltUpAreas()
        {
            var builtUpAreas = new List<Feature>();
            builtUpAreas.AddRange(ImportFile("SE.kml"));
            builtUpAreas.AddRange(ImportFile("E.kml"));
            return builtUpAreas;
        }

        private static List<Feature> ImportFile(string fileName)
        {
            var parser = new Parser();
            var fileReader = new FileStream(Path.Combine(_folderPath, fileName), FileMode.Open);
            parser.Parse(fileReader);
            var kmlRoot = parser.Root;
            if (kmlRoot is Kml builtUpAreaKmls && builtUpAreaKmls.Feature is Document builtUpAreaDocument)
            {
                return builtUpAreaDocument.Features.ToList();
            }
            return new List<Feature>();
        }
    }
}
