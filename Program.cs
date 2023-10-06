using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PistonMotion
{
    /// <summary>
    /// Calculates piston velocity based on crank stroke, rod length, and max RPM.
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            //Title
            Console.WriteLine(" _____________       _____                   ______  ___     __________              ");
            Console.WriteLine(" ___  __ \\__(_)________  /_____________      ___   |/  /_______  /___(_)____________ ");
            Console.WriteLine(" __  /_/ /_  /__  ___/  __/  __ \\_  __ \\     __  /|_/ /_  __ \\  __/_  /_  __ \\_  __ \\");
            Console.WriteLine(" _  ____/_  / _(__  )/ /_ / /_/ /  / / /     _  /  / / / /_/ / /_ _  / / /_/ /  / / /");
            Console.WriteLine(" /_/     /_/  /____/ \\__/ \\____//_/ /_/      /_/  /_/  \\____/\\__/ /_/  \\____//_/ /_/ ");
            Console.WriteLine("\n\t\t\t Piston Motion and Velocity Calc v0.3 \n \t Enter stroke, rod length, and max RPM - Outputs velocity to CSV\n");

            while (true)
            {
                try
                {
                    var arguments = new Arguments();
                    var results = new Results();

                    if (args.Length == 0)
                    {
                        Console.Write("File location (Defaults to Temp:) \t");
                        string _testFileLocation = Console.ReadLine();
                        if (_testFileLocation != null)
                        {
                            arguments.FileLocation = "C:\\Windows\\Temp\\Piston-Motion-Calc\\";
                        }
                        else
                        {
                            arguments.FileLocation = _testFileLocation;
                        }
                        Console.Write("File name: \t \t \t \t");
                        arguments.Filename = arguments.FileLocation + Console.ReadLine() + ".csv";
                        Console.Write("Using metric units (mm/cc), y/n? \t");
                        string _checkIfMetric = Console.ReadLine();
                        if (_checkIfMetric == null ^ _checkIfMetric == "n")
                        {
                            arguments.IsMetric = false;
                        }
                        else if (_checkIfMetric == "y")
                        {
                            arguments.IsMetric = true;
                        }
                        Console.Write("Bore: \t \t \t \t \t");
                        arguments.Bore = double.Parse(Console.ReadLine());
                        Console.Write("Stroke: \t \t \t \t");
                        arguments.Stroke = double.Parse(Console.ReadLine());
                        Console.Write("Rod Length: \t \t \t \t");
                        arguments.RodLength = double.Parse(Console.ReadLine());
                        Console.Write("Block Deck Height: \t \t \t");
                        arguments.DeckHeight = double.Parse(Console.ReadLine());
                        Console.Write("Piston Compression Height: \t \t");
                        arguments.CompHeight = double.Parse(Console.ReadLine());
                        Console.Write("Piston Dome Volume: \t \t \t");
                        arguments.PistonVolume = float.Parse(Console.ReadLine());
                        Console.Write("(Negative value for dish) \n");
                        Console.Write("Combustion Chamber Volume: \t \t");
                        arguments.ChamberVolume = float.Parse(Console.ReadLine());
                        Console.Write("Head Gasket Compressed Thickness: \t");
                        arguments.GasketHeight = double.Parse(Console.ReadLine());
                        Console.Write("Max RPM: \t \t \t \t");
                        arguments.RPM = int.Parse(Console.ReadLine());
                        Console.Write("Cylinder Count: \t \t \t");
                        arguments.CylinderCount = int.Parse(Console.ReadLine());
                    }
                    else
                    {
                        Console.WriteLine($"Arguments: {args[0]},{args[1]},{args[2]},{args[3]},{args[4]},{args[5]},{args[6]},{args[7]},{args[8]}");
                        arguments.FileLocation = args[0];
                        arguments.Filename = args[1];
                        arguments.IsMetric = bool.Parse(args[2]);
                        arguments.Bore = double.Parse(args[3]);
                        arguments.Stroke = double.Parse(args[4]);
                        arguments.RodLength = double.Parse(args[5]);
                        arguments.DeckHeight= double.Parse(args[6]);
                        arguments.CompHeight= double.Parse(args[7]);
                        arguments.PistonVolume = float.Parse(args[8]);
                        arguments.ChamberVolume = float.Parse(args[9]);
                        arguments.GasketHeight= double.Parse(args[10]);
                        arguments.RPM = int.Parse(args[11]);
                        arguments.CylinderCount= int.Parse(args[12]);
                        break;
                    }

                    Console.WriteLine("\n");
                    var csvResults = Calculate(arguments, results);

                    ConsoleOutput(arguments, results);

                    SaveResults(arguments.FileLocation, arguments.Filename, csvResults);
                    Console.WriteLine("\n\n");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }



        }

        public static List<PistonResult> Calculate(Arguments arguments, Results results)
        {
            var csvResults = new List<PistonResult>();

            double angVelocity = 2 * Math.PI * (arguments.RPM / 60);
            double radius = arguments.Stroke / 2;

            double totalDeckHeight = arguments.DeckHeight + arguments.GasketHeight;

            int _metricMod;
            if (arguments.IsMetric)
            {
                _metricMod = 1000;
            }
            else
            {
                _metricMod = 0;
            }


            //Calculate static results; displacement, bore to stroke, rod ratio, etc.
            //Displacement per cylinder
            results.Displacement = Math.PI * Math.Pow(arguments.Bore / 2, 2) * arguments.Stroke * arguments.CylinderCount;
            //Bore to stroke ratio
            results.BoreRatio = arguments.Bore / arguments.Stroke;
            //Rod ratio
            results.RodRatio = arguments.RodLength / arguments.Stroke;
            //Piston to deck
            results.Piston2deck = (arguments.DeckHeight + arguments.GasketHeight) - (arguments.RodLength + arguments.CompHeight + radius);
            //Compression Ratio
            results.CompressionRatio = (Math.PI * Math.Pow(arguments.Bore / 2, 2) * (arguments.GasketHeight + arguments.Stroke)) / ((arguments.ChamberVolume - arguments.PistonVolume) * _metricMod);
 
            


            for (int angle = 0; angle <= 180; angle++)
            {
                //Degrees to radians
                double radAngle = ((double)angle / 180) * Math.PI;

                //Piston maths
                double negRadius = radius * -1;
                double sqrRadius = Math.Pow((double)radius, 2);
                double sinAngle = Math.Sin(radAngle);
                double cosAngle = Math.Cos(radAngle);
                double sqrRodl = Math.Pow(arguments.RodLength, 2);

                double velocity = CalculateVelocity(angVelocity, negRadius, sqrRadius, sinAngle, cosAngle, sqrRodl);
                
                var x = radius * cosAngle + Math.Sqrt(sqrRodl - sqrRadius * Math.Pow(sinAngle, 2));
                var pistonPosition = 0 - (totalDeckHeight - (x + arguments.CompHeight));

                var result = new PistonResult(angle, pistonPosition, velocity);

                csvResults.Add(result);

                //Check for peak velocity
                if (results.MaxVelocity < velocity)
                {
                    results.MaxVelocity = velocity;
                    results.MaxVelocityDeg = angle;
                }
            }

            return csvResults;
        }
        //Method for calculating piston velocity
        private static double CalculateVelocity(double angVelocity, double negRadius, double sqrRadius, double sinAngle, double cosAngle, double sqrRodl)
        {
            return Math.Abs(angVelocity * (negRadius * sinAngle - ((sqrRadius * sinAngle * cosAngle) / (Math.Sqrt(sqrRodl - sqrRadius * Math.Pow(sinAngle, 2))))));
        }

        //Method for outputting max velocity to console
        public static void ConsoleOutput(Arguments arguments, Results results)
        {
            int _metricMod;
            if (arguments.IsMetric)
            {
                _metricMod = 1000;
            }
            else
            {
                _metricMod = 1;
            }
            
            Console.WriteLine("Total swept displacement: \t \t" + results.Displacement / _metricMod);
            Console.WriteLine("Bore to Stroke Ratio: \t \t \t" + results.BoreRatio);
            Console.WriteLine("Rod Ratio: \t \t \t \t" + results.RodRatio);
            Console.WriteLine("Piston to deck including gasket: \t" + results.Piston2deck);
            Console.WriteLine("(Negative value is 'out of the hole') \t \t \t");
            Console.WriteLine("Static Compression Ratio: \t \t" + results.CompressionRatio);
            Console.WriteLine("Peak piston velocity is " + results.MaxVelocity + " at " + results.MaxVelocityDeg + " degrees \n");
        }

        public static void SaveResults(string fileLocation, string fileName, List<PistonResult> csvResults)
        {
            if (!Directory.Exists(fileLocation))
            {
                Directory.CreateDirectory(fileLocation);
            }
            
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("angle,ppos,pvel");

            foreach(var result in csvResults)
            {
                stringBuilder.AppendLine($"{result}");
            }

            File.WriteAllText(fileName, stringBuilder.ToString());

            Console.WriteLine($"File results written to {fileName}");
        }

    }
}
