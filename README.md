# Raspberryio HC-SR04 [![.NET](https://github.com/valcriss/raspberryio-hc-sr04/actions/workflows/dotnet.yml/badge.svg)](https://github.com/valcriss/raspberryio-hc-sr04/actions/workflows/dotnet.yml)
Using the [raspberryio](https://github.com/unosquare/raspberryio "raspberryio") library to interract with the [hc-sr04](https://cdn.sparkfun.com/datasheets/Sensors/Proximity/HCSR04.pdf "hc-sr04") ultrasonic ranging detector sensor module.

This example is based on this very good [article](https://raspberry-lab.fr/Composants/Mesure-de-distance-avec-HC-SR04-Raspberry-Francais/) 

## Prerequisites
- A Raspberry pi
- A HC-SR04 module
- A 1KΩ resistor and A 2KΩ resistor
- Wires

## Hardware installation

![Installation Schema](https://raspberry-lab.fr/Composants/Mesure-de-distance-avec-HC-SR04-Raspberry-Francais/Images/Schema-Branchement-Raspberry-Model.3-HC-SR04.png "Installation Schema")

## Using the hcsr04 class
```C#
private static bool Running { get; set; }

private static void Hcsr04DistanceUpdated(Hcsr04 sender)
{
    Console.WriteLine("Distance : " + (sender.Distance != null ? sender.Distance + "cm" : "No obstacle detected or too close ( distance < 2cm or distance >3m )"));
}

private static void Main(string[] args)
{
    Console.CancelKeyPress += Console_CancelKeyPress;
    Running = true;

    Console.WriteLine("Raspberryio - HC-SR04");
    // Initializing the Hcsr04
    Hcsr04 hcsr04 = new Hcsr04(Pi.Gpio[P1.Gpio23], Pi.Gpio[P1.Gpio24], 100);
    // Registering Hcsr04 distance updated event listener
    hcsr04.DistanceUpdated += Hcsr04DistanceUpdated;
    // Starting to listen for distance update
    hcsr04.Start();

    Console.WriteLine("Program will read the distance from HC-SR04 until you cancel it by CTRL+C");
    while (Running)
    {
        Thread.Sleep(100);
    }

    // Stopping
    hcsr04.Stop();
    hcsr04.Dispose();
}

private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    Running = false;
    Console.WriteLine("Shutting down...");
}
```

At any time, the current detected distance is available by the Distance property of the Hcsr04 object.

```C#
double? currentDistance = hcsr04.Distance;
```