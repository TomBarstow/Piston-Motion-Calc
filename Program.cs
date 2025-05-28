using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PistonMotion
{
    /// <summary>
    /// Calculates piston velocity based on crank stroke, rod length, and max RPM.
    /// Now includes cam profile calculations for piston-to-valve clearance analysis.
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            DisplayTitle();

            while (true)
            {
                try
                {
                    var arguments = GetArguments(args);

                    if (!ValidateInputs(arguments))
                    {
                        Console.WriteLine("Invalid input values detected. Please check your inputs and try again.");
                        continue;
                    }

                    var results = new ConsoleResults();
                    var csvResults = Calculate(arguments, results);

                    DisplayResults(arguments, results);
                    SaveResults(arguments.FileLocation, arguments.Filename, csvResults);

                    Console.WriteLine("\n\nPress any key to continue or Ctrl+C to exit...");
                    Console.ReadKey();
                    Console.Clear();
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid number format. Please enter valid numeric values.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine("Please try again.");
                }
            }
        }

        private static void DisplayTitle()
        {
            Console.WriteLine(" _____________       _____                   ______  ___     __________              ");
            Console.WriteLine(" ___  __ \\__(_)________  /_____________      ___   |/  /_______  /___(_)____________ ");
            Console.WriteLine(" __  /_/ /_  /__  ___/  __/  __ \\_  __ \\     __  /|_/ /_  __ \\  __/_  /_  __ \\_  __ \\");
            Console.WriteLine(" _  ____/_  / _(__  )/ /_ / /_/ /  / / /     _  /  / / / /_/ / /_ _  / / /_/ /  / / /");
            Console.WriteLine(" /_/     /_/  /____/ \\__/ \\____//_/ /_/      /_/  /_/  \\____/\\__/ /_/  \\____//_/ /_/ ");
            Console.WriteLine("\n\t\t\t Piston Motion and Valve Timing Calc v0.3 \n \t Enter stroke, rod length, max RPM, and cam specs - Outputs motion data to CSV\n");
        }

        private static Arguments GetArguments(string[] args)
        {
            var arguments = new Arguments();

            if (args.Length == 0)
            {
                GetArgumentsFromConsole(arguments);
            }
            else
            {
                GetArgumentsFromCommandLine(args, arguments);
            }

            return arguments;
        }

        private static void GetArgumentsFromConsole(Arguments arguments)
        {
            Console.Write("File location (Press Enter for default: C:\\Windows\\Temp\\Piston-Motion-Calc\\): ");
            string fileLocation = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(fileLocation))
            {
                arguments.FileLocation = "C:\\Windows\\Temp\\Piston-Motion-Calc\\";
            }
            else
            {
                arguments.FileLocation = fileLocation.EndsWith("\\") ? fileLocation : fileLocation + "\\";
            }

            Console.Write("File name: ");
            string filename = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = "debug_output";
            }
            arguments.Filename = Path.Combine(arguments.FileLocation, filename + ".csv");

            Console.Write("Using metric units (mm/cc), y/n? [y]: ");
            string checkIfMetric = Console.ReadLine()?.ToLower();
            arguments.IsMetric = string.IsNullOrWhiteSpace(checkIfMetric) || checkIfMetric == "y";

            string units = arguments.IsMetric ? "mm" : "inches";
            string volumeUnits = arguments.IsMetric ? "cc" : "cubic inches";

            // Engine specifications
            Console.WriteLine("\n=== ENGINE SPECIFICATIONS ===");

            Console.Write($"Bore ({units}): ");
            string boreInput = Console.ReadLine();
            arguments.Bore = string.IsNullOrWhiteSpace(boreInput) ? 100.0 : double.Parse(boreInput);

            Console.Write($"Stroke ({units}): ");
            string strokeInput = Console.ReadLine();
            arguments.Stroke = string.IsNullOrWhiteSpace(strokeInput) ? 100.0 : double.Parse(strokeInput);

            Console.Write($"Rod Length ({units}): ");
            string rodLengthInput = Console.ReadLine();
            arguments.RodLength = string.IsNullOrWhiteSpace(rodLengthInput) ? 200.0 : double.Parse(rodLengthInput);

            Console.Write($"Block Deck Height ({units}): ");
            string deckHeightInput = Console.ReadLine();
            arguments.DeckHeight = string.IsNullOrWhiteSpace(deckHeightInput) ? 300.0 : double.Parse(deckHeightInput);

            Console.Write($"Piston Compression Height ({units}): ");
            string compHeightInput = Console.ReadLine();
            arguments.CompHeight = string.IsNullOrWhiteSpace(compHeightInput) ? 50.0 : double.Parse(compHeightInput);

            Console.Write($"Piston Dome Volume ({volumeUnits}) (Negative for dish): ");
            string pistonVolumeInput = Console.ReadLine();
            arguments.PistonVolume = string.IsNullOrWhiteSpace(pistonVolumeInput) ? 0.0 : double.Parse(pistonVolumeInput);

            Console.Write($"Combustion Chamber Volume ({volumeUnits}): ");
            string chamberVolumeInput = Console.ReadLine();
            arguments.ChamberVolume = string.IsNullOrWhiteSpace(chamberVolumeInput) ? 70.0 : double.Parse(chamberVolumeInput);

            Console.Write($"Head Gasket Compressed Thickness ({units}): ");
            string gasketHeightInput = Console.ReadLine();
            arguments.GasketHeight = string.IsNullOrWhiteSpace(gasketHeightInput) ? 1.0 : double.Parse(gasketHeightInput);

            Console.Write("Max RPM: ");
            string rpmInput = Console.ReadLine();
            arguments.RPM = string.IsNullOrWhiteSpace(rpmInput) ? 7000 : int.Parse(rpmInput);

            Console.Write("Cylinder Count: ");
            string cylinderCountInput = Console.ReadLine();
            arguments.CylinderCount = string.IsNullOrWhiteSpace(cylinderCountInput) ? 8 : int.Parse(cylinderCountInput);

            // Cam specifications
            Console.WriteLine("\n=== CAM SPECIFICATIONS ===");
            Console.Write("Include cam profile calculations? (y/n) [y]: ");
            string includeCam = Console.ReadLine()?.ToLower();
            arguments.IncludeCamProfile = string.IsNullOrWhiteSpace(includeCam) || includeCam == "y";

            if (arguments.IncludeCamProfile)
            {
                arguments.CamSpec = new CamSpecification();
                GetCamSpecifications(arguments.CamSpec, arguments.IsMetric);
            }
        }

        private static void GetCamSpecifications(CamSpecification camSpec, bool isMetric)
        {
            string units = isMetric ? "mm" : "inches";

            Console.WriteLine("\n--- INTAKE CAM ---");

            Console.Write($"Intake cam max lift ({units}): ");
            string intakeLiftInput = Console.ReadLine();
            camSpec.IntakeCam.MaxLift = string.IsNullOrWhiteSpace(intakeLiftInput) ? 10.0 : double.Parse(intakeLiftInput);

            Console.Write("Intake duration @ 0.050\" (degrees): ");
            string intakeDurationInput = Console.ReadLine();
            camSpec.IntakeCam.DurationAtLift = string.IsNullOrWhiteSpace(intakeDurationInput) ? 226.0 : double.Parse(intakeDurationInput);
            camSpec.IntakeCam.CheckingLift = isMetric ? 1.27 : 0.050; // Convert 0.050" to mm if metric

            Console.Write("Intake lobe centerline (degrees ATDC intake): ");
            string intakeCenterlineInput = Console.ReadLine();
            camSpec.IntakeCam.LobeCenterline = string.IsNullOrWhiteSpace(intakeCenterlineInput) ? 108.0 : double.Parse(intakeCenterlineInput);

            Console.Write("Intake rocker ratio [1.5]: ");
            string intakeRocker = Console.ReadLine();
            camSpec.IntakeCam.RockerRatio = string.IsNullOrWhiteSpace(intakeRocker) ? 1.5 : double.Parse(intakeRocker);

            Console.WriteLine("\n--- EXHAUST CAM ---");

            Console.Write($"Exhaust cam max lift ({units}): ");
            string exhaustLiftInput = Console.ReadLine();
            camSpec.ExhaustCam.MaxLift = string.IsNullOrWhiteSpace(exhaustLiftInput) ? 10.0 : double.Parse(exhaustLiftInput);

            Console.Write("Exhaust duration @ 0.050\" (degrees): ");
            string exhaustDurationInput = Console.ReadLine();
            camSpec.ExhaustCam.DurationAtLift = string.IsNullOrWhiteSpace(exhaustDurationInput) ? 230.0 : double.Parse(exhaustDurationInput);
            camSpec.ExhaustCam.CheckingLift = isMetric ? 1.27 : 0.050; // Convert 0.050" to mm if metric

            Console.Write("Exhaust lobe centerline (degrees BTDC exhaust): ");
            string exhaustCenterlineInput = Console.ReadLine();
            double exhaustCenterlineBTDC = string.IsNullOrWhiteSpace(exhaustCenterlineInput) ? 116.0 : double.Parse(exhaustCenterlineInput);
            // Convert BTDC exhaust to ATDC from compression TDC
            // Exhaust stroke runs from 540-720° ATDC
            // Exhaust TDC is at 720° (or 0° of next cycle)
            // So BTDC exhaust becomes: 720 - BTDC
            camSpec.ExhaustCam.LobeCenterline = 720 - exhaustCenterlineBTDC;

            Console.Write("Exhaust rocker ratio: ");
            string exhaustRocker = Console.ReadLine();
            camSpec.ExhaustCam.RockerRatio = string.IsNullOrWhiteSpace(exhaustRocker) ? 1.5 : double.Parse(exhaustRocker);

            // Initialize the cam profiles
            camSpec.Initialize();

            // Display calculated timing events
            Console.WriteLine("\n--- CALCULATED TIMING EVENTS ---");
            Console.WriteLine($"Intake opens: {camSpec.IntakeCam.OpeningPoint:F1}° ATDC | closes: {camSpec.IntakeCam.ClosingPoint:F1}° ATDC | duration: {camSpec.IntakeCam.TotalDuration:F1}°");
            Console.WriteLine($"Exhaust opens: {camSpec.ExhaustCam.OpeningPoint:F1}° ATDC | closes: {camSpec.ExhaustCam.ClosingPoint:F1}° ATDC | duration: {camSpec.ExhaustCam.TotalDuration:F1}°");
            Console.WriteLine($"(Exhaust centerline converted from {exhaustCenterlineBTDC:F1}° BTDC to {camSpec.ExhaustCam.LobeCenterline:F1}° ATDC)");
        }

        private static void GetArgumentsFromCommandLine(string[] args, Arguments arguments)
        {
            // For now, command line arguments don't include cam specs
            // This could be expanded later if needed
            if (args.Length < 13)
            {
                throw new ArgumentException($"Expected 13 command line arguments, got {args.Length}");
            }

            Console.WriteLine($"Arguments: {string.Join(",", args)}");

            arguments.FileLocation = args[0];
            arguments.Filename = args[1];
            arguments.IsMetric = bool.Parse(args[2]);
            arguments.Bore = double.Parse(args[3]);
            arguments.Stroke = double.Parse(args[4]);
            arguments.RodLength = double.Parse(args[5]);
            arguments.DeckHeight = double.Parse(args[6]);
            arguments.CompHeight = double.Parse(args[7]);
            arguments.PistonVolume = double.Parse(args[8]);
            arguments.ChamberVolume = double.Parse(args[9]);
            arguments.GasketHeight = double.Parse(args[10]);
            arguments.RPM = int.Parse(args[11]);
            arguments.CylinderCount = int.Parse(args[12]);

            // Default to no cam profile for command line usage
            arguments.IncludeCamProfile = false;
        }

        private static bool ValidateInputs(Arguments arguments)
        {
            var validationErrors = new List<string>();

            if (arguments.Stroke <= 0) validationErrors.Add("Stroke must be positive");
            if (arguments.Bore <= 0) validationErrors.Add("Bore must be positive");
            if (arguments.RodLength <= 0) validationErrors.Add("Rod length must be positive");
            if (arguments.DeckHeight < 0) validationErrors.Add("Deck height cannot be negative");
            if (arguments.CompHeight <= 0) validationErrors.Add("Compression height must be positive");
            if (arguments.ChamberVolume <= 0) validationErrors.Add("Chamber volume must be positive");
            if (arguments.GasketHeight < 0) validationErrors.Add("Gasket height cannot be negative");
            if (arguments.RPM <= 0) validationErrors.Add("RPM must be positive");
            if (arguments.CylinderCount <= 0) validationErrors.Add("Cylinder count must be positive");

            // Rod length should be greater than stroke
            if (arguments.RodLength < arguments.Stroke)
            {
                validationErrors.Add("Warning: Rod length is shorter than stroke - this may produce unrealistic results");
            }

            // Check for potential compression ratio issues
            double clearanceVolume = arguments.ChamberVolume - arguments.PistonVolume;
            if (clearanceVolume <= 0)
            {
                validationErrors.Add("Chamber volume minus piston volume must be positive");
            }

            // Validate cam specifications if included
            if (arguments.IncludeCamProfile && arguments.CamSpec != null)
            {
                if (arguments.CamSpec.IntakeCam.MaxLift <= 0) validationErrors.Add("Intake cam lift must be positive");
                if (arguments.CamSpec.IntakeCam.DurationAtLift <= 0) validationErrors.Add("Intake cam duration must be positive");
                if (arguments.CamSpec.ExhaustCam.MaxLift <= 0) validationErrors.Add("Exhaust cam lift must be positive");
                if (arguments.CamSpec.ExhaustCam.DurationAtLift <= 0) validationErrors.Add("Exhaust cam duration must be positive");
            }

            if (validationErrors.Count > 0)
            {
                Console.WriteLine("Validation errors:");
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($"  - {error}");
                }
                return false;
            }

            return true;
        }

        public static List<CSVResults> Calculate(Arguments arguments, ConsoleResults results)
        {
            var csvResults = new List<CSVResults>();

            double angVelocity = 2 * Math.PI * (arguments.RPM / 60.0);
            double radius = arguments.Stroke / 2.0;
            double totalDeckHeight = arguments.DeckHeight + arguments.GasketHeight;

            // Calculate static results
            CalculateStaticResults(arguments, results);

            // Calculate piston motion for full 720-degree 4-stroke cycle
            for (int angle = 0; angle < 720; angle++)
            {
                // For piston calculations, we only need the angle within one revolution (0-360)
                // but we track the full 720-degree cycle for valve events
                double pistonAngle = angle % 360;
                double radAngle = (pistonAngle / 180.0) * Math.PI;
                double sinAngle = Math.Sin(radAngle);
                double cosAngle = Math.Cos(radAngle);

                // Calculate piston velocity
                double velocity = CalculateVelocity(angVelocity, radius, sinAngle, cosAngle, arguments.RodLength);

                // Calculate piston position
                double x = radius * cosAngle + Math.Sqrt(Math.Pow(arguments.RodLength, 2) - Math.Pow(radius * sinAngle, 2));
                double pistonPosition = -(totalDeckHeight - (x + arguments.CompHeight));

                // Calculate valve lifts if cam profile is included
                double intakeValveLift = 0;
                double exhaustValveLift = 0;

                if (arguments.IncludeCamProfile && arguments.CamSpec != null)
                {
                    var valveLifts = arguments.CamSpec.GetValveLiftAtAngle(angle);
                    intakeValveLift = valveLifts.IntakeLift;
                    exhaustValveLift = valveLifts.ExhaustLift;
                }

                var result = new CSVResults(angle, pistonPosition, velocity, intakeValveLift, exhaustValveLift);
                csvResults.Add(result);

                // Track peak velocity (only consider first 360 degrees to avoid duplicates)
                if (angle < 360 && results.MaxVelocity < velocity)
                {
                    results.MaxVelocity = velocity;
                    results.MaxVelocityDeg = angle;
                }
            }

            return csvResults;
        }

        private static void CalculateStaticResults(Arguments arguments, ConsoleResults results)
        {
            // Displacement per cylinder (total for all cylinders)
            double cylinderVolume = Math.PI * Math.Pow(arguments.Bore / 2.0, 2) * arguments.Stroke;
            results.Displacement = cylinderVolume * arguments.CylinderCount;

            // Bore to stroke ratio
            results.BoreRatio = arguments.Bore / arguments.Stroke;

            // Rod ratio
            results.RodRatio = arguments.RodLength / arguments.Stroke;

            // Piston to deck
            results.Piston2deck = (arguments.DeckHeight + arguments.GasketHeight) -
                                 (arguments.RodLength + arguments.CompHeight + arguments.Stroke / 2.0);

            // Compression ratio - fixed calculation
            results.CompressionRatio = CalculateCompressionRatio(arguments);
        }

        private static double CalculateCompressionRatio(Arguments arguments)
        {
            // Swept volume (cylinder displacement)
            double sweptVolume = Math.PI * Math.Pow(arguments.Bore / 2.0, 2) * arguments.Stroke;

            // Gasket volume 
            double gasketVolume = Math.PI * Math.Pow(arguments.Bore / 2.0, 2) * arguments.GasketHeight;

            // Clearance volume = chamber volume + gasket volume - piston dome volume
            double clearanceVolume = arguments.ChamberVolume + gasketVolume - arguments.PistonVolume;

            // Unit conversion handling
            if (arguments.IsMetric)
            {
                // Metric: bore/stroke in mm creates volume in mm³
                // Chamber/piston volumes typically entered in cc (cm³)
                // Convert cc to mm³: 1 cc = 1000 mm³
                double chamberAndPistonInMM3 = (arguments.ChamberVolume - arguments.PistonVolume) * 1000;
                clearanceVolume = chamberAndPistonInMM3 + gasketVolume;
            }
            else
            {
                // Imperial: bore/stroke in inches creates volume in in³
                // Chamber/piston volumes should be in in³ (cubic inches)
                clearanceVolume = (arguments.ChamberVolume - arguments.PistonVolume) + gasketVolume;
            }

            if (clearanceVolume <= 0)
            {
                throw new InvalidOperationException("Clearance volume must be positive. Check chamber volume and piston volume values.");
            }

            return (sweptVolume + clearanceVolume) / clearanceVolume;
        }

        private static double CalculateVelocity(double angVelocity, double radius, double sinAngle, double cosAngle, double rodLength)
        {
            double term1 = radius * sinAngle;
            double term2 = (Math.Pow(radius, 2) * sinAngle * cosAngle) /
                          Math.Sqrt(Math.Pow(rodLength, 2) - Math.Pow(radius * sinAngle, 2));

            return Math.Abs(angVelocity * (term1 + term2));
        }

        public static void DisplayResults(Arguments arguments, ConsoleResults results)
        {
            string units = arguments.IsMetric ? "mm" : "inches";
            string volumeUnits = arguments.IsMetric ? "cc" : "cubic inches";
            string velocityUnits = arguments.IsMetric ? "mm/s" : "inches/s";

            // Convert displacement for display
            double displayDisplacement = results.Displacement;
            if (arguments.IsMetric)
            {
                displayDisplacement = displayDisplacement / 1000; // Convert cubic mm to cc
            }

            Console.WriteLine("\n=== CALCULATION RESULTS ===");
            Console.WriteLine($"Total swept displacement: \t\t{displayDisplacement:F2} {volumeUnits}");
            Console.WriteLine($"Bore to Stroke Ratio: \t\t\t{results.BoreRatio:F3}");
            Console.WriteLine($"Rod Ratio: \t\t\t\t{results.RodRatio:F3}");
            Console.WriteLine($"Piston to deck (including gasket): \t{results.Piston2deck:F3} {units}");
            Console.WriteLine($"  (Negative value indicates 'out of the hole')");
            Console.WriteLine($"Static Compression Ratio: \t\t{results.CompressionRatio:F2}:1");
            Console.WriteLine($"Peak piston velocity: \t\t\t{results.MaxVelocity:F2} {velocityUnits} at {results.MaxVelocityDeg}°");

            // Display cam timing information if included
            if (arguments.IncludeCamProfile && arguments.CamSpec != null)
            {
                Console.WriteLine("\n=== CAM TIMING SUMMARY ===");
                Console.WriteLine($"Intake:  Opens {arguments.CamSpec.IntakeCam.OpeningPoint:F1}°, Closes {arguments.CamSpec.IntakeCam.ClosingPoint:F1}°, Max lift {arguments.CamSpec.IntakeCam.MaxLift * arguments.CamSpec.IntakeCam.RockerRatio:F3} {units}");
                Console.WriteLine($"Exhaust: Opens {arguments.CamSpec.ExhaustCam.OpeningPoint:F1}°, Closes {arguments.CamSpec.ExhaustCam.ClosingPoint:F1}°, Max lift {arguments.CamSpec.ExhaustCam.MaxLift * arguments.CamSpec.ExhaustCam.RockerRatio:F3} {units}");
            }
        }

        public static void SaveResults(string fileLocation, string fileName, List<CSVResults> csvResults)
        {
            try
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

                // Determine if cam data is included
                bool hasCamData = csvResults.Count > 0 && (csvResults[0].IntakeValveLift != 0 || csvResults[0].ExhaustValveLift != 0 ||
                                                         csvResults.Exists(r => r.IntakeValveLift != 0 || r.ExhaustValveLift != 0));

                if (hasCamData)
                {
                    stringBuilder.AppendLine("Angle (degrees ATDC),Position (units),Velocity (units/s),Intake Valve Lift (units),Exhaust Valve Lift (units)");
                }
                else
                {
                    stringBuilder.AppendLine("Angle (degrees ATDC),Position (units),Velocity (units/s)");
                }

                foreach (var result in csvResults)
                {
                    if (hasCamData)
                    {
                        stringBuilder.AppendLine($"{result.Angle},{result.PistonPosition:F6},{result.PistonVelocity:F6},{result.IntakeValveLift:F6},{result.ExhaustValveLift:F6}");
                    }
                    else
                    {
                        stringBuilder.AppendLine($"{result.Angle},{result.PistonPosition:F6},{result.PistonVelocity:F6}");
                    }
                }

                File.WriteAllText(fileName, stringBuilder.ToString());
                Console.WriteLine($"\nResults saved to: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }
    }
}