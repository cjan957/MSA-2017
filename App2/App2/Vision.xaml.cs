using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using Tabs.Model;
using App2.DataModels;
using App2.Model;



namespace App2
{

[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Vision : ContentPage
    {

        MediaFile cameraFile;
        MediaFile galleryFile;
        String cameraFaceID;
        double camereFaceAge;
        byte[] byteData_camera;
        byte[] byteData_gallery;


        public Vision()
        {
            InitializeComponent();
            image.Source = "emptyProfile.jpg";
            imageGallery.Source = "emptyProfile.jpg";
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
            if (cameraFile != null && galleryFile != null)
            {
                DisableButtons();
                await analyseTheFace(cameraFile, galleryFile);
                cameraFile = null;
                galleryFile = null;
            }
            else
            {
                await DisplayAlert("Missing Photos", "Pick a photo from the gallery and take a photo of yourself first!", "Got it");
            }
        }

        private void EnableButtons()
        {
            myButton.IsEnabled = true;
            AnalyseButton.IsEnabled = true;
            CameraButton.IsEnabled = true;
            image.Source = "emptyProfile.jpg";
            imageGallery.Source = "emptyProfile.jpg";
        }

        private void DisableButtons()
        {
            myButton.IsEnabled = false;
            AnalyseButton.IsEnabled = false;
            CameraButton.IsEnabled = false;
            
        }

    private async Task analyseTheFace(MediaFile camerafile, MediaFile galleryfile2)
        {
            const string subscriptionKey = "d3ba5bb7fd3f408897632bb39782b57e";
            const string connectionEndPoint = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";
            
            //First we'll check to make sure that there's only 1 person in the camera file (1 person taking a selfie only)
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,smile";

            string urlToRequest = connectionEndPoint + "?" + requestParameters;

            HttpResponseMessage response;
            HttpResponseMessage response_gallery;
            HttpResponseMessage response_verify;

            byteData_camera = GetImageAsByteArray(camerafile);
            byteData_gallery = GetImageAsByteArray(galleryfile2);

            using (var content = new ByteArrayContent(byteData_camera))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                try
                {
                    response = await client.PostAsync(urlToRequest, content);
                    Debug.WriteLine(response);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        List<EvaluationModel> responseModel = JsonConvert.DeserializeObject<List<EvaluationModel>>(responseString);

                        int faceCount = responseModel.Count;
                        if (faceCount > 1)
                        {
                            await DisplayAlert("Multiple faces detected", "Multiple people detected in your selfie. Please try again ", "OK");
                            EnableButtons();
                        }
                        else if(faceCount == 0)
                        {
                            await DisplayAlert("No faces detected", "No one was found in the photo. Try taking a photo in a better lighting condition", "OK");
                            EnableButtons();
                        }
                        else
                        { 
                            EvaluationModel faceToWorkWith = responseModel[0];
                            cameraFaceID = faceToWorkWith.faceId;
                            camereFaceAge = faceToWorkWith.faceAttributes.age;

                            using (var photoContent = new ByteArrayContent(byteData_gallery))
                            {
                                photoContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                                try
                                {
                                    HttpClient clientSecondImage = new HttpClient();
                                    clientSecondImage.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                                    response_gallery = await clientSecondImage.PostAsync(urlToRequest, photoContent);
                                    if (response_gallery.IsSuccessStatusCode)
                                    {
                                        var response2String = await response_gallery.Content.ReadAsStringAsync();
                                        List<EvaluationModel> responseModel2 = JsonConvert.DeserializeObject<List<EvaluationModel>>(response2String);
                                        int faceCountInGallery = responseModel2.Count;
                                        if (faceCountInGallery == 0)
                                        {
                                            await DisplayAlert("No faces detected", "No one was found in the photo. Try again or use a different photo", "OK");
                                            EnableButtons();
                                        }
                                        else
                                        {
                                            bool matched = false;
                                            for (int i = 0; i < faceCountInGallery; i++) {
                                                const string connectionEndPoint_Verify = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/verify?";
                                                HttpClient client_verify = new HttpClient();
                                                client_verify.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                                                //string requestParametersVerify = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,smile";

                                                string urlToRequestVerify = connectionEndPoint_Verify;
                                                HttpResponseMessage verifyResult;

                                                FaceVerifyModel faceToMatchModel = new FaceVerifyModel()
                                                {
                                                    faceId1 = cameraFaceID,
                                                    faceId2 = responseModel2[i].faceId
                                                };

                                                var json = JsonConvert.SerializeObject(faceToMatchModel);

                                                byte[] byteDataVerify = Encoding.UTF8.GetBytes(json);

                                                using (var contentVerify = new ByteArrayContent(byteDataVerify))
                                                {
                                                    contentVerify.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                                    response_verify = await client.PostAsync(urlToRequestVerify, contentVerify);
                                                    if (response_verify.IsSuccessStatusCode)
                                                    {
                                                        var verifyResponseToString = await response_verify.Content.ReadAsStringAsync();
                                                        var verifyResponseJson = JsonConvert.DeserializeObject<VerifyResponseModel>(verifyResponseToString);
                                                        if(verifyResponseJson.isIdentical == true)
                                                        {
                                                            matched = true;
                                                            FaceInfo model = new FaceInfo()
                                                            {

                                                                FaceId1 = cameraFaceID,
                                                                FaceId2 = responseModel2[i].faceId,
                                                                timeStamp = DateTime.UtcNow,
                                                                Age1 = camereFaceAge,
                                                                Age2 = responseModel2[i].faceAttributes.age
                                                            };
                                                            await AzureManager.AzureManagerInstance.PostFaceAnalysisInformation(model);
                                                            i = faceCountInGallery;
                                                        }
                                                    }
                                                }
                                            }
                                            if(matched == true)
                                            {
                                                await DisplayAlert("Match Found!", "You are in the photo! Go to the History tab to see age comparison.", "OK");
                                                EnableButtons();
                                            }
                                            else
                                            {
                                                await DisplayAlert("No match found.", "Sorry, we could not find a match of the face between the two photos", "OK");
                                                EnableButtons();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Something went wrong", "Process could not be completed", "OK");
                                    }
                                }
                                catch(Exception e)
                                {
                                    String exceptionInfo = e.ToString();
                                }
                            }

                            //Debug.WriteLine(JsonPrettyPrint(responseString));

                            //FaceInfo model = new FaceInfo()
                            //{
                            //    FaceID = faceToWorkWith.faceId,
                            //    Happiness = (float)faceToWorkWith.faceAttributes.smile,
                            //    Gender = faceToWorkWith.faceAttributes.gender
                            //};
                            //await AzureManager.AzureManagerInstance.PostFaceAnalysisInformation(model);
                        }

                    }
                }
                catch (Exception e)
                {
                    string error = e.ToString();
                }

                //Get rid of file once we have finished using it
                camerafile.Dispose();
                galleryfile2.Dispose();
            }

        }

        static byte[] GetImageAsByteArray(MediaFile imageFile)
        {
            var fileStream = imageFile.GetStream();
            BinaryReader binaryReader = new BinaryReader(fileStream);
            byte[] toReturn = binaryReader.ReadBytes((int)fileStream.Length);
            binaryReader.Dispose();
            return toReturn;
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