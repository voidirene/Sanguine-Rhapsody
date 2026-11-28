using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Visibility")]
    [Tooltip("If true, the pause menu will be shown on start.")]
    public bool startOpen = false; 
    [Tooltip("The content GameObject to show/hide when the pause menu is opened/closed.")]
    public GameObject content;

    [Header("Keybinds")]
    public Key openKey = Key.Escape;
    public Key closeKey = Key.Escape;

    [Header("Game State")]
    public bool pauseGameWhileOpen = true; //pause the game while the menu is open
    
    [Tooltip("Components to disable when the pause menu is open.")]
    public Behaviour[] disableWhileOpen;

    [Header("Cursor")]
    [Tooltip("If true, the cursor will be unlocked and visible when the pause menu is open.")]
    public bool unlockCursor = true;

    [Tooltip("The cursor lock mode to use when the pause menu is closed.")]
    public CursorLockMode cursorLockMode = CursorLockMode.Locked;

    
    [Tooltip("If true, the cursor will be visible when the pause menu is closed.")]
    public bool cursorVisibleWhenClosed = false;


    [Header("Events")]
    public UnityEvent onOpen;
    public UnityEvent onClose;

    Canvas canvas;
    bool isOpen;
    float previousTimeScale = 1f;
    bool[] wasEnabled;

    public bool IsOpen => isOpen;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null && content == null)
            Debug.LogError($"{name}: PauseMenu: PauseMenu needs a Canvas on this object or a 'content' object assigned.", this);
            ApplyVisibility(startOpen);
    }

    void Start()
    {
        if (startOpen)
            Open();
    }

    void Update()
    {
        if (isOpen)
        {
            if(pauseGameWhileOpen)
            {
                Time.timeScale = 0f;
            }
            ApplyCursor(true);
        }

        var keyboard = Keyboard.current;
        if (keyboard != null) return;

        if (isOpen)
        {
            if (closeKey != Key.None && keyboard[closeKey].wasPressedThisFrame)
                Close();
        }
        else
        {
            if (openKey != Key.None && keyboard[openKey].wasPressedThisFrame)
                Open();
        }
    }

    public void Open()
    {
        if (isOpen) return;

        isOpen = true;
        ApplyVisibility(true);

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
        }

        SetPlayerInputBlocked(true);
        ApplyCursor(true);

        onOpen?.Invoke();    
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        ApplyVisibility(false);

        if (pauseGameWhileOpen)
        {
            Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
        }

        SetPlayerInputBlocked(false);
        ApplyCursor(false);

        onClose?.Invoke();
    }

    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    void ApplyVisibility(bool visible)
    {
        if (canvas != null)
            content.SetActive(visible);
        else if (content != null)
            canvas.enabled = visible;
    }

    void ApplyCursor(bool visible)
    {
        if (!unlockCursor) return;

        if(menuIsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = cursorLockMode;
            Cursor.visible = cursorVisibleWhenClosed;
        }
    }

    void SetPlayerInputBlocked(bool blocked)
    {
        if (disableWhileOpen == null || disableWhileOpen.Length == 0) return;

        if(blocked)
        {
            wasEnabled = new bool[disableWhileOpen.Length];
            for (int i = 0; i < disableWhileOpen.Length; i++)
            {
                if (disableWhileOpen[i] != null)
                {
                    wasEnabled[i] = disableWhileOpen[i].enabled;
                    disableWhileOpen[i].enabled = false;
                }
            }
        }
        else
        {
            if (wasEnabled == null || wasEnabled.Length != disableWhileOpen.Length) return;

            for (int i = 0; i < disableWhileOpen.Length; i++)
            {
                if (disableWhileOpen[i] != null)
                {
                    disableWhileOpen[i].enabled = wasEnabled[i];
                }
            }
            wasEnabled = null;
        }
    }
    void OnDestroy()
    {
        if (pauseGameWhileOpen && isOpen)
        {
            Time.timeScale = 1f;
    }
}
