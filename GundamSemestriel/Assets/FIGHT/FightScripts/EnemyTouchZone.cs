using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyTouchZone : MonoBehaviour, IPointerClickHandler
{
    public FightManager fightManager;
    public int enemySlotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (fightManager != null)
        {
            fightManager.SetManualEnemyTarget(enemySlotIndex);
        }
    }
}