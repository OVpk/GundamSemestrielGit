//ETHAN
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagesVisualizer : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private GameObject dmgObj;
    
    private List<GameObject> _dmgPullList =  new List<GameObject>();
    private List<GameObject> _activeDmgList =  new List<GameObject>();

    [Header("Parameters")] 
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    [SerializeField] private float maxSize;
    [SerializeField] private float critSizeMultiplier = 1.5f;
    
    [Header("Curve")]
    [SerializeField] private float distance = 2f;
    [SerializeField] private float curveWidth = 1.5f;
    [SerializeField] private AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Fall")]
    [SerializeField, Range(0f, 1f)] private float fallStart = 0.5f;
    [SerializeField] private float gravity = 8f;
    
    [Header("Fade")]
    [SerializeField, Range(0f, 1f)] private float fadeStart = 0.7f;
    
    [Header("Color")]
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private int[] damageTiers = { 0, 20, 50, 100 };

    public void DisplayDamages(int damages, bool isCrit)
    {
        GameObject newDmgObj = PullDamage();
        newDmgObj.transform.position = transform.position;
        newDmgObj.transform.localScale = Vector3.zero;
        
        TMP_Text dmgText = newDmgObj.GetComponentInChildren<TMP_Text>();
        dmgText.text = damages.ToString();
        dmgText.color  = GetColor(damages);
        
        newDmgObj.SetActive(true);
        StartCoroutine(DmgAnimation(newDmgObj, dmgText, isCrit));
    }
    
    private Color GetColor(int damages)
    {
        if (damageTiers == null || damageTiers.Length <= 1)
            return colorGradient.Evaluate(0f);

        int tier = 0;
        for (int i = damageTiers.Length - 1; i >= 0; i--)
        {
            if (damages >= damageTiers[i])
            {
                tier = i;
                break;
            }
        }

        float t = (float)tier / (damageTiers.Length - 1);
        return colorGradient.Evaluate(t);
    }

    private GameObject PullDamage()
    {
        GameObject newDmgObj;
        
        if (_dmgPullList.Count > 0)
        {
            newDmgObj = _dmgPullList[0];
            _dmgPullList.RemoveAt(0);
        }
        else
        {
            newDmgObj = Instantiate(dmgObj, transform, true);
        }
        
        _activeDmgList.Add(newDmgObj);
        return newDmgObj;
    }

    private IEnumerator DmgAnimation(GameObject dmgObject, TMP_Text dmgText, bool isCrit)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(Random.Range(-distance * 0.5f, distance * 0.5f), distance, 0f);

        Vector3 midPos = Vector3.Lerp(startPos, endPos, 0.5f);
        Vector3 control = midPos + new Vector3(Random.Range(-curveWidth, curveWidth),
            Random.Range(-curveWidth * 0.5f, curveWidth * 0.5f), 0f);

        float size = isCrit ? maxSize * critSizeMultiplier : maxSize;
        Color baseColor = dmgText.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / lifeTime;
            float k = Mathf.Clamp01(t);

            Vector3 pos = Mathf.Pow(1 - k, 2) * startPos
                          + 2 * (1 - k) * k * control
                          + k * k * endPos;

            float fallK = Mathf.InverseLerp(fallStart, 1f, k);
            float fallTime = fallK * lifeTime * (1f - fallStart);
            pos.y -= 0.5f * gravity * fallTime * fallTime;

            dmgObject.transform.position = pos;
            dmgObject.transform.localScale = Vector3.one * (sizeCurve.Evaluate(k) * size);

            if (k > fadeStart)
            {
                float a = baseColor.a * (1f - Mathf.InverseLerp(fadeStart, 1f, k));
                dmgText.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            }

            yield return null;
        }

        dmgObject.SetActive(false);
        _activeDmgList.Remove(dmgObject);
        _dmgPullList.Add(dmgObject);
    }
}
