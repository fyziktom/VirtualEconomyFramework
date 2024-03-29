﻿using Microsoft.AspNetCore.Components;
using VEFramework.VEBlazor.Components.NFTs.Common;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;
using VEDriversLite.NeblioAPI;
using VEDriversLite;

namespace VEFramework.VEBlazor.Components.Base
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

    public abstract class NFTBase : AccountRelatedComponentBase
    {
        public const string EmptyImage = "_content/VEFramework.VEBlazor/images/empty.jpg";
        [Parameter]
        public INFT NFT { get; set; } = new ImageNFT("");
        [Parameter]
        public EventCallback<INFT> NFTChanged { get; set; }
        [Parameter]
        public EventCallback<NFTSentResultDto> NFTSent { get; set; }

        /// <summary>
        /// Get image from the NFT.
        /// If there is the preview you can set the returnPreviewIfExists and it will return the preview link.
        /// It does not download the image data. Just provide the link. If the link is empty it will return emtpy image from the the static files.
        /// </summary>
        /// <param name="returnPreviewIfExists"></param>
        /// <returns></returns>
        public string GetImageUrl(bool returnPreviewIfExists = false)
        {
            if (NFT == null)
                return EmptyImage;

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
                return EmptyImage;
        }
        /// <summary>
        /// Get preview image from the NFT.
        /// If not exists it will return empty image from the the static files.
        /// </summary>
        /// <returns></returns>
        public string GetPreviewUrl()
        {
            if (NFT == null)
                return EmptyImage;

            if (!string.IsNullOrEmpty(NFT.Preview))
                return NFT.Preview;
            else
                return EmptyImage;
        }

        public string actualImageLink
        {
            get
            {
                if (NFT.DataItems != null && NFT.DataItems.Count >= 0)
                {
                    foreach (var item in NFT.DataItems)
                    {
                        if (item.IsMain)
                        {
                            if (item.Storage == DataItemStorageType.IPFS)
                                return VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetIPFSLinkFromHash(item.Hash);
                            else if (item.Storage == DataItemStorageType.Url)
                                return item.Hash;
                        }
                    }
                    if (NFT.DataItems.Count > 0)
                    {
                        var il = VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetIPFSLinkFromHash(NFT.DataItems.FirstOrDefault()?.Hash);
                        return !string.IsNullOrEmpty(il) ? il : GetImageUrl();
                    }
                }
                return GetImageUrl();
            }
        }
        public DataItemType actualFileType
        {
            get
            {
                if (NFT.DataItems != null && NFT.DataItems.Count >= 0)
                {
                    foreach (var item in NFT.DataItems)
                    {
                        if (item.IsMain)
                            return item.Type;
                    }
                    if (NFT.DataItems.Count > 0)
                        return NFT.DataItems.FirstOrDefault()?.Type ?? DataItemType.Image;
                }
                return DataItemType.Image;
            }
        }
        public string ActualImageLinkFromNFT(INFT nft)
        {
            if (nft.DataItems != null && nft.DataItems.Count >= 0)
            {
                foreach (var item in nft.DataItems)
                {
                    if (item.IsMain)
                    {
                        if (item.Storage == DataItemStorageType.IPFS)
                            return VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetIPFSLinkFromHash(item.Hash);
                        else if (item.Storage == DataItemStorageType.Url)
                            return item.Hash;
                    }
                }
                if (nft.DataItems.Count > 0)
                {
                    var il = VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetIPFSLinkFromHash(nft.DataItems.FirstOrDefault()?.Hash);
                    return !string.IsNullOrEmpty(il) ? il : GetImageUrl();
                }
            }
            return GetImageUrl();
        }
        public DataItemType ActualFileTypeFromNFT(INFT nft)
        {
            if (nft.DataItems != null && nft.DataItems.Count >= 0)
            {
                foreach (var item in nft.DataItems)
                {
                    if (item.IsMain)
                        return item.Type;
                }
                if (nft.DataItems.Count > 0)
                    return nft.DataItems.FirstOrDefault()?.Type ?? DataItemType.Image;
            }
            return DataItemType.Image;
        }
    }
    public abstract class NFTComponentBase : NFTBase
    {
        [Parameter]
        public string Utxo { get; set; } = string.Empty;
        [Parameter]
        public EventCallback<string> UtxoChanged { get; set; }
        [Parameter]
        public int UtxoIndex { get; set; } = 0;
        [Parameter]
        public EventCallback<int> UtxoIndexChanged { get; set; }

        [Parameter]
        public EventCallback<List<INFT>> OpenNFTsInWorkTab { get; set; }

        [Parameter]
        public EventCallback<INFT> OpenNFTDetailsRequest { get; set; }

        [Parameter]
        public bool HideOpenInWorkTabButton { get; set; } = false;

        public bool Loading = false;
        public NFTCard? nftCard;

        public async Task LoadNFT(INFT nft)
        {
            if (nft != null)
            {
                NFT = nft;
                Utxo = NFT.Utxo;
                UtxoIndex = NFT.UtxoIndex;
                await InvokeAsync(StateHasChanged);
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
                        await nftCard.LoadNFT(NFT);
                }
                else
                    NFT = new ImageNFT("");

                Loading = false;
            }
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Check if the NFT contains the ImageData in bytes. 
        /// If not, it will download it from IPFS and then convert as image base64 string
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetImage()
        {
            if (NFT == null)
                return EmptyImage;

            if (NFT.ImageData != null && NFT.ImageData.Length > 0)
                return "data:image;base64," + Convert.ToBase64String(NFT.ImageData);
            else if (NFT.ImageData == null && !string.IsNullOrEmpty(NFT.ImageLink))
            {
                //var imd = await NFTHelpers.IPFSDownloadFromInfuraAsync(VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetHashFromIPFSLink(NFT.ImageLink));
                var result = await VEDriversLite.VEDLDataContext.Storage.GetFileFromIPFS(new VEDriversLite.StorageDriver.StorageDrivers.Dto.ReadFileRequestDto()
                {
                    DriverType = VEDriversLite.StorageDriver.StorageDrivers.StorageDriverType.IPFS,
                    Hash = VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetHashFromIPFSLink(NFT.ImageLink),
                });
                if (result.Item1)
                {
                    NFT.ImageData = result.Item2;
                    return "data:image;base64," + Convert.ToBase64String(result.Item2);
                }
            }

            return EmptyImage;
        }

        /// <summary>
        /// Check if the DataItem contains the Image Data in bytes. 
        /// If not, it will download it from IPFS and then convert as image base64 string
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetImageFromHash(string hash)
        {
            if (NFT == null)
                return EmptyImage;
            var di = NFT.DataItems.FirstOrDefault(x => x.Hash == hash);

            if (di != null && di.Data.Length > 0)
                return "data:image;base64," + Convert.ToBase64String(di.Data);
            else if (di != null && !string.IsNullOrEmpty(di.Hash))
            {
                //var imd = await NFTHelpers.IPFSDownloadFromInfuraAsync(di.Hash);
                var result = await VEDriversLite.VEDLDataContext.Storage.GetFileFromIPFS(new VEDriversLite.StorageDriver.StorageDrivers.Dto.ReadFileRequestDto()
                {
                    DriverType = VEDriversLite.StorageDriver.StorageDrivers.StorageDriverType.IPFS,
                    Hash = di.Hash,
                });
                if (result.Item1)
                {
                    di.Data = result.Item2;
                    di.Base64Data = Convert.ToBase64String(result.Item2);
                    return "data:image;base64," + di.Base64Data;
                }
            }

            return EmptyImage;
        }
        /// <summary>
        /// Get image as base64 string.
        /// Function will parse it from input bytes array
        /// </summary>
        /// <param name="itemdata"></param>
        /// <returns></returns>
        public static string GetImageStringFromBytes(byte[] itemdata)
        {
            if (itemdata is not null && itemdata.Length > 0)
                return "data:video;base64," + Convert.ToBase64String(itemdata);
            else
                return EmptyImage;
        }
        /// <summary>
        /// Get video as base64 string.
        /// Function will parse it from input bytes array
        /// </summary>
        /// <param name="itemdata"></param>
        /// <returns></returns>
        public static string GetVideoStringFromBytes(byte[] itemdata)
        {
            if (itemdata is not null && itemdata.Length > 0)
                return "data:image;base64," + Convert.ToBase64String(itemdata);
            else
                return "_content/VEFramework.VEBlazor/images/blankvideo.png";
        }

        public string GetImageGalleryUrl(VEDriversLite.NFT.Dto.NFTDataItem item)
        {
            if (NFT == null)
                return EmptyImage;

            if (item != null)
            {
                if (item.Storage == VEDriversLite.NFT.Dto.DataItemStorageType.IPFS)
                    return VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetIPFSLinkFromHash(item.Hash);
                else if (item.Storage == VEDriversLite.NFT.Dto.DataItemStorageType.Url)
                    return item.Hash;
            }

            return EmptyImage;
        }
        public List<string> GetImageGalleryUrls()
        {
            if (NFT == null)
                return new List<string>() { EmptyImage };

            if (NFT.DataItems != null && NFT.DataItems.Count > 0)
            {
                var urls = new List<string>();
                foreach (var i in NFT.DataItems)
                    urls.Add(GetImageGalleryUrl(i));
                if (urls.Count > 0)
                    return urls;
            }
            return new List<string>() { EmptyImage };
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

        internal virtual async Task onDataItemCreatedHandler(VEDriversLite.NFT.Dto.NFTDataItem item)
        {
            if (item != null)
            {
                NFT.DataItems.Add(item);
                await InvokeAsync(StateHasChanged);
            }
        }
        
        internal virtual async Task onTextAppliedHandler(string text)
        {
            if (!string.IsNullOrEmpty(text) && string.IsNullOrEmpty(NFT.Text))
            {
                NFT.Text = text;
                await InvokeAsync(StateHasChanged);
            }
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
                InvokeAsync(StateHasChanged);
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