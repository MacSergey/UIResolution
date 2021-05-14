using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Vector2 DefaulSize => new Vector2(1920f, 1080f);
        public override string NameRaw => "UI Resolution";
        public override string Description => "Change game UI resolution";

        public override string WorkshopUrl => string.Empty;
        public override string BetaWorkshopUrl => string.Empty;

        public override List<Version> Versions { get; } = new List<Version>
        {
            new Version("1.0"),
        };

        public override bool IsBeta => true;
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
            success &= Patch_UIView_OnResolutionChanged2();
            success &= Patch_CameraController_UpdateFreeCamera();

            return success;
        }

        private bool Patch_Screen_SetResolution()
        {
            var parameters = new Type[] { typeof(int), typeof(int), typeof(bool) };
            return AddPostfix(typeof(Mod), nameof(Mod.SetResolutionPostfix), typeof(Screen), nameof(Screen.SetResolution), parameters);
        }
        private bool Patch_UIView_OnResolutionChanged2()
        {
            var parameters = new Type[] { typeof(Vector2), typeof(Vector2) };
            return AddPrefix(typeof(Mod), nameof(Mod.OnResolutionChangedPrefix2), typeof(UIView), "OnResolutionChanged", parameters);
        }
        private bool Patch_CameraController_UpdateFreeCamera()
        {
            return AddPostfix(typeof(Mod), nameof(Mod.UpdateFreeCameraPostfix), typeof(CameraController), "UpdateFreeCamera");
        }

        private static void SetViewHeight(UIView view, int height) => view.fixedHeight = Math.Max(height, 1080);
        private static void SetResolutionPostfix(int width, int height)
        {
            if (UIView.GetAView() is UIView view)
                SetViewHeight(view, height);
        }

        private static void OnResolutionChangedPrefix2(UIView __instance, Vector2 currentSize)
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
    }
}
