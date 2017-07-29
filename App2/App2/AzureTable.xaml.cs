using App2.DataModels;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;




namespace App2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AzureTable : ContentPage
    {
        //MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;

        public AzureTable()
        {
            InitializeComponent();
        }

        async void Handle_ClickedAsync(object sender, System.EventArgs e)
        {
            try
            {
                List<FaceInfo> notHotDogInformation = await AzureManager.AzureManagerInstance.GetFaceInformation();
                FaceList.ItemsSource = notHotDogInformation;
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", "An error occured while trying to access the database. Please make sure the internet connection is active.", "OK");
                Debug.WriteLine(ex.ToString());
            }

        }
    }
}