using static PistonMotion.CamProfile;

namespace PistonMotion
{
    public class Arguments
    {
        public string FileLocation { get; set; }
        public string Filename { get; set; }
        public double Stroke { get; set; }
        public double Bore { get; set; }
        public double RodLength { get; set; }
        public double DeckHeight { get; set; }
        public double CompHeight { get; set; }
        public double GasketHeight { get; set; }
        public int RPM { get; set; }

<<<<<<< Updated upstream

=======
        // Cam profile related properties
        public bool IncludeCamProfile { get; set; } = false;
        public CamSpecification CamSpec { get; set; }

        /// <summary>
        /// Creates a copy of the current Arguments object
        /// </summary>
        public Arguments Clone()
        {
            var clone = new Arguments
            {
                FileLocation = this.FileLocation,
                Filename = this.Filename,
                IsMetric = this.IsMetric,
                Stroke = this.Stroke,
                Bore = this.Bore,
                RodLength = this.RodLength,
                DeckHeight = this.DeckHeight,
                CompHeight = this.CompHeight,
                PistonVolume = this.PistonVolume,
                ChamberVolume = this.ChamberVolume,
                GasketHeight = this.GasketHeight,
                RPM = this.RPM,
                CylinderCount = this.CylinderCount,
                IncludeCamProfile = this.IncludeCamProfile
            };

            // Deep copy the cam specification if it exists
            if (this.CamSpec != null)
            {
                clone.CamSpec = new CamSpecification
                {
                    IntakeCam = new CamProfile
                    {
                        MaxLift = this.CamSpec.IntakeCam.MaxLift,
                        DurationAtLift = this.CamSpec.IntakeCam.DurationAtLift,
                        CheckingLift = this.CamSpec.IntakeCam.CheckingLift,
                        LobeCenterline = this.CamSpec.IntakeCam.LobeCenterline,
                        RockerRatio = this.CamSpec.IntakeCam.RockerRatio,
                        ValveDiameter = this.CamSpec.IntakeCam.ValveDiameter,
                        ValveAngle = this.CamSpec.IntakeCam.ValveAngle,
                        PocketDepth = this.CamSpec.IntakeCam.PocketDepth
                    },
                    ExhaustCam = new CamProfile
                    {
                        MaxLift = this.CamSpec.ExhaustCam.MaxLift,
                        DurationAtLift = this.CamSpec.ExhaustCam.DurationAtLift,
                        CheckingLift = this.CamSpec.ExhaustCam.CheckingLift,
                        LobeCenterline = this.CamSpec.ExhaustCam.LobeCenterline,
                        RockerRatio = this.CamSpec.ExhaustCam.RockerRatio,
                        ValveDiameter = this.CamSpec.ExhaustCam.ValveDiameter,
                        ValveAngle = this.CamSpec.ExhaustCam.ValveAngle,
                        PocketDepth = this.CamSpec.ExhaustCam.PocketDepth
                    }
                };
                clone.CamSpec.Initialize();
            }

            return clone;
        }

        /// <summary>
        /// Returns a string representation of the engine configuration
        /// </summary>
        public override string ToString()
        {
            string units = IsMetric ? "mm" : "in";
            string volumeUnits = IsMetric ? "cc" : "ci";

            string baseInfo = $"Engine: {Bore:F1}x{Stroke:F1}{units}, {CylinderCount} cyl, {RPM} RPM, Rod: {RodLength:F1}{units}";

            if (IncludeCamProfile && CamSpec != null)
            {
                baseInfo += $"\nCam: I={CamSpec.IntakeCam.MaxLift:F3}{units}@{CamSpec.IntakeCam.DurationAtLift:F0}°, E={CamSpec.ExhaustCam.MaxLift:F3}{units}@{CamSpec.ExhaustCam.DurationAtLift:F0}°";
            }

            return baseInfo;
        }
>>>>>>> Stashed changes
    }
}
