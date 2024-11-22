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
        Debug.Log($"Enabling Desktop Camera");
        GameManager.instance.inVR = false;

        // Disable the XR Origin.
        xrOrigin.gameObject.SetActive(false);

        // Enable the Desktop Camera.
        desktopCamera.gameObject.SetActive(true);
    }

    /// <summary>
    /// Switches from the VR camera to the desktop camera.
    /// </summary>
    private void SwitchToDesktopCamera()
    {
        // Set the desktop camera's scale to the same as the XR origin's scale.
        desktopCamera.transform.localScale = xrOrigin.transform.localScale;

        // Enable the desktop camera.
        EnableDesktopCamera();
    }

    private bool EnableVRCamera()
    {
        Debug.Log("Enabling VR Camera");
        GameManager.instance.inVR = true;
        GameManager.instance.vrCapable = true;
        var xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null)
        {
            Debug.Log($"XRGeneralSettings is null.");
            return false;
        }

        var xrManager = xrSettings.Manager;
        if (xrManager == null)
        {
            Debug.Log($"XRManagerSettings is null.");
            return false;
        }

        var xrLoader = xrManager.activeLoader;
        if (xrLoader == null) xrManager.InitializeLoader();
        xrManager.InitializeLoader();

        xrOrigin.gameObject.SetActive(true); // Enable the XR Origin.
        desktopCamera.gameObject.SetActive(false); // Disable the Desktop Camera.
        return true;
    }
}