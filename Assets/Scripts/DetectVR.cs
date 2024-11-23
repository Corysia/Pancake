using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

// Inspired by: https://gist.github.com/demonixis/fc2f9154cd9d87e5f1c6a7a1de2dbb70
public class DetectVR : MonoBehaviour
{
    [SerializeField] [Tooltip("The XR Origin or VR Rig in the scene.")]
    private XROrigin xrOrigin;

    [SerializeField]
    [Tooltip("The Desktop Camera.  This could be a First Person Controller, Third Person Controller, or other camera.")]
    private Camera desktopCamera;

    /// <summary>
    /// Start is called before the first frame update.
    /// This method determines whether to enable VR or desktop camera based on the platform, command line arguments, and XR settings.
    /// </summary>
    private void Start()
    {
        var operatingSystem = Application.platform;
        Debug.Log($"Running on {operatingSystem}.");

        // Check if running on macOS
        if (operatingSystem == RuntimePlatform.OSXEditor || operatingSystem == RuntimePlatform.OSXPlayer)
        {
            EnableDesktopCamera();
            return;
        }

        // Check command line arguments for disabling VR
        var args = System.Environment.GetCommandLineArgs();
        Debug.Log($"Arguments: {string.Join(", ", args)}");
        foreach (var arg in args)
        {
            Debug.Log($"Argument: {arg}");
            if (arg == "-novr")
            {
                Debug.Log($"VR is disabled by command line argument.");
                EnableDesktopCamera();
                return;
            }
        }

        var xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null)
        {
            Debug.Log($"XRGeneralSettings is null.");
            EnableDesktopCamera();
            return;
        }

        var xrManager = xrSettings.Manager;
        if (xrManager == null)
        {
            Debug.Log($"XRManagerSettings is null.");
            EnableDesktopCamera();
            return;
        }

        var xrLoader = xrManager.activeLoader;
        if (xrLoader == null)
        {
            // If the XR Loader is null, we don't have a VR camera.
            Debug.Log($"XRLoader is null.");
            EnableDesktopCamera();
            GameManager.instance.vrCapable = false;
            return;
        }

        Debug.Log($"Loaded XR Device: {xrLoader.name}");

        // Check XR Display Subsystem
        var xrDisplay = xrLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
        Debug.Log($"XRDisplay: {xrDisplay != null}");

        if (xrDisplay != null)
        {
            if (xrDisplay.TryGetDisplayRefreshRate(out var refreshRate))
            {
                Debug.Log($"Refresh Rate: {refreshRate}hz");
                Time.fixedDeltaTime = 1f / refreshRate;
            }
        }
        else
        {
            xrDisplay.Destroy();
            xrManager.DeinitializeLoader();
            EnableDesktopCamera();
            GameManager.instance.vrCapable = false;
            return;
        }

        // Check XR Input Subsystem
        var xrInput = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();
        Debug.Log($"XRInput: {xrInput != null}");

        if (xrInput != null)
        {
            xrInput.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
            xrInput.TryRecenter();
        }

        // Enable VR Camera
        EnableVRCamera();
    }

    /// <summary>
    /// Switches from the VR camera to the desktop camera.
    /// </summary>
    private void EnableDesktopCamera()
    {
        // Switch from the VR camera to the desktop camera.
        Debug.Log("<color=green>Enabling Desktop Camera</color>");
        GameManager.instance.inVR = false;

        // Disable the XR Origin.
        xrOrigin.gameObject.SetActive(false);

        // Enable the Desktop Camera.
        desktopCamera.gameObject.SetActive(true);
    }

    /// <summary>
    /// Switches from the VR camera to the desktop camera.
    /// </summary>
    ContextMenu("Switch to Desktop Camera")
    private void SwitchToDesktopCamera()
    {
        // Set the desktop camera's scale to the same as the XR origin's scale.
        desktopCamera.transform.localScale = xrOrigin.transform.localScale;

        // Set the desktop camera's position to the same as the XR origin's position.
        desktopCamera.transform.position = xrOrigin.transform.position;

        // Set the desktop camera's rotation to the same as the XR origin's rotation.
        desktopCamera.transform.rotation = xrOrigin.transform.rotation;

        // Enable the desktop camera.
        EnableDesktopCamera();
    }

    /// <summary>
    /// Switches from the desktop camera to the VR camera.
    /// </summary>
    ContextMenu("Switch to VR Camera")
    private void SwitchToVRCamera()
    {
        EnableVRCamera();
    }

    /// <summary>
    /// Switches from the desktop camera to the VR camera.
    /// </summary>
    /// <returns>true if the switch was successful, false otherwise.</returns>
    private bool EnableVRCamera()
    {
        Debug.Log("<color=green>Enabling VR Camera</color>");
        
        if (GameManager.instance.inVR) return true;

        // Set the XR origin's scale to the same as the desktop camera's scale.
        xrOrigin.transform.localScale = desktopCamera.transform.localScale;
        
        // Set the XR origin's position to the same as the desktop camera's position.
        xrOrigin.transform.position = desktopCamera.transform.position;
        
        // Set the XR origin's rotation to the same as the desktop camera's rotation.
        xrOrigin.transform.rotation = desktopCamera.transform.rotation;
        
        GameManager.instance.inVR = true;
        GameManager.instance.vrCapable = true;
        var xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null)
        {
            Debug.Log("XRGeneralSettings is null.");
            return false;
        }

        var xrManager = xrSettings.Manager;
        if (xrManager == null)
        {
            Debug.Log("XRManagerSettings is null.");
            return false;
        }

        var xrLoader = xrManager.activeLoader;
        if (xrLoader == null) xrManager.InitializeLoader();
        xrManager.InitializeLoader();

        // Enable the XR origin.
        xrOrigin.gameObject.SetActive(true);
        
        // Disable the desktop camera.
        desktopCamera.gameObject.SetActive(false);
        return true;
    }

    /// <summary>
    /// Switch between the desktop camera and the VR camera when the TAB key is pressed.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (GameManager.instance.inVR)
            {
                // Switch from the VR camera to the desktop camera.
                SwitchToDesktopCamera();
            }
            else
            {
                // Switch from the desktop camera to the VR camera.
                SwitchToVRCamera();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}