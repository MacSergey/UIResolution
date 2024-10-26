using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using ModsCommon;
using ModsCommon.Settings;
using UnityEngine;

namespace UIResolution
{
    public class Mod : BasePatcherMod<Mod>
    {
        public override string NameRaw => "UI Resolution";
        public override string Description => Localize.Mod_Description;

        protected override ulong StableWorkshopId => 2487213155ul;
        protected override ulong BetaWorkshopId => 2487959237ul;

        public override List<ModVersion> Versions { get; } = new List<ModVersion>
        {
            new ModVersion(new Version("1.3.1"), new DateTime(2024, 10, 26)),
            new ModVersion(new Version("1.3"), new DateTime(2023, 4, 6)),
            new ModVersion(new Version("1.2"), new DateTime(2022,9,14)),
            new ModVersion(new Version("1.1.2"), new DateTime(2021,8,1)),
            new ModVersion(new Version("1.1.1"), new DateTime(2021,5,24)),
            new ModVersion(new Version("1.1"), new DateTime(2021,5,19)),
            new ModVersion(new Version("1.0"), new DateTime(2021,5,15)),
        };
        protected override Version RequiredGameVersion => new Version(1, 18, 1, 3);

#if BETA
        public override bool IsBeta => true;
#else
        public override bool IsBeta => false;
#endif

        protected override string IdRaw => nameof(UIResolution);
        protected override LocalizeManager LocalizeManager => Localize.LocaleManager;

        private static UISlider UIScaleSlider { get; set; }
        private static UILabel UIScaleLabel { get; set; }

