using System;
using DG.Tweening;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField] private RectTransform statsDrawerRectTransform;
    [SerializeField] private Vector2 closedPosition;
    [SerializeField] private Vector2 openedPosition;
    [SerializeField] private bool isStatsOpen = true;
    [SerializeField] private float statsDrawerAnimationTime = 0.5f;

    private void Start()
    {
        CloseStatsDrawer();
    }

    public void ToggleStatsDrawer()
    {
        if (isStatsOpen) { CloseStatsDrawer(); }
        else { OpenStatsDrawer(); }
    }

    private void OpenStatsDrawer()
    {
        if (isStatsOpen) return;
        statsDrawerRectTransform.DOAnchorPos(openedPosition, statsDrawerAnimationTime).SetEase(Ease.InOutBack);
        isStatsOpen = true;
    }
    
    private void CloseStatsDrawer()
    {
        if (!isStatsOpen) return;
        statsDrawerRectTransform.DOAnchorPos(closedPosition, statsDrawerAnimationTime).SetEase(Ease.InOutBack);
        isStatsOpen = false;
    }
}
