using System;
using System.Timers;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

// ReSharper disable IdentifierTypo

namespace Raspberryio_HC_SR04
{
    public class Hcsr04 : IDisposable
    {
        #region Public Delegates

        public delegate void DistanceUpdatedEventHandler(Hcsr04 sender);

        #endregion Public Delegates

        #region Public Events

        public event DistanceUpdatedEventHandler DistanceUpdated;

        #endregion Public Events

        #region Public Properties

        public double? Distance { get; set; }

        #endregion Public Properties

        #region Private Properties

        private GpioPin EchoPin { get; set; }
        private uint ProcessStart { get; set; }
        private GpioPin TriggerPin { get; set; }
        private uint UpdateIntervalMs { get; set; }
        private Timer UpdateTimer { get; set; }

        #endregion Private Properties

        #region Private Fields

        private const int MAXIMUM_DISTANCE = 300;
        private const int SOUND_SPEED = 340;
        private const double UPDATE_DISTANCE_THRESHOLD = 0.01;

        #endregion Private Fields

        #region Public Constructors

        public Hcsr04(GpioPin triggerPin, GpioPin echoPin, uint updateIntervalMs = 500)
        {
            TriggerPin = triggerPin;
            EchoPin = echoPin;
            UpdateIntervalMs = updateIntervalMs;

            TriggerPin.PinMode = GpioPinDriveMode.Output;
            EchoPin.PinMode = GpioPinDriveMode.Input;
            Distance = double.MinValue;
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {
            if (UpdateTimer == null) return;
            Distance = null;
            UpdateTimer.Enabled = false;
            UpdateTimer.Stop();
            UpdateTimer.Dispose();
        }

        public void Start()
        {
            if (UpdateTimer == null)
            {
                UpdateTimer = new Timer(UpdateIntervalMs);
                UpdateTimer.Elapsed += TimeElasped;
            }

            Distance = null;
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
            Update();
        }

        public void Stop()
        {
            if (UpdateTimer == null) return;
            Distance = null;
            UpdateTimer.Enabled = false;
            UpdateTimer.Stop();
        }

        #endregion Public Methods

        #region Private Methods

        private bool DistanceIsUpdated(double? distance)
        {
            if (distance == null && Distance == null) return false;
            if (distance == null && Distance != null) return true;
            if (distance != null && Distance == null) return true;
            return distance != null && Distance != null &&
                   Math.Abs(Distance.Value - distance.Value) > UPDATE_DISTANCE_THRESHOLD;
        }

        private bool MeasureIsTimeout()
        {
            return Pi.Timing.MillisecondsSinceSetup > (ProcessStart + UpdateIntervalMs);
        }

        private void TimeElasped(object sender, ElapsedEventArgs args)
        {
            Update();
        }

        private void Update()
        {
            TriggerPin.Write(GpioPinValue.High);
            Pi.Timing.SleepMicroseconds(1);
            TriggerPin.Write(GpioPinValue.Low);

            ProcessStart = Pi.Timing.MillisecondsSinceSetup;

            double? impulsionStart = null;
            double? impulsionEnd = null;
            double? distance = null;

            while (EchoPin.Read() == false && !MeasureIsTimeout())
            {
                impulsionStart = Pi.Timing.MicrosecondsSinceSetup;
            }

            while (EchoPin.Read() && !MeasureIsTimeout())
            {
                impulsionEnd = Pi.Timing.MicrosecondsSinceSetup;
            }

            if (impulsionStart == null || impulsionEnd == null)
            {
                distance = null;
            }
            else
            {
                distance = Math.Round((impulsionEnd.Value - impulsionStart.Value) / 10000 * SOUND_SPEED / 2, 2);
                if (distance > MAXIMUM_DISTANCE) distance = null;
            }

            if (!DistanceIsUpdated(distance)) return;

            Distance = distance;
            DistanceUpdated?.BeginInvoke(this, null, null);
        }

        #endregion Private Methods
    }
}