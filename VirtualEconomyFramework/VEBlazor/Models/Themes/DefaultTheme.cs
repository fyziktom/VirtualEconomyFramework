using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VEBlazor.Models.Themes
{
    internal static class DefaultTheme
    {
        public static Theme GetDefaultTheme()
        {
            return new Theme
            {
                BarOptions = new ThemeBarOptions
                {
                    DarkColors = new ThemeBarColorOptions
                    {
                        BackgroundColor = "#0071c1",
                        GradientBlendPercentage = 10,
                    },
                    LightColors = new ThemeBarColorOptions
                    {
                        ItemColorOptions = new ThemeBarItemColorOptions
                        {
                            ActiveBackgroundColor = "#6366F1",
                            ActiveColor = "#ffffff",
                            HoverBackgroundColor = "#585aad",
                        }
                    }
                },
                ColorOptions = new ThemeColorOptions
                {
                    Primary = "#6366F1",
                    Secondary = "#566ea8",
                    Success = "#12a55d",
                    Info = "#AEC5EB",
                    Warning = "#e4cb12",
                    Danger = "#e4cb12",
                    Light = "#d8dfe8",
                    Dark = "#0f100e",
                    Link = "#204167",
                },
                BackgroundOptions = new ThemeBackgroundOptions
                {
                    Primary = "#6366F1",
                    Secondary = "#566ea8",
                    Success = "#12a55d",
                    Info = "#AEC5EB",
                    Warning = "#e4cb12",
                    Danger = "#e4cb12",
                    Light = "#d8dfe8",
                    Dark = "#0f100e",
                },
                TextColorOptions = new ThemeTextColorOptions
                {
                    Primary = "#6366F1",
                    Secondary = "#566ea8",
                    Success = "#12a55d",
                    Info = "#AEC5EB",
                    Warning = "#e4cb12",
                    Danger = "#e4cb12",
                    Light = "#d8dfe8",
                    Dark = "#0f100e",
                },
                ButtonOptions = new ThemeButtonOptions
                {
                    BorderRadius = "0",
                },
                CardOptions = new ThemeCardOptions
                {
                    BorderRadius = "0",
                },
                ModalOptions = new ThemeModalOptions
                {
                    BorderRadius = "0",
                },
                ProgressOptions = new ThemeProgressOptions
                {
                    BorderRadius = "0",
                },
            };
        }
    }
}
