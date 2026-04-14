using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Mustahik : MonoBehaviour, IInteractable
{
    [Header("Mustahik Settings")]
    public int requiredAmount = 1;
    private bool fulfilled = false;
    public static event Action<Mustahik> OnMustahikFulfilledGlobal;

    [Header("Visuals & UI (Fade)")]
    public GameObject iconObject;      // Tanda Seru/Tangan
    public GameObject fulfilledIndicator; // Tanda Love/Centang

    public float iconVisibleDistance = 15f;
    public float distanceFadeSpeed = 3f;

    private CanvasGroup iconCanvasGroup;
    private CanvasGroup givenCanvasGroup;

    [Header("Animation & Rotation")]
    public float detectionRange = 5f;
    public float stayAwayRange = 7f;
    public float rotationSpeed = 5f;

    private bool hasFacedPlayer = false;
    private bool isRotating = false;
    private GameObject interactor;

    private Rigidbody rb;
    private Animator anim;
    private Camera mainCamera;
    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        mainCamera = Camera.main;

        // --- SETUP VISUAL ---
        if (iconObject != null)
        {
            iconCanvasGroup = iconObject.GetComponent<CanvasGroup>();
            iconObject.SetActive(false);
            if (iconCanvasGroup != null) iconCanvasGroup.alpha = 0f;
        }

        if (fulfilledIndicator != null)
        {
            givenCanvasGroup = fulfilledIndicator.GetComponent<CanvasGroup>();
            fulfilledIndicator.SetActive(true);
            if (givenCanvasGroup != null) givenCanvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        HandleAutoRotation();
        HandleIconVisibility();
    }

    void HandleAutoRotation()
    {
        if (playerTransform == null) return;

        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        if (dist <= detectionRange && !hasFacedPlayer)
        {
            isRotating = true;
            interactor = playerTransform.gameObject;
            hasFacedPlayer = true;
        }
        else if (dist > stayAwayRange)
        {
            isRotating = false;
            hasFacedPlayer = false;
            anim.SetBool("Turning", false);
        }

        if (isRotating && interactor != null)
        {
            anim.SetBool("Turning", true);
            FacePlayer(interactor);

            Vector3 directionToPlayer = interactor.transform.position - transform.position;
            directionToPlayer.y = 0;
            if (Vector3.Angle(transform.forward, directionToPlayer) < 5f)
            {
                anim.SetBool("Turning", false);
            }
        }
    }

    void FacePlayer(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }

    void HandleIconVisibility()
    {
        if (fulfilled)
        {
            if (fulfilledIndicator != null && fulfilledIndicator.activeSelf && mainCamera != null)
                fulfilledIndicator.transform.rotation = mainCamera.transform.rotation;
            return;
        }

        if (iconObject == null || playerTransform == null || iconCanvasGroup == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        float targetAlpha = (dist <= iconVisibleDistance) ? 1f : 0f;
        iconCanvasGroup.alpha = Mathf.MoveTowards(iconCanvasGroup.alpha, targetAlpha, Time.deltaTime * distanceFadeSpeed);

        if (iconCanvasGroup.alpha > 0.01f && !iconObject.activeSelf) iconObject.SetActive(true);
        else if (iconCanvasGroup.alpha <= 0.01f && iconObject.activeSelf) iconObject.SetActive(false);

        if (iconObject.activeSelf && mainCamera != null)
            iconObject.transform.rotation = mainCamera.transform.rotation;
    }

    public void Interact(GameObject interactor)
    {
        if (fulfilled) return;

        this.interactor = interactor;
        isRotating = true;
        FacePlayer(interactor);

        if (InventoryManager.Instance.CanGive(requiredAmount))
        {
            bool ok = InventoryManager.Instance.RemoveZakat(requiredAmount);
            if (ok)
            {
                anim.SetBool("Turning", false);
                anim.SetTrigger("Talking");
                StartCoroutine(WaitForTalkingAnimation());

                // TAMBAHKAN INI:
                InventoryManager.Instance.NotifyZakatDistributed(requiredAmount);

                fulfilled = true;
                OnMustahikFulfilledGlobal?.Invoke(this);

                // Jalankan Fade Visual
                StartCoroutine(DoTransitionVisuals());
            }
        }
        else
        {
            Debug.Log("Zakat kurang!");
        }
    }

    IEnumerator WaitForTalkingAnimation()
    {
        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator DoTransitionVisuals()
    {
        float duration = 1.0f;
        float timer = 0;
        float startIconAlpha = (iconCanvasGroup != null) ? iconCanvasGroup.alpha : 1f;
        bool canFadeIcon = iconCanvasGroup != null;
        bool canFadeGiven = givenCanvasGroup != null;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            if (canFadeIcon) iconCanvasGroup.alpha = Mathf.Lerp(startIconAlpha, 0f, t);
            if (canFadeGiven) givenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        if (canFadeIcon) { iconCanvasGroup.alpha = 0f; iconObject.SetActive(false); }
        if (canFadeGiven) givenCanvasGroup.alpha = 1f;
    }

    public bool IsFulfilled() => fulfilled;
    public int GetRequiredAmount() => requiredAmount;
}