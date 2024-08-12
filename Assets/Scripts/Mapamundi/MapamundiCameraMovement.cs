using UnityEngine;
using GD.MinMaxSlider;

namespace Assets.Scripts.Mapamundi
{
    [RequireComponent(typeof(Camera))]
    public class MapamundiCameraMovement : MonoBehaviour
    {
        [Header("Bounds")]
        [Space(20.0f)]
        [Tooltip("Defines the camera movement range in th Y axis")]
        [MinMaxSlider(-100.0f, 100.0f)]
        [SerializeField] private Vector2 _yMinMaxPosition = new Vector2(-13.5f, 13.5f);

        [Tooltip("Defines the camera movement range in th X axis")]
        [MinMaxSlider(-100.0f, 100.0f)]
        [SerializeField] private Vector2 _xMinMaxPosition = new Vector2(-20.0f, 20.0f);

        [Header("Pan movement")]
        [Space(20.0f)]
        [Tooltip("Pan smoothness value. Larger means smoother")]
        [Range(1.0f, 30.0f)]
        [SerializeField] private float _panningSmoothness = 10.0f;

        [Header("Zoom")]
        [Space(20.0f)]
        [Tooltip("Camera zoom speed percentage relative on how big the zoomRange is")]
        [Range(1.0f, 100.0f)]
        [SerializeField] private float _zoomingSpeed = 20.0f;

        [Tooltip("Controls min and max zoom values. Camera size initial value must be within the range")]
        [MinMaxSlider(1.0f, 20.0f)]
        [SerializeField] private Vector2 _zoomRange = new Vector2(2.0f, 7.0f);

        [Tooltip("Zoom smoothness value. Larger means smoother")]
        [Range(1.0f, 30.0f)]
        [SerializeField] private float _zoomSmoothness = 10.0f;


        private const string ZoomAxis = "Mouse ScrollWheel";
        private const string PanButton = "Fire1";
        private const float ZoomingRatio = 100.0f;
        private const float CameraZoomRatio = 70.0f;
        private const float CameraPanRatio = 70.0f;

        private Camera _cameraComponent;
        private Transform _parentTransform;
        private Vector3 _panLastPosition;
        private Vector3 _targetLocalPosition;
        private float _targetOrthographicSize;
        private float _zoomingSpeedRatio;
        private bool _isPanning = false;

        private void Start()
        {
            SetDefaultInstanceVariables();
            CheckForOrthographicCamera();
        }

        private void SetDefaultInstanceVariables()
        {
            this._cameraComponent = this.GetComponent<Camera>();
            this._parentTransform = this.transform.parent;
            this._targetOrthographicSize = this._cameraComponent.orthographicSize;
            this._targetLocalPosition = this.transform.localPosition;
            this._zoomingSpeedRatio = this._zoomingSpeed / ZoomingRatio;
        }

        private void CheckForOrthographicCamera()
        {
            if (!this._cameraComponent.orthographic)
            {
                Debug.LogError("Camera projection must be set to orthographic mode");
            }
        }

        private void Update()
        {
            CameraMovement();
        }

        private void CameraMovement()
        {
            CheckPanButtonAndUpdateTargetZoomWhenMouseInViewport();
            CheckIfPanbuttonIsReleased();
            ZoomCamera();
            UpdateTargetLocalPositionAndPan();
        }

        private void CheckPanButtonAndUpdateTargetZoomWhenMouseInViewport()
        {
            if (IsMouseInViewport())
            {
                UpdateTargetZoom();
                CheckIfPanButtonIsPressed();
            }
        }

        private bool IsMouseInViewport()
        {
            var mouseInViewport = this._cameraComponent.ScreenToViewportPoint(Input.mousePosition);
            return mouseInViewport.x >= 0.0f && mouseInViewport.y >= 0.0f && mouseInViewport.x <= 1.0f && mouseInViewport.y <= 1.0f;
        }

