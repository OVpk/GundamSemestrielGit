using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SwipeAxis
{
    Horizontal,
    Vertical
}

public class SwipeCarousel : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public event Action<int> OnItemClicked;
    public event Action<int> OnIndexChanged;

    public SwipeAxis swipeAxis = SwipeAxis.Horizontal;
    public List<GameObject> prefabs;
    public Transform spawnContainer;

    public Vector3 normalScale = Vector3.one;
    public Vector3 hiddenScale = Vector3.zero;

    public float transitionDuration = 0.4f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float minSwipeDistance = 50f;
    [SerializeField] private bool invertSwipeDirection = false;

    public Vector3 centerPos = new Vector3(0, 0, -2);
    public Vector3 prevPos = new Vector3(-3, 0, 0);
    public Vector3 nextPos = new Vector3(3, 0, 0);
    public Vector3 outPrevPos = new Vector3(-6, 0, 2);
    public Vector3 outNextPos = new Vector3(6, 0, 2);

    private int currentIndex = 0;
    private GameObject currentCenter;
    private GameObject currentPrev;
    private GameObject currentNext;
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
        if (prefabs == null || prefabs.Count == 0) return;

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

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAnimating) return;
        if (prefabs == null || prefabs.Count <= 1) return;

        float swipeDistance = swipeAxis == SwipeAxis.Horizontal
            ? eventData.position.x - startTouchPosition.x
            : eventData.position.y - startTouchPosition.y;

        float swipeDuration = Time.time - startTouchTime;

        if (swipeDuration > 0 && Mathf.Abs(swipeDistance) >= minSwipeDistance)
        {
            bool goToPrev = swipeDistance > 0;
            if (invertSwipeDirection) goToPrev = !goToPrev;

            if (goToPrev) SwipePrev();
            else SwipeNext();
        }
    }

    private void InitializeCarousel()
    {
        hasBeenInitialized = true;

        currentCenter = SpawnPrefab(currentIndex, centerPos, normalScale);
        currentPrev = SpawnPrefab(GetWrappedIndex(currentIndex - 1), prevPos, normalScale);
        currentNext = SpawnPrefab(GetWrappedIndex(currentIndex + 1), nextPos, normalScale);
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

    private void SwipeNext() => StartCoroutine(AnimateCarousel(1));
    private void SwipePrev() => StartCoroutine(AnimateCarousel(-1));

    private IEnumerator AnimateCarousel(int direction)
    {
        isAnimating = true;

        GameObject oldCenter = currentCenter;
        GameObject oldPrev = currentPrev;
        GameObject oldNext = currentNext;

        if (direction == 1)
        {
            currentIndex = GetWrappedIndex(currentIndex + 1);

            StartCoroutine(MoveAndScale(oldPrev, outPrevPos, hiddenScale, true));
            StartCoroutine(MoveAndScale(oldCenter, prevPos, normalScale, false));
            StartCoroutine(MoveAndScale(oldNext, centerPos, normalScale, false));

            GameObject newItem = SpawnPrefab(GetWrappedIndex(currentIndex + 1), outNextPos, hiddenScale);
            StartCoroutine(MoveAndScale(newItem, nextPos, normalScale, false));

            currentPrev = oldCenter;
            currentCenter = oldNext;
            currentNext = newItem;
        }
        else if (direction == -1)
        {
            currentIndex = GetWrappedIndex(currentIndex - 1);

            StartCoroutine(MoveAndScale(oldNext, outNextPos, hiddenScale, true));
            StartCoroutine(MoveAndScale(oldCenter, nextPos, normalScale, false));
            StartCoroutine(MoveAndScale(oldPrev, centerPos, normalScale, false));

            GameObject newItem = SpawnPrefab(GetWrappedIndex(currentIndex - 1), outPrevPos, hiddenScale);
            StartCoroutine(MoveAndScale(newItem, prevPos, normalScale, false));

            currentNext = oldCenter;
            currentCenter = oldPrev;
            currentPrev = newItem;
        }

        OnIndexChanged?.Invoke(currentIndex);

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

        ClearSpawnContainer();

        currentCenter = null;
        currentPrev = null;
        currentNext = null;

        prefabs = newPrefabs;
        currentIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, (prefabs?.Count ?? 1) - 1));

        if (prefabs != null && prefabs.Count > 0)
        {
            if (prefabs.Count == 1)
            {
                currentCenter = SpawnPrefab(currentIndex, centerPos, normalScale);
            }
            else if (prefabs.Count == 2)
            {
                currentCenter = SpawnPrefab(currentIndex, centerPos, normalScale);
                currentNext = SpawnPrefab(GetWrappedIndex(currentIndex + 1), nextPos, normalScale);
                currentPrev = SpawnPrefab(GetWrappedIndex(currentIndex + 1), prevPos, normalScale);
            }
            else
            {
                InitializeCarousel();
            }

            OnIndexChanged?.Invoke(currentIndex);
        }
    }

    public void ClearCarousel()
    {
        StopAllCoroutines();
        isAnimating = false;
        ClearSpawnContainer();

        currentCenter = null;
        currentPrev = null;
        currentNext = null;
    }

    private void ClearSpawnContainer()
    {
        if (spawnContainer == null) return;

        foreach (Transform child in spawnContainer)
        {
            Destroy(child.gameObject);
        }
    }
}