using Microsoft.AspNetCore.Components;
using VEBlazor.Components.NFTs.Common;
using VEDriversLite.NFT;

namespace VEBlazor.Components.Base
{
    public class NFTSentResultDto
    {
        public bool sucess { get; set; } = false;
        public string message { get; set; } = string.Empty;
        public INFT NFT { get; set; } = new ImageNFT("");
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
    public abstract class AccountRelatedComponentBase : StyledComponent
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

        [Parameter]
        public EventCallback<INFT> OpenNFTDetailsRequest { get; set; }

        [Parameter]
        public bool HideOpenInWorkTabButton { get; set; } = false;

        public bool Loading = false;
        public NFTCard? nftCard;        

        public void LoadNFT(INFT nft)
        {
            if (nft != null)
            {
                NFT = nft;
                Utxo = NFT.Utxo;
                UtxoIndex = NFT.UtxoIndex;
                StateHasChanged();
            }
        }

        public async Task LoadNFTFromNetwork()
        {
            if (!string.IsNullOrEmpty(Utxo))
            {
                Loading = true;
                if (Utxo.Contains(':'))
                    NFT = await NFTFactory.GetNFT("", Utxo.Split(':')[0], wait: true);
                else
                    NFT = await NFTFactory.GetNFT("", Utxo, wait: true);

                if (NFT != null)
                {
                    if (nftCard != null)
                        nftCard.LoadNFT(NFT);
                }
                else
                    NFT = new ImageNFT("");
                
                Loading = false;
            }
            StateHasChanged();
        }
        
        public async Task<string> GetImage()
        {
            if (NFT == null)
                return "_content/VEBlazor/images/empty.jpg";

            if (NFT.ImageData != null && NFT.ImageData.Length > 0)
                return "data:image;base64," + Convert.ToBase64String(NFT.ImageData);
            else if (NFT.ImageData == null && !string.IsNullOrEmpty(NFT.ImageLink))
            {
                var imd = await NFTHelpers.IPFSDownloadFromInfuraAsync(NFTHelpers.GetHashFromIPFSLink(NFT.ImageLink));
                if (imd != null)
                {
                    NFT.ImageData = imd;
                    return "data:image;base64," + Convert.ToBase64String(imd);
                }
            }
            
            return "_content/VEBlazor/images/empty.jpg";
        }
        public static string GetImageStringFromBytes(byte[] itemdata)
        {
            if (itemdata is not null && itemdata.Length > 0)
                return "data:image;base64," + Convert.ToBase64String(itemdata);
            else
                return "_content/VEBlazor/images/empty.jpg";
        }
        public string GetImageUrl(bool returnPreviewIfExists = false)
        {
            if (NFT == null)
                return "_content/VEBlazor/images/empty.jpg";

            if (returnPreviewIfExists)
            {
                var preview = GetPreviewUrl();
                if (!string.IsNullOrEmpty(preview))
                    return preview;
            }

            if (!string.IsNullOrEmpty(NFT.ImageLink))
                return NFT.ImageLink;
            else if (NFT.ImageData != null && NFT.ImageData.Length > 0)
                return "data:image;base64," + Convert.ToBase64String(NFT.ImageData);
            else
                return "_content/VEBlazor/images/empty.jpg";
        }
        public string GetImageGalleryUrl(VEDriversLite.NFT.Dto.NFTDataItem item)
        {
            if (NFT == null)
                return "_content/VEBlazor/images/empty.jpg" ;
            
            if (item != null)
            {
                    if (item.Storage == VEDriversLite.NFT.Dto.DataItemStorageType.IPFS)
                        return NFTHelpers.GetIPFSLinkFromHash(item.Hash);
                    else if (item.Storage == VEDriversLite.NFT.Dto.DataItemStorageType.Url)
                        return item.Hash;
            }

            return  "_content/VEBlazor/images/empty.jpg" ;
        }        
        public List<string> GetImageGalleryUrls()
        {
            if (NFT == null)
                return new List<string>() { "_content/VEBlazor/images/empty.jpg" };

            if (NFT.DataItems != null && NFT.DataItems.Count > 0)
            {
                var urls = new List<string>();
                foreach(var i in NFT.DataItems)
                    urls.Add(GetImageGalleryUrl(i));
                if (urls.Count > 0)
                    return urls;
            }   
            return new List<string>() { "_content/VEBlazor/images/empty.jpg" };
        }
        public string GetPreviewUrl()
        {
            if (NFT == null)
                return "_content/VEBlazor/images/empty.jpg";

            if (!string.IsNullOrEmpty(NFT.Preview))
                return NFT.Preview;
            else
                return "_content/VEBlazor/images/empty.jpg";
        }
        internal virtual async Task OpenNFTInWorkTab()
        {
            if (NFT == null)
                return;
            await OpenNFTsInWorkTab.InvokeAsync(new List<INFT>() { NFT });
        }
        
        internal virtual async Task OpenNFTInWorkTabHandler(List<INFT> e)
        {
            if (NFT == null)
                return;
            await OpenNFTsInWorkTab.InvokeAsync(e);
        }
        
        internal virtual Task NFTSentHandler(NFTSentResultDto e)
        {
            return NFTSent.InvokeAsync(e);
        }
        
    }
    
    public abstract class NFTDetailsBase : NFTComponentBase
    {
        public NFTDetails? NFTDetailsComponent;
        public bool ShowNFTDetails(INFT nft)
        {
            if (NFT == null && nft == null)
                return false;
            else if (NFT == null && nft != null)
            {
                NFT = nft;
                StateHasChanged();
            }

            if (nft != null)
                NFTDetailsComponent?.ShowNFTDetails(nft);
            return true;
        }
        public bool HideNFTDetails()
        {
            NFTDetailsComponent?.HideNFTDetails();
            return true;
        }
    }
}