        private void UpdateTargetZoom()
        {
            var zoomAmount = Input.GetAxisRaw(ZoomAxis) * this._zoomingSpeedRatio * (this._zoomRange.x - this._zoomRange.y);

            if (IsZoomingIn())
            {
                PanToMouseAccordingToZoomAmount(zoomAmount);
            }

            this._targetOrthographicSize += zoomAmount;

            this._targetOrthographicSize = Mathf.Clamp(this._targetOrthographicSize, this._zoomRange.x, this._zoomRange.y);
        }

        private bool IsZoomingIn()
        {
            return Input.GetAxisRaw(ZoomAxis) > 0.0f && this._targetOrthographicSize > this._zoomRange.x;
        }

        private void PanToMouseAccordingToZoomAmount(float zoomAmount)
        {
            var mousePosition = this._cameraComponent.ScreenToWorldPoint(Input.mousePosition);
            var mousePositionRelativeToParent = this._parentTransform.InverseTransformPoint(mousePosition);
            var multiplier = (1.0f / this._cameraComponent.orthographicSize * -zoomAmount);

            this._targetLocalPosition += (mousePositionRelativeToParent - this._targetLocalPosition) * multiplier;
        }

        private void CheckIfPanButtonIsPressed()
        {
            if (Input.GetButtonDown(PanButton))
            {
                this._isPanning = true;
                this._panLastPosition = this._cameraComponent.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        private void CheckIfPanbuttonIsReleased()
        {
            if (Input.GetButtonUp(PanButton))
            {
                this._isPanning = false;
            }
        }

        private void UpdateTargetLocalPositionAndPan()
        {
            if (this._isPanning)
            {
                AddMouseMovementToTargetLocalPosition();
            }

            CameraPan();

            this._panLastPosition = this._cameraComponent.ScreenToWorldPoint(Input.mousePosition);

            ClampTargetLocalPosition();
        }

        private void ZoomCamera()
        {
            this._cameraComponent.orthographicSize = Mathf.Lerp(this._cameraComponent.orthographicSize,
                                                                this._targetOrthographicSize,
                                                                Time.deltaTime * CameraZoomRatio / this._zoomSmoothness);
        }

        private void AddMouseMovementToTargetLocalPosition()
        {
            var direction = this._panLastPosition - this._cameraComponent.ScreenToWorldPoint(Input.mousePosition);
            this._targetLocalPosition += direction;
        }

        private void ClampTargetLocalPosition()
        {
            var minXPosition = this._xMinMaxPosition.x + this._cameraComponent.orthographicSize * this._cameraComponent.aspect;
            var minYPoisition = this._yMinMaxPosition.x + this._cameraComponent.orthographicSize;

            var maxXPosition = this._xMinMaxPosition.y - this._cameraComponent.orthographicSize * this._cameraComponent.aspect;
            var maxYPosition = this._yMinMaxPosition.y - this._cameraComponent.orthographicSize;

            var clampedXValue = Mathf.Clamp(this._targetLocalPosition.x, minXPosition, maxXPosition);
            var clampedYvalue = Mathf.Clamp(this._targetLocalPosition.y, minYPoisition, maxYPosition);

            this._targetLocalPosition = new Vector3(clampedXValue, clampedYvalue, this._targetLocalPosition.z);
        }

        private void CameraPan()
        {
            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition,
                                                        this._targetLocalPosition,
                                                        Time.deltaTime * CameraPanRatio / this._panningSmoothness);
        }

        public void SetTargetPosition(Vector3 position, float widthToBeSeen)
        {
            var newTtargetLocalPosition = transform.InverseTransformDirection(position);

            this._targetLocalPosition.x = newTtargetLocalPosition.x;
            this._targetLocalPosition.y = newTtargetLocalPosition.y;

            this._targetOrthographicSize = widthToBeSeen * Screen.height / Screen.width * 0.5f;
        }

        public Collider2D ColliderHitByMousePosition()
        {
            var raycastHit = Physics2D.Raycast(this._cameraComponent.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            return raycastHit.collider;
        }

        public Vector2 MousePositionInViewport()
        {
            return this._cameraComponent.ScreenToViewportPoint(Input.mousePosition);
        }
    }
}