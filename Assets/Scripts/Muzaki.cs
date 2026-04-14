using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Muzaki : MonoBehaviour, IInteractable
{
    [Header("Muzaki Settings")]
    public int zakatAmount = 1;
    public bool randomizeOnStart = true;
    public int randomMin = 1;
    public int randomMax = 3;

    private bool hasGiven = false;

    [Header("Visuals & UI (Fade)")]
    [Tooltip("Drag object Canvas Icon (Tanda Tanya)")]
    public GameObject iconObject;
    [Tooltip("Drag object Indicator Selesai (Centang/Terima Kasih)")]
    public GameObject givenIndicator;

    public float iconVisibleDistance = 15f;
    public float distanceFadeSpeed = 3f; // Kecepatan fade jarak

    // Komponen untuk mengatur transparansi
    private CanvasGroup iconCanvasGroup;
    private CanvasGroup givenCanvasGroup;

    [Header("Animation & Rotation")]
    public float detectionRange = 5f; // Jarak NPC mulai nengok ke player
    public float stayAwayRange = 7f;  // Jarak reset nengok
    public float rotationSpeed = 5f;

    private bool hasFacedPlayer = false;
    private bool isRotating = false;
    private GameObject interactor; // Player

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
        // Cari Player untuk referensi jarak & rotasi
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        mainCamera = Camera.main;

        if (randomizeOnStart)
            zakatAmount = Random.Range(randomMin, randomMax + 1);

        // --- SETUP VISUAL & CANVAS GROUP ---
        if (iconObject != null)
        {
            iconCanvasGroup = iconObject.GetComponent<CanvasGroup>();
            iconObject.SetActive(false); // Mulai mati & transparan
            if (iconCanvasGroup != null) iconCanvasGroup.alpha = 0f;
        }

        if (givenIndicator != null)
        {
            givenCanvasGroup = givenIndicator.GetComponent<CanvasGroup>();
            givenIndicator.SetActive(true); // Nyalakan tapi alpha 0
            if (givenCanvasGroup != null) givenCanvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        HandleAutoRotation();   // Logika NPC nengok otomatis
        HandleIconVisibility(); // Logika Icon Fade in/out
    }

    // --- BAGIAN 1: LOGIKA ROTASI & ANIMASI ---
    void HandleAutoRotation()
    {
        if (playerTransform == null) return;

        // Hitung jarak horizontal
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        // Cek jarak untuk mulai nengok
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

        // Eksekusi Rotasi
        if (isRotating && interactor != null)
        {
            anim.SetBool("Turning", true);
            FacePlayer(interactor);

            // Stop animasi turning jika sudah pas menghadap
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

    // --- BAGIAN 2: LOGIKA ICON FADE JARAK ---
    void HandleIconVisibility()
    {
        // Kalau sudah memberi zakat, stop logika icon jarak jauh (biarkan Coroutine handle sisanya)
        if (hasGiven)
        {
            // Tetap pastikan Indicator Selesai menghadap kamera
            if (givenIndicator != null && givenIndicator.activeSelf && mainCamera != null)
                givenIndicator.transform.rotation = mainCamera.transform.rotation;
            return;
        }

        if (iconObject == null || playerTransform == null || iconCanvasGroup == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        // Target Alpha: 1 jika dekat, 0 jika jauh
        float targetAlpha = (dist <= iconVisibleDistance) ? 1f : 0f;

        // Gerakkan Alpha pelan-pelan
        iconCanvasGroup.alpha = Mathf.MoveTowards(iconCanvasGroup.alpha, targetAlpha, Time.deltaTime * distanceFadeSpeed);

        // Optimasi Active/Inactive
        if (iconCanvasGroup.alpha > 0.01f && !iconObject.activeSelf) iconObject.SetActive(true);
        else if (iconCanvasGroup.alpha <= 0.01f && iconObject.activeSelf) iconObject.SetActive(false);

        // Billboard Effect
        if (iconObject.activeSelf && mainCamera != null)
            iconObject.transform.rotation = mainCamera.transform.rotation;
    }

    // --- BAGIAN 3: INTERAKSI ---
    public void Interact(GameObject interactor)
    {
        if (hasGiven) return;

        this.interactor = interactor;
        isRotating = true;
        FacePlayer(interactor);

        int added = InventoryManager.Instance.AddZakat(zakatAmount);
        if (added > 0)
        {
            // Animasi
            anim.SetBool("Turning", false);
            anim.SetTrigger("Talking");
            StartCoroutine(WaitForTalkingAnimation());

            hasGiven = true;
            ZakatTracker.Instance?.NotifyMuzakiGiven(this, added);

            // Jalankan Fade Visual (Icon hilang, Centang muncul)
            StartCoroutine(DoTransitionVisuals());
        }
    }

    IEnumerator WaitForTalkingAnimation()
    {
        yield return new WaitForSeconds(1.5f); // Estimasi durasi bicara, atau pakai logic animator state
    }

    // --- COROUTINE FADE TRANSITION (CROSSFADE) ---
    IEnumerator DoTransitionVisuals()
    {
        float duration = 1.0f;
        float timer = 0;

        // Mulai dari alpha terakhir icon (supaya mulus)
        float startIconAlpha = (iconCanvasGroup != null) ? iconCanvasGroup.alpha : 1f;

        bool canFadeIcon = iconCanvasGroup != null;
        bool canFadeGiven = givenCanvasGroup != null;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Fade OUT Icon
            if (canFadeIcon) iconCanvasGroup.alpha = Mathf.Lerp(startIconAlpha, 0f, t);
            // Fade IN Indicator
            if (canFadeGiven) givenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        if (canFadeIcon)
        {
            iconCanvasGroup.alpha = 0f;
            iconObject.SetActive(false);
        }
        if (canFadeGiven) givenCanvasGroup.alpha = 1f;
    }

    public bool HasGiven() => hasGiven;
    public int GetZakatAmount() => zakatAmount;
}