using Xamarin.Forms;

namespace MatrixFormatter
{
    public class BorderEntry : Entry
    {
        public static readonly BindableProperty IsBorderVisibleProperty = 
            BindableProperty.Create(
                "IsBorderVisible", 
                typeof(bool), 
                typeof(BorderEntry), 
                true);

        public bool IsBorderVisible
        {
            get => (bool)GetValue(IsBorderVisibleProperty);
            set => SetValue(IsBorderVisibleProperty, value);
        }
    }
}