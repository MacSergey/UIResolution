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
        public static SavedFloat UIScale { get; } = new SavedFloat(nameof(UIScale), SettingsFile, 1f, true);

        protected override void OnSettingsUI()
        {
            
        }
    }
}
