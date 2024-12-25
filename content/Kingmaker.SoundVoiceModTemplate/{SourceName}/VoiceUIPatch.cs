using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.SettingsUI;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace {SourceName};

internal class VoiceUIPatch
{
    private static Scrollbar _scrollbar;

    [HarmonyPatch]
    private static class CharBVoiceSelector_Init_Patch
    {
        [HarmonyPatch(typeof(CharBVoiceSelector), nameof(CharBVoiceSelector.Init))]
        [HarmonyPrefix]
        public static void PrePatchInit(CharBVoiceSelector __instance)
        {
            if (__instance.m_isInit) return;

            Setup(__instance.m_MaleContainer, BlueprintRoot.Instance.CharGen.MaleVoices);
            Setup(__instance.m_FemaleContainer, BlueprintRoot.Instance.CharGen.FemaleVoices);
        }

        [HarmonyPatch(typeof(CharBVoiceSelector), nameof(CharBVoiceSelector.SwitchGenderView))]
        [HarmonyPostfix]
        public static void PostSwitchGenderViewPatch(CharBVoiceSelector __instance, bool state)
        {
            // Container does actually point to container, parent is container that needs to be disabled on Gender change.
            __instance.m_FemaleContainer.parent.gameObject.SetActive(__instance.m_VoicesGender == Gender.Female);
            __instance.m_MaleContainer.parent.gameObject.SetActive(__instance.m_VoicesGender == Gender.Male);
        }

        private static void Setup(Transform containter, BlueprintUnitAsksList[] askList)
        {
            // Adds missing voice objects. Should be automatic.
            var items = containter.GetComponentsInChildren<CharBVoiceItem>();

            for (int i = items.Length; i < askList.Length; i++)
                GameObject.Instantiate(items[0], items[0].transform.parent, false);

            // Start building scroll view.
            var parent = containter.parent;
            var vLayout = parent.GetComponent<VerticalLayoutGroupWorkaround>();

            // If parent doesn't have VerticalLayoutGroupWorkaround we know the UI has already been patched.
            if (vLayout == null) return;

            // Don't need this layout.
            GameObject.DestroyImmediate(vLayout);

            var backgroud = containter.Find("Backgroud");

            // Gets a background from one of the containers and moves it to the proper place as the Mask will hide it
            // and you don't want it in the ScrollRect's content.
            if (parent.parent.Find("Backgroud") == null)
            {
                backgroud.SetParent(parent.parent);
                backgroud.SetAsFirstSibling();
            }
            else
                GameObject.DestroyImmediate(backgroud.gameObject);

            // Try to add scroll bar. Should always work but it wont fail now if one can't be found.
            Scrollbar scollBar = null;

            if (_scrollbar != null)
                scollBar = GameObject.Instantiate(_scrollbar, parent.transform, false);

            // Add Mask. Hides anything not in view of the scroll area.
            var mask = parent.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Both required by Mask above
            parent.gameObject.AddComponent<CanvasRenderer>();
            parent.gameObject.AddComponent<Image>();

            // Add ScrollRect.
            var scrollRect = parent.gameObject.AddComponent<ScrollRectExtended>();
            scrollRect.viewport = (RectTransform)parent;
            scrollRect.content = (RectTransform)containter;
            scrollRect.verticalScrollbar = scollBar;
            scrollRect.vertical = scollBar != null;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.movementType = ScrollRectExtended.MovementType.Clamped;

            // Add Layout Element since the parent of 'parent' has a VerticalLayoutElement and we need to declare the
            // minHeight or the height will be set to 0.
            var layout = parent.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 500f;

            // ScrollRect content needs this to resize based on how many elements it contains.
            var csf = containter.gameObject.AddComponent<ContentSizeFitterExtended>();
            csf.m_VerticalFit = ContentSizeFitterExtended.FitMode.MinSize;
        }
    }

    [HarmonyPatch]
    private static class CharacterBuildConroller_Patch
    {
        [HarmonyPatch(typeof(CharacterBuildController), nameof(CharacterBuildController.OnShow))]
        [HarmonyPostfix]
        static void GetInternalScrollBar(CharacterBuildController __instance)
        {
            if (_scrollbar != null)
                return;

            var scrollBar = __instance.gameObject.GetComponentInChildren<Scrollbar>();

            if (scrollBar == null)
            {
                Main.log.Error("A ScrollBar was not found!");
                return;
            }

            _scrollbar = GameObject.Instantiate(scrollBar);
        }
    }
}
