using App2.DataModels;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;

        private IMobileServiceTable<FaceInfo> faceInfoTable;


        private AzureManager()
        {
            this.client = new MobileServiceClient("http://theface.azurewebsites.net/");
            this.faceInfoTable = this.client.GetTable<FaceInfo>();

        }

        public async Task<List<FaceInfo>> GetFaceInformation()
        {
            return await this.faceInfoTable.ToListAsync();
        }

        public async Task PostFaceAnalysisInformation(FaceInfo faceInfo)
        {
            await this.faceInfoTable.InsertAsync(faceInfo);
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }
    }
}
