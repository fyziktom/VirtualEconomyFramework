using Microsoft.AspNetCore.Components;
using VEDriversLite.NFT;

namespace VEBlazor.Components.Base
{
    public class NFTSentResultDto
    {
        public bool sucess { get; set; } = false;
        public string message { get; set; } = string.Empty;
        public INFT NFT { get; set; } = new ImageNFT("");
    }

    public abstract class AccountRelatedComponentBase : ComponentBase
    {
        [Parameter]
        public string Address { get; set; } = string.Empty;
        [Parameter]
        public EventCallback<string> AddressChanged { get; set; }
        [Parameter]
        public bool IsSubAccount { get; set; } = false;
        [Parameter]
        public EventCallback<bool> IsSubAccountChanged { get; set; }
        [Parameter]
        public bool IsOwnNFT { get; set; } = false;
        [Parameter]
        public EventCallback<bool> IsOwnNFTChanged { get; set; }
    }

    public abstract class NFTComponentBase : AccountRelatedComponentBase
    {
        [Parameter]
        public INFT NFT { get; set; } = new ImageNFT("");
        [Parameter]
        public EventCallback<INFT> NFTChanged { get; set; }
        [Parameter]
        public string Utxo { get; set; } = string.Empty;
        [Parameter]
        public EventCallback<string> UtxoChanged { get; set; }
        [Parameter]
        public int UtxoIndex { get; set; } = 0;
        [Parameter]
        public EventCallback<int> UtxoIndexChanged { get; set; }

        [Parameter]
        public EventCallback<NFTSentResultDto> NFTSent { get; set; }

        [Parameter]
        public EventCallback<List<INFT>> OpenNFTsInWorkTab { get; set; }

        public string GetImageUrl(bool returnPreviewIfExists = false)
        {
            if (NFT == null)
                return string.Empty;

            if (returnPreviewIfExists)
            {
                var preview = GetPreviewUrl();
                if (!string.IsNullOrEmpty(preview))
                    return preview;
            }

            if (!string.IsNullOrEmpty(NFT.ImageLink))
                return NFT.ImageLink;
            else
                return string.Empty;
        }
        public string GetPreviewUrl()
        {
            if (NFT == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(NFT.Preview))
                return NFT.Preview;
            else
                return string.Empty;
        }
    }
}