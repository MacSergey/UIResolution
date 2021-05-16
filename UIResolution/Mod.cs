using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using ModsCommon;
using UnityEngine;

namespace UIResolution
{
    public class Mod : BasePatcherMod<Mod>
    {
        private static MethodInfo UIComponentOnResolutionChanged { get; } = AccessTools.Method(typeof(UIComponent), "OnResolutionChanged");
        public override string NameRaw => "UI Resolution";
        public override string Description => "Change game UI resolution";

        public override string WorkshopUrl => "https://steamcommunity.com/sharedfiles/filedetails/?edit=true&id=2487213155";
        public override string BetaWorkshopUrl => "https://steamcommunity.com/sharedfiles/filedetails/?id=2487959237";

        public override List<Version> Versions { get; } = new List<Version>
        {
            new Version("1.0"),
        };

#if BETA
        public override bool IsBeta => true;
#else
        public override bool IsBeta => false;
#endif

        protected override string IdRaw => nameof(UIResolution);
        private static UISlider UIScaleSlider { get; set; }
        private static UILabel UIScaleLabel { get; set; }
        private static Dictionary<string, string> LocaleDic { get; } = new Dictionary<string, string>()
        {
            {string.Empty, "UI Scale ({0}%)" },
            {"de", "UI-Skalierung ({0}%)" },
            {"en", "UI Scale ({0}%)" },
            {"es", "Escala de UI ({0}%)" },
            {"fr", "Échelle de UI ({0}%)" },
            {"it", "Scala UI ({0}%)" },
            {"jp", "UIの拡大率 ({0}%)" },
            {"nl", "UI schaal ({0}%)" },
            {"pl", "Skalowanie UI ({0}%)" },
            {"pt", "Escala da UI ({0}%)" },
            {"ru", "Масштаб UI ({0}%)" },
            {"zh", "UI缩放 ({0}%)" },
        };
        public static float SelectedUIScale;

        protected override void GetSettings(UIHelperBase helper)
        {
            var settings = new Settings();
            settings.OnSettingsUI(helper);
        }
        protected override void Enable()
        {
            base.Enable();
            AddScale();

            if (UIView.GetAView() is UIView view)
                SetViewSize(view, view.uiCamera.pixelWidth, view.uiCamera.pixelHeight);
        }
        protected override void Disable()
        {
            base.Disable();

            if (UIView.GetAView() is UIView view)
                SetViewSize(view, 1920, 1080);

            RemoveScale();
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
            return AddPostfix(typeof(Mod), nameof(Mod.SetResolutionPostfix), typeof(Screen), nameof(Screen.SetResolution), parameters);
        }

        private bool Patch_UIView_OnResolutionChanged()
        {
            return AddPrefix(typeof(Mod), nameof(Mod.OnResolutionChanged), typeof(UIView), "OnResolutionChanged", new Type[0]);
        }
        private bool Patch_UIView_OnResolutionChangedPrefix()
        {
            var parameters = new Type[] { typeof(Vector2), typeof(Vector2) };
            return AddPrefix(typeof(Mod), nameof(Mod.OnResolutionChangedPrefix), typeof(UIView), "OnResolutionChanged", parameters);
        }
        private bool Patch_UIView_OnResolutionChangedPostfix()
        {
            var parameters = new Type[] { typeof(Vector2), typeof(Vector2) };
            return AddPostfix(typeof(Mod), nameof(Mod.OnResolutionChangedPostfix), typeof(UIView), "OnResolutionChanged", parameters);
        }

        private bool Patch_CameraController_UpdateFreeCamera()
        {
            return AddPostfix(typeof(Mod), nameof(Mod.UpdateFreeCameraPostfix), typeof(CameraController), "UpdateFreeCamera");
        }
        private bool Patch_UIComponent_AttachUIComponent()
        {
            return AddPostfix(typeof(Mod), nameof(Mod.AttachUIComponentPostfix), typeof(UIComponent), nameof(UIComponent.AttachUIComponent));
        }
        private bool Patch_OptionsGraphicsPanel_OnApplyGraphics()
        {
            return AddTranspiler(typeof(Mod), nameof(Mod.OnApplyGraphicsTranspiler), typeof(OptionsGraphicsPanel), nameof(OptionsGraphicsPanel.OnApplyGraphics));
        }
        private bool Patch_OptionsGraphicsPanel_SetSavedResolutionSettings()
        {
            return AddPostfix(typeof(Mod), nameof(Mod.SetSavedResolutionSettingsPostfix), typeof(OptionsGraphicsPanel), "SetSavedResolutionSettings");
        }
        private bool Patch_OptionsGraphicsPanel_Awake()
        {
            return AddPostfix(typeof(Mod), nameof(Mod.AwakePostfix), typeof(OptionsGraphicsPanel), "Awake");
        }


