using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controlador del UI de login en el MainMenu
/// </summary>
public class LoginUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject loginFormPanel;

    [Header("Main UI")]
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;

    [Header("Login Form")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button submitLoginButton;
    [SerializeField] private TextMeshProUGUI messageText;

    private bool isFormVisible = false;

    private void Start()
    {
        // Configurar listeners de botones
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        if (submitLoginButton != null)
        {
            submitLoginButton.onClick.AddListener(OnSubmitLogin);
        }

        // Suscribirse a eventos de autenticación
        SupabaseAuthManager.Instance.OnAuthStateChanged += UpdateUI;
        SupabaseAuthManager.Instance.OnLoginSuccess += OnLoginSuccess;
        SupabaseAuthManager.Instance.OnLoginError += OnLoginError;
        SupabaseAuthManager.Instance.OnLogoutSuccess += OnLogoutSuccess;

        // Actualizar UI según estado actual
        UpdateUI(SupabaseAuthManager.Instance.IsLoggedIn);

        // Asegurar que el formulario esté oculto al inicio
        if (loginFormPanel != null)
        {
            loginFormPanel.SetActive(false);
        }

        // Limpiar mensaje
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (SupabaseAuthManager.Instance != null)
        {
            SupabaseAuthManager.Instance.OnAuthStateChanged -= UpdateUI;
            SupabaseAuthManager.Instance.OnLoginSuccess -= OnLoginSuccess;
            SupabaseAuthManager.Instance.OnLoginError -= OnLoginError;
            SupabaseAuthManager.Instance.OnLogoutSuccess -= OnLogoutSuccess;
        }
    }

    /// <summary>
    /// Actualiza el UI según el estado de autenticación
    /// </summary>
    private void UpdateUI(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            // Usuario logueado
            if (userNameText != null)
            {
                userNameText.text = SupabaseAuthManager.Instance.CurrentUserDisplayName;
            }

            if (actionButtonText != null)
            {
                actionButtonText.text = "Cerrar Sesión";
            }

            // Ocultar formulario si está visible
            if (loginFormPanel != null)
            {
                loginFormPanel.SetActive(false);
                isFormVisible = false;
            }
        }
        else
        {
            // Usuario no logueado
            if (userNameText != null)
            {
                userNameText.text = "Inicia sesión";
            }

            if (actionButtonText != null)
            {
                actionButtonText.text = "Iniciar Sesión";
            }
        }
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón principal
    /// </summary>
    private void OnActionButtonClicked()
    {
        if (SupabaseAuthManager.Instance.IsLoggedIn)
        {
            // Si está logueado, cerrar sesión
            Logout();
        }
        else
        {
            // Si no está logueado, mostrar/ocultar formulario
            ToggleLoginForm();
        }
    }

    /// <summary>
    /// Muestra u oculta el formulario de login
    /// </summary>
    private void ToggleLoginForm()
    {
        isFormVisible = !isFormVisible;

        if (loginFormPanel != null)
        {
            loginFormPanel.SetActive(isFormVisible);
        }

        // Limpiar campos y mensaje al mostrar
        if (isFormVisible)
        {
            if (emailInputField != null) emailInputField.text = "";
            if (passwordInputField != null) passwordInputField.text = "";
            if (messageText != null) messageText.text = "";
        }
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón de submit del formulario
    /// </summary>
    private void OnSubmitLogin()
    {
        string email = emailInputField != null ? emailInputField.text : "";
        string password = passwordInputField != null ? passwordInputField.text : "";

        // Validaciones básicas
        if (string.IsNullOrEmpty(email))
        {
            ShowMessage("Por favor ingresa tu email", true);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage("Por favor ingresa tu contraseña", true);
            return;
        }

        // Mostrar mensaje de carga
        ShowMessage("Iniciando sesión...", false);

        // Deshabilitar botón mientras se procesa
        if (submitLoginButton != null)
        {
            submitLoginButton.interactable = false;
        }

        // Intentar login
        SupabaseAuthManager.Instance.Login(email, password);
    }

    /// <summary>
    /// Se llama cuando el login es exitoso
    /// </summary>
    private void OnLoginSuccess(string displayName)
    {
        ShowMessage($"¡Bienvenido, {displayName}!", false);

        // Esperar un momento y ocultar el formulario
        StartCoroutine(HideFormAfterDelay(1.5f));

        // Rehabilitar botón
        if (submitLoginButton != null)
        {
            submitLoginButton.interactable = true;
        }
    }

    /// <summary>
    /// Se llama cuando hay un error en el login
    /// </summary>
    private void OnLoginError(string errorMessage)
    {
        ShowMessage(errorMessage, true);

        // Rehabilitar botón
        if (submitLoginButton != null)
        {
            submitLoginButton.interactable = true;
        }
    }

    /// <summary>
    /// Cierra la sesión
    /// </summary>
    private void Logout()
    {
        SupabaseAuthManager.Instance.Logout();
    }

    /// <summary>
    /// Se llama cuando el logout es exitoso
    /// </summary>
    private void OnLogoutSuccess()
    {
        UnityEngine.Debug.Log("[LoginUI] Sesión cerrada exitosamente");
    }

    /// <summary>
    /// Muestra un mensaje en el UI
    /// </summary>
    private void ShowMessage(string message, bool isError)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = isError ? Color.red : Color.green;
        }
    }

    /// <summary>
    /// Oculta el formulario después de un delay
    /// </summary>
    private System.Collections.IEnumerator HideFormAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (loginFormPanel != null)
        {
            loginFormPanel.SetActive(false);
            isFormVisible = false;
        }

        // Limpiar mensaje
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}