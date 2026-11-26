using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeCarousel : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public event Action<int> OnItemClicked;

    public List<GameObject> prefabs; 

    public Transform spawnContainer;
    
    public Vector3 normalScale = Vector3.one; 
    public Vector3 hiddenScale = Vector3.zero; 

    public float transitionDuration = 0.4f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float minSwipeDistance = 50f;
    [SerializeField] private bool invertSwipeDirection = false;

    [Header("Positions")]
    public Vector3 centerPos = new Vector3(0, 0, -2);
    public Vector3 leftPos = new Vector3(-3, 0, 0);
    public Vector3 rightPos = new Vector3(3, 0, 0);
    public Vector3 outLeftPos = new Vector3(-6, 0, 2); 
    public Vector3 outRightPos = new Vector3(6, 0, 2); 
    
    private int currentIndex = 0;
    private GameObject currentCenter;
    private GameObject currentLeft;
    private GameObject currentRight;
    private bool isAnimating = false;

    private bool hasBeenInitialized = false; 

    private Vector2 startTouchPosition;
    private float startTouchTime;

    void Start()
    {
        if (spawnContainer == null) spawnContainer = this.transform;
        
        if (hasBeenInitialized) return; 

        if (prefabs == null || prefabs.Count == 0) return;
        
        InitializeCarousel();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isAnimating) return;
        
        startTouchPosition = eventData.position;
        startTouchTime = Time.time;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating) return;

        float distance = Vector2.Distance(startTouchPosition, eventData.position);
        if (distance < minSwipeDistance)
        {
            OnItemClicked?.Invoke(currentIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return;
        startTouchPosition = eventData.position;
        startTouchTime = Time.time;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAnimating) return;
        if (prefabs == null || prefabs.Count <= 1) return;

        float swipeDistance = eventData.position.x - startTouchPosition.x;
        float swipeDuration = Time.time - startTouchTime;

        if (swipeDuration > 0)
        {
            float swipeSpeed = Mathf.Abs(swipeDistance) / swipeDuration;

            if (Mathf.Abs(swipeDistance) >= minSwipeDistance)
            {
                bool isSwipingRight = swipeDistance > 0;
                if (invertSwipeDirection) isSwipingRight = !isSwipingRight;

                if (isSwipingRight) SwipeRight();
                else SwipeLeft();
            }
        }
    }
    
    private void InitializeCarousel()
    {
        hasBeenInitialized = true; 

        currentCenter = SpawnPrefab(currentIndex, centerPos, normalScale);
        currentLeft = SpawnPrefab(GetWrappedIndex(currentIndex - 1), leftPos, normalScale);
        currentRight = SpawnPrefab(GetWrappedIndex(currentIndex + 1), rightPos, normalScale);
    }

    private int GetWrappedIndex(int index)
    {
        if (index < 0) return prefabs.Count - 1;
        if (index >= prefabs.Count) return 0;
        return index;
    }

    private GameObject SpawnPrefab(int index, Vector3 localPosition, Vector3 scale)
    {
        GameObject obj = Instantiate(prefabs[index], spawnContainer);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = scale;
        return obj;
    }

    private IEnumerator MoveAndScale(GameObject obj, Vector3 targetLocalPos, Vector3 targetScale, bool destroyAtEnd)
    {
        Vector3 startLocalPos = obj.transform.localPosition;
        Vector3 startScale = obj.transform.localScale;
        float timeElapsed = 0f;

        while (timeElapsed < transitionDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = movementCurve.Evaluate(timeElapsed / transitionDuration);
            
            obj.transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, t);
            obj.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            yield return null;
        }

        obj.transform.localPosition = targetLocalPos;
        obj.transform.localScale = targetScale;
        
        if (destroyAtEnd) Destroy(obj);
    }

    private void SwipeLeft()
    {
        StartCoroutine(AnimateCarousel(1));
    }

    private void SwipeRight()
    {
        StartCoroutine(AnimateCarousel(-1));
    }

    private IEnumerator AnimateCarousel(int direction)
    {
        isAnimating = true;

        GameObject oldCenter = currentCenter;
        GameObject oldLeft = currentLeft;
        GameObject oldRight = currentRight;
        
        GameObject itemToDestroy = null;
        GameObject newItem = null;

        if (direction == 1)
        {
            currentIndex = GetWrappedIndex(currentIndex + 1);
            itemToDestroy = oldLeft;
            
            StartCoroutine(MoveAndScale(oldLeft, outLeftPos, hiddenScale, true)); 
            StartCoroutine(MoveAndScale(oldCenter, leftPos, normalScale, false));
            StartCoroutine(MoveAndScale(oldRight, centerPos, normalScale, false));

            newItem = SpawnPrefab(GetWrappedIndex(currentIndex + 1), outRightPos, hiddenScale);
            StartCoroutine(MoveAndScale(newItem, rightPos, normalScale, false));

            currentLeft = oldCenter;
            currentCenter = oldRight;
            currentRight = newItem;
        }
        else if (direction == -1)
        {
            currentIndex = GetWrappedIndex(currentIndex - 1);
            itemToDestroy = oldRight;
            
            StartCoroutine(MoveAndScale(oldRight, outRightPos, hiddenScale, true));
            StartCoroutine(MoveAndScale(oldCenter, rightPos, normalScale, false));
            StartCoroutine(MoveAndScale(oldLeft, centerPos, normalScale, false));

            newItem = SpawnPrefab(GetWrappedIndex(currentIndex - 1), outLeftPos, hiddenScale);
            StartCoroutine(MoveAndScale(newItem, leftPos, normalScale, false));

            currentRight = oldCenter;
            currentCenter = oldLeft;
            currentLeft = newItem;
        }

        yield return new WaitForSeconds(transitionDuration);
        isAnimating = false;
    }

    public void LoadNewPrefabs(List<GameObject> newPrefabs, Transform newContainer, int startIndex = 0)
    {
        hasBeenInitialized = true;

        StopAllCoroutines();
        isAnimating = false;

        if (newContainer != null)
        {
            spawnContainer = newContainer;
        }

        if (spawnContainer != null)
        {
            foreach (Transform child in spawnContainer)
            {
                Destroy(child.gameObject);
            }
        }

        currentCenter = null;
        currentLeft = null;
        currentRight = null;

        prefabs = newPrefabs;
        
        currentIndex = startIndex;

        if (prefabs != null && prefabs.Count > 0)
        {
            if (prefabs.Count == 1)
            {
                currentCenter = SpawnPrefab(currentIndex, centerPos, normalScale);
            }
            else if (prefabs.Count == 2)
            {
                currentCenter = SpawnPrefab(currentIndex, centerPos, normalScale);
                currentRight = SpawnPrefab(GetWrappedIndex(currentIndex + 1), rightPos, normalScale);
                currentLeft = SpawnPrefab(GetWrappedIndex(currentIndex + 1), leftPos, normalScale); 
            }
            else
            {
                InitializeCarousel();
            }
        }
    }
    
    public void ClearCarousel()
    {
        StopAllCoroutines();
        isAnimating = false;

        if (spawnContainer != null)
        {
            foreach (Transform child in spawnContainer)
            {
                Destroy(child.gameObject);
            }
        }

        currentCenter = null;
        currentLeft = null;
        currentRight = null;
    }
}
