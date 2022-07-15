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
            //Calculates peak piston velocity based on crank stroke, rod length, and max RPM.
            Console.WriteLine("\t\t Piston Velocity Calc v0.2 \n Enter stroke, rod length, and max RPM - Outputs velocity to CSV\n");

            while (true)
            {
                try
                {
                    var arguments = new Arguments();

                    if (args.Length == 0)
                    {
                        Console.WriteLine("File name: ");
                        arguments.Filename = Console.ReadLine() + ".csv";
                        Console.Write("Stroke: ");
                        arguments.Stroke = double.Parse(Console.ReadLine());
                        Console.Write("Rod Length: ");
                        arguments.RodLength = double.Parse(Console.ReadLine());
                        Console.Write("Max RPM: ");
                        arguments.RPM = int.Parse(Console.ReadLine());
                    }
                    else
                    {
                        Console.WriteLine($"Arguments: {args[0]},{args[1]},{args[2]},{args[3]}");
                        arguments.Filename = args[0];
                        arguments.Stroke = double.Parse(args[1]);
                        arguments.RodLength = double.Parse(args[2]);
                        arguments.RPM = int.Parse(args[3]);
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
                var pistonPosition = (x - arguments.RodLength);

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
