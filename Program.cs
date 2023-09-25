using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PistonMotion
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            //Calculates piston velocity based on crank stroke, rod length, and max RPM.
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

                    if (args.Length == 0)
                    {
                        Console.Write("File name: \t \t \t \t");
                        arguments.Filename = Console.ReadLine() + ".csv";
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
                        Console.Write("Head Gasket Compressed Thickness: \t");
                        arguments.GasketHeight = double.Parse(Console.ReadLine());
                        Console.Write("Max RPM: \t \t \t \t");
                        arguments.RPM = int.Parse(Console.ReadLine());
                    }
                    else
                    {
                        Console.WriteLine($"Arguments: {args[0]},{args[1]},{args[2]},{args[3]},{args[4]},{args[5]},{args[6]},{args[7]}");
                        arguments.Filename = args[0];
                        arguments.Bore = double.Parse(args[1]);
                        arguments.Stroke = double.Parse(args[2]);
                        arguments.RodLength = double.Parse(args[3]);
                        arguments.DeckHeight= double.Parse(args[4]);
                        arguments.CompHeight= double.Parse(args[5]);
                        arguments.GasketHeight= double.Parse(args[6]);
                        arguments.RPM = int.Parse(args[7]);
                        break;
                    }

                    Console.WriteLine("\n");

                    var results = Calculate(arguments);
                    SaveResults(arguments.Filename, results);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static List<PistonResult> Calculate(Arguments arguments)
        {
            var results = new List<PistonResult>();

            double angVelocity = 2 * Math.PI * (arguments.RPM / 60);
            double radius = arguments.Stroke / 2;

            double totalDeckHeight = arguments.DeckHeight + arguments.GasketHeight;

            for (int angle = 0; angle <= 180; angle++)
            {
                //Degrees to radians
                double radAngle = ((double)angle / 180) * Math.PI;

                //Piston velocity maths
                double negRadius = radius * -1;
                double sqrRadius = Math.Pow((double)radius, 2);
                double sinAngle = Math.Sin(radAngle);
                double cosAngle = Math.Cos(radAngle);
                double sqrRodl = Math.Pow(arguments.RodLength, 2);

                double velocity = CalculateVelocity(angVelocity, negRadius, sqrRadius, sinAngle, cosAngle, sqrRodl);
                
                var x = radius * cosAngle + Math.Sqrt(sqrRodl - sqrRadius * Math.Pow(sinAngle, 2));
                var pistonPosition = 0 - (totalDeckHeight - (x + arguments.CompHeight));

                var result = new PistonResult(angle, pistonPosition, velocity);

                results.Add(result);
            }

            return results;
        }

        private static double CalculateVelocity(double angVelocity, double negRadius, double sqrRadius, double sinAngle, double cosAngle, double sqrRodl)
        {
            return Math.Abs(angVelocity * (negRadius * sinAngle - ((sqrRadius * sinAngle * cosAngle) / (Math.Sqrt(sqrRodl - sqrRadius * Math.Pow(sinAngle, 2))))));
        }

        public static void SaveResults(string fileName, List<PistonResult> results)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("angle,ppos,pvel");

            foreach(var result in results)
            {
                stringBuilder.AppendLine($"{result}");
            }

            File.WriteAllText(fileName, stringBuilder.ToString());

            Console.WriteLine($"File results written to {fileName}");
        }
    }
}
