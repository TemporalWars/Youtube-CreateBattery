using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImageNexus.BenScharbach.YouTube.CreateBattery.Battery
{
    // P2
    /// <summary>
    /// The <see cref="Battery"/> class.
    /// </summary>
    internal sealed class Battery
    {
        // vars
        private readonly Rectangle _connectingRod;
        private const int FullEnergyCharge = 5000;
        private bool _doConnect;
        private bool _connectingRodMoving;

        // Render-Transforms for WPF item.
        private readonly TranslateTransform _itemTranslateTransform = new TranslateTransform(); // ConnectingRod

        // BatteryGuage Window
        private readonly Window _window;
        private readonly Dispatcher _dispatcher;

        #region Events
        
        /// <summary>
        /// Occurs when the battery loses one unit of energy.
        /// </summary>
        internal event EventHandler EnergyDown;

        /// <summary>
        /// Occurs when the battery is recharged.
        /// </summary>
        internal event EventHandler Recharged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current energy for this battery.
        /// </summary>
        internal int Energy { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// ctr
        /// </summary>
        public Battery(Window window, Rectangle connectingRod)
        {
            if (window == null) throw new ArgumentNullException("window");

            // Set Values
            _connectingRod = connectingRod;
            _window = window;
            _dispatcher = _window.Dispatcher;
            
            // Set the TranslateTransform to the connecting-rod
            _connectingRod.RenderTransform = _itemTranslateTransform;

            // set energy
            Energy = FullEnergyCharge;

            // Start Motion Thread.
            Task.Factory.StartNew(ConnectionRodAction);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Disconnects the rod from the device.
        /// </summary>
        internal void DisconnectRod()
        {
            _doConnect = false;
            _connectingRodMoving = true;
        }

        /// <summary>
        /// Connects the rod to the device.
        /// </summary>
        internal void ConnectRod()
        {
            _doConnect = true;
            _connectingRodMoving = true;
        }

        /// <summary>
        /// Recharges the battery back to 100%.
        /// </summary>
        internal void RechargeEnergy()
        {
            // set energy
            Energy = FullEnergyCharge;
            OnRecharged();
        }

        /// <summary>
        /// Reduces the batteries energy by one unit.
        /// </summary>
        internal bool ConsumeEnergy()
        {
            if (_doConnect == false) return false; // if rod is not connected, no energy for you.
            if (Energy <= 0) return false;

            // reduce by one unit
            Energy--;
            OnEnergyDown();

            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Runs the connection rod's animation on task thread.
        /// </summary>
        private void ConnectionRodAction()
        {
           while (true)
           {
               // wait for a connect/disconnect request
               while (_connectingRodMoving == false)
               {
                   Thread.Sleep(500);
               }

               double translateX;

               // check direction of movement request
               if (_doConnect)
               {
                   // first get the translation values for item's translate-transform
                   translateX = (double)_connectingRod.Dispatcher.Invoke(new Func<double>(() => _itemTranslateTransform.Value.OffsetX));

                   // move to the right.
                   if (translateX < 55)
                   {
                       translateX++;
                       // Note: Set the transform x/y directly.
                       var x = translateX;
                       _connectingRod.Dispatcher.Invoke(new Action(() => _itemTranslateTransform.X = x));
                       Thread.Sleep(1);
                   }
                   else
                   {
                       _connectingRodMoving = false;
                   }

                   continue;
               }

               // move to the left.
               // first get the translation values for item's translate-transform
               translateX = (double)_connectingRod.Dispatcher.Invoke(new Func<double>(() => _itemTranslateTransform.Value.OffsetX));

               // move to the right.
               if (translateX > 0)
               {
                   translateX--;
                   // Note: Set the transform x/y directly.
                   var x = translateX;
                   _connectingRod.Dispatcher.Invoke(new Action(() => _itemTranslateTransform.X = x));
                   Thread.Sleep(1);
               }
               else
               {
                   _connectingRodMoving = false;
               }
           }
        }

        #endregion

        #region Private Event Trigger Methods

        /// <summary>
        /// Triggers the EnergyDown event.
        /// </summary>
        private void OnEnergyDown()
        {
            if (EnergyDown != null)
                EnergyDown(this, EventArgs.Empty);
        }

        /// <summary>
        /// Triggers the Recharged event.
        /// </summary>
        private void OnRecharged()
        {
            if (Recharged != null)
                Recharged(this, EventArgs.Empty);
        }

        #endregion


    }
}