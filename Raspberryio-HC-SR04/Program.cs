using System;
using System.Threading;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Raspberryio_HC_SR04
{
    internal class Program
    {
        #region Private Properties

        private static bool Running { get; set; }

        #endregion Private Properties

        #region Private Methods

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Running = false;
            Console.WriteLine("Shutting down...");
        }

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

        #endregion Private Methods
    }
}