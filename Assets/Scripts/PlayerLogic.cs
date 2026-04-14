    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro; 

    public class PlayerLogic : MonoBehaviour
    {
        [Header("Player Setting")]
        public Transform PlayerOrientation;
        public CameraLogic camlogic;
        public Animator anim;
        public float walkspeed = 1.5f;
        public float runspeed = 3f;

        private float horizontalInput;
        private float verticalInput;
        private Vector3 moveDirection;
        private Rigidbody rb;
        private bool grounded = true;

        [Header("SFX Zakat")]
        public AudioClip StepAudio;
        public AudioClip RunStepAudio;
        public AudioClip sfxAmbilZakat;
        public AudioClip sfxBeriZakat;
        AudioSource PlayerAudio;

        [Header("Interaction UI")]
        public TextMeshProUGUI promptText; 

        [Header("Cheat UI (BARU)")]
        public TextMeshProUGUI cheatText; 
        private Coroutine cheatAnimCoroutine;
        private float cheatFadeDuration = 0.3f; 

        [Header("Interaction Settings")]
        public float interactRadius = 2f;

        private GameObject currentlyPromptedObject = null;
        private bool canInteract = true; 
        private Color originalTextColor; 
        private Coroutine uiAnimCoroutine; 

        // --- TAMBAHAN UNTUK QTE (STRUGGLE) ---
        private bool isStruggling = false; 
        // -------------------------------------

        void Start()
        {
            rb = this.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }

            if (PlayerOrientation == null) PlayerOrientation = transform;
            PlayerAudio = this.GetComponent<AudioSource>();

            if (promptText != null)
            {
                originalTextColor = promptText.color; 
                Color invisibleColor = originalTextColor;
                invisibleColor.a = 0;
                promptText.color = invisibleColor;
                promptText.gameObject.SetActive(false);
            }

            if (cheatText != null)
            {
                cheatText.text = "";
                Color c = cheatText.color;
                c.a = 0f;
                cheatText.color = c;
                cheatText.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (isStruggling) return;

            Movement();
            DetectAndPrompt();
            HandleInteraction();
        }

        public void StartStruggleQTE(float duration, int requiredPresses, System.Action onSuccess, System.Action onFail)
        {
            StartCoroutine(StruggleRoutine(duration, requiredPresses, onSuccess, onFail));
        }

        private IEnumerator StruggleRoutine(float duration, int requiredPresses, System.Action onSuccess, System.Action onFail)
        {
            isStruggling = true;
            Time.timeScale = 0.2f; 
            
            int currentPresses = 0;
            float timer = 0f;

            if (cheatText != null) 
            {
                cheatText.gameObject.SetActive(true);
                cheatText.color = Color.red; 
            }

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime; 

                if (Input.GetKeyDown(KeyCode.F))
                {
                    currentPresses++;
                }

                if (cheatText != null)
                {
                    cheatText.text = $"TAHAN MALING!\nTEKAN [F] BERULANG KALI!\n{currentPresses}/{requiredPresses}";
                }

                if (currentPresses >= requiredPresses)
                {
                    Time.timeScale = 1f; 
                    isStruggling = false;
                    
                    if (cheatText != null) cheatText.gameObject.SetActive(false);
                    ShowCheatText("BERHASIL MENGHALAU MALING!", 2f);
                    
                    onSuccess?.Invoke();
                    yield break; 
                }

                yield return null;
            }

            Time.timeScale = 1f; 
            isStruggling = false;

            if (cheatText != null) cheatText.gameObject.SetActive(false);
            ShowCheatText("GAGAL! ZAKAT DICURI!", 2f);

            onFail?.Invoke();
        }

        public void ShowCheatText(string message, float duration = 2f)
        {
            if (cheatText == null) return;
            if (cheatAnimCoroutine != null) StopCoroutine(cheatAnimCoroutine);
            cheatAnimCoroutine = StartCoroutine(HandleCheatTextDisplay(message, duration));
        }

        private IEnumerator HandleCheatTextDisplay(string message, float duration)
        {
            cheatText.gameObject.SetActive(true);
            cheatText.text = message;
            cheatText.color = Color.white; 
            
            Color currentColor = cheatText.color;
            float timer = 0f;

            currentColor.a = 0f;
            cheatText.color = currentColor;
            while (timer < cheatFadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / cheatFadeDuration;
                currentColor.a = Mathf.Lerp(0f, 1f, t);
                cheatText.color = currentColor;
                yield return null;
            }
            currentColor.a = 1f;
            cheatText.color = currentColor;

            yield return new WaitForSeconds(duration);

            timer = 0f; 
            while (timer < cheatFadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / cheatFadeDuration;
                currentColor.a = Mathf.Lerp(1f, 0f, t);
                cheatText.color = currentColor;
                yield return null;
            }
            currentColor.a = 0f;
            cheatText.color = currentColor;

            cheatText.gameObject.SetActive(false);
        }

        private void Movement()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
            if (camlogic == null || camlogic.ViewPoint == null) return;

            Transform camTransform = camlogic.ViewPoint;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();
            moveDirection = camForward * verticalInput + camRight * horizontalInput;

            if (grounded && moveDirection != Vector3.zero)
            {
                float speed = Input.GetKey(KeyCode.LeftShift) ? runspeed : walkspeed;
                anim.SetBool("Run", Input.GetKey(KeyCode.LeftShift));
                anim.SetBool("Walk", !Input.GetKey(KeyCode.LeftShift));
                rb.velocity = new Vector3(moveDirection.normalized.x * speed, rb.velocity.y, moveDirection.normalized.z * speed);
            }
            else
            {
                anim.SetBool("Walk", false); anim.SetBool("Run", false);
                if (grounded) rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }

            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.z));
                PlayerOrientation.rotation = Quaternion.Slerp(PlayerOrientation.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        void DetectAndPrompt()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius);
            GameObject nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var c in hits)
            {
                var ia = c.GetComponent<IInteractable>() ?? c.GetComponentInParent<IInteractable>();
                if (ia == null) continue;
                float d = Vector3.Distance(transform.position, c.transform.position);
                if (d < nearestDist) { nearest = c.gameObject; nearestDist = d; }
            }

            if (nearest != currentlyPromptedObject)
            {
                if (currentlyPromptedObject != null && nearest == null) AnimatePrompt(false);
                else if (currentlyPromptedObject == null && nearest != null) AnimatePrompt(true);
                currentlyPromptedObject = nearest;
            }

            if (currentlyPromptedObject != null && promptText != null)
            {
                if (!promptText.gameObject.activeSelf && promptText.color.a > 0)
                    promptText.gameObject.SetActive(true);

                var muz = currentlyPromptedObject.GetComponent<Muzaki>() ?? currentlyPromptedObject.GetComponentInParent<Muzaki>();
                var mus = currentlyPromptedObject.GetComponent<Mustahik>() ?? currentlyPromptedObject.GetComponentInParent<Mustahik>();
                var maling = currentlyPromptedObject.GetComponent<Maling>() ?? currentlyPromptedObject.GetComponentInParent<Maling>();

                Color darkGreen = new Color(0.196f, 0.804f, 0.196f, 1f);
                Color darkOrange = new Color(0.863f, 0.192f, 0.196f, 1f);

                canInteract = true; 

                if (muz != null)
                {
                    if (muz.HasGiven())
                    {
                        promptText.text = "Zakat sudah diambil";
                        SetTextColorSafe(darkGreen);
                        canInteract = false; 
                    }
                    else
                    {
                        int amount = muz.GetZakatAmount();
                        if (InventoryManager.Instance.HasSpaceFor(amount))
                        {
                            promptText.text = $"Tekan [F] Ambil Zakat ({amount})";
                            SetTextColorSafe(Color.white);
                            canInteract = true;
                        }
                        else
                        {
                            promptText.text = "Tas Penuh / Tidak Cukup!";
                            SetTextColorSafe(darkOrange); 
                            canInteract = false; 
                        }
                    }
                }
                else if (mus != null)
                {
                    if (mus.IsFulfilled())
                    {
                        promptText.text = "Zakat sudah diberi";
                        SetTextColorSafe(darkGreen);
                        canInteract = false;
                    }
                    else
                    {
                        bool cukup = InventoryManager.Instance.CurrentZakat >= mus.GetRequiredAmount();
                        if (cukup)
                        {
                            promptText.text = $"Tekan [F] Beri Zakat ({mus.GetRequiredAmount()})";
                            SetTextColorSafe(Color.white);
                            canInteract = true;
                        }
                        else
                        {
                            promptText.text = $"Butuh {mus.GetRequiredAmount()} (Kurang)";
                            SetTextColorSafe(darkOrange);
                            canInteract = false;
                        }
                    }
                }
                else if (maling != null)
                {
                    int stolen = maling.GetStolenAmount();

                    if (maling.IsInvincible) 
                    {
                        promptText.text = "Maling Kabur! (Tidak bisa dikejar)";
                        SetTextColorSafe(darkOrange);
                        canInteract = false;
                    }
                    else if (stolen > 0)
                    {
                        promptText.text = $"Tekan [F] Rebut Zakat ({stolen})";
                        SetTextColorSafe(Color.white);
                        canInteract = true;
                    }
                    else
                    {
                        promptText.gameObject.SetActive(false);
                        canInteract = false;
                    }
                }
                else
                {
                    promptText.text = "Tekan [F] Interaksi";
                    SetTextColorSafe(Color.white);
                    canInteract = true;
                }
            }
        }

        private void HandleInteraction()
        {
            if (Input.GetKeyDown(KeyCode.F) && currentlyPromptedObject != null && canInteract)
            {
                var interactable = currentlyPromptedObject.GetComponent<IInteractable>() ?? currentlyPromptedObject.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    int before = InventoryManager.Instance != null ? InventoryManager.Instance.CurrentZakat : -1;
                    interactable.Interact(gameObject);
                    int after = InventoryManager.Instance != null ? InventoryManager.Instance.CurrentZakat : -1;

                    if (before >= 0 && after >= 0)
                    {
                        if (after > before)
                        {
                            if (sfxAmbilZakat != null) PlayerAudio.PlayOneShot(sfxAmbilZakat);
                            DetectAndPrompt();
                        }
                        else if (after < before)
                        {
                            if (sfxBeriZakat != null) PlayerAudio.PlayOneShot(sfxBeriZakat);
                            DetectAndPrompt();
                        }
                    }
                }
            }
        }

        public void step() { PlayerAudio.clip = StepAudio; PlayerAudio.Play(); }
        public void runstep() { PlayerAudio.clip = RunStepAudio; PlayerAudio.Play(); }

        void AnimatePrompt(bool show)
        {
            if (uiAnimCoroutine != null) StopCoroutine(uiAnimCoroutine);
            uiAnimCoroutine = StartCoroutine(FadeUI(show));
        }

        IEnumerator FadeUI(bool show)
        {
            float duration = 0.3f;
            float timer = 0;
            float startAlpha = promptText.color.a;
            float endAlpha = show ? 1f : 0f;

            if (show) promptText.gameObject.SetActive(true);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                Color currentColor = promptText.color;
                currentColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
                promptText.color = currentColor;
                yield return null;
            }

            Color finalColor = promptText.color;
            finalColor.a = endAlpha;
            promptText.color = finalColor;

            if (!show) promptText.gameObject.SetActive(false);
        }

        void SetTextColorSafe(Color targetColor)
        {
            float currentAlpha = promptText.color.a;
            promptText.color = new Color(targetColor.r, targetColor.g, targetColor.b, currentAlpha);
            originalTextColor = new Color(targetColor.r, targetColor.g, targetColor.b, originalTextColor.a);
        }
    }