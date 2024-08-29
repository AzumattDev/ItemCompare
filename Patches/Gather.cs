using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ItemCompare.Patches;

[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
static class GatherConversions
{
    public static readonly Dictionary<string?, ItemDrop?> CookingStationConversions = new();
    public static readonly Dictionary<string?, ItemDrop?> FermenterConversions = new();

    [HarmonyPriority(Priority.Last)]
    static void Postfix(ZNetScene __instance)
    {
        // Get all prefabs that have a CookingStation component
        foreach (GameObject prefab in __instance.m_prefabs)
        {
            if (prefab.TryGetComponent(out CookingStation cookingStation))
            {
                // Get all the m_from items from the CookingStation
                foreach (CookingStation.ItemConversion conversion in cookingStation.m_conversion)
                {
                    // Add conversion.m_from as a key to a dictionary so we can compare and keep track of what uncooked items turn into with the conversion.m_to
                    if (!GatherConversions.CookingStationConversions.ContainsKey(conversion.m_from.m_itemData?.m_shared?.m_name))
                    {
                        GatherConversions.CookingStationConversions.Add(conversion.m_from.m_itemData?.m_shared?.m_name, conversion.m_to);
                    }
                }
            }
            
            if (prefab.TryGetComponent(out Fermenter ferment))
            {
                // Get all the m_from items from the CookingStation
                foreach (Fermenter.ItemConversion conversion in ferment.m_conversion)
                {
                    // Add conversion.m_from as a key to a dictionary so we can compare and keep track of what uncooked items turn into with the conversion.m_to
                    if (!GatherConversions.FermenterConversions.ContainsKey(conversion.m_from.m_itemData?.m_shared?.m_name))
                    {
                        GatherConversions.FermenterConversions.Add(conversion.m_from.m_itemData?.m_shared?.m_name, conversion.m_to);
                    }
                }
            }
        }
    }
}