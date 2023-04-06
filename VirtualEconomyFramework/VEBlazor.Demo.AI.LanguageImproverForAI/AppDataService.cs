using Blazored.LocalStorage;
using System.Xml.Linq;
using VEBlazor.Demo.AI.LanguageImproverForAI.Common;

namespace VEBlazor.Demo.AI.LanguageImproverForAI
{
    public class AppDataService
    {
        protected readonly ILocalStorageService localStorage;

        public AppDataService(ILocalStorageService LocalStorage)
        {
            localStorage = LocalStorage;
        }

        public async Task<string> GetAuthor()
        {
            var author = await localStorage.GetItemAsync<string>("author");
            if (!string.IsNullOrEmpty(author))
                return author;
            else
                return string.Empty;
        }

        public async Task<bool> SaveAuthor(string author)
        {
            try
            {
                await localStorage.SetItemAsStringAsync("author", author);
            }
            catch { }
            return false;
        }

        public void SaveAdditionalInfo(AdditionalInfo info)
        {
            SaveAdditionalInfoAsync(info);
        }
        public async Task<bool> SaveAdditionalInfoAsync(AdditionalInfo info)
        {
            try
            {
                await localStorage.SetItemAsync<AdditionalInfo>("additionalInfo", info);
            }
            catch { }
            return false;
        }
        public async Task<AdditionalInfo> GetAdditionalInfoAsync()
        {
            try
            {
                var info = await localStorage.GetItemAsync<AdditionalInfo>("additionalInfo");
                if (info != null)
                    return info;
            }
            catch { }
            return new AdditionalInfo();
        }

        public void SaveNameAndTags(string name, string tags)
        {
            SaveNameAndTagsAsync(name, tags);
        }
        public async Task<bool> SaveNameAndTagsAsync(string name, string tags)
        {
            try
            {
                await localStorage.SetItemAsync<string>("Name", name);
                await localStorage.SetItemAsync<string>("Tags", tags);
            }
            catch { }
            return false;
        }
        public async Task<(string, string)> GetNameAndTagsAsync()
        {
            try
            {
                var name = await localStorage.GetItemAsync<string>("Name");
                var tags = await localStorage.GetItemAsync<string>("Tags");
                if (name == null)
                    name = string.Empty;
                if (tags == null)
                    tags = string.Empty;
                return (name, tags);

            }
            catch { };
            return (string.Empty, string.Empty);
        }
    }
}