        private static void SetViewSize(UIView view, int width, int height)
        {
            width = (int)Mathf.Max(width / SelectedUIScale, 1080f / height * width);
            height = (int)Math.Max(height / SelectedUIScale, 1080f);

            AccessTools.Field(typeof(UIView), "m_FixedWidth").SetValue(view, width);
            view.fixedHeight = height;
        }
        private static void SetResolutionPostfix(int width, int height)
        {
            if (UIView.GetAView() is UIView view)
                SetViewSize(view, width, height);
        }
        private static bool OnResolutionChanged(UIView __instance)
        {
            SetViewSize(__instance, __instance.uiCamera.pixelWidth, __instance.uiCamera.pixelHeight);
            return false;
        }
        private static void OnResolutionChangedPrefix(UIView __instance, Vector2 oldSize, Vector2 currentSize)
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
        private static void OnResolutionChangedPostfix(UIView __instance, Vector2 oldSize, Vector2 currentSize)
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

        private static void UpdateFreeCameraPostfix(Camera ___m_camera, bool ___m_cachedFreeCamera)
        {
            if (!___m_cachedFreeCamera && UIView.GetAView() is UIView view)
            {
                var shift = 0.105f * 1080f / view.fixedHeight;
                ___m_camera.rect = new Rect(0f, shift, 1f, 1f - shift);
            }
        }

        private static void AttachUIComponentPostfix(UIComponent __result)
        {
            if (__result.GetUIView() is UIView view)
            {
                var parameters = new object[] { new Vector2(1920, 1080), new Vector2(view.fixedHeight * view.uiCamera.aspect, view.fixedHeight) };

                UIComponentOnResolutionChanged.Invoke(__result, parameters);
                __result.PerformLayout();

                var components = __result.GetComponentsInChildren<UIComponent>();
                foreach (var component in components)
                {
                    UIComponentOnResolutionChanged.Invoke(component, parameters);
                    component.PerformLayout();
                }
            }
        }
        private static IEnumerable<CodeInstruction> OnApplyGraphicsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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
        private static void SetSavedResolutionSettingsPostfix() => Settings.UIScale.value = SelectedUIScale;

        private static void AwakePostfix(OptionsGraphicsPanel __instance)
        {
            if (__instance.Find<UIPanel>("DisplaySettings") is UIPanel displaySettings)
                AddScale(displaySettings);
        }

        private static void AddScale()
        {
            if (UIView.GetAView() is UIView view && view.FindUIComponent<UIPanel>("DisplaySettings") is UIPanel displaySettings)
                AddScale(displaySettings);
        }
        private static void AddScale(UIPanel displaySettings)
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
            UIScaleSlider.stepSize = 0.1f;

            UIScaleLabel = uiScale.Find<UILabel>("Label");
            UIScaleLabel.name = nameof(UIScaleLabel);
            UIScaleLabel.relativePosition = new Vector2(0f, 2f);
            UIScaleLabel.localeID = "OPTIONS_RESOLUTION";
            UIScaleLabel.isLocalized = true;

            UIScaleSlider.eventValueChanged += ScaleChanged;
            UIScaleSlider.value = Settings.UIScale;

            static void LabelTextChanged(UIComponent component, string value)
            {
                var locale = SingletonLite<LocaleManager>.instance.language;
                if (!LocaleDic.ContainsKey(locale))
                    locale = string.Empty;

                UIScaleLabel.eventTextChanged -= LabelTextChanged;
                UIScaleLabel.text = string.Format(LocaleDic[locale], 100 * SelectedUIScale);
                UIScaleLabel.eventTextChanged += LabelTextChanged;
            }

            static void ScaleChanged(UIComponent component, float value)
            {
                SelectedUIScale = value;
                LabelTextChanged(UIScaleLabel, UIScaleLabel.text);
            }
        }

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

                    foreach(var component in uiResolution.GetComponentsInChildren<UIComponent>())
                        GameObject.Destroy(component);

                    GameObject.Destroy(uiResolution);
                }
            }
        }
    }
}
