using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VEDriversLite.NFT.Dto
{
    /// <summary>
    /// Type of the content
    /// </summary>
    public enum DataItemType
    {
        /// <summary>
        /// Image file, file .bmp, .jpg, .jpeg, .gif, .png, jfif
        /// </summary>
        Image,
        /// <summary>
        /// Audio or Video file, file .wav, .ogg, .mp3, .mp4, .avi, .mov
        /// </summary>
        AVMedia,
        /// <summary>
        /// PDF file, file .pdf
        /// </summary>
        PDF,
        /// <summary>
        /// Simple text, file .txt
        /// </summary>
        Text,
        /// <summary>
        /// Text file with Markdown formating, file .md
        /// </summary>
        Markdown,
        /// <summary>
        /// Mermaid graph, file .mmd
        /// </summary>
        Mermaid,
        /// <summary>
        /// HTML file, file .html
        /// </summary>
        HTML,
        /// <summary>
        /// Json file, file .json
        /// </summary>       
        JSON,
        /// <summary>
        /// 3d model in STL, file .stl
        /// </summary>
        Model3dSTL
    }
    /// <summary>
    /// Type of the storage where the data are stored. Usually it is IPFS.
    /// </summary>
    public enum DataItemStorageType
    {
        /// <summary>
        /// IPFS storage
        /// </summary>
        IPFS,
        /// <summary>
        /// Common url
        /// </summary>
        Url,
        /// <summary>
        /// Local storage
        /// </summary>
        Local
    }
    /// <summary>
    /// Item in the NFT gallery. It is usually some image with tags
    /// </summary>
    public class NFTDataItem
    {
        /// <summary>
        /// Type of the item
        /// </summary>
        public DataItemType Type { get; set; } = DataItemType.Image;
        /// <summary>
        /// Hash of the transaction
        /// </summary>        
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// Type of the storage where the data are stored. Usually it is IPFS.
        /// </summary>
        public DataItemStorageType Storage { get; set; } = DataItemStorageType.IPFS;

        /// <summary>
        /// Parsed tags from the TagsList
        /// </summary>
        public List<string> TagsList { get; set; } = new List<string>();
        /// <summary>
        /// Loaded data as byte array
        /// </summary>
        [JsonIgnore]
        public byte[] Data { get; set; } = new byte[0];
        /// <summary>
        /// Display flag for UI
        /// </summary>
        public bool IsMain { get; set; } = false;

        public static DataItemType GetItemType(string filename)
        {
            var type = DataItemType.Image;
            if (!string.IsNullOrEmpty(filename))
            {
                if (filename.ToLower().Contains(".bmp") ||
                    filename.ToLower().Contains(".jpg") ||
                    filename.ToLower().Contains(".jpeg") ||
                    filename.ToLower().Contains(".png") ||
                    filename.ToLower().Contains(".gif") ||
                    filename.ToLower().Contains(".jfif"))
                    type = DataItemType.Image;
                else if (filename.ToLower().Contains(".mp3") ||
                         filename.ToLower().Contains(".wav") ||
                         filename.ToLower().Contains(".ogg") ||
                         filename.ToLower().Contains(".mp4") ||
                         filename.ToLower().Contains(".avi") ||
                         filename.ToLower().Contains(".mov"))
                    type = DataItemType.AVMedia;
                else if (filename.ToLower().Contains(".pdf"))
                    type = DataItemType.PDF;
                else if (filename.ToLower().Contains(".stl"))
                    type = DataItemType.Model3dSTL;
                else if (filename.ToLower().Contains(".md"))
                    type = DataItemType.Markdown;
                else if (filename.ToLower().Contains(".mmd"))
                    type = DataItemType.Mermaid;
                else if (filename.ToLower().Contains(".html"))
                    type = DataItemType.HTML;
                else if (filename.ToLower().Contains(".txt"))
                    type = DataItemType.Text;
                else if (filename.ToLower().Contains(".json"))
                    type = DataItemType.JSON;
            }
            return type;
        }        
    }
}
