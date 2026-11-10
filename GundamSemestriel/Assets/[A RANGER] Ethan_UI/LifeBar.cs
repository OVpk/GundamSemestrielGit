using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LifeBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image back;
    [SerializeField] private Image lifeBar;
    [SerializeField] private Image damageBar;
    [SerializeField] private EnemyTouchZone _enemy;

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
        if (_enemy == null)
            _enemy = GetComponentInParent<EnemyTouchZone>();
    }

    private void Start()
    {
        InitColors();

        FightManager.OnFMReady += LinkEntity;

        lifeBar.fillAmount = 1f;
        damageBar.fillAmount = 1f;
    }

    private void LinkEntity()
    {
        _entity = _enemy.fightManager.EnemyTeam[_enemy.enemySlotIndex];
        if (_entity == null) return;

        _maxLife = _entity.BaseStats.Hp;
        _entity.OnDamaged += HandleDamaged;
    }

    private void OnDestroy()
    {
        if (_entity != null)
            _entity.OnDamaged -= HandleDamaged;
        
        FightManager.OnFMReady -= LinkEntity;
    }

    private void InitColors()
    {
        if (back != null) back.color = backColor;
        if (lifeBar != null) lifeBar.color = lifeBarColor;
        if (damageBar != null) damageBar.color = damagesBarColor;
    }

    private void HandleDamaged(FightEntity entity, int damage)
    {
        float ratio = (float)entity.CurrentHp / _maxLife;

        lifeBar.fillAmount = ratio;

        damageBar.DOKill();
        damageBar.DOFillAmount(ratio, damagesLifeAnimTime).SetDelay(damagesLifeAnimDelay);
        //Debug.Log("took damages " + damage);
    }

    // DEBUG
    [ContextMenu("Test Take Damage")]
    public void TestDamage()
    {
        if (_entity != null) _entity.TakeDamage(10);
    }
}