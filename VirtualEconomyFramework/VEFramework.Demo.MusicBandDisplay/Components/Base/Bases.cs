using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using VEFramework.Demo.MusicBandDisplay.Models;
using VEFramework.Demo.MusicBandDisplay.Services.NFTs;
using VEFramework.Demo.MusicBandDisplay.Services;

namespace VEFramework.Demo.MusicBandDisplay.Components.Base
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
                post = await NFTFactory.GetNFT(NFTHelpers.TokenId, Utxo, 0, 0, true, allowCache: true);
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
        public INFT NFT { get; set; } = new PostNFT("");

        public string NFTTextMarkuptext => Markdig.Markdown.ToHtml(NFT.Text.Trim(' '), new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

        public string GetName(INFT pnft)
        {
            if (pnft == null && NFT != null)
            {
                if (NFT.Type == NFTTypes.Post)
                    pnft = NFT as PostNFT;
                else if (NFT.Type == NFTTypes.Music)
                    pnft = NFT as MusicNFT;
            }
            if (pnft == null) return string.Empty;

            var name = pnft.Name;
            return name.Length > 50 ? name.Substring(0, 50) + "..." : name;
        }
    }
}
