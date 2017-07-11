using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageNexus.BenScharbach.YouTube.CreateBattery.Balls
{
    /// <summary>
    /// The <see cref="Blower"/> class lows puffs of air to make the <see cref="Ball"/> rise.
    /// </summary>
    internal sealed class Blower
    {
        // ~blowercreatevars
        // Collection of Balls.
        private readonly List<Ball> _balls = new List<Ball>();

        // p2
        // battery & DeviceLight
        private readonly Battery.Battery _battery;
        private readonly Rectangle _deviceLight;

        // Randomizer
        private readonly Random _random = new Random();
        // Stopwatch
        private readonly Stopwatch _stopwatch = new Stopwatch();

        // Air Generator Items
        private Vector _air;
        private const double PuffDelayTime = 1500;

        // Thread Stuff
        private readonly ManualResetEvent _mreStartBlower = new ManualResetEvent(false);
       

        #region Constructors

        // P2 - Add Battery & DeviceLight params
        /// <summary>
        /// Ctr
        /// </summary>
        /// <param name="battery"></param>
        /// <param name="deviceLight"></param>
        internal Blower(Battery.Battery battery, Rectangle deviceLight)
        {
            // set values
            _battery = battery;
            _deviceLight = deviceLight;

            // Start Motion Thread.
            Task.Factory.StartNew(BlowAirGeneratorAction);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Add ball to affect.
        /// </summary>
        /// <param name="ball"></param>
        internal void AddBall(Ball ball)
        {
            // ~bloweraddballcode
            if (ball == null) throw new ArgumentNullException("ball");
            _balls.Add(ball);
        }

        /// <summary>
        /// Blows a puff of air.
        /// </summary>
        internal void BlowAir()
        {
            // ~blowerblowaircode
            // add Random movement left/right
            var horizontalEnergy = (_random.NextDouble() * 0.5f) * 0.5f;

            // random direction
            var directionRandomizer = _random.Next(1, 20);
            if (directionRandomizer < 10)
            {
                horizontalEnergy *= -1;
            }

            // Set Air Puff Velocity
            _air = new Vector(horizontalEnergy, -1.5f);
        }

        /// <summary>
        /// Starts Blower.
        /// </summary>
        internal void StartBlower()
        {
            _mreStartBlower.Set();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Starts the thread action to randomly trigger puffs of air.
        /// </summary>
        private void BlowAirGeneratorAction()
        {
            // ~blowergenactioncode
            // Wait until user Start Motion.
            while (_mreStartBlower.WaitOne(1) == false)
            {
                Thread.Sleep(10);
            }

            // Main Air-Generator Engine
            _stopwatch.Start();
            while (true)
            {
                // p2
                // check if battery has energy
                while (_battery.ConsumeEnergy() == false)
                {
                    // update deviceLight to OFF color.
                    _deviceLight.Dispatcher.Invoke(new Action(() => _deviceLight.Fill = Brushes.LightGray));

                    // since no energy, let's just wait right here and check again later.
                    Thread.Sleep(500);
                }

                // update deviceLight to On color. (Not most optimal, since changing every cycle... but good for now)
                _deviceLight.Dispatcher.BeginInvoke(new Action(() => _deviceLight.Fill = Brushes.LimeGreen));

                // Update balls
                var elapedMill = _stopwatch.ElapsedMilliseconds;
                if (elapedMill > PuffDelayTime)
                {
                    _stopwatch.Restart();
                    var count = _balls.Count;
                    for (var index = 0; index < count; index++)
                    {
                        var ball = _balls[index];
                        if (ball == null) continue;

                        BlowAir(); // create random air vector
                        ball.SetVelocity(_air);
                    }
                }

                Thread.Sleep(1);
            }
        }

        #endregion

    }
}