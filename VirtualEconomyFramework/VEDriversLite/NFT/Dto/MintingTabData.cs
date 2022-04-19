using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Dto
{
    /// <summary>
    /// List of the receivers for multiminting.
    /// </summary>
    public class ReceiversListItem
    {
        /// <summary>
        /// Address of the receiver
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Use as the receiver during the sending
        /// </summary>
        public bool UseAsReceiver { get; set; } = true;
        /// <summary>
        /// Was send to this receiver
        /// </summary>
        public bool Done { get; set; } = true;
        /// <summary>
        /// Transaction hash of the sending
        /// </summary>
        public string TxId { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Data for the minting tab/page
    /// It contains common dtos for usuall need of minting page
    /// </summary>
    public class MintingTabData
    {
        /// <summary>
        /// Type of the NFT which should be minted on the form
        /// </summary>
        public NFTTypes MintingNFTType { get; set; } = NFTTypes.Image;
        /// <summary>
        /// Label of the button in the menu.
        /// </summary>
        public string MenuButonLabel { get; set; } = "Add New";
        /// <summary>
        /// Second part in header of the page "{AppData.Nick} - {HeaderLabel}"
        /// </summary>
        public string HeaderLabel { get; set; } = "Add New";
        /// <summary>
        /// Page location name for example for https://basedataplace.com/addnew is "addnew"
        /// </summary>
        public string TabPageLocationName { get; set; } = "addnew";
        /// <summary>
        /// NFT to mint
        /// </summary>
        public INFT NFT { get; set; } = new ImageNFT("");
        /// <summary>
        /// Template NFT loaded from network, text or file
        /// </summary>
        public INFT NFTTemplate { get; set; } = new ImageNFT("");
        /// <summary>
        /// Is minting in process?
        /// </summary>
        public bool IsMinting { get; set; } = false;
        /// <summary>
        /// Is minting finished
        /// </summary>
        public bool IsMinted { get; set; } = false;
        /// <summary>
        /// Result of the minting.
        /// In case of false it holds the message, in case of true it holds NFT hash
        /// </summary>
        public (bool,string) MintingResult { get; set; } = (false,string.Empty);
        /// <summary>
        /// Receivers with details about the transaction if it is done
        /// </summary>
        public Dictionary<string, ReceiversListItem> Addresses { get; set; } = new Dictionary<string, ReceiversListItem>();
        /// <summary>
        /// Console output from the minting process
        /// </summary>
        public string ConsoleOutFromMinting { get; set; } = string.Empty;
    }
}
