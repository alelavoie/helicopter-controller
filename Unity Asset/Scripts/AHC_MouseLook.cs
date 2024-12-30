using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace alelavoie
{
    [AddComponentMenu("Camera-Control/AHC Mouse Look")]
    public class AHC_MouseLook : MonoBehaviour
    {

        [Range(0f, 1f)]
        [Tooltip("Speed at which the camera rotates around. Set to 0 to disable looking around")]
        public float LookAroundSensitivity = 0.5F;

        [Range(0f, 1f)]
        [Tooltip("Speed at which the camera comes back to original position")]
        public float FallBackSensitivity = 0.5f;

        [Tooltip("The object the camera will follow")]
        public GameObject TargetObject;

        [Tooltip("The distance between the camera and the helicopter")]
        public float CamDistance = 10f;
        
        [Range(-1f, 1f)]
        [Tooltip("The elevation of the camera relative to the helicopter. Accepts negative values")]
        public float CamElevation = 0.2f;

        private AHC _ahc;
        private float _camDistance;

        private float _rotationX = 0F;
        private float _rotationY = 0F;

        private Vector3 _lastPosition;
        private Vector3 _lastRotation;        
        private bool _isLookingAround = false;

        void Start()
        {
            _ahc = TargetObject.GetComponent<AHC>();
            _lastPosition = TargetObject.transform.position;
            _lastRotation = TargetObject.transform.localEulerAngles;

            _camDistance = CamDistance;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb) rb.freezeRotation = true;

            Vector3 cameraDirection = GetDefaultCameraDirection();

            gameObject.transform.position = TargetObject.transform.position - (cameraDirection * _camDistance);
            gameObject.transform.forward = cameraDirection;
        }

        void Update()
        {
            FollowObject();
            if (!_isLookingAround)
                ReturnToDefaultPosition();

        }
        void FollowObject()
        {
            Vector3 objectTranslation = TargetObject.transform.position - _lastPosition;
            gameObject.transform.Translate(objectTranslation, Space.World);

            float objectRotation = TargetObject.transform.localEulerAngles.y - _lastRotation.y;

            gameObject.transform.RotateAround(TargetObject.transform.position, Vector3.up, objectRotation);

            _lastPosition = TargetObject.transform.position;
            _lastRotation = TargetObject.transform.localEulerAngles;
        }
        
        void ReturnToDefaultPosition()
        {
            Vector3 targetDirection = GetDefaultCameraDirection();
            float angleToTarget = Vector3.Angle(targetDirection, gameObject.transform.forward);

            if (angleToTarget < 0.1f) {
                return;
            }
            float framesLeft = Mathf.Ceil(angleToTarget / Mathf.Lerp(0.3f, 5f, FallBackSensitivity));

            Vector3 targetHorizontalDirection = Vector3.ProjectOnPlane(targetDirection, Vector3.up); 
            Vector3 currentHorizontalDirection = Vector3.ProjectOnPlane(gameObject.transform.forward, Vector3.up); 

            float horizontalDeltaAngle = Vector3.Angle(targetHorizontalDirection, currentHorizontalDirection);
            Vector3 horizontalRotateAroundAxis = Vector3.Cross(targetHorizontalDirection, currentHorizontalDirection);
            if (horizontalDeltaAngle > 0.1f && horizontalDeltaAngle < 179.9f)
            {
                transform.RotateAround(TargetObject.transform.position, -horizontalRotateAroundAxis, horizontalDeltaAngle / framesLeft);
            }


            float verticalDeltaAngle = Vector3.Angle(targetDirection, Vector3.up) - Vector3.Angle(gameObject.transform.forward, Vector3.up);
            Vector3 verticalRotateAroundAxis = verticalDeltaAngle < 0 ? -gameObject.transform.right : gameObject.transform.right;
            verticalDeltaAngle = Mathf.Abs(verticalDeltaAngle);
            if (verticalDeltaAngle > 0.1f && verticalDeltaAngle < 179.9f)
            {
                transform.RotateAround(TargetObject.transform.position, verticalRotateAroundAxis, verticalDeltaAngle / framesLeft);
            }
        }


        Vector3 GetDefaultCameraDirection()
        {
            return Vector3.Normalize(new Vector3(TargetObject.transform.forward.x, -CamElevation, TargetObject.transform.forward.z));
        }

        public void OnLookAroundKey(InputValue value)
        {
            _isLookingAround = value.isPressed;
        }
        public void OnLookAround(InputValue input)
        {
            if (!_isLookingAround)
                return;
                
            Vector2 delta = input.Get<Vector2>();
            _rotationX = delta.x * Mathf.Lerp(2f, 10f, LookAroundSensitivity);
            _rotationY = delta.y * Mathf.Lerp(2f, 10f, LookAroundSensitivity);

            transform.RotateAround(TargetObject.transform.position, Vector3.up, _rotationX * Time.deltaTime);
            transform.RotateAround(TargetObject.transform.position, gameObject.transform.right, -_rotationY * Time.deltaTime);

            //Don't let camera go upside down
            if (transform.up.y < 0)
            {
                transform.RotateAround(TargetObject.transform.position, -gameObject.transform.right, -_rotationY * Time.deltaTime);
            }
        }

    }
}