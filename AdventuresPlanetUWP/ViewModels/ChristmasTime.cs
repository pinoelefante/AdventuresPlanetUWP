using AdventuresPlanetUWP.Classes;
using AdventuresPlanetUWP.Converters;
using AdventuresPlanetUWP.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace AdventuresPlanetUWP.ViewModels
{
    class ChristmasTime
    {
        private static ChristmasVisibility ChristmasChecker = new ChristmasVisibility(); 
        public static DispatcherTimer LetItSnow(Grid layout)
        {
            if (Settings.Instance.LetItSnow)
            {
                DispatcherTimer snowflakes = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
                snowflakes.Tick += (s, arg) => Snow(layout);
                snowflakes.Start();
                return snowflakes;
            }
            return null;
        }
        private static Random _Random = new Random((int)DateTime.Now.Ticks);
        private static void Snow(Grid LayoutRoot)
        {
            var x = _Random.Next(-100, (int)LayoutRoot.ActualWidth - 100);
            var y = -100;
            var s = _Random.Next(1, 5) * .1;
            var r = _Random.Next(0, 270);
            var flake = new Snowflake
            {
                RenderTransform = new CompositeTransform
                {
                    TranslateX = x,
                    TranslateY = y,
                    ScaleX = s,
                    ScaleY = s,
                    Rotation = r,
                },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            LayoutRoot.Children.Add(flake);

            var d = TimeSpan.FromSeconds(_Random.Next(4, 20));

            x += _Random.Next(100, 500);
            var ax = new DoubleAnimation { To = x, Duration = d };
            Storyboard.SetTarget(ax, flake.RenderTransform);
            Storyboard.SetTargetProperty(ax, "TranslateX");

            y += (int)(LayoutRoot.ActualHeight);
            var ay = new DoubleAnimation { To = y, Duration = d };
            Storyboard.SetTarget(ay, flake.RenderTransform);
            Storyboard.SetTargetProperty(ay, "TranslateY");

            r += _Random.Next(90, 360);
            var ar = new DoubleAnimation { To = r, Duration = d };
            Storyboard.SetTarget(ar, flake.RenderTransform);
            Storyboard.SetTargetProperty(ar, "Rotation");

            var story = new Storyboard();
            story.Completed += (sender, e) => LayoutRoot.Children.Remove(flake);
            story.Children.Add(ax);
            story.Children.Add(ay);
            story.Children.Add(ar);
            story.Begin();
        }
        public static bool IsChristmasTime()
        {
            Visibility v = (Visibility)ChristmasChecker.Convert(null, null, null, null);
            return v == Visibility.Visible;
        }
        public static bool IsChristmas()
        {
            DateTime now = DateTime.Now;
            if (now.Month == 12 && now.Day == 25)
                return true; 
            return false;
        }
        public static bool IsCapodanno()
        {
            DateTime now = DateTime.Now;
            if (now.Month == 1 && now.Day == 1)
                return true;
            return false;
        }
        public static bool IsEpifania()
        {
            DateTime now = DateTime.Now;
            if (now.Month == 1 && now.Day == 6)
                return true;
            return false;
        }
    }
}
