 ```
 _____________       _____                   ______  ___     __________              
 ___  __ \\__(_)________  /_____________     ___   |/  /_______  /___(_)____________ 
 __  /_/ /_  /__  ___/  __/  __ \\_  __ \\   __  /|_/ /_  __ \\  __/_  /_  __ \\_  __ \\
 _  ____/_  / _(__  )/ /_ / /_/ /  / / /     _  /  / / / /_/ / /_ _  / / /_/ /  / / /
 /_/     /_/  /____/ \\__/ \\____//_/ /_/    /_/  /_/  \\____/\\__/ /_/  \\____//_/ /_/ 
 ```

This script calculates the velocity of an internal combustion engine's piston(s) during it's cycle.

The output of this script can be useful for:
* Analyzing the affect of stroke, rod ratio, and max rpm on peak piston velocity and angle
* Engine Management Tuning
* Comparing different engines geometries
* Making informed decisions about operating RPM as related to longevity
* Arguing with people on car forums


Console Example:

![Console Example](https://github.com/TomBarstow/Piston-Motion-Calc/blob/main/example.png?raw=true)

CSV Output Example Visualized as Graph:

![Output Example](https://github.com/TomBarstow/Piston-Motion-Calc/blob/main/exampleGraph.png?raw=true)


Future development:
* Piston to valve (p2v) modeling
* Static and dynamic compression calculation
* Piston acceleration
* Wrist pin offset (Left out right now because it seems to have a negligible effect on piston velocity)
* GUI
