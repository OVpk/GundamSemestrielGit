using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector2 screenSize;
    [SerializeField] private MenuState menuState = MenuState.MechaModificator; 
    [SerializeField] private List<MenuState> menuOrder; 
    
    [Header("Menu")]
    [SerializeField] private RectTransform allMenuRectTransform;
    [SerializeField] private float menuSwitchAnimationTime = 1f;
    
    [Space(20)]
    [Header("Stats Drawer")]
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

    public void OpenGachaMenu() => SwitchMenu(MenuState.Gacha);
    public void OpenMechaMenu() => SwitchMenu(MenuState.MechaModificator);
    public void OpenInventoryMenu() => SwitchMenu(MenuState.Inventory);
    
    private void SwitchMenu(MenuState state)
    {
        if (state == menuState) return;
        
        int currentOrder = menuOrder.IndexOf(menuState)+1;
        int desiredOrder = menuOrder.IndexOf(state)+1;
        int direction = desiredOrder - currentOrder;
        
        Vector2 position = allMenuRectTransform.anchoredPosition;
        position.x += screenSize.x*-direction;
        allMenuRectTransform.DOAnchorPos(position, menuSwitchAnimationTime).SetEase(Ease.Linear);
        
        menuState = state;
    }
}

[Serializable]
public enum MenuState
{
    MechaModificator,
    Gacha,
    Inventory
}