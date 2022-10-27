using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using VEDriversLite.Common;
using VEDriversLite.Common.Calendar;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Financial;
using VEDriversLite.EntitiesBlocks.PVECalculations.Dto;

namespace VEDriversLite.EntitiesBlocks.PVECalculations
{
    /// <summary>
    /// PV Panels Groups handler cover the PVPanelGroups to whole PVE
    /// For example two sides of house with 5 panels can has own PVPanelGroup both. 
    /// Both will be agregated through PVPanelsGroupsHandler class to act as one whole PVE
    /// </summary>
    public class PVPanelsGroupsHandler : CommonSimulator
    {
        /// <summary>
        /// Name of the PV Panels Groups = whole PVE block
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Total count of Panels in group
        /// </summary>
        public int PanelCount { get => PVPanelsGroups.Values.Where(p => p.PanelCount > 0).Select(p => p.PanelCount).Sum(); }
        /// <summary>
        /// Total Peak power of all panels in the group
        /// </summary>
        public double TotalPeakPower { get => PVPanelsGroups.Values.Where(p => p.TotalPeakPower > 0).Select(p => p.TotalPeakPower).Sum(); }
        /// <summary>
        /// Total width of all panels if imagine in one row
        /// </summary>
        public double TotalWidth { get => PVPanelsGroups.Values.Where(p => p.TotalWidth > 0).Select(p => p.TotalWidth).Sum(); }
        /// <summary>
        /// total width of all panels if imagine in one column
        /// </summary>
        public double TotalHeight { get => PVPanelsGroups.Values.Where(p => p.TotalHeight > 0).Select(p => p.TotalHeight).Sum(); }
        /// <summary>
        /// Median Sun Beam To Panel Peak Angle (where Peak Power was measured) across all panels in all groups in this PVE Block
        /// </summary>
        public double MedianPanelPeakAngle { get => MathHelpers.Median<double>(PVPanelsGroups.Values.Where(p => p.MedianPanelPeakAngle > 0).Select(p => p.MedianPanelPeakAngle)); }
        /// <summary>
        /// Median Azimuth across all panels in all groups in this PVE Block
        /// </summary>
        public double MedianAzimuth { get => MathHelpers.Median<double>(PVPanelsGroups.Values.Where(p => p.MedianAzimuth > 0).Select(p => p.MedianAzimuth)); }
        /// <summary>
        /// Median Efficiency across all panels in all groups in this PVE Block
        /// </summary>
        public double MedianEfficiency { get => MathHelpers.Median<double>(PVPanelsGroups.Values.Where(p => p.MedianEfficiency > 0).Select(p => p.MedianEfficiency)); }
        /// <summary>
        /// Median Dirt ratio across all panels in all groups in this PVE Block
        /// </summary>
        public double MedianDirtRatio { get => MathHelpers.Median<double>(PVPanelsGroups.Values.Where(p => p.MedianDirtRatio > 0).Select(p => p.MedianDirtRatio)); }
        /// <summary>
        /// Median Latitude across all panels in all groups in this PVE Block
        /// </summary>
        public double MedianLatitude { get => MathHelpers.Median<double>(PVPanelsGroups.Values.Where(p => p.MedianLatitude > 0).Select(p => p.MedianLatitude)); }
        /// <summary>
        /// Median Longitude across all panels in all groups in this PVE Block
        /// </summary>
        public double MedianLongitude { get => MathHelpers.Median<double>(PVPanelsGroups.Values.Where(p => p.MedianLongitude > 0).Select(p => p.MedianLongitude)); }
        /// <summary>
        /// Median Base angle (against the earth plane) across all panels in all groups in this PVE Block
        /// </summary>
        public double MedianBaseAngle { get => MathHelpers.Median<double>(PVPanelsGroups.Values.Where(p => p.MedianBaseAngle > 0).Select(p => p.MedianBaseAngle)); }
        /// <summary>
        /// Dictionary with all panels groups in this PVE Block
        /// </summary>
        public ConcurrentDictionary<string, PVPanelsGroup> PVPanelsGroups { get; set; } = new ConcurrentDictionary<string, PVPanelsGroup>();
        /// <summary>
        /// Template panel for this Panels Group
        /// </summary>
        public PVPanel CommonPanel { get; set; } = new PVPanel();
        /// <summary>
        /// Financial info about the whole PVE
        /// </summary>
        public FinancialInfo FinancialInfo { get; set; } = new FinancialInfo();
        /// <summary>
        /// Get total investment based on total peak power of PVE in time based on actual time and specified discont
        /// </summary>
        public double TotalInvestmentBasedOnPeakPower { get => FinancialInfo.DiscontedPrice; }

