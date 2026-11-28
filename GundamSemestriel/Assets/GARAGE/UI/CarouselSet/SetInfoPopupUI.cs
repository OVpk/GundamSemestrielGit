using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SetInfoPopupUI : MonoBehaviour
{
    [Header("Textes")]
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text pieceCountText;
    [SerializeField] private TMP_Text bonusesText;

    [Header("Couleur")]
    [SerializeField] private Color activeColor = new Color(0.25f, 0.85f, 0.35f, 1f);
    [SerializeField] private Color inactiveColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private float scaleDuration = 0.3f;
    [SerializeField] private float peakScaleMultiplier = 1.08f;

    private CanvasGroup canvasGroup;
    private Vector3 baseScale;
    private Color defaultCountColor;
    private Coroutine animCoroutine;
    private bool isInitialized = false;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        baseScale = transform.localScale;
        defaultCountColor = pieceCountText != null ? pieceCountText.color : Color.white;

        isInitialized = true;
    }

    public void ShowOrRefresh(SetData setData, int currentCount, int projectedCount)
    {
        EnsureInitialized();
        UpdateContent(setData, currentCount, projectedCount);

        if (!IsOpen)
        {
            IsOpen = true;
            gameObject.SetActive(true);

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(AnimateOpen());
        }
    }

    public void UpdateContent(SetData setData, int currentCount, int projectedCount)
    {
        EnsureInitialized();
        if (setData == null) return;

        if (setNameText != null)
        {
            setNameText.text = setData.SetName;
        }

        if (pieceCountText != null)
        {
            if (projectedCount != currentCount)
            {
                pieceCountText.text = $"{currentCount} -> {projectedCount}";
                pieceCountText.color = activeColor;
            }
            else
            {
                pieceCountText.text = currentCount.ToString();
                pieceCountText.color = defaultCountColor;
            }
        }

        if (bonusesText != null)
        {
            bonusesText.text = BuildBonusesText(setData.SetBonus, currentCount);
        }
    }

    private string BuildBonusesText(ConditionnalSubstat[] bonuses, int currentCount)
    {
        if (bonuses == null || bonuses.Length == 0) return string.Empty;

        string activeHex = ColorUtility.ToHtmlStringRGBA(activeColor);
        string inactiveHex = ColorUtility.ToHtmlStringRGBA(inactiveColor);

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < bonuses.Length; i++)
        {
            ConditionnalSubstat bonus = bonuses[i];
            bool isActive = currentCount >= bonus.RequiredSetNb;
            string hex = isActive ? activeHex : inactiveHex;

            sb.Append($"<color=#{hex}>");
            sb.Append($"{bonus.RequiredSetNb} pièces équipées :\n{bonus.Description}");
            sb.Append("</color>");

            if (i < bonuses.Length - 1)
            {
                sb.Append("\n\n");
            }
        }

        return sb.ToString();
    }

    public void Hide()
    {
        EnsureInitialized();
        IsOpen = false;

        if (!gameObject.activeInHierarchy)
        {
            HideImmediate();
            return;
        }

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateClose());
    }

    public void HideImmediate()
    {
        EnsureInitialized();
        IsOpen = false;

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }

        canvasGroup.alpha = 0f;
        transform.localScale = baseScale;
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateOpen()
    {
        canvasGroup.alpha = 0f;
        transform.localScale = baseScale;

        float totalDuration = Mathf.Max(fadeInDuration, scaleDuration);
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            float fadeT = fadeInDuration > 0f ? Mathf.Clamp01(elapsed / fadeInDuration) : 1f;
            canvasGroup.alpha = fadeT;

            float scaleT = scaleDuration > 0f ? Mathf.Clamp01(elapsed / scaleDuration) : 1f;
            float easedT = 1f - Mathf.Pow(1f - scaleT, 2f);
            float bump = Mathf.Sin(easedT * Mathf.PI) * (peakScaleMultiplier - 1f);
            transform.localScale = baseScale * (1f + bump);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        transform.localScale = baseScale;
        animCoroutine = null;
    }

    private IEnumerator AnimateClose()
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeOutDuration > 0f ? Mathf.Clamp01(elapsed / fadeOutDuration) : 1f;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        transform.localScale = baseScale;
        gameObject.SetActive(false);
        animCoroutine = null;
    }
}