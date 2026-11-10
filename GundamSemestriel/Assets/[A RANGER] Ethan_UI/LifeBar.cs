//ETHAN
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LifeBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image back;
    [SerializeField] private Image lifeBar;
    [SerializeField] private Image damageBar;
    [SerializeField] private DamagesVisualizer damagesVisualizer;
    
    [SerializeField] private FightManager fightManager;
    [SerializeField] private bool isEnemy;
    [SerializeField] private MechaTestProto mecha;
    [SerializeField] private EnemyTouchZone enemy;

    [Header("Colors")]
    [SerializeField] private Color backColor;
    [SerializeField] private Color lifeBarColor;
    [SerializeField] private Color damagesBarColor;

    [Header("Parameters")]
    [SerializeField] private float damagesLifeAnimTime = 0.5f;
    [SerializeField] private float damagesLifeAnimDelay = 0.5f;

    private FightEntity _entity;
    private float _maxLife;

    private void OnValidate()
    {
        InitColors();
    }

    private void Awake()
    {
        if (isEnemy && enemy == null) enemy = GetComponentInParent<EnemyTouchZone>();
        if (damagesVisualizer == null) damagesVisualizer = GetComponentInChildren<DamagesVisualizer>();
        
        FightManager.OnFMReady += LinkEntity;
    }

    private void Start()
    {
        InitColors();
        lifeBar.fillAmount = 1f;
        damageBar.fillAmount = 1f;
    }

    private void LinkEntity()
    {
        if (_entity != null)
            _entity.OnDamaged -= HandleDamaged;
        
        if (fightManager == null && enemy != null) fightManager = enemy.fightManager;
        if (fightManager == null)
        {
            Debug.LogWarning("[LifeBar] no fight manager", this);
            return;
        } 
        
        if (isEnemy)
        {
            _entity = fightManager.EnemyTeam[enemy.enemySlotIndex];
        }
        else
        {
            _entity = fightManager.GetPlayerEntity(mecha);
        }
        
        _maxLife = _entity.BaseStats.Hp;
        _entity.OnDamaged += HandleDamaged;

        lifeBar.fillAmount = _entity.CurrentHp / _maxLife;
        damageBar.fillAmount = lifeBar.fillAmount;
    }

    private void OnDestroy()
    {
        FightManager.OnFMReady -= LinkEntity;

        if (_entity != null)
            _entity.OnDamaged -= HandleDamaged;

        damageBar.DOKill();
    }

    private void InitColors()
    {
        if (back != null) back.color = backColor;
        if (lifeBar != null) lifeBar.color = lifeBarColor;
        if (damageBar != null) damageBar.color = damagesBarColor;
    }

    private void HandleDamaged(FightEntity entity, int damage, bool isCrit)
    {
        float ratio = (float)entity.CurrentHp / _maxLife;

        lifeBar.fillAmount = ratio;

        if (damagesVisualizer != null)
            damagesVisualizer.DisplayDamages(damage, isCrit);

        damageBar.DOKill();
        damageBar.DOFillAmount(ratio, damagesLifeAnimTime).SetDelay(damagesLifeAnimDelay);
    }

    // DEBUG
    [ContextMenu("Test Take Damage")]
    public void TestDamage()
    {
        if (_entity != null) _entity.TakeDamage(10);
    }
}