        public static float SelectedUIScale = 1f;
        private static UIAnchorStyle TopLeft { get; } = UIAnchorStyle.Top | UIAnchorStyle.Left;
        private static UIAnchorStyle TopRight { get; } = UIAnchorStyle.Top | UIAnchorStyle.Right;
        private static UIAnchorStyle BottomLeft { get; } = UIAnchorStyle.Bottom | UIAnchorStyle.Left;
        private static UIAnchorStyle BottomRight { get; } = UIAnchorStyle.Bottom | UIAnchorStyle.Right;
        private static HashSet<string> StopList { get; } = new HashSet<string>
        {
            "WarningPhasePanel",
            "FullScreenContainer",
            "FootballPoliciesTooltip",
            "ModalEffect",
            "ChirperPanel",
            "InfoPanel",
            "Esc",
            "InfoRoadTooltip",
            "ThumbnailBar",
            "DefaultTooltip",
            "TSBar",
            "BulldozerBar",
            "CursorInfo",
            "UnlockingPanel",
            "AdvisorLocationHelper169",
            "UnlockTooltip",
            "InfoTooltip",
            "InfoAdvancedTooltip",
            "InfoAdvancedTooltipDetail",
            "InfoParkTooltipDetail",
            "InfoIndustryBuildingTooltipDetail",
            "InfoIndustryMainBuildingTooltipDetail",
            "InfoUniqueFactoryTooltipDetail",
            "AdvisorLocationHelperOthers",
            "RadioPanel",
            "InfoLandscapingTooltip",
            "(Library) ExceptionPanel",
            "(Library) ConfirmPanel",
            "(Library) ExitConfirmPanel",
            "(Library) LoadPanel",
            "(Library) SavePanel",
            "(Library) OptionsPanel",
            "(Library) BailoutPanel",
            "(Library) ZonedBuildingWorldInfoPanel",
            "(Library) CityServiceWorldInfoPanel",
            "(Library) PublicTransportWorldInfoPanel",
            "(Library) DistrictWorldInfoPanel",
            "(Library) TutorialPanel",
            "(Library) HealthInfoViewPanel",
            "(Library) OutsideConnectionsInfoViewPanel",
            "(Library) CrimeInfoViewPanel",
            "(Library) PopulationInfoViewPanel",
            "(Library) PollutionInfoViewPanel",
            "(Library) NoisePollutionInfoViewPanel",
            "(Library) WindInfoViewPanel",
            "(Library) LevelsInfoViewPanel",
            "(Library) TrafficInfoViewPanel",
            "(Library) LandValueInfoViewPanel",
            "(Library) NaturalResourcesInfoViewPanel",
            "(Library) PublicTransportInfoViewPanel",
            "(Library) ElectricityInfoViewPanel",
            "(Library) HappinessInfoViewPanel",
            "(Library) EducationInfoViewPanel",
            "(Library) WaterInfoViewPanel",
            "(Library) HeatingInfoViewPanel",
            "(Library) GarbageInfoViewPanel",
            "(Library) PauseMenu",
            "(Library) TimerConfirmPanel",
            "(Library) FireSafetyInfoViewPanel",
            "(Library) EntertainmentInfoViewPanel",
            "(Library) CitizenWorldInfoPanel",
            "(Library) TouristWorldInfoPanel",
            "(Library) AnimalWorldInfoPanel",
            "(Library) CitizenVehicleWorldInfoPanel",
            "(Library) TouristVehicleWorldInfoPanel",
            "(Library) CityServiceVehicleWorldInfoPanel",
            "(Library) PublicTransportVehicleWorldInfoPanel",
            "(Library) GameAreaInfoPanel",
            "(Library) CityInfoPanel",
            "(Library) StatisticsPanel",
            "(Library) ServicePersonWorldInfoPanel",
            "(Library) DebugOutputPanel",
            "(Library) WaitPanel",
            "(Library) PublicTransportDetailPanel",
            "(Library) ChirperOptionPanel",
            "(Library) RoadMaintenanceInfoViewPanel",
            "(Library) RoadSnowInfoViewPanel",
            "(Library) LandscapingInfoPanel",
            "(Library) FootballPanel",
            "(Library) MeteorWorldInfoPanel",
            "(Library) ShelterWorldInfoPanel",
            "(Library) StoryMessagePanel",
            "(Library) DisasterReportPanel",
            "(Library) EscapeRoutesInfoViewPanel",
            "(Library) RadioInfoViewPanel",
            "(Library) DestructionInfoViewPanel",
            "(Library) DisasterDetectionInfoViewPanel",
            "(Library) TerrainHeightInfoViewPanel",
            "(Library) DisasterRiskInfoViewPanel",
            "(Library) MessageBoxPanel",
            "(Library) TrafficRoutesInfoViewPanel",
            "(Library) RoadWorldInfoPanel",
            "(Library) SnapSettingsPanel",
            "(Library) FestivalPanel",
            "(Library) ParkWorldInfoPanel",
            "(Library) ToursInfoViewPanel",
            "(Library) ParkMaintenanceInfoViewPanel",
            "(Library) ParksOverviewPanel",
            "(Library) ParkAreaUnlockingPanel",
            "(Library) TourismInfoViewPanel",
            "(Library) ChirpXPanel",
            "(Library) IndustryWorldInfoPanel",
            "(Library) PostInfoViewPanel",
            "(Library) IndustryInfoViewPanel",
            "(Library) WarehouseWorldInfoPanel",
            "(Library) UniqueFactoryWorldInfoPanel",
            "(Library) IndustryOverviewPanel",
            "(Library) CampusWorldInfoPanel",
            "(Library) VarsitySportsArenaPanel",
            "(Library) AcademicYearReportPanel",
            "(Library) FishingInfoViewPanel",
            "(Library) TutorialsLogPanel",
            "(Library) EpicAchievementPanel",
            "(Library) AirportWorldInfoPanel",
            "(Library) PedestrianZoneWorldInfoPanel",
            "(Library) ServicePointInfoViewPanel",
        };

        protected override void Enable()
        {
            base.Enable();

            SelectedUIScale = Settings.UIScale;

            if (UIView.GetAView() is UIView view)
                SetViewSize(view, view.uiCamera.pixelWidth, view.uiCamera.pixelHeight);

            AddScale();
            ShowWhatsNew();
        }
        protected override void Disable()
        {
            base.Disable();

            SelectedUIScale = 1f;

            if (UIView.GetAView() is UIView view)
                SetViewSize(view, 1920, 1080);

            RemoveScale();
        }
        protected override void SetCulture(CultureInfo culture)
        {
            Localize.Culture = culture;

            if (UIScaleLabel != null)
                SetScaleText();
        }
        protected override void GetSettings(UIHelperBase helper)
        {
            var settings = new Settings();
            settings.OnSettingsUI(helper);
        }

