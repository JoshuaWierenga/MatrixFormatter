using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using MatrixFormatter.UWP;

//From https://stackoverflow.com/a/50875216
[assembly: Xamarin.Forms.Dependency(typeof(MessageUWP))]
namespace MatrixFormatter.UWP
{
    public class MessageUWP : IMessageToast
    {
        public void DisplayToast(string message)
        {
            TimeSpan duration = new TimeSpan(0, 0, 2);
            SolidColorBrush colour = new SolidColorBrush(Windows.UI.Colors.Blue);

            TextBlock label = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Windows.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Style style = new Style { TargetType = typeof(FlyoutPresenter) };
            style.Setters.Add(new Setter(Control.BackgroundProperty, colour));
            style.Setters.Add(new Setter(FrameworkElement.MaxHeightProperty, 1));
            
            Flyout flyout = new Flyout
            {
                Content = label,
                Placement = FlyoutPlacementMode.Full,
                FlyoutPresenterStyle = style
            };
            flyout.ShowAt(Window.Current.Content as FrameworkElement);

            DispatcherTimer timer = new DispatcherTimer { Interval = duration };
            timer.Tick += (sender, e) => {
                timer.Stop();
                flyout.Hide();
            };
            timer.Start();
        }
    }
}