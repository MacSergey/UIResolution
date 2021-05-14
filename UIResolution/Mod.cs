using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public override string BetaWorkshopUrl => string.Empty;

        public override List<Version> Versions { get; } = new List<Version>
        {
            new Version("1.0"),
        };

#if DEBUG
        public override bool IsBeta => true;
#else
        public override bool IsBeta => true;
#endif

        protected override string IdRaw => nameof(UIResolution);

        protected override void GetSettings(UIHelperBase helper)
        {
            var settings = new Settings();
            settings.OnSettingsUI(helper);
        }
        protected override void Enable()
        {
            base.Enable();

            if (UIView.GetAView() is UIView view)
                SetViewHeight(view, view.uiCamera.pixelHeight);
        }
        protected override void Disable()
        {
            base.Disable();

            if (UIView.GetAView() is UIView view)
                SetViewHeight(view, 1080);
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

            return success;
        }

        private bool Patch_Screen_SetResolution()
        {
            var parameters = new Type[] { typeof(int), typeof(int), typeof(bool) };
            return AddPostfix(typeof(Mod), nameof(Mod.SetResolutionPostfix), typeof(Screen), nameof(Screen.SetResolution), parameters);
        }

        private bool Patch_UIView_OnResolutionChanged()
        {
            return AddPrefix(typeof(Mod), nameof(Mod.OnResolutionChanged), typeof(UIView), "OnResolutionChanged");
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

        private static void SetViewHeight(UIView view, int height) => view.fixedHeight = Math.Max(height, 1080);
        private static void SetResolutionPostfix(int height)
        {
            if (UIView.GetAView() is UIView view)
                SetViewHeight(view, height);
        }
        private static bool OnResolutionChanged(UIView __instance)
        {
            SetViewHeight(__instance, __instance.uiCamera.pixelHeight);
            return false;
        }
        private static void OnResolutionChangedPrefix(UIView __instance, Vector2 currentSize)
        {
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
    }
}
