using Microsoft.AspNetCore.Components;
using VEDriversLite.NFT;

public class NFTSentResultDto
{
    public bool sucess { get; set; } = false;
    public string message { get; set; } = string.Empty;
    public string error { get; set; } = string.Empty;
    public INFT NFT { get; set; } = null;
}

public class AccountRelatedComponentBase //: ComponentBase //todo does not work ???
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

public class NFTComponentBase : AccountRelatedComponentBase
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

