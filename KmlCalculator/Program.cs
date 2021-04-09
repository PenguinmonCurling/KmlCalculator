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
        private readonly static string _folderPath = "C:\\Users\\Jansuine\\Documents\\Stupid images i find mildly amusing\\Factory\\QGIS\\GIS\\splottery";

        static void Main(string[] args)
        {
            //CaclulateUrbanAreasWithinDistance("Greater London", 1500, 50000);
            //DisplayBuiltUpAreasBySize();
            Console.WriteLine("Type in file to split...");
            var filePathToSplit = Console.ReadLine();
            SplitKmlFile(filePathToSplit);
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }

        private static void DisplayBuiltUpAreasBySize()
        {
            var builtUpAreas = PopulateBuiltUpAreas();
            var builtUpAreasSorted = new Dictionary<string, int>();
            foreach (var builtuparea in builtUpAreas)
            {
                if (int.TryParse(builtuparea.Description.Text.Split(' ')[0], out int parsedPopulation))
                {
                    builtUpAreasSorted.Add(builtuparea.Name, parsedPopulation);
                }
            }
            foreach (var area in builtUpAreasSorted.Where(x => x.Value > 10000).OrderByDescending(x => x.Value))
            {
                Console.WriteLine(area.Key + ":" + area.Value);
            }
        }

        private static void SplitKmlFile(string filePathToSplit)
        {
            var builtUpAreas = new List<Feature>();
            builtUpAreas.AddRange(ImportFileWithWholePath(filePathToSplit));

            foreach (var builtUpArea in builtUpAreas)
            {
                var clonedPlacemark = ((Placemark)builtUpArea).ClonePlacemark();
                var kmlParent = new Kml();
                var kmlDocument = new Document();
                kmlDocument.AddFeature(clonedPlacemark);
                kmlParent.Feature = kmlDocument;
                var outPutStream = new FileStream(Path.Combine(_folderPath, string.Format("{0}.kml", GetNewFileName(clonedPlacemark))), FileMode.Create);
                var serialiser = new Serializer();
                serialiser.Serialize(kmlParent, outPutStream);
            }
        }

        private static string GetNewFileName(Placemark clonedPlacemark)
        {
            if (clonedPlacemark.Name.Contains(":"))
            {
                return clonedPlacemark.Name.Split(':')[1].Replace("/", "-");
            }
            else
            {
                return clonedPlacemark.Name;
            }
        }

        private static void SplitKmlFile()
        {
            SplitKmlFile("data.kml");
        }

        private static void CaclulateUrbanAreasWithinDistance(string areaToCompare, int comparisonValue, int distanceToIgnore)
        {
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
            builtUpAreas.AddRange(ImportFile("EastMidsdata.kml"));
            builtUpAreas.AddRange(ImportFile("LancsyData.kml"));
            builtUpAreas.AddRange(ImportFile("NorthEastData.kml"));
            builtUpAreas.AddRange(ImportFile("WestCountryData.kml"));
            builtUpAreas.AddRange(ImportFile("WestMidsData.kml"));
            builtUpAreas.AddRange(ImportFile("YorkshireData.kml"));
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

        private static List<Feature> ImportFileWithWholePath(string filePath)
        {
            var parser = new Parser();
            var fileReader = new FileStream(filePath, FileMode.Open);
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
