using Blazorise;
using Microsoft.AspNetCore.Components;
using VEDriversLite.NFT;

namespace VEBlazor.Demo.AI.LearnLanguageWithAI.Common
{
    public enum Languages
    {
        cz2es,
        cz2en,
        cz2de,
        cz2nl,
        en2es,
        de2es,
        nl2es
    }

    public class CommonComponentBase: ComponentBase
    {
        [Inject] 
        public INotificationService? NotificationService { get; set; }

        [Parameter]
        public bool Loading { get; set; } = false;

        [Parameter]
        public Languages Language { get; set; } = Languages.cz2es;

        public async Task LoadingStatus(bool loading)
        {
            Loading = loading;
            await InvokeAsync(StateHasChanged);
        }

        public async Task NotifySuccess(string msg, string title)
        {
            if (NotificationService != null)
                await NotificationService.Success(msg, title);
        }
    }
    public class NFTComponentBase : CommonComponentBase
    {
        [Parameter]
        public INFT NFT { get; set; } = new PostNFT("");
        [Parameter]
        public bool MintingNFT { get; set; } = false;

        public async Task MintingStatus(bool loading)
        {
            MintingNFT = loading;
            await InvokeAsync(StateHasChanged);
        }

        public async Task LoadNFT(string NFTTxId)
        {
            await LoadingStatus(true);

            if (!string.IsNullOrEmpty(NFTTxId))
            {
                var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, NFTTxId, 0, 0, true, true, NFTTypes.Post);
                if (nft != null)
                    NFT = nft;
            }

            await LoadingStatus(false);
        }
    }
}