        /// <summary>
        /// Set parameters for the Common panel template
        /// </summary>
        /// <param name="panel"></param>
        public void SetCommonPanel(PVPanel panel)
        {
            CommonPanel = panel.Clone();
            CommonPanel.GroupId = Id;
        }
        /// <summary>
        /// Set discont of financial info
        /// </summary>
        /// <param name="discont"></param>
        public void SetDiscont(double discont)
        {
            FinancialInfo.Discont.DiscontInPercentagePerYear = discont;
        }

        /// <summary>
        /// Function will refresh some internal stats which relates on number of panels, etc.
        /// </summary>
        public void Refresh()
        {
            FinancialInfo.InitialUnitPrice = FinancialInfoHelpers.AvgPricePerWPeakForPVE * TotalPeakPower * 1000;
        }
        /// <summary>
        /// Get disconted value of the investment in time
        /// </summary>
        /// <param name="end">End time when the disconted price is requested</param>
        /// <returns></returns>
        public double GetInvestmentDiscontedInTime(DateTime end)
        {
            return FinancialInfo.GetDiscontedValue(end);
        }

        /// <summary>
        /// Add new group with specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Id of new added group, null when not success</returns>
        public string AddGroup(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            if (!PVPanelsGroups.ContainsKey(name))
            {
                var group = new PVPanelsGroup() { Name = name };
                PVPanelsGroups.TryAdd(group.Id, group);

                return group.Id;
            }
            return string.Empty;
        }

