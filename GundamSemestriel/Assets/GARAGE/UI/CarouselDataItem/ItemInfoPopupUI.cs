using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ItemInfoPopupUI : MonoBehaviour
{
    [Header("Textes")]
    [SerializeField] private TMP_Text mechaStatsText;
    [SerializeField] private TMP_Text itemStatsText;
    [SerializeField] private TMP_Text statsDiffText;

    [Header("Couleurs")]
    [SerializeField] private Color increaseColor = new Color(0.25f, 0.85f, 0.35f, 1f);
    [SerializeField] private Color decreaseColor = new Color(0.90f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color neutralColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    [Header("Symboles")]
    [SerializeField] private string upSymbol = "▲";
    [SerializeField] private string downSymbol = "▼";
    [SerializeField] private string neutralSymbol = "-";

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

    private void EnsureInitialized()
    {
        if (isInitialized) return;

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        baseScale = transform.localScale;
        isInitialized = true;
    }

    public void ShowOrRefresh(EntityRuntimeData currentStats, EntityRuntimeData simulatedStats, ScriptableObject focusedItem)
    {
        EnsureInitialized();
        UpdateContent(currentStats, simulatedStats, focusedItem);

        if (!IsOpen)
        {
            IsOpen = true;
            gameObject.SetActive(true);

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(AnimateOpen());
        }
    }

    public void UpdateContent(EntityRuntimeData currentStats, EntityRuntimeData simulatedStats, ScriptableObject focusedItem)
    {
        EnsureInitialized();

        if (mechaStatsText != null)
        {
            mechaStatsText.text =
                    $"{currentStats.Hp}\n" +
                    $"{currentStats.Attack}\n" +
                    $"{currentStats.Def}\n" +
                    $"{currentStats.Speed}\n" +
                    $"{currentStats.CritRate}\n" +
                    $"{currentStats.CritDamage}";
        }

        if (itemStatsText != null)
        {
            itemStatsText.text = BuildItemStatsText(focusedItem);
        }

        if (statsDiffText != null)
        {
            statsDiffText.text = BuildDiffText(currentStats, simulatedStats);
        }
    }

    private string BuildItemStatsText(ScriptableObject item)
    {
        if (item == null) return string.Empty;

        if (item is ModuleData module)
        {
            int hp = 0;
            int attack = 0;
            int def = 0;
            int speed = 0;
            int critRate = 0;
            int critDamage = 0;

            if (module.SubStats != null)
            {
                foreach (SubStat subStat in module.SubStats)
                {
                    switch (subStat.Stat)
                    {
                        case Stat.Hp: hp += subStat.StatValue; break;
                        case Stat.Attack: attack += subStat.StatValue; break;
                        case Stat.Def: def += subStat.StatValue; break;
                        case Stat.Speed: speed += subStat.StatValue; break;
                        case Stat.CritRate: critRate += subStat.StatValue; break;
                        case Stat.CritDamage: critDamage += subStat.StatValue; break;
                    }
                }
            }

            return
                $"HP : {FormatModuleValue(hp)}\n" +
                $"Attack : {FormatModuleValue(attack)}\n" +
                $"Def : {FormatModuleValue(def)}\n" +
                $"Speed : {FormatModuleValue(speed)}\n" +
                $"Crit Rate : {FormatModuleValue(critRate)}\n" +
                $"Crit Damage : {FormatModuleValue(critDamage)}";
        }

        if (item is MechaBaseData baseData)
        {
            return
                $"HP : {baseData.Hp}\n" +
                $"Attack : {baseData.Attack}\n" +
                $"Def : {baseData.Def}\n" +
                $"Speed : {baseData.Speed}\n" +
                $"Crit Rate : {baseData.CritRate}\n" +
                $"Crit Damage : {baseData.CritDamage}";
        }

        return item.name;
    }

    private string FormatModuleValue(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private string BuildDiffText(EntityRuntimeData current, EntityRuntimeData simulated)
    {

        string upHex = ColorUtility.ToHtmlStringRGBA(increaseColor);
        string downHex = ColorUtility.ToHtmlStringRGBA(decreaseColor);
        string neutralHex = ColorUtility.ToHtmlStringRGBA(neutralColor);

        StringBuilder sb = new StringBuilder();
        AppendDiffLine(sb, simulated.Hp - current.Hp, upHex, downHex, neutralHex);
        AppendDiffLine(sb, simulated.Attack - current.Attack, upHex, downHex, neutralHex);
        AppendDiffLine(sb, simulated.Def - current.Def, upHex, downHex, neutralHex);
        AppendDiffLine(sb, simulated.Speed - current.Speed, upHex, downHex, neutralHex);
        AppendDiffLine(sb, simulated.CritRate - current.CritRate, upHex, downHex, neutralHex);
        AppendDiffLine(sb, simulated.CritDamage - current.CritDamage, upHex, downHex, neutralHex);

        return sb.ToString().TrimEnd();
    }

    private void AppendDiffLine(StringBuilder sb, float diff, string upHex, string downHex, string neutralHex)
    {
        const float epsilon = 0.001f;

        if (diff > epsilon)
        {
            sb.AppendLine($"<color=#{upHex}>{upSymbol} +{diff:0.##}</color>");
        }
        else if (diff < -epsilon)
        {
            sb.AppendLine($"<color=#{downHex}>{downSymbol} {diff:0.##}</color>");
        }
        else
        {
            sb.AppendLine($"<color=#{neutralHex}>{neutralSymbol}</color>");
        }
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