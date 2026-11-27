using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GarageManager : MonoBehaviour
{
    private enum GarageStep
    {
        BodyPartSelection,
        SetSelection,
        ItemSelection
    }

    public MechaInventory inventory;
    public GameObject uiBodyPart;

    [Header("Carousels")]
    public SwipeCarousel setCarousel;
    public SwipeCarousel itemCarousel;
    
    public MechaTestProto currentMecha;
    
    public Image backgroundOverlay; 
    public float fadeDuration = 0.4f; 
    private float maxAlpha;
    private Coroutine fadeCoroutine;
    
    [Header("Anchors 3D")]
    public Transform torsoAnchor;
    public Transform headAnchor;
    public Transform leftArmAnchor;
    public Transform rightArmAnchor;
    public Transform leftLegAnchor;
    public Transform rightLegAnchor;

    private int currentMechaIndex = 0;
    private GarageStep currentStep = GarageStep.BodyPartSelection;
    private MechaPartType currentPartType;
    private Transform currentAnchor;

    private List<SetData> currentOwnedSets = new List<SetData>();
    private List<ScriptableObject> currentFilteredItems = new List<ScriptableObject>();

    private void Start()
    {
        if (setCarousel != null)
        {
            setCarousel.OnItemClicked += HandleSetClicked;
            setCarousel.gameObject.SetActive(false);
        }

        if (itemCarousel != null)
        {
            itemCarousel.OnItemClicked += HandleItemClicked;
            itemCarousel.OnIndexChanged += HandleItemFocused;
            itemCarousel.gameObject.SetActive(false);
        }
        
        if (backgroundOverlay != null)
        {
            maxAlpha = backgroundOverlay.color.a;
            SetOverlayAlpha(0f);
            backgroundOverlay.gameObject.SetActive(true); 
        }

        if (currentMecha != null && inventory != null)
        {
            // InitializeDefaultMecha();
        }
        
        if (GlobalRuntimeDatas.Instance != null)
            LoadMechaFromData(currentMechaIndex);
    }

    private void OnDestroy()
    {
        if (setCarousel != null)
        {
            setCarousel.OnItemClicked -= HandleSetClicked;
        }

        if (itemCarousel != null)
        {
            itemCarousel.OnItemClicked -= HandleItemClicked;
            itemCarousel.OnIndexChanged -= HandleItemFocused;
        }
    }

    public void GoBattle()
    {
        SaveMechaInData(currentMechaIndex);
        SceneManager.LoadScene("Fight");
    }

    private void LoadMechaFromData(int index)
    {
        MechaTestProto mechaToLoad = GlobalRuntimeDatas.Instance.mechas[index];
        
        currentMecha.EquipPart(MechaPartType.Torse, mechaToLoad.GetEquippedPart(MechaPartType.Torse));
        currentMecha.EquipPart(MechaPartType.Tete, mechaToLoad.GetEquippedPart(MechaPartType.Tete));
        currentMecha.EquipPart(MechaPartType.BrasGauche, mechaToLoad.GetEquippedPart(MechaPartType.BrasGauche));
        currentMecha.EquipPart(MechaPartType.BrasDroit, mechaToLoad.GetEquippedPart(MechaPartType.BrasDroit));
        currentMecha.EquipPart(MechaPartType.JambeGauche, mechaToLoad.GetEquippedPart(MechaPartType.JambeGauche));
        currentMecha.EquipPart(MechaPartType.JambeDroite, mechaToLoad.GetEquippedPart(MechaPartType.JambeDroite));
    }
    
    private void SaveMechaInData(int index)
    {
        MechaTestProto mechaToSave = GlobalRuntimeDatas.Instance.mechas[index];
        
        mechaToSave.EquipPart(MechaPartType.Torse, currentMecha.GetEquippedPart(MechaPartType.Torse));
        mechaToSave.EquipPart(MechaPartType.Tete, currentMecha.GetEquippedPart(MechaPartType.Tete));
        mechaToSave.EquipPart(MechaPartType.BrasGauche, currentMecha.GetEquippedPart(MechaPartType.BrasGauche));
        mechaToSave.EquipPart(MechaPartType.BrasDroit, currentMecha.GetEquippedPart(MechaPartType.BrasDroit));
        mechaToSave.EquipPart(MechaPartType.JambeGauche, currentMecha.GetEquippedPart(MechaPartType.JambeGauche));
        mechaToSave.EquipPart(MechaPartType.JambeDroite, currentMecha.GetEquippedPart(MechaPartType.JambeDroite));
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
        currentAnchor = GetAnchorForPart(partToLoad);

        if (currentAnchor == null)
        {
            Debug.LogWarning("Anchor pas assigné !");
            return;
        }

        OpenSetCarousel();
    }

    private void OpenSetCarousel()
    {
        List<SetData> rawOwnedSets = inventory.GetOwnedSetsForPart(currentPartType);
        currentOwnedSets.Clear();
        List<GameObject> setPrefabs = new List<GameObject>();

        foreach (SetData set in rawOwnedSets)
        {
            GameObject prefab = set.GetPrefab(currentPartType);
            if (prefab != null)
            {
                currentOwnedSets.Add(set);
                setPrefabs.Add(prefab);
            }
        }

        if (setPrefabs.Count == 0) return;

        int startIndex = 0;
        SetData equippedSet = GetSetFromData(currentMecha.GetEquippedPart(currentPartType));
        if (equippedSet != null)
        {
            int foundIndex = currentOwnedSets.IndexOf(equippedSet);
            if (foundIndex != -1) startIndex = foundIndex;
        }

        currentStep = GarageStep.SetSelection;

        uiBodyPart.SetActive(false);
        itemCarousel.ClearCarousel();
        itemCarousel.gameObject.SetActive(false);

        setCarousel.gameObject.SetActive(true);
        setCarousel.LoadNewPrefabs(setPrefabs, currentAnchor, startIndex);

        FadeOverlayTo(maxAlpha);
    }
    
    private void HandleSetClicked(int setIndex)
    {
        SetData selectedSet = currentOwnedSets[setIndex];
        currentFilteredItems = inventory.GetItemsByPartAndSet(currentPartType, selectedSet);

        if (currentFilteredItems.Count == 0) return;

        GameObject partPrefab = selectedSet.GetPrefab(currentPartType);
        List<GameObject> itemPrefabs = new List<GameObject>();
        for (int i = 0; i < currentFilteredItems.Count; i++)
        {
            itemPrefabs.Add(partPrefab);
        }

        int startIndex = 0;
        ScriptableObject equippedPart = currentMecha.GetEquippedPart(currentPartType);
        if (equippedPart != null)
        {
            int foundIndex = currentFilteredItems.IndexOf(equippedPart);
            if (foundIndex != -1) startIndex = foundIndex;
        }

        currentStep = GarageStep.ItemSelection;

        setCarousel.ClearCarousel();
        setCarousel.gameObject.SetActive(false);

        itemCarousel.gameObject.SetActive(true);
        itemCarousel.LoadNewPrefabs(itemPrefabs, currentAnchor, startIndex);
    }

    private void HandleItemFocused(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= currentFilteredItems.Count) return;

        ScriptableObject focusedItem = currentFilteredItems[itemIndex];
        Debug.Log($"[Carousel Vertical] Exemplaire centré ({itemIndex + 1}/{currentFilteredItems.Count}) : {focusedItem.name}");
    }
    
    private void HandleItemClicked(int itemIndex)
    {
        ScriptableObject selectedData = currentFilteredItems[itemIndex];
        
        currentMecha.EquipPart(currentPartType, selectedData);

        itemCarousel.ClearCarousel();
        itemCarousel.gameObject.SetActive(false);
        uiBodyPart.SetActive(true);

        currentStep = GarageStep.BodyPartSelection;
        FadeOverlayTo(0f);
        
        currentMecha.DisplayCalculatedStats();
    }

    public void GoBack()
    {
        if (currentStep == GarageStep.ItemSelection)
        {
            OpenSetCarousel();
        }
        else if (currentStep == GarageStep.SetSelection)
        {
            setCarousel.ClearCarousel();
            setCarousel.gameObject.SetActive(false);
            uiBodyPart.SetActive(true);
            currentStep = GarageStep.BodyPartSelection;
            FadeOverlayTo(0f);
        }
    }

    private Transform GetAnchorForPart(MechaPartType partType)
    {
        switch (partType)
        {
            case MechaPartType.Torse: return torsoAnchor;
            case MechaPartType.Tete: return headAnchor;
            case MechaPartType.BrasGauche: return leftArmAnchor;
            case MechaPartType.BrasDroit: return rightArmAnchor;
            case MechaPartType.JambeGauche: return leftLegAnchor;
            case MechaPartType.JambeDroite: return rightLegAnchor;
            default: return null;
        }
    }

    private SetData GetSetFromData(ScriptableObject data)
    {
        if (data is MechaBaseData bd) return bd.Set;
        if (data is ModuleData md) return md.Set;
        return null;
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