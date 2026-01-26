using System;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Tetracosm_Remembrance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Timer TargettingTimer = new Timer();
        static Timer MomentumAdjustmentTimer = new Timer();
        static Random Rand = new Random();
        static double[] Momentum = { 0, 0 };
        static double[] Target = { 0, 0 };
        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        public MainWindow()
        {
            InitializeComponent();
            Height = SystemParameters.VirtualScreenHeight-1;
            Width = SystemParameters.VirtualScreenWidth;
            MainObject.RenderTransformOrigin = new Point(0.5,0.5);
            TargettingTimer.AutoReset = true;
            TargettingTimer.Interval = 3500;
            TargettingTimer.Elapsed += New_Target;
            TargettingTimer.Start();
            MomentumAdjustmentTimer.AutoReset = true;
            MomentumAdjustmentTimer.Interval = 160;
            MomentumAdjustmentTimer.Elapsed += AdjustMomentum;
            MomentumAdjustmentTimer.Start();

        }

        private void AdjustMomentum(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                Momentum[0] = Clamp(Momentum[0] + (Target[0] - MainObject.Margin.Left) / 180, -60, 60);
                Momentum[1] = Clamp(Momentum[1] + (Target[1] - MainObject.Margin.Top) / 90, -60, 60);
                if (MainObject.Margin.Left + Momentum[0] < 0 || MainObject.Margin.Left + Momentum[0] > Width - 10)
                {
                    Momentum[0] *= 0.4;
                }
                if (MainObject.Margin.Top + Momentum[1] < 0 || MainObject.Margin.Top + Momentum[1] > Height - 10)
                {
                    Momentum[1] *= 0.4;
                }
                if (Momentum[0] != Math.Abs(Momentum[0]))
                {
                    MainObject.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                }
                else
                {
                    MainObject.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                }
                ThicknessAnimation anim = new ThicknessAnimation(
                        new Thickness(MainObject.Margin.Left, MainObject.Margin.Top, 0, 0),
                        new Thickness(MainObject.Margin.Left + Momentum[0], MainObject.Margin.Top + Momentum[1], 0, 0),
                        TimeSpan.FromMilliseconds(300));
                MainObject.BeginAnimation(Rectangle.MarginProperty, anim);
                MomentumXDisplay.Content = "XMomentum: " + Momentum[0].ToString();
                MomentumYDisplay.Content = "YMomentum: " + Momentum[1].ToString();
                XPosition.Content = "LeftMargin: " + MainObject.Margin.Left.ToString();
                YPosition.Content = "TopMargin: " + MainObject.Margin.Top.ToString();
                ScreenWidth.Content = "ScreenWidth: " + Width.ToString();
                ScreenHeight.Content = "ScreenHeight: " + Height.ToString();
            });
        }

        private void New_Target(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                Target[0] = Rand.Next(((int)Width));
                Target[1] = Rand.Next(((int)Height));
                TargetX.Content = "TargetX: " + Target[0].ToString();
                TargetY.Content = "TargetY: " + Target[1].ToString();
            });
        }
    }
}
