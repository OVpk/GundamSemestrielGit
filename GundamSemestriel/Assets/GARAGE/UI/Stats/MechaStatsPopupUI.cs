using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MechaStatsPopupUI : MonoBehaviour
{
    [SerializeField] private MechaTestProto targetMecha;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text setBonusesText;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private float scaleDuration = 0.3f;
    [SerializeField] private float peakScaleMultiplier = 1.08f;

    private CanvasGroup canvasGroup;
    private Vector3 baseScale;
    private Coroutine animCoroutine;
    private bool isInitialized;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        if (!IsOpen)
        {
            HideImmediate();
        }
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;

        canvasGroup = GetComponent<CanvasGroup>();
        baseScale = transform.localScale;
        isInitialized = true;
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        EnsureInitialized();
        Refresh();

        IsOpen = true;
        gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateOpen());
    }

    public void Close()
    {
        EnsureInitialized();
        IsOpen = false;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

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
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = baseScale;
        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (targetMecha == null) return;

        UpdateStatsText();
        UpdateSetBonusesText();
    }

    private void UpdateStatsText()
    {
        if (statsText == null) return;

        EntityRuntimeData runtimeData = targetMecha.GetCalculatedStats();

        statsText.text =
            $"HP : {runtimeData.Hp}\n" +
            $"Attack : {runtimeData.Attack}\n" +
            $"Def : {runtimeData.Def}\n" +
            $"Speed : {runtimeData.Speed}\n" +
            $"Crit Rate : {runtimeData.CritRate}\n" +
            $"Crit Damage : {runtimeData.CritDamage}";
    }

    private void UpdateSetBonusesText()
    {
        if (setBonusesText == null) return;

        Dictionary<SetData, int> equippedSets = targetMecha.GetEquippedSetsCounts();
        StringBuilder sb = new StringBuilder();

        foreach (KeyValuePair<SetData, int> entry in equippedSets)
        {
            if (TryGetHighestUnlockedBonus(entry.Key, entry.Value, out ConditionnalSubstat activeBonus))
            {
                sb.AppendLine($"{entry.Key.SetName}({activeBonus.RequiredSetNb}) : {activeBonus.Description}");
            }
        }

        setBonusesText.text = sb.ToString().TrimEnd();
    }

    private bool TryGetHighestUnlockedBonus(SetData set, int equippedCount, out ConditionnalSubstat highestBonus)
    {
        highestBonus = default;
        bool found = false;

        if (set == null || set.SetBonus == null) return false;

        foreach (ConditionnalSubstat bonus in set.SetBonus)
        {
            if (equippedCount >= bonus.RequiredSetNb)
            {
                if (!found || bonus.RequiredSetNb > highestBonus.RequiredSetNb)
                {
                    highestBonus = bonus;
                    found = true;
                }
            }
        }

        return found;
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