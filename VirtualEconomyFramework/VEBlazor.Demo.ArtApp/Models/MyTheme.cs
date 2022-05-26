using Blazorise;

namespace VEBlazor.Demo.ArtApp.Models
{
    public static class MyTheme
    {
        public static Theme GetDefaultTheme()
        {
            return new Theme
            {
                BarOptions = new ThemeBarOptions
                {
                    DarkColors = new ThemeBarColorOptions
                    {
                        BackgroundColor = "#161718",
                        GradientBlendPercentage = 10f,
                        ItemColorOptions = new ThemeBarItemColorOptions
                        {
                            ActiveBackgroundColor = "#2B3C54",
                            ActiveColor = "#ffffff",
                            HoverBackgroundColor = "#2F3741",
                            HoverColor = "#ffffff"
                        }
                    },
                    LightColors = new ThemeBarColorOptions
                    {
                        BackgroundColor = "#4A545C",
                        GradientBlendPercentage = 10f,
                        ItemColorOptions = new ThemeBarItemColorOptions
                        {
                             
                            ActiveBackgroundColor = "#1F5C8A",
                            ActiveColor = "#137DCB",
                            HoverBackgroundColor = "rgba(20,40,220,.12)",
                            HoverColor = "#61B2EF"
                        }
                    }
                },
                ColorOptions = new ThemeColorOptions
                {
                    Primary = "#C7D4E1",
                    Secondary = "#B09F9F",
                    Success = "#1ca44e",
                    Info = "#2768f5",
                    Warning = "#698BFC",
                    Danger = "#4E4BC3",
                    Light = "#96B3F7",
                    Dark = "#0f100e",
                    Link = "#204167"
                },
                BackgroundOptions = new ThemeBackgroundOptions
                {
                    Primary = "#161718",
                    Secondary = "#3C4147",
                    Success = "#1ca44e",
                    Info = "#2768f5",
                    Warning = "#698BFC",
                    Danger = "#4E4BC3",
                    Light = "#96B3F7",
                    Dark = "#0f100e",
                    Body = "#160505"
                },
                TextColorOptions = new ThemeTextColorOptions
                {
                    Primary = "#CB0000",
                    Secondary = "#CEC1C1",
                    Success = "#1ca44e",
                    Info = "#2768f5",
                    Warning = "#698BFC",
                    Danger = "#4E4BC3",
                    Light = "#96B3F7",
                    Dark = "#0E0E10",
                    Body = "#050516",
                    White = "#7080B6"
                },
                SidebarOptions = new ThemeSidebarOptions
                {
                    BackgroundColor = "#3E4247",
                    Color = "#9D9EAE"
                },
                InputOptions = new ThemeInputOptions
                {
                    Color = "#61627D",
                    CheckColor = "#3B3E57",
                    SliderColor = "#0049CB"
                },
                BodyOptions = new ThemeBodyOptions
                {
                    BackgroundColor = "#161718",
                    TextColor = "#B1B6DE"
                }
            };
        }
    }
}
