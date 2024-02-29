using System.Collections.Generic;
using ItemCompare;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class CustomUITooltip :
    MonoBehaviour,
    IPointerEnterHandler,
    IEventSystemHandler,
    IPointerExitHandler
{
    public Selectable m_selectable;
    public static GameObject m_tooltipPrefab;
    public RectTransform m_anchor;
    public Vector2 m_fixedPosition;
    public string m_text = "";
    public string m_topic = "";
    public GameObject m_gamepadFocusObject;
    public static CustomUITooltip m_current;
    public static GameObject m_tooltip;
    public static GameObject m_hovered;
    public const float m_showDelay = 0.5f;
    public float m_showTimer;

    public void Awake() => m_selectable = GetComponent<Selectable>();

    public void LateUpdate()
    {
        if (m_current == this && !m_tooltip.activeSelf)
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("LateUpdate, m_current == this && !m_tooltip.activeSelf");
            m_showTimer += Time.deltaTime;
            if (m_showTimer > 0.5 || ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
            {
                ItemComparePlugin.ItemCompareLogger.LogInfo("LateUpdate, setting tooltip active");
                m_tooltip.SetActive(true);
            }
        }

        if (ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
        {
            if (m_gamepadFocusObject != null)
            {
                if (m_gamepadFocusObject.activeSelf && m_current != this)
                    OnHoverStart(m_gamepadFocusObject);
                else if (!m_gamepadFocusObject.activeSelf && m_current == this)
                    HideTooltip();
            }
            else if (m_selectable)
            {
                if (EventSystem.current.currentSelectedGameObject == m_selectable.gameObject && m_current != this)
                    OnHoverStart(m_selectable.gameObject);
                else if (EventSystem.current.currentSelectedGameObject != m_selectable.gameObject && m_current == this)
                    HideTooltip();
            }

            if (!(m_current == this) || !(m_tooltip != null))
                return;
            if (m_anchor != null)
            {
                m_tooltip.transform.SetParent(m_anchor);
                m_tooltip.transform.localPosition = m_fixedPosition;
            }
            else if (m_fixedPosition != Vector2.zero)
            {
                m_tooltip.transform.position = m_fixedPosition;
            }
            else
            {
                RectTransform transform = gameObject.transform as RectTransform;
                Vector3[] vector3Array = new Vector3[4];
                Vector3[] fourCornersArray = vector3Array;
                transform.GetWorldCorners(fourCornersArray);
                m_tooltip.transform.position = (vector3Array[1] + vector3Array[2]) / 2f;
                Utils.ClampUIToScreen(m_tooltip.transform.GetChild(0).transform as RectTransform);
            }
        }
        else
        {
            if (m_current == this)
            {
                if (m_hovered == null)
                    HideTooltip();
                else if (m_tooltip.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(m_hovered.transform as RectTransform, ZInput.mousePosition))
                {
                    ItemComparePlugin.ItemCompareLogger.LogInfo("LateUpdate, hiding tooltip");
                    HideTooltip();
                }
                else
                {
                    m_tooltip.transform.position = ZInput.mousePosition;
                    Utils.ClampUIToScreen(m_tooltip.transform.GetChild(0).transform as RectTransform);
                    ItemComparePlugin.ItemCompareLogger.LogInfo("LateUpdate, clamp to screen");
                }
            }
            else
            {
                ItemComparePlugin.ItemCompareLogger.LogInfo("(ELSE) LateUpdate, m_current != this");
            }
        }
    }

    public void OnDisable()
    {
        if (m_current != this)
            return;
        HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverStart(eventData.pointerEnter);
    }

    public void OnHoverStart(GameObject go)
    {
        ItemComparePlugin.ItemCompareLogger.LogInfo("OnHoverStart");
        if (m_current)
            HideTooltip();
        if (m_tooltip != null || m_text == "" && m_topic == "" || m_tooltipPrefab == null) // Changed from UITooltip.cs
            return;
        m_tooltip = Instantiate<GameObject>(m_tooltipPrefab, transform.GetComponentInParent<Canvas>().transform);
        UpdateTextElements();
        Utils.ClampUIToScreen(m_tooltip.transform.GetChild(0).transform as RectTransform);
        m_hovered = go;
        ItemComparePlugin.ItemCompareLogger.LogInfo("OnHoverStart, m_hovered = go");
        m_current = this;
        ItemComparePlugin.ItemCompareLogger.LogInfo("OnHoverStart, m_current = this");
        m_tooltip.SetActive(false);
        m_showTimer = 0.0f;
    }

    public void UpdateTextElements()
    {
        if (m_tooltip != null)
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("UpdateTextElements, tooltip is not null");
            Transform child1 = Utils.FindChild(m_tooltip.transform, "Text");
            if (child1 != null)
                child1.GetComponent<TMP_Text>().text = Localization.instance.Localize(m_text);
            Transform child2 = Utils.FindChild(m_tooltip.transform, "Topic");
            if (!(child2 != null))
                return;
            child2.GetComponent<TMP_Text>().text = Localization.instance.Localize(m_topic);
        }
        else
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("UpdateTextElements, tooltip is null");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_current == this)
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("OnPointerExit, calling HideTooltip");
            HideTooltip();
        }
        else
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("OnPointerExit, m_current != this");
        }
    }

    public static void HideTooltip()
    {
        if (!m_tooltip)
            return;
        Destroy(m_tooltip);
        m_current = null;
        m_tooltip = null;
        m_hovered = null;
    }

    public void Set(string topic, string text, RectTransform anchor = null, Vector2 fixedPosition = default)
    {
        m_anchor = anchor;
        m_fixedPosition = fixedPosition;
        if (topic != m_topic || text != m_text)
        {
            m_topic = topic;
            m_text = text;
            if (m_current == this && m_tooltip != null)
            {
                UpdateTextElements();
            }
            else
            {
                if (m_selectable == null)
                    return;
                RectTransform transform = m_selectable.transform as RectTransform;
                if (!transform.rect.Contains(transform.InverseTransformPoint(ZInput.mousePosition)))
                    return;
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
                {
                    position = ZInput.mousePosition,
                }, raycastResults);
                if (raycastResults.Count <= 0 || !(raycastResults[0].gameObject == m_selectable.gameObject))
                    return;
                OnHoverStart(m_selectable.gameObject);
            }
        }
        else
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("Set, topic == m_topic && text == m_text");
        }
    }
}