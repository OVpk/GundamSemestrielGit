using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GarageManager : MonoBehaviour
{
    public MechaInventory inventory;
    public SwipeCarousel carousel;
    public GameObject uiBodyPart;
    
    public MechaTestProto currentMecha;
    
    public Image backgroundOverlay; 
    public float fadeDuration = 0.4f; 
    private float maxAlpha;
    private Coroutine fadeCoroutine;
    
    public Transform torsoAnchor;
    public Transform headAnchor;
    public Transform leftArmAnchor;
    public Transform rightArmAnchor;
    public Transform leftLegAnchor;
    public Transform rightLegAnchor;

    private MechaPartType currentPartType;
    private List<ScriptableObject> currentValidData = new List<ScriptableObject>();

    private void Start()
    {
        if (carousel != null) carousel.OnItemClicked += HandleCarouselClick;
        
        if (backgroundOverlay != null)
        {
            maxAlpha = backgroundOverlay.color.a;
            SetOverlayAlpha(0f);
            backgroundOverlay.gameObject.SetActive(true); 
        }

        if (currentMecha != null && inventory != null)
        {
            InitializeDefaultMecha();
        }
    }

    private void OnDestroy()
    {
        if (carousel != null) carousel.OnItemClicked -= HandleCarouselClick;
    }

    private void InitializeDefaultMecha()
    {
        if (currentMecha.BaseData == null && inventory.torses.Count > 0) currentMecha.EquipPart(MechaPartType.Torse, inventory.torses[0]);
        if (currentMecha.GetEquippedPart(MechaPartType.Tete) == null && inventory.tetes.Count > 0) currentMecha.EquipPart(MechaPartType.Tete, inventory.tetes[0]);
        if (currentMecha.GetEquippedPart(MechaPartType.BrasGauche) == null && inventory.brasGauches.Count > 0) currentMecha.EquipPart(MechaPartType.BrasGauche, inventory.brasGauches[0]);
        if (currentMecha.GetEquippedPart(MechaPartType.BrasDroit) == null && inventory.brasDroits.Count > 0) currentMecha.EquipPart(MechaPartType.BrasDroit, inventory.brasDroits[0]);
        if (currentMecha.GetEquippedPart(MechaPartType.JambeGauche) == null && inventory.jambesGauches.Count > 0) currentMecha.EquipPart(MechaPartType.JambeGauche, inventory.jambesGauches[0]);
        if (currentMecha.GetEquippedPart(MechaPartType.JambeDroite) == null && inventory.jambesDroites.Count > 0) currentMecha.EquipPart(MechaPartType.JambeDroite, inventory.jambesDroites[0]);
    }

    public void SelectBodyPart(MechaPartType partToLoad)
    {
        currentPartType = partToLoad;
        currentValidData.Clear();
        List<GameObject> prefabsToLoad = new List<GameObject>();
        
        Transform selectedAnchor = null; 

        switch (partToLoad)
        {
            case MechaPartType.Torse:
                selectedAnchor = torsoAnchor;
                currentValidData.AddRange(inventory.torses);
                foreach (var d in inventory.torses) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.Tete:
                selectedAnchor = headAnchor;
                currentValidData.AddRange(inventory.tetes);
                foreach (var d in inventory.tetes) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.BrasGauche:
                selectedAnchor = leftArmAnchor;
                currentValidData.AddRange(inventory.brasGauches);
                foreach (var d in inventory.brasGauches) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.BrasDroit:
                selectedAnchor = rightArmAnchor;
                currentValidData.AddRange(inventory.brasDroits);
                foreach (var d in inventory.brasDroits) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.JambeGauche:
                selectedAnchor = leftLegAnchor;
                currentValidData.AddRange(inventory.jambesGauches);
                foreach (var d in inventory.jambesGauches) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.JambeDroite:
                selectedAnchor = rightLegAnchor;
                currentValidData.AddRange(inventory.jambesDroites);
                foreach (var d in inventory.jambesDroites) prefabsToLoad.Add(d.prefab);
                break;
        }

        if (prefabsToLoad.Count > 0 && selectedAnchor != null)
        {
            int startIndex = 0;
            ScriptableObject equippedPart = currentMecha.GetEquippedPart(partToLoad);
            if (equippedPart != null)
            {
                int foundIndex = currentValidData.IndexOf(equippedPart);
                if (foundIndex != -1) startIndex = foundIndex;
            }

            uiBodyPart.SetActive(false);
            carousel.gameObject.SetActive(true);
            
            carousel.LoadNewPrefabs(prefabsToLoad, selectedAnchor, startIndex);

            FadeOverlayTo(maxAlpha);
        }
    }

    private void HandleCarouselClick(int index)
    {
        ScriptableObject selectedData = currentValidData[index];
        
        currentMecha.EquipPart(currentPartType, selectedData);
        carousel.ClearCarousel();
        
        carousel.gameObject.SetActive(false);
        uiBodyPart.SetActive(true);

        FadeOverlayTo(0f);
        
        currentMecha.DisplayCalculatedStats();
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (backgroundOverlay == null) return;
        Color c = backgroundOverlay.color;
        c.a = alpha;
        backgroundOverlay.color = c;
    }

    private void FadeOverlayTo(float targetAlpha)
    {
        if (backgroundOverlay == null) return;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = backgroundOverlay.color.a;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            SetOverlayAlpha(newAlpha);
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);
    }

    public void LoadTorso() => SelectBodyPart(MechaPartType.Torse);
    public void LoadHead() => SelectBodyPart(MechaPartType.Tete);
    public void LoadLeftArm() => SelectBodyPart(MechaPartType.BrasGauche);
    public void LoadRightArm() => SelectBodyPart(MechaPartType.BrasDroit);
    public void LoadLeftLeg() => SelectBodyPart(MechaPartType.JambeGauche);
    public void LoadRightLeg() => SelectBodyPart(MechaPartType.JambeDroite);
}