        protected override bool PatchProcess()
        {
            var success = true;

            success &= Patch_Screen_SetResolution();
            success &= Patch_UIView_OnResolutionChanged();
            success &= Patch_UIView_OnResolutionChangedPrefix();
            success &= Patch_UIView_OnResolutionChangedPostfix();


            success &= Patch_CameraController_UpdateFreeCamera();
            success &= Patch_UIComponent_AttachUIComponent();


            success &= Patch_OptionsGraphicsPanel_OnApplyGraphics();
            success &= Patch_OptionsGraphicsPanel_SetSavedResolutionSettings();
            success &= Patch_OptionsGraphicsPanel_Awake();

            return success;
        }

        private bool Patch_Screen_SetResolution()
        {
            var parameters = new Type[] { typeof(int), typeof(int), typeof(bool) };
            return AddPostfix(typeof(Patcher), nameof(Patcher.SetResolutionPostfix), typeof(Screen), nameof(Screen.SetResolution), parameters);
        }

        private bool Patch_UIView_OnResolutionChanged()
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.OnResolutionChanged), typeof(UIView), "OnResolutionChanged", new Type[0]);
        }
        private bool Patch_UIView_OnResolutionChangedPrefix()
        {
            var parameters = new Type[] { typeof(Vector2), typeof(Vector2) };
            return AddPrefix(typeof(Patcher), nameof(Patcher.OnResolutionChangedPrefix), typeof(UIView), "OnResolutionChanged", parameters);
        }
        private bool Patch_UIView_OnResolutionChangedPostfix()
        {
            var parameters = new Type[] { typeof(Vector2), typeof(Vector2) };
            return AddPostfix(typeof(Patcher), nameof(Patcher.OnResolutionChangedPostfix), typeof(UIView), "OnResolutionChanged", parameters);
        }

        private bool Patch_CameraController_UpdateFreeCamera()
        {
            return AddPostfix(typeof(Patcher), nameof(Patcher.UpdateFreeCameraPostfix), typeof(CameraController), "UpdateFreeCamera");
        }
        private bool Patch_UIComponent_AttachUIComponent()
        {
            return AddPostfix(typeof(Patcher), nameof(Patcher.AttachUIComponentPostfix), typeof(UIComponent), nameof(UIComponent.AttachUIComponent));
        }

        private bool Patch_OptionsGraphicsPanel_OnApplyGraphics()
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.OnApplyGraphicsTranspiler), typeof(OptionsGraphicsPanel), nameof(OptionsGraphicsPanel.OnApplyGraphics));
        }
        private bool Patch_OptionsGraphicsPanel_SetSavedResolutionSettings()
        {
            return AddPostfix(typeof(Patcher), nameof(Patcher.SetSavedResolutionSettingsPostfix), typeof(OptionsGraphicsPanel), "SetSavedResolutionSettings");
        }
        private bool Patch_OptionsGraphicsPanel_Awake()
        {
            return AddPostfix(typeof(Patcher), nameof(Patcher.AwakePostfix), typeof(OptionsGraphicsPanel), "Awake");
        }

        public static void SetViewSize(UIView view, int width, int height)
        {
            FixAnchor(view);

            width = (int)Mathf.Max(width / SelectedUIScale, 1080f / height * width);
            height = (int)Math.Max(height / SelectedUIScale, 1080f);

            typeof(UIView).GetField("m_FixedWidth", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, width);
            AccessTools.Field(typeof(UIView), "m_FixedWidth").SetValue(view, width);
            int oldHeight = (int)AccessTools.Field(typeof(UIView), "m_FixedHeight").GetValue(view);
            AccessTools.Field(typeof(UIView), "m_FixedHeight").SetValue(view, height);
            AccessTools.Method(typeof(UIView), "OnResolutionChanged", new Type[2] { typeof(int), typeof(int) }).Invoke(view, new object[2] { oldHeight, height });
        }
        public static void FixAnchor(UIView view)
        {
            var oldSize = view.GetScreenResolution();
            foreach (var component in view.GetComponentsInChildren<UIComponent>())
            {
                if (component.parent == null && !StopList.Contains(component.name) && (component.anchor == TopLeft || component.anchor == TopRight || component.anchor == BottomLeft || component.anchor == BottomRight))
                {
                    var position = (Vector2)component.absolutePosition;
                    var center = position + component.size / 2f;
                    component.anchor = (center.x <= oldSize.x / 2f ? UIAnchorStyle.Left : UIAnchorStyle.Right) | (center.y <= oldSize.y / 2f ? UIAnchorStyle.Top : UIAnchorStyle.Bottom);

                    var x = Mathf.Max(Mathf.Min(position.x, oldSize.x - component.size.x), 0f);
                    var y = Mathf.Max(Mathf.Min(position.y, oldSize.y - component.size.y), 0f);

                    component.absolutePosition = new Vector2(x, y);
                }
            }
        }

        public static void AddScale()
        {
            if (UIView.GetAView() is UIView view && view.FindUIComponent<UIPanel>("DisplaySettings") is UIPanel displaySettings)
                AddScale(displaySettings);
        }
        public static void AddScale(UIPanel displaySettings)
        {
            if (displaySettings.Find<UIPanel>("RefreshRate") is UIPanel refreshRate)
                refreshRate.relativePosition = new Vector2(260f, 107f);

            if (displaySettings.Find<UIPanel>("Fullscreen") is UIPanel fullscreen)
                fullscreen.relativePosition = new Vector2(14f, 107f);

            if (displaySettings.Find<UIButton>("Apply") is UIButton apply)
                apply.textPadding = new RectOffset(8, 8, 8, 8);

            var uiScale = displaySettings.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;
            uiScale.height = 72f;
            uiScale.name = "UIScale";
            uiScale.relativePosition = new Vector2(503f, 35f);

            UIScaleSlider = uiScale.Find<UISlider>("Slider");
            UIScaleSlider.name = nameof(UIScaleSlider);
            UIScaleSlider.relativePosition = new Vector2(0f, 34f);
            UIScaleSlider.minValue = 0.5f;
            UIScaleSlider.maxValue = 2f;
            UIScaleSlider.stepSize = 0.05f;

            UIScaleLabel = uiScale.Find<UILabel>("Label");
            UIScaleLabel.name = nameof(UIScaleLabel);
            UIScaleLabel.relativePosition = new Vector2(0f, 2f);

            UIScaleSlider.eventValueChanged += ScaleChanged;
            UIScaleSlider.value = SelectedUIScale;
        }
        static void ScaleChanged(UIComponent component, float value)
        {
            SelectedUIScale = value;
            SetScaleText();
        }
        static void SetScaleText() => UIScaleLabel.text = string.Format(Localize.UIScale, 100 * SelectedUIScale);

        private static void RemoveScale()
        {
            if (UIView.GetAView() is UIView view && view.FindUIComponent<UIPanel>("DisplaySettings") is UIPanel displaySettings)
            {
                if (displaySettings.Find<UIPanel>("RefreshRate") is UIPanel refreshRate)
                    refreshRate.relativePosition = new Vector2(14f, 107f);

                if (displaySettings.Find<UIPanel>("Fullscreen") is UIPanel fullscreen)
                    fullscreen.relativePosition = new Vector2(503f, 35f);

                if (displaySettings.Find<UIButton>("Apply") is UIButton apply)
                    apply.textPadding = new RectOffset(32, 32, 8, 8);

                if (displaySettings.Find<UIPanel>("UIScale") is UIPanel uiResolution)
                {
                    uiResolution.parent?.RemoveUIComponent(uiResolution);

                    foreach (var component in uiResolution.GetComponentsInChildren<UIComponent>())
                    {
                        GameObject.Destroy(component.gameObject);
                        GameObject.Destroy(component);
                    }

                    GameObject.Destroy(uiResolution.gameObject);
                    GameObject.Destroy(uiResolution);
                }
            }
        }
    }

    public class Settings : BaseSettings<Mod>
    {
        public static SavedFloat UIScale { get; } = new SavedFloat(nameof(UIScale), SettingsFile, 1f, true);

        protected override void FillSettings()
        {
            base.FillSettings();
            AddNotifications(GeneralTab);
        }
    }

    public static class Patcher
    {
        private delegate void OnResolutionDelegate(UIComponent component, Vector2 previousResolution, Vector2 currentResolution);
        private static OnResolutionDelegate UIComponentOnResolutionChanged { get; }
        static Patcher()
        {
            UIComponentOnResolutionChanged = AccessTools.MethodDelegate<OnResolutionDelegate>(AccessTools.Method(typeof(UIComponent), "OnResolutionChanged"), virtualCall: true);
        }

        public static void SetResolutionPostfix(int width, int height)
        {
            if (UIView.GetAView() is UIView view)
                Mod.SetViewSize(view, width, height);
        }
        public static bool OnResolutionChanged(UIView __instance)
        {
            Mod.SetViewSize(__instance, __instance.uiCamera.pixelWidth, __instance.uiCamera.pixelHeight);
            return false;
        }

        public static void OnResolutionChangedPrefix(UIView __instance, Vector2 oldSize, Vector2 currentSize)
        {
            SingletonMod<Mod>.Logger.Debug($"Resolution changed from {oldSize} to {currentSize} camera {__instance.uiCamera.pixelRect.max}");

            if (__instance.FindUIComponent<UITextureSprite>("BackgroundSprite2") is UITextureSprite background)
            {
                background.size = currentSize;
                background.parent.size = currentSize;
            }

            if (__instance.FindUIComponent<UIPanel>("MenuContainer") is UIPanel menuContainer)
                menuContainer.anchor = UIAnchorStyle.Proportional | UIAnchorStyle.CenterHorizontal;

            if (__instance.FindUIComponent<UIPanel>("WorkshopAdPanel") is UIPanel workshopAdPanel)
                workshopAdPanel.anchor &= ~UIAnchorStyle.Proportional;

            if (__instance.FindUIComponent<UIPanel>("FullScreenContainer") is UIPanel fullScreenContainer)
                fullScreenContainer.anchor = UIAnchorStyle.All;
        }
        public static void OnResolutionChangedPostfix(UIView __instance, Vector2 oldSize, Vector2 currentSize)
        {
            if (__instance.FindUIComponent<UIPanel>("FullScreenContainer") is UIPanel fullScreenContainer)
            {
                fullScreenContainer.width = currentSize.x;
                fullScreenContainer.relativePosition = Vector2.zero;
            }
            if (__instance.FindUIComponent<UIPanel>("InfoPanel") is UIPanel infoPanel)
            {
                var delta = Mathf.Max(currentSize.y - 1080f, 0f);

                if (infoPanel.Find<UITabContainer>("InfoViewsContainer") is UITabContainer infoViewsContainer)
                    infoViewsContainer.relativePosition = new Vector2(5f, -978f - delta);
                if (infoPanel.Find<UITabstrip>("InfoMenu") is UITabstrip infoMenu)
                    infoMenu.relativePosition = new Vector2(15f, -1026f - delta);
            }
        }

        public static void UpdateFreeCameraPostfix(Camera ___m_camera, bool ___m_cachedFreeCamera)
        {
            if (!___m_cachedFreeCamera && UIView.GetAView() is UIView view)
            {
                var shift = 0.105f * 1080f / view.fixedHeight;
                ___m_camera.rect = new Rect(0f, shift, 1f, 1f - shift);
            }
        }

        public static void AttachUIComponentPostfix(UIComponent __result)
        {
            if (__result.GetUIView() is UIView view)
            {
                var previousResolution = new Vector2(1920, 1080);
                var currentResolution = new Vector2(view.fixedWidth, view.fixedHeight);

                var components = __result.GetComponentsInChildren<UIComponent>();
                Array.Sort(components, RenderSortFunc);

                foreach (var component in components)
                    UIComponentOnResolutionChanged(component, previousResolution, currentResolution);

                foreach (var component in components)
                    component.PerformLayout();
            }

            static int RenderSortFunc(UIComponent lhs, UIComponent rhs) => lhs.renderOrder.CompareTo(rhs.renderOrder);
        }

        public static IEnumerable<CodeInstruction> OnApplyGraphicsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var bneUnLabel = default(Label);
            var additionalLabel = generator.DefineLabel();

            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Bne_Un)
                    bneUnLabel = (Label)instruction.operand;
                else if (instruction.opcode == OpCodes.Beq)
                {
                    yield return new CodeInstruction(OpCodes.Bne_Un, additionalLabel);

                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Mod), nameof(Mod.SelectedUIScale)));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Settings), nameof(Settings.UIScale)));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(SavedFloat), nameof(SavedFloat.value)));
                }
                else if (instruction.labels.Contains(bneUnLabel))
                    instruction.labels.Add(additionalLabel);

                yield return instruction;
            }
        }
        public static void SetSavedResolutionSettingsPostfix() => Settings.UIScale.value = Mod.SelectedUIScale;

        public static void AwakePostfix(OptionsGraphicsPanel __instance)
        {
            if (__instance.Find<UIPanel>("DisplaySettings") is UIPanel displaySettings)
                Mod.AddScale(displaySettings);
        }
    }
}
