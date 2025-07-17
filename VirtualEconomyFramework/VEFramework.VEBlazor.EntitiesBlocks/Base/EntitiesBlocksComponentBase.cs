using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Tree;

namespace VEFramework.VEBlazor.EntitiesBlocks.Base
{
    public abstract class StyledComponent : ComponentBase
    {
        [Parameter]
        public string Style { get; set; } = string.Empty;
        [Parameter]
        public string Class { get; set; } = string.Empty;
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
    }

    public abstract class StyledActionComponent : StyledComponent
    {
        [Parameter]
        public EventCallback<string> ActionFinished { get; set; }
    }

    public abstract class TimeRangeComponentBase : StyledActionComponent
    {
        [Parameter]
        public string StartDayLabel { get; set; } = "Start DateTime";
        [Parameter]
        public string EndDayLabel { get; set; } = "End DateTime";

        [Parameter]
        public DateTime StartTime { get; set; } = new DateTime(2022, 1, 1);
        [Parameter]
        public DateTime EndTime { get; set; } = new DateTime(2023, 1, 1);
        public TimeSpan Duration { get => EndTime - StartTime; }

        [Parameter]
        public EventCallback<DateTime> StartTimeChanged { get; set; }
        [Parameter]
        public EventCallback<DateTime> EndTimeChanged { get; set; }
    }

    public abstract class OneButtonComponentBase : StyledActionComponent
    {
        [Parameter]
        public bool JustIcon { get; set; } = false;
        [Parameter]
        public string ButtonText { get; set; } = string.Empty;
        [Parameter]
        public Color ButtonColor { get; set; } = Color.Primary;

    }

    public abstract class BlockComponentBase : TimeRangeComponentBase
    {
        [Parameter]
        public IRepetitiveBlock Block { get; set; } = new BaseRepetitiveBlock();

        [Parameter]
        public EventCallback<IRepetitiveBlock> BlockChanged { get; set; }

        [Parameter]
        public EventCallback<IRepetitiveBlock> BlockDoubleClick { get; set; }
        [Parameter]
        public EventCallback<IRepetitiveBlock> BlockOneClick { get; set; }
        [Parameter]
        public EventCallback<IRepetitiveBlock> ChangeBlockRequest { get; set; }
        [Parameter]
        public EventCallback<IRepetitiveBlock> RemoveBlockRequest { get; set; }
    }

    public abstract class EntitiesActionButtonComponentBase : OneButtonComponentBase
    {
        [Parameter]
        public EventCallback<TreeItem> ItemChanged { get; set; }
        [Parameter]
        public TreeItem Item { get; set; }

    }

    public abstract class EntitiesComponentBase : BlockComponentBase
    {
        [Parameter]
        public EventCallback<TreeItem> ItemChanged { get; set; }
        [Parameter]
        public TreeItem? Item { get; set; }

    }
    public abstract class EntitiesBlocksComponentBase : EntitiesComponentBase
    {
        public List<IRepetitiveBlock> Blocks { get; set; } = new List<IRepetitiveBlock>();
    }

}
