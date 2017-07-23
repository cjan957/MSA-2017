using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GreetPage : ContentPage
    {
        public GreetPage()
        {
            InitializeComponent();

            //slider.Value = 0.5;

            var x = new OnPlatform<Thickness>
            {
                Android = new Thickness(0, 50, 0, 0),
                iOS = new Thickness(0, 20, 0, 0)
            };
            Padding = x;
        }

        async void testButton_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            await DisplayAlert("Clicked!",
                "The button labeled'" + button.Text + "' has been clicked",
                "OK");
            await Navigation.PushModalAsync(new Grid1());
       
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No camera", ":( No cam ava", "OK");
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                Directory = "sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });

            file.Dispose();
        }


        /*
private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
{
label.Text = String.Format("Value is {0:F2}", e.NewValue);
}
*/
    }
}