using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;

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

            await analyseTheFace(file);

            file.Dispose();
        }

        private async Task analyseTheFace(MediaFile file)
        {
            const string subscriptionKey = "d3ba5bb7fd3f408897632bb39782b57e";
            const string connectionEndPoint = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=true&returnFaceAttributes=age,gender,headPose,facialHair,glasses,hair,makeup,occlusion";

            string urlToRequest = connectionEndPoint + "?" + requestParameters;

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                try
                {
                    response = await client.PostAsync(urlToRequest, content);
                    Debug.WriteLine(response);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(responseString);
                    }
                }
                catch(Exception e)
                {
                    string error = e.ToString();
                }

                

                //Get rid of file once we have finished using it
                file.Dispose();
            }

        }

        static byte[] GetImageAsByteArray(MediaFile imageFile)
        {
            var fileStream = imageFile.GetStream();
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }


    }


        /*
private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
{
label.Text = String.Format("Value is {0:F2}", e.NewValue);
}
*/
    }
