using System;
using System.Collections.Generic;
using System.Linq;
using VEDriversLite;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Imaging.Xray.Dto;
using VEDriversLite.Bookmarks;

public enum TabType
{
    Main,
    WorkTab,
    ActiveTab
}

public class WorkTab
{
    public string Name { get; set; } = "Empty WT";
    public List<INFT> NFTs { get; set; } = new List<INFT>();
}

public class GalleryTab
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public TabType Type { get; set; } = TabType.WorkTab;
    public object Tab { get; set; } = null;
    public bool IsActive { get; set; } = false;
}
public enum MintingToolbarActionType
{
    None,
    PreviousStep,
    NextStep,
    Save,
    Finish,
    Mint,
    Cancel,
    ClearAll,
    ClearActualForm
}
public class MintingToolbarActionDto
{
    public MintingToolbarActionType Type { get; set; }
    public string[] Args { get; set; }
}
public class AppData
{
    public NeblioAccount Account { get; set; } = new NeblioAccount();
    public bool IsAccountLoaded { get; set; } = false;
    public Dictionary<string,XrayExposureParameters> ExposureParametersTemplates { get; set; } = new Dictionary<string, XrayExposureParameters>();
    public Dictionary<string, DetectorDataDto> DetectorParametersTemplates { get; set; } = new Dictionary<string, DetectorDataDto>();

    public List<GalleryTab> OpenedTabs { get; set; } = new List<GalleryTab>();
}

