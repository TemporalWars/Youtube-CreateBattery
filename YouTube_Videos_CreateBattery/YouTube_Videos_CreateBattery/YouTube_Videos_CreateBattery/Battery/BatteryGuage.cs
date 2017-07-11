using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ImageNexus.BenScharbach.YouTube.CreateBattery.Battery
{
    // P2
    /// <summary>
    /// The <see cref="BatteryGuage"/> class.
    /// </summary>
    internal sealed class BatteryGuage
    {
        // vars
        private readonly ProgressBar _progressBar;
        private readonly Battery _battery;

        // BatteryGuage Window
        private readonly Window _window;
        private readonly Dispatcher _dispatcher;

        // Thread Stuff
        private readonly ManualResetEvent _mreStartGuage = new ManualResetEvent(false);
        private readonly ManualResetEvent _mreUpdateGuage = new ManualResetEvent(false);
        private readonly ManualResetEvent _mreUpdateFinishGuage = new ManualResetEvent(false);
        
        #region Constructors

        /// <summary>
        /// ctr
        /// </summary>
        public BatteryGuage(Window window, ProgressBar progressBar, Battery battery)
        {
            if (window == null) throw new ArgumentNullException("window");

            // Set Values
            _progressBar = progressBar;
            _battery = battery;
            _window = window;
            _dispatcher = _window.Dispatcher;

            // subscribe to the battery's energy-down
            battery.EnergyDown += Battery_EnergyDown;
            // subscribe to the battery's recharge
            battery.Recharged += Battery_Recharged;
            // get the batteries energy
            _progressBar.Maximum = battery.Energy;
            _progressBar.Value = battery.Energy;
            
            // Start Motion Thread.
            Task.Factory.StartNew(BatteryGuageAction);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Starts Guage.
        /// </summary>
        internal void StartGuage()
        {
            _mreStartGuage.Set();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The BatteryGuage's thread action.
        /// </summary>
        private void BatteryGuageAction()
        {
            // Wait until user Start Guage.
            while (_mreStartGuage.WaitOne(1) == false)
            {
                Thread.Sleep(10);
            }

            // do the battery guage.
            while (true)
            {
                // Wait until battery energy-down occurs.
                while (_mreUpdateGuage.WaitOne(1) == false)
                {
                    Thread.Sleep(10);
                }
                // reset mreUpdateGuage
                _mreUpdateGuage.Reset();

                // update battery guage
                _progressBar.Dispatcher.Invoke(new Action(DecreaseBatteryGuageAction));

                // notify finish
                _mreUpdateFinishGuage.Set();

                // sleep
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Decreases the battery-guage by one unit.
        /// </summary>
        private void DecreaseBatteryGuageAction()
        {
            _progressBar.Value--;
        }

        #endregion

        #region Event-Handlers Methods

        /// <summary>
        /// Captures the battery's energy-down event.
        /// </summary>
        private void Battery_EnergyDown(object sender, EventArgs e)
        {
            _mreUpdateGuage.Set();
            while (_mreUpdateFinishGuage.WaitOne(1) == false)
            {
                Thread.Sleep(1);
            }
            _mreUpdateFinishGuage.Reset();
        }

        /// <summary>
        /// Captures the battery's recharged event.
        /// </summary>
        private void Battery_Recharged(object sender, EventArgs e)
        {
            _progressBar.Value = _battery.Energy;
        }

        #endregion
    }
}