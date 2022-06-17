using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using VEFramework.Demo.PublishingDisplay.Services;
using VEFramework.Demo.PublishingDisplay.Services.NFTs;
using VEFramework.Demo.PublishingDisplay.Services.NFTs.Coruzant;

namespace VEFramework.Demo.PublishingDisplay.Components.Base
{
    public abstract class NFTDetailPage : ComponentBase
    {
        [Inject]
        protected AppData AppData { get; set; }

        [Parameter]
        [SupplyParameterFromQuery(Name = "utxo")]
        public string? Utxo { get; set; }
        [Parameter]
        [SupplyParameterFromQuery(Name = "index")]
        public string? Index { get; set; } = "0";

        public async Task<INFT> SearchOrLoad()
        {
            if (string.IsNullOrEmpty(Index)) Index = "0";

            if (!AppData.NFTsDict.TryGetValue($"{Utxo}:{Index}", out var post))
                post = await NFTFactory.GetNFT(NFTHelpers.CoruzantTokenId, Utxo, 0, 0, true, allowCache: true);
            return post;
        }
        public async Task FirstLoad()
        {
            if (!AppData.Loading && !AppData.LoadedBase)
            {
                AppData.MaxLoaded = 10;
                await AppData.LoadNFTs();
            }
        }
    }
    public abstract class StyledComponent : ComponentBase
    {
        [Parameter]
        public string Style { get; set; } = string.Empty;
        [Parameter]
        public string Class { get; set; } = string.Empty;
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
    }

    public abstract class StyledComponent<T> : ComponentBase
    {
        [Parameter]
        public string Style { get; set; } = string.Empty;
        [Parameter]
        public string Class { get; set; } = string.Empty;
        [Parameter]
        public RenderFragment<T>? ChildContent { get; set; }
    }
    public abstract class BaseNFTComponent: StyledComponent
    {
        [Parameter]
        public INFT NFT { get; set; } = new CoruzantProfileNFT("");

        public string NFTTextMarkuptext => Markdig.Markdown.ToHtml(NFT.Text, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

        public string GetName(INFT pnft)
        {
            if (pnft == null && NFT != null)
            {
                if (NFT.Type == NFTTypes.CoruzantProfile)
                    pnft = NFT as CoruzantProfileNFT;
                else if (NFT.Type == NFTTypes.CoruzantArticle)
                    pnft = NFT as CoruzantArticleNFT;
            }
            if (pnft == null) return string.Empty;

            if (pnft.Type == NFTTypes.CoruzantProfile)
            {
                var name = pnft.Name + " " + (pnft as CoruzantProfileNFT).Surname;
                return name.Length > 50 ? name.Substring(0, 50) + "..." : name;
            }
            else if (pnft.Type == NFTTypes.CoruzantArticle)
            {
                var name = pnft.Name;
                return name.Length > 50 ? name.Substring(0, 50) + "..." : name;
            }

            return string.Empty;
        }
    }

    public class CoruzantProfileComponentBase : BaseNFTComponent
    {
        [Inject]
        protected HttpClient _client { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        private string buzzsproudLink = string.Empty;
        public CoruzantProfileNFT CurrentPodcast { get; set; }
        public void ClosePlayer() => CurrentPodcast = null;

        public async Task LoadBuzzproudPodcastLink(CoruzantProfileNFT pnft)
        {
            CurrentPodcast = pnft;
            try
            {
                if (!string.IsNullOrEmpty(pnft.PodcastId))
                {
                    var filename = string.Empty;
                    var req = new HttpRequestMessage(HttpMethod.Get, $"https://nftticketverifierapp.azurewebsites.net/api/GetBuzzsproutData/866092/{pnft.PodcastId}");
                    req.Headers.Add("Accept", "application/json");
                    req.Headers.Add("User-Agent", "VENFT-App");

                    var resp = await _client.SendAsync(req);
                    var respmsg = await resp.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(respmsg))
                        return;
                    var podcastData = JsonConvert.DeserializeObject<List<BuzzsproutEpisodeDto>>(respmsg);
                    if (podcastData != null && podcastData.Count > 0)
                    {
                        var pddto = podcastData.FirstOrDefault();
                        if (!string.IsNullOrEmpty(pddto.audio_url))
                        {
                            filename = pddto.audio_url.Replace("https://www.buzzsprout.com/866092/", string.Empty).Replace(".mp3", string.Empty);
                            var link = $"https://www.buzzsprout.com/866092/{filename}.js?container_id=buzzsprout-player-{pnft.PodcastId}&player=small";
                            buzzsproudLink = link;
                            await InvokeAsync(StateHasChanged);
                            await Task.Delay(200);
                            await JSRuntime.InvokeVoidAsync("jsFunctions.buzzsproutPodcast", buzzsproudLink);
                            await Task.Delay(200);
                            //await JSRuntime.InvokeVoidAsync("setCoruzantPodcastInfo", pnft.Name + " " + pnft.Surname, pddto.artist, pddto.title);
                            await InvokeAsync(StateHasChanged);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load Buzzsprout podcast." + ex.Message);
            }
        }
    }
}
