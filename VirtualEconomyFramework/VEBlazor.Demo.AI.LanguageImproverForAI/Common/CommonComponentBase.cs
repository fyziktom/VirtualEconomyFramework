using Blazorise;
using Microsoft.AspNetCore.Components;
using VEDriversLite.NFT;

namespace VEBlazor.Demo.AI.LanguageImproverForAI.Common
{
    public class AdditionalInfo
    {
        public string OrigUserBaseText { get; set; } = string.Empty;
        public string OrigAIText { get; set; } = string.Empty;
        public string OrigTranslation { get; set; } = string.Empty;
        public string UserTranslation { get; set; } = string.Empty;
        public string UserVariation1 { get; set; } = string.Empty;
        public string UserVariation2 { get; set; } = string.Empty;
    }
    public enum Languages
    {
        cz2es,
        cz2en,
        cz2de,
        cz2nl,
        en2es,
        de2es,
        nl2es,
        en2pap,
        pap2es
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

        public bool Minted { get; set; } = false;
        public string LastMintedTx { get; set; } = string.Empty;

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
