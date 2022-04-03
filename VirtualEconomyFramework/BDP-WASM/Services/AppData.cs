using System;
using System.Collections.Generic;
using System.Linq;
using VEDriversLite;
using VEDriversLite.NFT.Imaging.Xray.Dto;
using VEDriversLite.Bookmarks;

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
}

