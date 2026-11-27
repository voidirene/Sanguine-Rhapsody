using StarterAssets;
using Unity.Cinemachine;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

// Handles the switching of visuals to and from transformation mode
public class TransformationSwitcher : MonoBehaviour
{
    public Transform playerCameraRoot;
    public Transform transformationCameraRoot;
    public float playerCameraDistance = 5.5f;
    public float transformationCameraDistance = 10f;

    [SerializeField] private GameObject playerMesh;
    [SerializeField] private GameObject transformationMesh;
    
    [SerializeField] private GameObject playerFollowCamera;
    private CinemachineCamera _followCamera;
    private CinemachineThirdPersonFollow _thirdPersonFollow;
    
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator transformationAnimator;
    
    private TransformationController _transformationController;
    private ThirdPersonController _thirdPersonController;
    private WeaponController _weaponController;
    
    private void Awake()
    {
        if (!playerMesh || !transformationMesh)
        {
            Debug.LogWarning("No player or transformation mesh set on transformation switcher!!");
            enabled = false;
            return;
        }
        if (transformationMesh.activeSelf)
            transformationMesh.SetActive(false);
        
        if (!playerFollowCamera)
        {
            Debug.LogWarning("No player follow camera set on transformation switcher!!");
            enabled = false;
            return;
        }
        _followCamera = playerFollowCamera.GetComponent<CinemachineCamera>();
        _thirdPersonFollow = playerFollowCamera.GetComponent<CinemachineThirdPersonFollow>();

        if (!playerAnimator || !transformationAnimator)
        {
            Debug.LogWarning("No player or transformation animator set on transformation switcher!!");
            enabled = false;
            return;
        }

        _transformationController = GetComponent<TransformationController>();
        _thirdPersonController = transform.parent.GetComponentInChildren<ThirdPersonController>();
        _weaponController = transform.parent.GetComponentInChildren<WeaponController>();
    }

    private void OnEnable()
    {
        if (_transformationController != null)
        {
            _transformationController.OnChangeFormToTransformation += SwitchToTransformation;
            _transformationController.OnChangeFormToNormal += SwitchToHuman;
        }
    }
    private void OnDisable()
    {
        if (_transformationController != null)
        {
            _transformationController.OnChangeFormToTransformation -= SwitchToTransformation;
            _transformationController.OnChangeFormToNormal -= SwitchToHuman;
        }
    }
    
    private void SwitchToTransformation()
    {
        // Tweak camera
        _followCamera.Target.TrackingTarget = transformationCameraRoot;
        _thirdPersonFollow.CameraDistance = transformationCameraDistance;
        _thirdPersonController.CinemachineCameraTarget = transformationCameraRoot.gameObject;
        
        // Swap models
        playerMesh.SetActive(false);
        transformationMesh.SetActive(true);
        
        // Swap animator
        _thirdPersonController.animator = transformationAnimator;
        _weaponController.animator = transformationAnimator;
    }
    private void SwitchToHuman()
    {
        // Tweak camera
        _followCamera.Target.TrackingTarget = playerCameraRoot;
        _thirdPersonFollow.CameraDistance = playerCameraDistance;
        _thirdPersonController.CinemachineCameraTarget = playerCameraRoot.gameObject;
        
        // Swap models
        playerMesh.SetActive(true);
        transformationMesh.SetActive(false);
        
        // Swap animator
        _thirdPersonController.animator = playerAnimator;
        _weaponController.animator = playerAnimator;
    }

}
