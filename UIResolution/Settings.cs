using ColossalFramework;
using ModsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UIResolution
{
    public class Settings : BaseSettings<Mod>
    {
        //public static Resolution Resolution
        //{
        //    get => new Resolution() { height = ResolutionHeight, width = ResolutionWidth };
        //    set
        //    {
        //        ResolutionHeight.value = value.height;
        //        ResolutionWidth.value = value.width;
        //    }
        //}
        //public static SavedInt ResolutionHeight { get; } = new SavedInt(nameof(ResolutionHeight), SettingsFile, 1080, true);
        //public static SavedInt ResolutionWidth { get; } = new SavedInt(nameof(ResolutionWidth), SettingsFile, 1920, true);

        public static SavedFloat UIScale { get; } = new SavedFloat(nameof(UIScale), SettingsFile, 1f, true);

        protected override void OnSettingsUI()
        {
            
        }
    }
}
