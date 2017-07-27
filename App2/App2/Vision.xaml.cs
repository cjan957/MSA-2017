using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using Tabs.Model;
using App2.DataModels;

namespace App2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Vision : ContentPage
    {

        MediaFile cameraFile;
        MediaFile galleryFile;

        public Vision()
        {
            InitializeComponent();
        }

        async void testButton_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            await DisplayAlert("Clicked!",
                "The button labeled'" + button.Text + "' has been clicked",
                "OK");
            await Navigation.PushModalAsync(new Grid1());

        }

        private async void browsePhoto(object sender, EventArgs e)
        {
            if (CrossMedia.Current.IsPickPhotoSupported)
            {
                galleryFile = await CrossMedia.Current.PickPhotoAsync();
                if (galleryFile == null)
                    return;

                imageGallery.Source = ImageSource.FromStream(() =>
                {
                    return galleryFile.GetStream();                  
                });
            }
            else
            {
                await DisplayAlert("Error", "Cannot launch photo picker. Check if the permission is correctly set", "OK");
            }
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No camera", "No camera is available", "OK");
            }

            cameraFile = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                Directory = "sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (cameraFile == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return cameraFile.GetStream();
            });

            //await analyseTheFace(cameraFile);

            //cameraFile.Dispose();
        }

        private async void analyse(object sender, EventArgs e)
        {
            if (cameraFile != null && cameraFile != null)
            {
                await analyseTheFace(cameraFile, galleryFile);
                cameraFile.Dispose();
                galleryFile.Dispose();
            }
            else
            {
                await DisplayAlert("Alert", "Take photos first!", "OK");
            }
        }

        private async Task analyseTheFace(MediaFile file, MediaFile file2)
        {
            const string subscriptionKey = "d3ba5bb7fd3f408897632bb39782b57e";
            const string connectionEndPoint = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,smile";

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

                        String responseStringCleaned = responseString.Replace("[0:]", "");
                        String responseStringCleaned2 = responseStringCleaned.Replace("[", "");
                        String responseStringCleaned3 = responseStringCleaned2.Replace("]", "");

                        Debug.WriteLine(JsonPrettyPrint(responseStringCleaned3));

                        List<EvaluationModel> responseModel = JsonConvert.DeserializeObject<List<EvaluationModel>>(responseString);

                        int faceCount = responseModel.Count;
                        for (int i = 0; i < faceCount; i++)
                        {
                            Debug.WriteLine("Face Count = " + i);

                        }

                        EvaluationModel faceToWorkWith = responseModel[0];

                        string faceID = faceToWorkWith.faceId;

                        Debug.WriteLine(JsonPrettyPrint(responseString));

                        FaceInfo model = new FaceInfo()
                        {
                            FaceID = faceToWorkWith.faceId,
                            Happiness = (float)faceToWorkWith.faceAttributes.smile,
                            Gender = faceToWorkWith.faceAttributes.gender
                        };

                        await AzureManager.AzureManagerInstance.PostFaceAnalysisInformation(model);


                    }
                }
                catch (Exception e)
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


        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}