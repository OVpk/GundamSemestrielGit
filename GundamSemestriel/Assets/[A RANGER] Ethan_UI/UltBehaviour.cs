using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UltBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button ultButton;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private MechaTestProto mecha;
    
    [Header("Parameters")]
    [SerializeField] private int cooldownTime;

    public void StartCooldown()
    {
        CallUlt(mecha);
        
        ultButton.interactable = false;
        cooldownImage.fillAmount = 1;
        cooldownText.text = cooldownTime+"s";
        cooldownImage.gameObject.SetActive(true);
        
        DOVirtual.Int(cooldownTime, 0, cooldownTime, (valeur) => 
            {
                cooldownText.text = valeur+"s";
            })
            .SetEase(Ease.Linear)
;
        cooldownImage.DOFillAmount(0, cooldownTime).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                ultButton.interactable = true;
                cooldownImage.gameObject.SetActive(false);
            });
    }

    private void CallUlt(MechaTestProto mechaTestProto)
    {
        Debug.Log("Implement ult");
    }
}
