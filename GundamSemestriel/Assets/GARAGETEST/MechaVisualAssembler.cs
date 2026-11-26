using System.Collections.Generic;
using UnityEngine;

public class MechaVisualAssembler : MonoBehaviour
{
    public Transform mainRootBone;

    private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

    private Dictionary<MechaPartType, GameObject> equippedVisuals = new Dictionary<MechaPartType, GameObject>();

    private void Awake()
    {
        BuildBoneMap();
    }

    private void BuildBoneMap()
    {

        boneMap.Clear();
        Transform[] allBones = mainRootBone.GetComponentsInChildren<Transform>(true);
        foreach (Transform bone in allBones)
        {
            if (!boneMap.ContainsKey(bone.name))
            {
                boneMap.Add(bone.name, bone);
            }
        }
    }

    public void UpdatePartVisual(MechaPartType partType, GameObject partPrefab)
    {
        if (equippedVisuals.TryGetValue(partType, out GameObject oldVisual) && oldVisual != null)
        {
            Destroy(oldVisual);
        }

        if (partPrefab == null) return;

        GameObject tempInstance = Instantiate(partPrefab, transform);
        SkinnedMeshRenderer[] smrs = tempInstance.GetComponentsInChildren<SkinnedMeshRenderer>();

        if (smrs.Length == 0)
        {
            Destroy(tempInstance);
            return;
        }

        GameObject partContainer = new GameObject($"Part_{partType}");
        partContainer.transform.SetParent(transform, false);

        foreach (SkinnedMeshRenderer smr in smrs)
        {
            RemapBones(smr);
            smr.transform.SetParent(partContainer.transform, false);
        }

        Destroy(tempInstance);

        equippedVisuals[partType] = partContainer;
    }

    private void RemapBones(SkinnedMeshRenderer smr)
    {
        Transform[] newBones = new Transform[smr.bones.Length];

        for (int i = 0; i < smr.bones.Length; i++)
        {
            if (smr.bones[i] == null) continue;

            string boneName = smr.bones[i].name;
            if (boneMap.TryGetValue(boneName, out Transform targetBone))
            {
                newBones[i] = targetBone;
            }
        }

        smr.bones = newBones;
        smr.rootBone = mainRootBone;
    }
}