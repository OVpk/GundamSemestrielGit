using System.Collections.Generic;
using UnityEngine;

public class GarageManager : MonoBehaviour
{
    public MechaInventory inventory;
    public SwipeCarousel carousel;
    public GameObject uiBodyPart;
    
    private MechaPartType currentPartType;
    private List<ScriptableObject> currentValidData = new List<ScriptableObject>();

    private void Start()
    {
        if (carousel != null) carousel.OnItemClicked += HandleCarouselClick;
    }

    private void OnDestroy()
    {
        if (carousel != null) carousel.OnItemClicked -= HandleCarouselClick;
    }

    public void SelectBodyPart(MechaPartType partToLoad)
    {
        currentPartType = partToLoad;
        currentValidData.Clear();
        List<GameObject> prefabsToLoad = new List<GameObject>();

        switch (partToLoad)
        {
            case MechaPartType.Torse:
                currentValidData.AddRange(inventory.torses);
                foreach (var d in inventory.torses) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.Tete:
                currentValidData.AddRange(inventory.tetes);
                foreach (var d in inventory.tetes) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.BrasGauche:
                currentValidData.AddRange(inventory.brasGauches);
                foreach (var d in inventory.brasGauches) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.BrasDroit:
                currentValidData.AddRange(inventory.brasDroits);
                foreach (var d in inventory.brasDroits) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.JambeGauche:
                currentValidData.AddRange(inventory.jambesGauches);
                foreach (var d in inventory.jambesGauches) prefabsToLoad.Add(d.prefab);
                break;
            case MechaPartType.JambeDroite:
                currentValidData.AddRange(inventory.jambesDroites);
                foreach (var d in inventory.jambesDroites) prefabsToLoad.Add(d.prefab);
                break;
        }

        if (prefabsToLoad.Count > 0)
        {
            uiBodyPart.SetActive(false);
            carousel.gameObject.SetActive(true);
            carousel.LoadNewPrefabs(prefabsToLoad);
        }
    }

    private void HandleCarouselClick(int index)
    {
        ScriptableObject selectedData = currentValidData[index];

        Debug.Log($"PARTIE SELECTIONNER");
        Debug.Log($"Partie : {currentPartType} | Data : {selectedData.name}");
    }

    public void LoadTorso() => SelectBodyPart(MechaPartType.Torse);
    public void LoadHead() => SelectBodyPart(MechaPartType.Tete);
    public void LoadLeftArm() => SelectBodyPart(MechaPartType.BrasGauche);
    public void LoadRightArm() => SelectBodyPart(MechaPartType.BrasDroit);
    public void LoadLeftLeg() => SelectBodyPart(MechaPartType.JambeGauche);
    public void LoadRightLeg() => SelectBodyPart(MechaPartType.JambeDroite);
}