        /// <summary>
        /// Remove group by Id
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>true if success</returns>
        public bool RemoveGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return false;
            if (PVPanelsGroups.TryRemove(groupId, out var group))
            {
                Refresh();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Add panel to specific group
        /// </summary>
        /// <param name="groupId">Id of the group where to add the panel</param>
        /// <param name="panel">Panel object</param>
        /// <param name="addcount">Add multiple times. In that case each panel will have new created Id</param>
        /// <returns>Id of last added panel</returns>
        public IEnumerable<string> AddPanelToGroup(string groupId, PVPanel panel, int addcount = 1)
        {
            if (panel != null)
            {
                panel.GroupId = groupId;
                if (PVPanelsGroups.TryGetValue(groupId, out var group))
                {
                    while (addcount > 0)
                    {
                        var p = panel.Clone();
                        p.Id = Guid.NewGuid().ToString();
                        group.AddPanel(p);
                        addcount--;
                        Refresh();
                        yield return p.Id;
                    }
                }
            }
        }
        /// <summary>
        /// Remove Panel from the group based on Group Id and Panel Id
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="panelId"></param>
        /// <returns>true if success</returns>
        public bool RemovePanelFromGroup(string groupId, string panelId)
        {
            if (string.IsNullOrEmpty(panelId))
                return false;
            if (PVPanelsGroups.TryGetValue(groupId, out var group))
            {
                if (group.RemovePanel(panelId))
                {
                    Refresh();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Export Config file to the Json
        /// </summary>
        /// <returns></returns>
        public override (bool, string) ExportConfig()
        {
            try
            {
                var exp = JsonConvert.SerializeObject(CreateConfigFile());
                if (exp != null)
                    return (true, exp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot serialize PVE settings. " + ex.Message);
            }
            return (false, string.Empty);
        }

        /// <summary>
        /// Fill the Config file
        /// </summary>
        /// <returns></returns>
        public PVConfigDto CreateConfigFile()
        {
            var config = new PVConfigDto();

            foreach (var group in PVPanelsGroups.Values)
            {
                var gr = new PVPanelsGroupConfigDto();
                gr.Id = group.Id;
                gr.Name = group.Name;
                foreach (var panel in group.PVPanels.Values)
                    gr.Panels.Add(panel.Id, panel.Clone());

                config.Groups.Add(group.Id, gr);
            }

            return config;
        }

        /// <summary>
        /// Import settings of the PVE from JSON
        /// </summary>
        /// <param name="jsonConfig"></param>
        /// <returns></returns>
        public override (bool, string) ImportConfig(string jsonConfig)
        {
            if (string.IsNullOrEmpty(jsonConfig))
                return (false, string.Empty);

            var config = JsonConvert.DeserializeObject<PVConfigDto>(jsonConfig);
            if (config != null)
                return (ImportConfigDto(config), string.Empty);
            else
                return (false, string.Empty);
        }

        /// <summary>
        /// Import config from config file
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool ImportConfigDto(PVConfigDto config)
        {
            if (config == null)
                return false;

            Id = config.Id;
            Name = config.Name;
            CommonPanel = config.CommonPanel;

            PVPanelsGroups.Clear();

            foreach (var group in config.Groups.Values)
            {
                var gr = new PVPanelsGroup()
                {
                    Id = group.Id,
                    Name = group.Name
                };
                foreach (var panel in group.Panels.Values)
                    gr.PVPanels.TryAdd(panel.Id, panel);
                PVPanelsGroups.TryAdd(group.Id, gr);
            }

            Refresh();

            return true;
        }

        /// <summary>
        /// Get panel which contains median values of all panels in the group
        /// </summary>
        /// <returns></returns>
        public PVPanel GetMedianPanel()
        {
            // todo split to threads
            var result = new PVPanel()
            {
                Azimuth = MedianAzimuth,
                Efficiency = MedianEfficiency,
                DirtRatio = MedianDirtRatio,
                Latitude = MedianLatitude,
                Longitude = MedianLongitude,
                BaseAngle = MedianBaseAngle,
                GroupId = Id,
                PeakPower = TotalPeakPower
            };
            return result;
        }
        /// <summary>
        /// Get PVPanelGroup object based on Id
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public PVPanelsGroup? GetGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return null;
            if (PVPanelsGroups.TryGetValue(groupId, out var group))
                return group;
            return null;
        }

        /// <summary>
        /// Get PVPanelGroup Panels list based on Id
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public IEnumerable<PVPanel> GetGroupPanelsList(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return new List<PVPanel>();
            if (PVPanelsGroups.TryGetValue(groupId, out var group))
                return group.PVPanels.Values;
            return new List<PVPanel>();
        }

        /// <summary>
        /// Get Group Panels count based on Id of the group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public int GetGroupPanelsCount(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return 0;
            if (PVPanelsGroups.TryGetValue(groupId, out var group))
                return group.PanelCount;
            return 0;
        }

        /// <summary>
        /// Get group peak power across all panels in the group based on the DateTime
        /// It affects sun position so it will change the real expected power in the specific date and time
        /// </summary>
        /// <param name="start">Date time of the moment</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns>peak energy in specific datetime</returns>
        public double GetGroupPeakPowerInDateTime(string groupId, DateTime start, Coordinates coord, double weatherFactor)
        {
            if (string.IsNullOrEmpty(groupId))
                return 0;
            if (PVPanelsGroups.TryGetValue(groupId, out var group))
                return group.GetGroupPeakPowerInDateTime(start, coord, weatherFactor);
            return 0;
        }

        /// <summary>
        /// Get group peak power across all panels in the group based on the DateTime and return as IBlock
        /// It affects sun position so it will change the real expected power in the specific date and time
        /// </summary>
        /// <param name="start">Date time of the moment</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns>Peak energy in specific datetime in form of IBlock</returns>
        public IBlock? GetGroupPeakPowerInDateBlock(string groupId, DateTime start, Coordinates coord, double weatherFactor)
        {
            if (string.IsNullOrEmpty(groupId))
                return null;
            var amount = 0.0;

            if (PVPanelsGroups.TryGetValue(groupId, out var group))
                amount = group.GetGroupPeakPowerInDateTime(start, coord, weatherFactor);

            var block = new BaseBlock();
            return block.GetBlock(BlockType.Simulated, BlockDirection.Created, start, new TimeSpan(1, 0, 0), amount, groupId, Name, null, groupId);
        }

        /// <summary>
        /// Get group peak power across all panels in the group based on the year and return as list of IBlock.
        /// For each day in year there will be one block created.
        /// </summary>
        /// <param name="year">Year as int</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns>Peak energy in specific datetime in form of List of IBlock for whole one year</returns>
        public IEnumerable<IBlock> GetGroupPeakPowerInYearBlock(string groupId, int year, Coordinates coord, double weatherFactor)
        {
            if (!string.IsNullOrEmpty(groupId))
            {
                var start = new DateTime(year, 1, 1);
                var end = start.AddYears(1);
                var block = new BaseBlock();
                var firstBlockId = string.Empty;

                if (PVPanelsGroups.TryGetValue(groupId, out var group))
                {
                    var tmp = start;
                    while (tmp < end)
                    {
                        var amount = 0.0;
                        for (var i = 0; i < 24; i++)
                            amount += group.GetGroupPeakPowerInDateTime(tmp.AddHours(i), coord, weatherFactor);

                        SunMoonCalcs.SunCalc.GetTimes(tmp,
                                                      coord.Latitude,
                                                      coord.Longitude,
                                                      out var rise,
                                                      out var set);

                        var effectiverise = rise.AddSeconds((set - rise).TotalSeconds * 0.1);
                        var effectiveset = set.AddSeconds(-(set - rise).TotalSeconds * 0.1);

                        var rblock = block.GetBlock(BlockType.Simulated,
                                                    BlockDirection.Created,
                                                    effectiverise,
                                                    effectiveset - effectiverise,
                                                    amount,
                                                    groupId,
                                                    Name,
                                                    null,
                                                    groupId);

                        rblock.IsInDayOnly = true;
                        if (string.IsNullOrEmpty(firstBlockId))
                            firstBlockId = rblock.Id;
                        else
                            rblock.RepetitiveSourceBlockId = firstBlockId;
                        yield return rblock;
                        tmp = tmp.AddDays(1);
                    }
                }
            }
        }

        /// <summary>
        /// Get total peak power across all groups and panels in the handler based on the year and return as list of IBlock.
        /// For each day in year there will be one block created.
        /// </summary>
        /// <param name="year">Year as int</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns>Peak energy in specific datetime in form of List of IBlock for whole one year</returns>
        public IEnumerable<IBlock> GetTotalPeakPowerInYearBlocks(int year, Coordinates coord, double weatherFactor)
        {
            var start = new DateTime(year, 1, 1);
            var end = start.AddYears(1);
            var block = new BaseBlock();
            var tmp = start;
            var firstBlockId = string.Empty;

            while (tmp < end)
            {
                var amount = 0.0;

                foreach (var group in PVPanelsGroups.Values)
                {
                    for (var i = 0; i < 24; i++)
                        amount += group.GetGroupPeakPowerInDateTime(tmp.AddHours(i), coord, weatherFactor);
                }

                SunMoonCalcs.SunCalc.GetTimes(tmp,
                                              coord.Latitude,
                                              coord.Longitude,
                                              out var rise,
                                              out var set);

                var effectiverise = rise.AddSeconds((set - rise).TotalSeconds * 0.1);
                var effectiveset = set.AddSeconds(-(set - rise).TotalSeconds * 0.1);

                var rblock = block.GetBlock(BlockType.Simulated,
                                            BlockDirection.Created,
                                            effectiverise,
                                            effectiveset - effectiverise,
                                            amount,
                                            Id,
                                            Name,
                                            null,
                                            Id);

                rblock.IsInDayOnly = true;
                if (string.IsNullOrEmpty(firstBlockId))
                    firstBlockId = rblock.Id;
                else
                    rblock.RepetitiveSourceBlockId = firstBlockId;

                yield return rblock;
                tmp = tmp.AddDays(1);
            }
        }

        /// <summary>
        /// Get group peak power across all panels in the group based on the start-end time and return as list of IBlock.
        /// For each day in timeframe there will be one block created.
        /// </summary>
        /// <param name="start">Start DateTime</param>
        /// <param name="end">End DateTime</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns>Peak energy in specific datetime in form of List of IBlock for whole one year</returns>
        public IEnumerable<IBlock> GetGroupPeakPowerInTimeframeBlock(string groupId, DateTime start, DateTime end, Coordinates coord, double weatherFactor)
        {
            if (string.IsNullOrEmpty(groupId))
                throw new Exception("annot get Peak power in year data. Group Id is required.");

            if (end < start)
                throw new Exception("Wrong input of the start and end. End cannot be earlier than start.");

            start = start.AddHours(-start.Hour).AddMinutes(-start.Minute).AddSeconds(-start.Second);
            end = end.AddHours(-end.Hour).AddMinutes(-end.Minute).AddSeconds(-end.Second);

            var block = new BaseBlock();
            var firstBlockId = string.Empty;

            if (PVPanelsGroups.TryGetValue(groupId, out var group))
            {
                var tmp = start;
                while (tmp < end)
                {
                    var amount = 0.0;
                    for (var i = 0; i < 24; i++)
                        amount += group.GetGroupPeakPowerInDateTime(tmp.AddHours(i), coord, weatherFactor);

                    SunMoonCalcs.SunCalc.GetTimes(tmp,
                                                  coord.Latitude,
                                                  coord.Longitude,
                                                  out var rise,
                                                  out var set);

                    var effectiverise = rise.AddSeconds((set - rise).TotalSeconds * 0.1);
                    var effectiveset = set.AddSeconds(-(set - rise).TotalSeconds * 0.1);

                    var rblock = block.GetBlock(BlockType.Simulated,
                                                BlockDirection.Created,
                                                effectiverise,
                                                effectiveset - effectiverise,
                                                amount,
                                                groupId,
                                                Name,
                                                null,
                                                groupId);

                    rblock.IsInDayOnly = true;
                    if (string.IsNullOrEmpty(firstBlockId))
                        firstBlockId = rblock.Id;
                    else
                        rblock.RepetitiveSourceBlockId = firstBlockId;

                    yield return rblock;
                    tmp = tmp.AddDays(1);
                }
            }
        }

        /// <summary>
        /// Get total peak power across all groups and panels in the handler based on the start-end datetime and return as list of IBlock.
        /// For each day in timeframe there will be one block created.
        /// </summary>
        /// <param name="start">Start DateTime</param>
        /// <param name="end">End DateTime</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns>Peak energy in specific datetime in form of List of IBlock for whole one year</returns>
        public IEnumerable<IBlock> GetTotalPeakPowerInTimeframeBlocks(DateTime start,
                                                                      DateTime end,
                                                                      Coordinates coord,
                                                                      double weatherFactor,
                                                                      string parentId = "",
                                                                      string sourceId = "",
                                                                      string name = "")
        {
            if (end < start)
                throw new Exception("Wrong input of the start and end. End cannot be earlier than start.");

            var block = new BaseBlock();

            start = start.AddHours(-start.Hour).AddMinutes(-start.Minute).AddSeconds(-start.Second);
            end = end.AddHours(-end.Hour).AddMinutes(-end.Minute).AddSeconds(-end.Second);
            var tmp = start;
            var firstBlockId = string.Empty;

            while (tmp < end)
            {
                var amount = 0.0;

                foreach (var group in PVPanelsGroups.Values)
                {
                    for (var i = 0; i < 24; i++)
                        amount += group.GetGroupPeakPowerInDateTime(tmp.AddHours(i), coord, weatherFactor);
                }

                SunMoonCalcs.SunCalc.GetTimes(tmp,
                                              coord.Latitude,
                                              coord.Longitude,
                                              out var rise,
                                              out var set);

                var effectiverise = rise.AddSeconds((set - rise).TotalSeconds * 0.1);
                var effectiveset = set.AddSeconds(-(set - rise).TotalSeconds * 0.1);

                if (string.IsNullOrEmpty(sourceId))
                    sourceId = Id;
                if (string.IsNullOrEmpty(parentId))
                    parentId = Id;
                if (string.IsNullOrEmpty(name))
                    name = Name;

                var rblock = block.GetBlock(BlockType.Simulated,
                                            BlockDirection.Created,
                                            effectiverise,
                                            effectiveset - effectiverise,
                                            amount,
                                            sourceId,
                                            name,
                                            null,
                                            parentId);

                rblock.IsInDayOnly = true;
                if (string.IsNullOrEmpty(firstBlockId))
                    firstBlockId = rblock.Id;
                else
                    rblock.RepetitiveSourceBlockId = firstBlockId;

                yield return rblock;
                tmp = tmp.AddDays(1);
            }
        }

        /// <summary>
        /// Get total peak power across all groups and panels in the handler based on the start-end datetime and return as list of IBlock.
        /// For each day in timeframe there will be one block created.
        /// </summary>
        /// <param name="start">Start DateTime</param>
        /// <param name="end">End DateTime</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns>Peak energy in specific datetime in form of List of IBlock for whole one year</returns>
        public IEnumerable<IBlock> GetTotalPeakPowerInHourTimeframeBlocks(DateTime start,
                                                                          DateTime end,
                                                                          Coordinates coord,
                                                                          double weatherFactor,
                                                                          string parentId = "",
                                                                          string sourceId = "",
                                                                          string name = "")
        {
            if (end < start)
                throw new Exception("Wrong input of the start and end. End cannot be earlier than start.");

            var block = new BaseBlock();

            start = start.AddHours(-start.Hour).AddMinutes(-start.Minute).AddSeconds(-start.Second);
            end = end.AddHours(-end.Hour).AddMinutes(-end.Minute).AddSeconds(-end.Second);
            var tmp = start;
            var firstBlockId = string.Empty;

            var timeframe = BlockTimeframe.Hour;
            var ts = BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, start);

            while (tmp < end)
            {
                var amount = 0.0;

                foreach (var group in PVPanelsGroups.Values)
                    amount += group.GetGroupPeakPowerInDateTime(tmp, coord, weatherFactor);


                if (string.IsNullOrEmpty(sourceId))
                    sourceId = Id;
                if (string.IsNullOrEmpty(parentId))
                    parentId = Id;
                if (string.IsNullOrEmpty(name))
                    name = Name;

                var rblock = block.GetBlock(BlockType.Simulated,
                                            BlockDirection.Created,
                                            tmp,
                                            ts,
                                            amount,
                                            sourceId,
                                            name,
                                            null,
                                            parentId);

                rblock.IsInDayOnly = true;
                if (string.IsNullOrEmpty(firstBlockId))
                    firstBlockId = rblock.Id;
                else
                    rblock.RepetitiveSourceBlockId = firstBlockId;

                yield return rblock;

                tmp = tmp.Add(ts);
                ts = BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, tmp);
            }
        }

        /// <summary>
        /// Get Total peak power across all groups in the this PVE Block based on the DateTime
        /// It affects sun position so it will change the real expected power in the specific date and time
        /// </summary>
        /// <param name="start">Date time of the moment</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns></returns>
        public double GetTotalPowerInDateTime(DateTime start, Coordinates coord, double weatherFactor)
        {
            var result = 0.0;
            foreach (var group in PVPanelsGroups.Values)
                result += group.GetGroupPeakPowerInDateTime(start, coord, weatherFactor);
            return result;
        }

        public override IEnumerable<IBlock> GetBlocks(BlockTimeframe timeframe, 
                                                      DateTime start, 
                                                      DateTime end, 
                                                      Dictionary<string, DataProfile> inputProfiles, 
                                                      Dictionary<string, List<IBlock>> inputBlocks, 
                                                      Dictionary<string, object> options)
        {
            if (end < start)
                throw new Exception("Wrong input of the start and end. End cannot be earlier than start.");

            var block = new BaseBlock();

            var sourceId = Id;
            if (options.TryGetValue("sourceId", out var sid))
                sourceId = sid as string;
            var parentId = ParentId;
            if (options.TryGetValue("parentId", out var pid))
                parentId = pid as string;
            var name = $"Block-{Name}";
            if (options.TryGetValue("name", out var n))
                name = n as string;
            var coord = new Coordinates(49.194103, 16.608998);
            if (options.TryGetValue("latitude", out var lt))
                coord.Latitude = (double)lt;
            if (options.TryGetValue("longitude", out var lg))
                coord.Longitude = (double)lg;
            var weatherFactor = 0.0;
            if (options.TryGetValue("weatherFactor", out var wf))
               weatherFactor = (double)wf;

            start = start.AddHours(-start.Hour).AddMinutes(-start.Minute).AddSeconds(-start.Second);
            end = end.AddHours(-end.Hour).AddMinutes(-end.Minute).AddSeconds(-end.Second);
            var tmp = start;
            var firstBlockId = string.Empty;

            var ts = BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, start);

            while (tmp < end)
            {
                var amount = 0.0;
                
                var htmp = tmp;
                var hend = htmp.Add(ts);
                while (htmp < hend)
                {
                    var tot = 0.0;
                    foreach (var group in PVPanelsGroups.Values)
                        tot += group.GetGroupPeakPowerInDateTime(htmp, coord, weatherFactor);

                    if (htmp.AddHours(1) < hend)
                        amount += tot;
                    else
                        amount += tot * (hend - htmp).TotalHours;

                    htmp = htmp.AddHours(1);
                }

                if (string.IsNullOrEmpty(sourceId))
                    sourceId = Id;
                if (string.IsNullOrEmpty(parentId))
                    parentId = Id;
                if (string.IsNullOrEmpty(name))
                    name = Name;

                var rblock = block.GetBlock(BlockType.Simulated,
                                            BlockDirection.Created,
                                            tmp,
                                            ts,
                                            amount,
                                            sourceId,
                                            name,
                                            null,
                                            parentId);

                rblock.IsInDayOnly = true;
                if (string.IsNullOrEmpty(firstBlockId))
                    firstBlockId = rblock.Id;
                else
                    rblock.RepetitiveSourceBlockId = firstBlockId;

                yield return rblock;

                tmp = tmp.Add(ts);
                ts = BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, tmp);
            }
        }

    }
}
