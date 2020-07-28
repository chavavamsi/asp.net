﻿using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.PublicApi
{
    public class WebFileSystem: IFileSystem
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        public const string AUTH_KEY = "AuthKeyOfDoomThatMustBeAMinimumNumberOfBytes";

        public WebFileSystem(string url)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("auth-key", AUTH_KEY);
            _url = url;
        }

        public async Task<bool> UploadFile(string fileName, byte[] fileData)
        {
            if (!fileData.IsValidImage(fileName))
            {
                return false;
            }

            return await UploadToWeb(fileName, fileData);
        }

        public static async Task<bool> SavePicture(string pictureName, string pictureBase64)
        {
            IFileSystem fileSystem = new WebFileSystem($"{BlazorShared.Authorization.Constants.GetWebUrlInternal(Startup.InDocker)}File");
            if (!await fileSystem.UploadFile(pictureName, Convert.FromBase64String(pictureBase64)))
            {
                return false;
            }

            return true;
        }

        private async Task<bool> UploadToWeb(string fileName, byte[] fileData)
        {
            var request = new FileItemDto
            {
                DataBase64 = Convert.ToBase64String(fileData), 
                FileName = fileName
            };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            using var message = await _httpClient.PostAsync(_url, content);
            if (!message.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }
    }
}
