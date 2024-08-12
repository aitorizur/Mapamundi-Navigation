using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Mapamundi
{
    public class MapamundiController : MonoBehaviour
    {
        [SerializeField] private MapamundiCameraMovement _cameraMovement = null;
        [SerializeField] private SelectableAreaUI _selectableAreaUI = null;
        [SerializeField] [Range(0.0f, 1.0f)] private float _maxDistanceToNotClick = 0.1f;
        [SerializeField] private List<SelectableAreaData> _selectableAreas = null;

        private Vector2 _mousePositionInViewportWhenClicked = default;
        private bool _isClicking = false;
        private Color _defaultSelectableAreaColor = Color.white;
        private SpriteRenderer _currentSelectedRenderer = null;

        private void Start()
        {
            this._selectableAreaUI.HideAreaInfo();
        }

        private void Update()
        {
            OnMouseDown();
            OnMouseUp();
        }

        private void OnMouseDown()
        {
            if (LeftClickDown())
            {
                this._isClicking = true;
                this._mousePositionInViewportWhenClicked = this._cameraMovement.MousePositionInViewport();
            }
        }

        private bool LeftClickDown()
        {
            return Input.GetMouseButtonDown(0);
        }

        private void OnMouseUp()
        {
            if (this._isClicking && LeftClickUp())
            {
                SelectAreaIfConsideredClick();
            }
        }

        private bool LeftClickUp()
        {
            return Input.GetMouseButtonUp(0);
        }

        private void SelectAreaIfConsideredClick()
        {
            if (HasMouseNotMovedOverMaxDistance())
            {
                var colliderHitByMousePosition = this._cameraMovement.ColliderHitByMousePosition();
                if (colliderHitByMousePosition != null)
                {
                    SelectAreaByValidAreaFrom(colliderHitByMousePosition);
                }
            }
        }

        private bool HasMouseNotMovedOverMaxDistance()
        {
            var mouseInViewport = this._cameraMovement.MousePositionInViewport();
            var movementMagnitude = (mouseInViewport - this._mousePositionInViewportWhenClicked).magnitude;

            return movementMagnitude < this._maxDistanceToNotClick;
        }

        private void SelectAreaByValidAreaFrom(Collider2D selectableAreaCollider)
        {
            var selectedArea = this._selectableAreas.First(x => x.Name == selectableAreaCollider.name);
            if (selectableAreaCollider.TryGetComponent<SpriteRenderer>(out var spriteRenderer) && selectedArea != null)
            {
                SelectAreaFrom(selectedArea, selectableAreaCollider.transform.position, spriteRenderer);
            }
        }

        private void SelectAreaFrom(SelectableAreaData selectableAreaData, Vector3 selectableAreaPosition, 
                                    SpriteRenderer areaSpriteRenderer)
        {
            SelectNewRendererFrom(selectableAreaData, areaSpriteRenderer);
            this._cameraMovement.SetTargetPosition(selectableAreaPosition, areaSpriteRenderer.size.x);
            this._selectableAreaUI.ShowAreaInfo(selectableAreaData);
        }

        private void SelectNewRendererFrom(SelectableAreaData selectableAreaData, 
                                           SpriteRenderer selectableAreaSpriteRenderer)
        {
            ChangeSelectedRendererToDefaultColor();
            this._currentSelectedRenderer = selectableAreaSpriteRenderer;
            selectableAreaSpriteRenderer.color = selectableAreaData.SelectionColor;
        }

        private void ChangeSelectedRendererToDefaultColor()
        {
            if (this._currentSelectedRenderer != null)
            {
                this._currentSelectedRenderer.color = this._defaultSelectableAreaColor;
            }
        }

        private void UnSelectArea()
        {
            this._selectableAreaUI.HideAreaInfo();
            ChangeSelectedRendererToDefaultColor();
        }
    }
}