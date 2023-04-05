using Blazored.LocalStorage;

namespace VEBlazor.Demo.AI.LearnLanguageWithAI
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
    }
}
