using System;
using System.Windows;
using System.Windows.Media;
using ImageNexus.BenScharbach.YouTube.CreateBattery.Balls;
using ImageNexus.BenScharbach.YouTube.CreateBattery.Battery;

namespace ImageNexus.BenScharbach.YouTube.CreateBattery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ~maincreatevars
        // Balls
        private readonly Ball _redBall;
        private readonly Ball _blueBall;

        // P2
        // Battery
        private readonly Battery.Battery _battery;
        // BatteryGuage
        private readonly BatteryGuage _batteryGuage;

        // Blower
        private readonly Blower _blower;

        // Randomizer
        private readonly Random _random = new Random();

        #region Constructors

        /// <summary>
        /// Ctrs
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // ~mainctrcode

            // P2
            // Create Battery / BatteryGuage
            _battery = new Battery.Battery(this, ConnectingRod);
            _batteryGuage = new BatteryGuage(this, BatteryGauge, _battery);
            _batteryGuage.StartGuage();

            // P2
            // Moved blower creation to ctr
            _blower = new Blower(_battery, BallsOnOffLight);

            // Spawn a couple of balls!
            _redBall = SpawnBall("RedBall", new Vector(0, 225), Brushes.Red);
            //_blueBall = SpawnBall("BlueBall", new Vector(25, 30), Brushes.Blue);

            // Set some velocity value
            _redBall.SetVelocity(new Vector(0.05f, 0));
        }
       
        #endregion

        /// <summary>
        /// Spawn ball
        /// </summary>
        private Ball SpawnBall(string idName, Vector position, Brush color)
        {
            // ~mainspawnballcode
            var ball = new Ball(this, idName, color);
            ball.SpawnItem(position);
            return ball;
        }

        #region Event Handlers

        /// <summary>
        /// Starts the motion balls
        /// </summary>
        private void BtnStartMotion_Click(object sender, RoutedEventArgs e)
        {
            _redBall.StartMotion();
        }

        /// <summary>
        /// Sets the ball to upward jump velocity.
        /// </summary>
        private void BtnJump_Click(object sender, RoutedEventArgs e)
        {
            // ~mainbtnjumpclickcode
            // add Random movement left/right
            var horizontalEnergy = (_random.NextDouble() * 0.5f) * 0.5f;

            // random direction
            var directionRandomizer = _random.Next(1, 20);
            if (directionRandomizer < 10)
            {
                horizontalEnergy *= -1;
            }

            // Set Velocity
            _redBall.SetVelocity(new Vector(horizontalEnergy, -1.5f));
        }

        /// <summary>
        /// Starts the blower to blow the balls up.
        /// </summary>
        private void BtnBlower_Click(object sender, RoutedEventArgs e)
        {
            // ~mainbtnblowerclickcode

            _blower.StartBlower();
            _blower.AddBall(_redBall);
           
        }

        // P2
        /// <summary>
        /// Button to connect rod to device!
        /// </summary>
        private void BtnConnectBattery_Click(object sender, RoutedEventArgs e)
        {
            _battery.ConnectRod();
        }

        // P2
        /// <summary>
        /// Button to disconnect rod from device!
        /// </summary>
        private void BtnDisconnectBattery_Click(object sender, RoutedEventArgs e)
        {
            _battery.DisconnectRod();
        }

        // P2
        /// <summary>
        /// Button to recharge battery.
        /// </summary>
        private void BtnRecharge_Click(object sender, RoutedEventArgs e)
        {
           _battery.RechargeEnergy();
        }

        #endregion
    }
}
