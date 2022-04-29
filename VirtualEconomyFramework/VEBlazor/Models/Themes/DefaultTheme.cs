using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VEBlazor.Models.Themes
{
    public static class DefaultTheme
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
                            ActiveBackgroundColor = "#6366f1",
                            ActiveColor = "#ffffff",
                            HoverBackgroundColor = "rgba(99,102,241,.12)",
                            HoverColor = "#6366f1"
                        }
                    }
                },
                ColorOptions = new ThemeColorOptions
                {
                    Primary = "#6366f1",
                    Secondary = "#cfd8f6",
                    Success = "#1ca44e",
                    Info = "#2768f5",
                    Warning = "#e1a200",
                    Danger = "#e1a200",
                    Light = "#d8dfe8",
                    Dark = "#0f100e",
                    Link = "#204167",
                },
                BackgroundOptions = new ThemeBackgroundOptions
                {
                    Primary = "#6366f1",
                    Secondary = "#cfd8f6",
                    Success = "#1ca44e",
                    Info = "#2768f5",
                    Warning = "#e1a200",
                    Danger = "#e1a200",
                    Light = "#d8dfe8",
                    Dark = "#0f100e",
                },
                TextColorOptions = new ThemeTextColorOptions
                {
                    Primary = "#6366f1",
                    Secondary = "#cfd8f6",
                    Success = "#1ca44e",
                    Info = "#2768f5",
                    Warning = "#e1a200",
                    Danger = "#e1a200",
                    Light = "#d8dfe8",
                    Dark = "#0f100e",
                },
                //ButtonOptions = new ThemeButtonOptions
                //{
                //    BorderRadius = "0",
                //},
                //CardOptions = new ThemeCardOptions
                //{
                //    BorderRadius = "0",
                //},
                //ModalOptions = new ThemeModalOptions
                //{
                //    BorderRadius = "0",
                //},
                //ProgressOptions = new ThemeProgressOptions
                //{
                //    BorderRadius = "0",
                //},
            };
        }
    }
}
