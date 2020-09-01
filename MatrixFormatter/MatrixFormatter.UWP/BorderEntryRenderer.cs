using System.ComponentModel;
using MatrixFormatter;
using MatrixFormatter.UWP;
using Xamarin.Forms.Platform.UWP;
using Thickness = Windows.UI.Xaml.Thickness;

[assembly: ExportRenderer(typeof(BorderEntry), typeof(BorderEntryRenderer))]
namespace MatrixFormatter.UWP
{
    public class BorderEntryRenderer : EntryRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == BorderEntry.IsBorderVisibleProperty.PropertyName)
            {
                Control.BorderThickness = ((BorderEntry)sender).IsBorderVisible ? new Thickness(1) : new Thickness(0);
            }
        }
    }
}