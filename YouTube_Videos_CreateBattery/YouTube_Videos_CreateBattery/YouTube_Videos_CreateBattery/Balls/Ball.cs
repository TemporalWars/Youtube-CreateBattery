using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImageNexus.BenScharbach.YouTube.CreateBattery.Balls
{
    // 6/6/2017
    /// <summary>
    /// The <see cref="Ball"/>
    /// </summary>
    internal sealed class Ball
    {
        private Ellipse _ball;
        private readonly string _idName;

        // Balls Movement Values
        private Vector _velocity;
        private Vector _position;
        private const double Friction = 0.999f; 
        private const double Gravity = 0.0075f;
        private const double GroundLevel = 272;

        // Render-Transforms for WPF item.
        private readonly TranslateTransform _itemTranslateTransform = new TranslateTransform();

        // Balls Visual Values
        private readonly Brush _color = Brushes.CadetBlue;
        private readonly Brush _stroke = Brushes.DarkBlue;
        private const double BallWidth = 25;
        private const double BallHeight = 25;

        // Balls Window
        private readonly Window _window;
        private readonly Dispatcher _dispatcher;

        // Thread Stuff
        private readonly ManualResetEvent _mreStartMotion = new ManualResetEvent(false);

        #region Constructors

        /// <summary>
        /// Ctrs
        /// </summary>
        /// <param name="window"></param>
        /// <param name="idName"></param>
        /// <param name="color"></param>
        internal Ball(Window window, string idName, Brush color)
        {
            if (window == null) throw new ArgumentNullException("window");

            // Set Values
            _window = window;
            _dispatcher = _window.Dispatcher;
            _idName = idName;
            _color = color;

            // Start Motion Thread.
            Task.Factory.StartNew(MotionAction);
        }

        #endregion

        /// <summary>
        /// Spawn ball at position.
        /// </summary>
        /// <param name="position"></param>
        internal void SpawnItem(Vector position)
        {
            _itemTranslateTransform.X = position.X;
            _itemTranslateTransform.Y = position.Y;

            // create the Ball
            _ball = new Ellipse
            {
                Width = BallWidth,
                Height = BallHeight,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0),
                Name = "Elp" + _idName,
                Fill = _color,
                Stroke = _stroke,
                RenderTransform = _itemTranslateTransform
            };

            // add to window screen uising the 'Content'
            // ref: https://social.msdn.microsoft.com/Forums/vstudio/en-US/98cc1596-0fe7-42b1-b796-dec075ce0b84/programmatically-add-a-wpf-usercontrol-to-a-wpf-window?forum=wpf
            ((Grid)_window.Content).Children.Add(_ball);
        }

        /// <summary>
        /// Sets ball's velocity
        /// </summary>
        /// <param name="velocity"></param>
        internal void SetVelocity(Vector velocity)
        {
            _velocity = velocity;
        }

        /// <summary>
        /// Starts motion.
        /// </summary>
        internal void StartMotion()
        {
            _mreStartMotion.Set();
        }

        /// <summary>
        /// The Ball's motion thread action.
        /// </summary>
        private void MotionAction()
        {
            // todo-idea: Inject velocity at any time to affect ball
            // todo-idea: Later, maybe add stopwatch time to multiply against values.

            // Wait until user Start Motion.
            while (_mreStartMotion.WaitOne(1) == false)
            {
                Thread.Sleep(10);
            }
           
            // Start Motion loop.
            while (true)
            {
                // first get the translation values for item's translate-transform
                var translateX = (double)_ball.Dispatcher.Invoke(new Func<double>(() => _itemTranslateTransform.Value.OffsetX));
                var translateY = (double)_ball.Dispatcher.Invoke(new Func<double>(() => _itemTranslateTransform.Value.OffsetY));
                //Console.WriteLine("Ball Pos:({0},{1})", translateX, translateY);
                
                // Get velocity
                var velocityX = _velocity.X;
                var velocityY = _velocity.Y;

                // reduce velocity by something... like friction/gravity
                // set gravity
                velocityY += Gravity;
                _velocity.Y = velocityY;
                // set friction on ground
                if (translateY >= GroundLevel)
                {
                    velocityX *= Friction;
                    _velocity.X = velocityX;
                }

                // Update transform
                translateX += velocityX;
                translateY += velocityY;

                // check if below ground level
                if (translateY > GroundLevel) translateY = GroundLevel;

                // left wall
                if (translateX < 0)
                {
                    translateX = 0;
                    _velocity.X *= -1;
                }
                // right wall
                if (translateX > 125)
                {
                    translateX = 125;
                    _velocity.X *= -1;
                }
                // top wall
                if (translateY < 8)
                {
                    translateY = 8;
                    _velocity.Y *= -1;
                }
                
                // Note: Set the transform x/y directly.
                var x = translateX;
                var y = translateY;
                _ball.Dispatcher.Invoke(new Action(() =>
                {
                    //_ball.Margin = new Thickness(vector.X, vector.Y, 0, 0);
                    _itemTranslateTransform.X = x;
                    _itemTranslateTransform.Y = y;
                }));
                
                // sleep
                Thread.Sleep(1);
            }
        }
    }
}