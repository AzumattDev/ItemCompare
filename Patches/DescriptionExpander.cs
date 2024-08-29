using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace ItemCompare.Patches;

public class DescriptionExpander
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipe))]
    [HarmonyAfter("org.bepinex.plugins.jewelcrafting")]
    [HarmonyPriority(Priority.VeryHigh)]
    public static class InventoryGuiUpdateRecipePatch
    {
        public static void Postfix(InventoryGui __instance)
        {
            TMP_Text? recipeDesc = __instance.m_recipeDecription;
            // Concatenate the recipe description with __instance.m_itemCraftType.text
            if (recipeDesc == null) return;
            Chainloader.PluginInfos.TryGetValue("org.bepinex.plugins.jewelcrafting", out PluginInfo? pluginInfo);
            if (pluginInfo != null && __instance.InCraftTab() && pluginInfo.Instance != null)
            {
                // Concatenate the recipe description with __instance.m_itemCraftType.text because the jewelcrafting plugin changes the text to show the chance to break the item/gem

                // Check for length of the __instance.m_itemCraftType.text to avoid adding a newline if it's empty
                if (__instance.m_itemCraftType.text.Length > 0)
                {
                    recipeDesc.text = $"{recipeDesc.text}{Environment.NewLine}{Environment.NewLine}<color=yellow>{__instance.m_itemCraftType.text}</color>";
                }

                __instance.m_itemCraftType.text = ""; // Null out the text but don't disable so that the base game or other mods can still use it properly
            }


            if (recipeDesc.GetComponent<ContentSizeFitter>() != null) return;
            ContentSizeFitter? contentSizeFitter = recipeDesc.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RecipeDescTweaks(ref recipeDesc);
            GameObject scrollRectGo = GenScrollView(ref __instance);
            RectTransform vrt = GenViewPort(ref scrollRectGo, ref recipeDesc);
            ScrollRect scrollRect = GenScrollRect(ref scrollRectGo, ref recipeDesc, ref vrt);
            GenScrollBar(ref __instance, ref scrollRectGo, ref scrollRect);
        }
    }

    internal static void RecipeDescTweaks(ref TMP_Text? recipeDesc)
    {
        if (recipeDesc != null)
        {
            recipeDesc.enableAutoSizing = false; // Vanilla makes the text much smaller the more lines there are. This will make it always the same size
            recipeDesc.fontSize = 18;
            recipeDesc.rectTransform.anchorMin = new Vector2(0, 1);
            recipeDesc.rectTransform.anchorMax = new Vector2(1, 1);
            recipeDesc.rectTransform.pivot = new Vector2(0, 1);
            recipeDesc.textWrappingMode = TextWrappingModes.Normal;
            recipeDesc.rectTransform.anchoredPosition = new Vector2(4, 4);
            recipeDesc.raycastTarget = false;
        }
    }


    private static List<string> RetrieveSetPieces(ObjectDB database, string setName)
    {
        return (from prefab in database.m_items
            where prefab != null
            select prefab.GetComponent<ItemDrop>()
            into itemDrop
            where itemDrop != null
            where itemDrop.m_itemData.m_shared.m_setName == setName
            select itemDrop.m_itemData.m_shared.m_name).ToList();
    }


    internal static GameObject GenScrollView(ref InventoryGui __instance)
    {
        GameObject scrollViewGameObject = new("RecipeExpansionScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollViewGameObject.transform.SetParent(__instance.m_recipeDecription.transform.parent, false);
        scrollViewGameObject.transform.SetSiblingIndex(0);
        RectTransform? rt = scrollViewGameObject.transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(11, -74);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 330);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 300);
        }

        scrollViewGameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);
        return scrollViewGameObject;
    }

    internal static RectTransform GenViewPort(ref GameObject scrollRectGo, ref TMP_Text? recipeDesc)
    {
        GameObject viewport = new("RecipeExpansionViewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollRectGo.transform, false);
        RectTransform? vrt = viewport.transform as RectTransform;
        vrt!.anchorMin = new Vector2(0, 0);
        vrt.anchorMax = new Vector2(1, 1);
        vrt.sizeDelta = new Vector2(0, 0);
        recipeDesc?.transform.SetParent(vrt, false);
        return vrt;
    }

    internal static ScrollRect GenScrollRect(ref GameObject scrollRectGo, ref TMP_Text? recipeDesc, ref RectTransform vrt)
    {
        ScrollRect? scrollRect = scrollRectGo.GetComponent<ScrollRect>();
        scrollRect.viewport = vrt;
        scrollRect.content = recipeDesc?.rectTransform;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        scrollRect.scrollSensitivity = 40;
        scrollRect.inertia = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.onValueChanged.RemoveAllListeners();
        return scrollRect;
    }

    internal static void GenScrollBar(ref InventoryGui __instance, ref GameObject scrollRectGo,
        ref ScrollRect scrollRect)
    {
        Scrollbar? newScrollbar = UnityEngine.Object.Instantiate(__instance.m_recipeListScroll, scrollRectGo.transform);
        newScrollbar.size = 0.4f;
        scrollRect.onValueChanged.AddListener((_) => newScrollbar.size = 0.4f);
        scrollRect.verticalScrollbar = newScrollbar;
    }
}

public static class PlayerExtentions
{
    public static IEnumerable<ItemDrop.ItemData> GetEquipment(this Player player)
    {
        List<ItemDrop.ItemData> results = new List<ItemDrop.ItemData>();
        if (player.m_rightItem != null)
            results.Add(player.m_rightItem);
        if (player.m_leftItem != null)
            results.Add(player.m_leftItem);
        if (player.m_chestItem != null)
            results.Add(player.m_chestItem);
        if (player.m_legItem != null)
            results.Add(player.m_legItem);
        if (player.m_helmetItem != null)
            results.Add(player.m_helmetItem);
        if (player.m_shoulderItem != null)
            results.Add(player.m_shoulderItem);
        if (player.m_utilityItem != null)
            results.Add(player.m_utilityItem);
        return results;
    }
}