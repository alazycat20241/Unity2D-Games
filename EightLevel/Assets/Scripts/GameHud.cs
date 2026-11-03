using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameHud : MonoBehaviour
{
    private GameBootstrap game;
    private Text levelText;
    private Text titleText;
    private CanvasGroup titleGroup;
    private Image damageFlash;
    private GameObject completionPanel;
    private Coroutine titleRoutine;

    private static Font BuiltinFont
    {
        get
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }

    public void Build(GameBootstrap owner)
    {
        game = owner;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        RectTransform root = GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        CreatePanel(
            "Top Shade",
            root,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0f),
            new Vector2(0f, 150f),
            new Color(0.02f, 0.035f, 0.07f, 0.38f));

        Image levelAccent = CreatePanel(
            "Level Accent",
            root,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(54f, -75f),
            new Vector2(5f, 44f),
            new Color(0.3f, 0.82f, 0.95f, 1f));

        levelText = CreateText(
            "Level",
            root,
            "01 / 08",
            24,
            TextAnchor.MiddleLeft,
            new Vector2(78f, -74f),
            new Vector2(620f, 50f),
            new Color(0.88f, 0.95f, 1f, 1f));

        Text controlsText = CreateText(
            "Controls",
            root,
            "A / D  移动     ·     左键  翻转     ·     空格  重来",
            18,
            TextAnchor.MiddleLeft,
            new Vector2(56f, 48f),
            new Vector2(900f, 40f),
            new Color(0.72f, 0.8f, 0.9f, 0.58f));
        RectTransform controlsRect = controlsText.rectTransform;
        controlsRect.anchorMin = Vector2.zero;
        controlsRect.anchorMax = Vector2.zero;
        controlsRect.pivot = Vector2.zero;
        controlsRect.anchoredPosition = new Vector2(56f, 42f);

        titleText = CreateText(
            "Level Title",
            root,
            string.Empty,
            58,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            new Vector2(900f, 100f),
            Color.white);
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.72f);
        titleRect.anchorMax = new Vector2(0.5f, 0.72f);
        titleRect.anchoredPosition = Vector2.zero;
        titleText.fontStyle = FontStyle.Bold;
        titleGroup = titleText.gameObject.AddComponent<CanvasGroup>();
        titleGroup.alpha = 0f;

        damageFlash = CreatePanel(
            "Damage Flash",
            root,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            new Color(0.9f, 0.1f, 0.08f, 0f));
        damageFlash.raycastTarget = false;

        CreateCompletionPanel(root);
        levelAccent.raycastTarget = false;
    }

    public void SetLevel(int index, string levelName)
    {
        levelText.text = $"{index + 1:00} / 08     {levelName}";
        if (titleRoutine != null)
        {
            StopCoroutine(titleRoutine);
        }

        titleRoutine = StartCoroutine(RevealTitle($"{index + 1:00}    {levelName}"));
    }

    public void FlashDamage()
    {
        StopCoroutine(nameof(FlashRoutine));
        StartCoroutine(nameof(FlashRoutine));
    }

    public void ShowCompletion()
    {
        completionPanel.SetActive(true);
        levelText.text = "08 / 08     全部";
    }

    private IEnumerator RevealTitle(string title)
    {
        titleText.text = title;
        titleGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            titleGroup.alpha = Mathf.Clamp01(elapsed / 0.18f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.72f);

        elapsed = 0f;
        while (elapsed < 0.42f)
        {
            elapsed += Time.unscaledDeltaTime;
            titleGroup.alpha = 1f - Mathf.Clamp01(elapsed / 0.42f);
            yield return null;
        }

        titleGroup.alpha = 0f;
    }

    private IEnumerator FlashRoutine()
    {
        float duration = 0.28f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            float t = elapsed / duration;
            Color color = damageFlash.color;
            color.a = Mathf.Sin(t * Mathf.PI) * 0.22f;
            damageFlash.color = color;
            yield return null;
        }

        Color final = damageFlash.color;
        final.a = 0f;
        damageFlash.color = final;
    }

    private void CreateCompletionPanel(RectTransform root)
    {
        completionPanel = new GameObject("Completion");
        completionPanel.transform.SetParent(root, false);
        RectTransform panelRect = completionPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image background = completionPanel.AddComponent<Image>();
        background.color = new Color(0.025f, 0.04f, 0.075f, 0.9f);

        Text headline = CreateText(
            "Headline",
            panelRect,
            "八关完成",
            72,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            new Vector2(900f, 120f),
            new Color(0.35f, 0.88f, 1f, 1f));
        headline.fontStyle = FontStyle.Bold;
        RectTransform headlineRect = headline.rectTransform;
        headlineRect.anchorMin = new Vector2(0.5f, 0.56f);
        headlineRect.anchorMax = new Vector2(0.5f, 0.56f);
        headlineRect.anchoredPosition = Vector2.zero;

        Text prompt = CreateText(
            "Prompt",
            panelRect,
            "按空格键重新开始",
            24,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            new Vector2(700f, 70f),
            new Color(0.76f, 0.86f, 0.96f, 0.8f));
        RectTransform promptRect = prompt.rectTransform;
        promptRect.anchorMin = new Vector2(0.5f, 0.43f);
        promptRect.anchorMax = new Vector2(0.5f, 0.43f);
        promptRect.anchoredPosition = Vector2.zero;

        completionPanel.SetActive(false);
    }

    private static Image CreatePanel(
        string name,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(
        string name,
        RectTransform parent,
        string text,
        int fontSize,
        TextAnchor alignment,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text textComponent = gameObject.AddComponent<Text>();
        textComponent.font = BuiltinFont;
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = color;
        textComponent.raycastTarget = false;
        textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        return textComponent;
    }
}
