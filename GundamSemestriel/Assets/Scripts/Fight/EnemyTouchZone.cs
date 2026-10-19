using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyTouchZone : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private FightManager fightManager;
    [SerializeField] private int enemySlotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (fightManager != null)
        {
            fightManager.SetManualEnemyTarget(enemySlotIndex);
        }
    }
}