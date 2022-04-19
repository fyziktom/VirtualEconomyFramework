using VEDriversLite.NFT.Imaging.Xray.Dto;

public class BDPXrayService
{
    public Dictionary<string, XrayExposureParameters> ExposureParametersTemplates { get; set; } = new Dictionary<string, XrayExposureParameters>();
    public Dictionary<string, DetectorDataDto> DetectorParametersTemplates { get; set; } = new Dictionary<string, DetectorDataDto>();

}

