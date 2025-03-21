﻿using System;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Jewelcrafting;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace ItemCompare.Patches;

// Not the best approach, but since my other attempts have failed, I say fuck it.
[HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
static class InventoryGridCreateItemTooltipPatch
{
    public static GameObject ClonedTooltip = null!;

    [HarmonyPriority(Priority.Last)]
    [HarmonyAfter("org.bepinex.plugins.jewelcrafting", "randyknapp.mods.epicloot")]
    public static void Postfix(ItemDrop.ItemData item, UITooltip tooltip, InventoryGrid __instance)
    {
        if (!ItemComparePlugin.HoverKeybind.Value.IsKeyHeld() && ItemComparePlugin.KeyHoldNeeded.Value != ItemComparePlugin.Toggle.Off) return;
        ItemDrop.ItemData? equippedItem = Util.FindEquippedItemMatching(item);
        if (equippedItem == null) return;
        if (equippedItem == item) return;

        if (ClonedTooltip != null)
        {
            Object.Destroy(ClonedTooltip);
            ClonedTooltip = null;
        }

        GameObject originalPrefab = tooltip.m_tooltipPrefab;
        if (originalPrefab == null) return;
        ClonedTooltip = Object.Instantiate(originalPrefab, tooltip.transform.GetComponentInParent<Canvas>().transform);


        RectTransform originalRT = tooltip.GetComponent<RectTransform>();
        RectTransform clonedRT = ClonedTooltip.GetComponent<RectTransform>();
        clonedRT.sizeDelta = originalRT.sizeDelta;
        clonedRT.anchorMin = originalRT.anchorMin;
        clonedRT.anchorMax = originalRT.anchorMax;
        clonedRT.pivot = originalRT.pivot;
        clonedRT.position = originalRT.position;
        clonedRT.localScale = originalRT.localScale;
        clonedRT.localRotation = originalRT.localRotation;
        clonedRT.localEulerAngles = originalRT.localEulerAngles;
        clonedRT.localPosition = originalRT.localPosition;
        clonedRT.offsetMin = originalRT.offsetMin;
        clonedRT.offsetMax = originalRT.offsetMax;
        //clonedRT.anchoredPosition = originalRT.anchoredPosition + new Vector2(originalRT.rect.width * 3 + clonedRT.rect.width, 0);


        Utils.ClampUIToScreen(clonedRT);
        UpdateClonedTooltipText(ClonedTooltip, item);
    }

    private static void UpdateClonedTooltipText(GameObject clonedTooltip, ItemDrop.ItemData hoveredItem)
    {
        ItemDrop.ItemData? matchingItem = Util.FindEquippedItemMatching(hoveredItem);
        string colorHexHover = Util.ColorToHexString(API.IsLoaded() ? API.GetSocketableItemColor(hoveredItem) ?? Color.yellow : Color.yellow);
        string colorHex = matchingItem != null ? Util.ColorToHexString(API.IsLoaded() ? API.GetSocketableItemColor(matchingItem) ?? Color.yellow : Color.yellow) : "FFFFFF";
        string? equippingText = Localization.instance.Localize("$hud_equipping");
        string comparisonText = $"{Environment.NewLine}{equippingText} <color=#{colorHexHover}>{Localization.instance.Localize(hoveredItem.m_shared.m_name)}</color> changes the following stats:{Environment.NewLine}{Environment.NewLine}" + GenerateComparisonText(hoveredItem);
        string comparisonTopic = $"<color=#{colorHex}>{Localization.instance.Localize(matchingItem?.m_shared.m_name)} (Equipped)</color>";
        if (API.IsLoaded())
        {
            if (API.GetJewelcraftingTooltipRoot(clonedTooltip) is { } jcRoot)
            {
                API.FillItemContainerTooltip(matchingItem, jcRoot, false);
            }
        }


        // Find and update the "Text" component with comparison data
        Transform textTransform = Utils.FindChild(clonedTooltip.transform, "Text");
        if (textTransform != null)
        {
            TMP_Text textComponent = textTransform.GetComponent<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = comparisonText; // Set or append comparison text
            }
        }

        // Find and update the "Topic" component with the new topic
        Transform topicTransform = Utils.FindChild(clonedTooltip.transform, "Topic");
        if (topicTransform != null)
        {
            TMP_Text topicComponent = topicTransform.GetComponent<TMP_Text>();
            if (topicComponent != null)
            {
                topicComponent.text = comparisonTopic; // Set the new topic
            }
        }
    }


    internal static string GenerateComparisonText(ItemDrop.ItemData hoveredItem)
    {
        StringBuilder comparisonText = new();
        Player player = Player.m_localPlayer;
        ItemDrop.ItemData? equippedItem = Util.FindEquippedItemMatching(hoveredItem);

        if (equippedItem != null)
        {
            Util.AddDurabilityComparison(hoveredItem, equippedItem, comparisonText);
            Util.AddWeightComparison(hoveredItem, equippedItem, comparisonText);
            Util.AddUseComparison(hoveredItem, equippedItem, comparisonText);
            Util.AddDamageComparison(hoveredItem, equippedItem, comparisonText, player);
            Util.AddArmorComparison(hoveredItem, equippedItem, comparisonText);
            Util.AddValueComparison(hoveredItem, equippedItem, comparisonText);
            Util.AddOtherStatComparison(hoveredItem, equippedItem, comparisonText);
            Util.AddSeInformation(hoveredItem, equippedItem, comparisonText);
        }

        return Localization.instance.Localize(comparisonText.ToString());
    }
}

[HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), typeof(ItemDrop.ItemData), typeof(int), typeof(bool), typeof(float), typeof(int))]
[HarmonyBefore("Azumatt.BindOnEquip")]
static class ItemDropItemDataGetTooltipPatch
{
    [UsedImplicitly]
    [HarmonyPriority(Priority.Last)]
    static void Postfix(ItemDrop.ItemData item, bool crafting, ref string __result)
    {
        if (crafting)
        {
            string stringToAdd = "";
            if (item.m_dropPrefab is { } prefab)
            {
                ItemDrop.ItemData? equippedItemMatching = Util.FindEquippedItemMatching(item);
                if (equippedItemMatching != null)
                {
                    string colorHexHover = Util.ColorToHexString(API.IsLoaded() ? API.GetSocketableItemColor(item) ?? Color.yellow : Color.yellow);
                    string colorHex = Util.ColorToHexString(API.IsLoaded() ? API.GetSocketableItemColor(equippedItemMatching) ?? Color.yellow : Color.yellow);
                    string comparisonHeader = $"{Environment.NewLine}{Environment.NewLine}<color=#{colorHexHover}>{Localization.instance.Localize(item.m_shared.m_name)}</color> vs. <color=#{colorHex}>{Localization.instance.Localize(equippedItemMatching?.m_shared.m_name)} (Equipped)</color>:{Environment.NewLine}{Environment.NewLine}";
                    stringToAdd += comparisonHeader + InventoryGridCreateItemTooltipPatch.GenerateComparisonText(item);
                }

                if (ItemComparePlugin.ShowConversionsInTooltip.Value == ItemComparePlugin.Toggle.On)
                {
                    if (GatherConversions.CookingStationConversions.TryGetValue(item.m_shared.m_name, out ItemDrop? stationConversion))
                    {
                        stringToAdd += $"{Environment.NewLine}{Environment.NewLine}Turns into:{Environment.NewLine}";
                        stringToAdd += Localization.instance.Localize(stationConversion?.m_itemData?.m_shared.m_name) + Environment.NewLine;
                        stringToAdd += stationConversion?.m_itemData?.GetTooltip();
                    }

                    if (GatherConversions.FermenterConversions.TryGetValue(item.m_shared.m_name, out ItemDrop? fermentConversion))
                    {
                        stringToAdd += $"{Environment.NewLine}{Environment.NewLine}Turns into:{Environment.NewLine}";
                        stringToAdd += Localization.instance.Localize(fermentConversion?.m_itemData?.m_shared.m_name) + Environment.NewLine;
                        stringToAdd += fermentConversion?.m_itemData?.GetTooltip();
                    }
                }

                __result += stringToAdd;
            }
        }
    }
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnHoverStart))]
static class UITooltipOnHoverStartPatch
{
    public static void Postfix()
    {
        if (InventoryGridCreateItemTooltipPatch.ClonedTooltip != null)
        {
            InventoryGridCreateItemTooltipPatch.ClonedTooltip.SetActive(true);
        }
    }
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnPointerExit))]
public static class UITooltipOnPointerExitPatch
{
    public static void Prefix()
    {
        if (InventoryGridCreateItemTooltipPatch.ClonedTooltip == null) return;
        Object.Destroy(InventoryGridCreateItemTooltipPatch.ClonedTooltip);
        InventoryGridCreateItemTooltipPatch.ClonedTooltip = null!;
    }
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.HideTooltip))]
static class UITooltipHideTooltipPatch
{
    static void Prefix(UITooltip __instance)
    {
        if (InventoryGridCreateItemTooltipPatch.ClonedTooltip == null) return;
        Object.Destroy(InventoryGridCreateItemTooltipPatch.ClonedTooltip);
        InventoryGridCreateItemTooltipPatch.ClonedTooltip = null!;
    }
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.LateUpdate))]
static class UITooltipLateUpdatePatch
{
    static void Postfix(UITooltip __instance)
    {
        if (InventoryGridCreateItemTooltipPatch.ClonedTooltip == null) return;
        RectTransform originalRT = __instance.GetComponent<RectTransform>();
        RectTransform tooltipRT = (API.IsLoaded() ? API.GetJewelcraftingTooltipRoot(InventoryGridCreateItemTooltipPatch.ClonedTooltip) ?? InventoryGridCreateItemTooltipPatch.ClonedTooltip.transform : InventoryGridCreateItemTooltipPatch.ClonedTooltip.transform).GetComponent<RectTransform>();
        if (UITooltip.m_current != null && !UITooltip.m_tooltip.activeSelf)
        {
            __instance.m_showTimer += Time.deltaTime;
            if (__instance.m_showTimer > 0.5 || ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
                InventoryGridCreateItemTooltipPatch.ClonedTooltip.SetActive(true);
        }

        if (ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
        {
            if (__instance.m_gamepadFocusObject != null)
            {
                if (__instance.m_gamepadFocusObject.activeSelf && UITooltip.m_current != __instance)
                    InventoryGridCreateItemTooltipPatch.ClonedTooltip.SetActive(true);
                else if (!__instance.m_gamepadFocusObject.activeSelf && UITooltip.m_current == __instance)
                    Util.DestroyTooltip();
            }
            else if (__instance.m_selectable)
            {
                if (EventSystem.current.currentSelectedGameObject == __instance.m_selectable.gameObject && UITooltip.m_current != __instance)
                    InventoryGridCreateItemTooltipPatch.ClonedTooltip.SetActive(true);
                else if (EventSystem.current.currentSelectedGameObject != __instance.m_selectable.gameObject && UITooltip.m_current == __instance)
                    Util.DestroyTooltip();
            }

            if (!(UITooltip.m_current == __instance) || !(UITooltip.m_tooltip != null))
                return;
            Vector2 tooltipTranslation = (Vector3.right * (originalRT.rect.width * 4 + tooltipRT.rect.width));
            if (__instance.m_anchor != null)
            {
                InventoryGridCreateItemTooltipPatch.ClonedTooltip.transform.SetParent(__instance.m_anchor);
                InventoryGridCreateItemTooltipPatch.ClonedTooltip.transform.localPosition = __instance.m_fixedPosition + tooltipTranslation;
            }
            else if (__instance.m_fixedPosition != Vector2.zero)
            {
                InventoryGridCreateItemTooltipPatch.ClonedTooltip.transform.position = __instance.m_fixedPosition + tooltipTranslation;
            }
            else
            {
                RectTransform? transform = __instance.gameObject.transform as RectTransform;
                Vector3[] vector3Array = new Vector3[4];
                Vector3[] fourCornersArray = vector3Array;
                transform?.GetWorldCorners(fourCornersArray);
                InventoryGridCreateItemTooltipPatch.ClonedTooltip.transform.position = (vector3Array[1] + vector3Array[2]) / 2f + (Vector3)tooltipTranslation;
                Utils.ClampUIToScreen(InventoryGridCreateItemTooltipPatch.ClonedTooltip.transform as RectTransform);
            }
        }
        else
        {
            if (UITooltip.m_current != __instance)
                return;
            if (UITooltip.m_hovered == null)
                Util.DestroyTooltip();
            else if (UITooltip.m_tooltip.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(UITooltip.m_hovered.transform as RectTransform, ZInput.mousePosition))
            {
                Util.DestroyTooltip();
            }
            else
            {
                if (API.IsLoaded())
                {
                    if (API.GetJewelcraftingTooltipRoot(InventoryGridCreateItemTooltipPatch.ClonedTooltip) is { } jcRoot)
                    {
                        jcRoot.transform.position = ZInput.mousePosition + Vector3.right * (originalRT.rect.width + tooltipRT.rect.width); // + Vector3.up * (originalRT.rect.height / 6.5f + tooltipRT.rect.height / 6.5f);
                    }
                    else
                    {
                        tooltipRT.transform.position = ZInput.mousePosition + Vector3.right * (originalRT.rect.width * 4 + tooltipRT.rect.width);
                    }
                }
                else
                {
                    tooltipRT.transform.position = ZInput.mousePosition + Vector3.right * (originalRT.rect.width * 4 + tooltipRT.rect.width);
                }

                Utils.ClampUIToScreen(tooltipRT.transform as RectTransform);
            }
        }
    }
}