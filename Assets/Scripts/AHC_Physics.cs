﻿using UnityEngine;

namespace alelavoie
{
    class AHC_Physics
    {
        const float MAX_LIFT_ANGLE = 60F;
        const float MIN_SPEED_ANGURAL_DRAG = 72f;
        const float ANGULAR_DRAG_COEFICIENT = 0.0005f;
        const float ROTOR_DRAG_MAX_DEVIATION_ANGLE = 15f;
        const float TIME_TO_FULL_INPUT = 1f;

        private AHC _ahc;

        private float _baseLiftMagnitude;
        private float _maxLiftMagnitude;
        private float _maxLiftVariation;
        private float _maxRotorDragMagnitude;

        public AHC_Physics(AHC ahc)
        {
            _ahc = ahc;

            _baseLiftMagnitude = _ahc.HeliRigidbody.mass * 9.8f;
            _maxLiftMagnitude = _baseLiftMagnitude + (_baseLiftMagnitude * _ahc.Settings.MotorStrength);
            _maxLiftVariation = _maxLiftMagnitude - _baseLiftMagnitude;
            _maxRotorDragMagnitude = _maxLiftMagnitude * 2f;
        }

        public void ApplyForces()
        {     
            ApplyRotorForces();
            RotateTowardsLeastResistance();
            ApplyControlInputs();
        }
        void ApplyControlInputs() {
            if (_ahc.Engine.Throttling)
                return;
            
            ApplyRollTorque();
            ApplyPitchTorque();
            ApplyYawTorque();
        }
        
        private void ApplyRotorForces()
        {
            float throttlingModifier = _ahc.Engine.Throttling ? 0.8f : 1f;
            Vector3 forceToApply = RotorLift() + RotorDrag();
            if (_ahc.Settings.AltitudeStabilizer)
            {
                forceToApply += AltitudeStabilizer();
            }
            _ahc.HeliConstantForce.force = forceToApply * _ahc.Engine.EngineSpeed * throttlingModifier;
        }

        private Vector3 AltitudeStabilizer()
        {

            if (_ahc.Controls.Collective != 0)
            {
                return Vector3.zero;
            }
            float heliUpY = _ahc.transform.up.y;
            float heliVeloDirectionY = _ahc.HeliRigidbody.velocity.normalized.y;

            Vector3 stabilizingForce = new Vector3(0, -heliVeloDirectionY * Mathf.Abs(heliUpY) * _maxLiftVariation / 2, 0);
            return stabilizingForce;
        }

        void RotateTowardsLeastResistance()
        {
            Vector3 VeloOnHeliPlane = Vector3.ProjectOnPlane(_ahc.HeliRigidbody.velocity, _ahc.transform.up);

            float angleForwardVeloHorizon = (VeloOnHeliPlane.magnitude > 0f) ? Vector3.Angle(VeloOnHeliPlane, _ahc.transform.forward) : 0;

            float speedModulationFactor = SpeedModulationFactor(VeloOnHeliPlane.magnitude);
            float angleModulationFactor = AngleModulationFactor(angleForwardVeloHorizon);
            Vector3 direction = TorqueDirection(VeloOnHeliPlane, _ahc.transform.forward, _ahc.transform.up);
            _ahc.HeliRigidbody.AddTorque(speedModulationFactor * angleModulationFactor * direction, ForceMode.Acceleration);

        }

        private float SpeedModulationFactor(float speed)
        {
            //Transform m/s in km/h
            speed = speed * 3.6f;
            if (speed < MIN_SPEED_ANGURAL_DRAG)
            {
                return 0;
            }
            return (ANGULAR_DRAG_COEFICIENT * _ahc.HeliRigidbody.angularDrag) * Mathf.Pow(speed - MIN_SPEED_ANGURAL_DRAG, 2f);
        }
        private float AngleModulationFactor(float angle)
        {
            float modulationFactor;
            if (angle <= 45f)
            {
                modulationFactor = Mathf.Pow(Mathf.InverseLerp(0, 45f, angle), 2f);
            }
            else if (angle <= 175f)
            {
                modulationFactor = 1 - Mathf.InverseLerp(45f, 175f, angle);
            }
            else
            {
                modulationFactor = 0;
            }
            return modulationFactor;
        }

        private Vector3 TorqueDirection(Vector3 velo, Vector3 targetDirection, Vector3 axis)
        {
            Vector3 veloForwardNormal = Vector3.Cross(targetDirection, velo);
            return axis * (Mathf.Sign(axis.y) * Mathf.Sign(veloForwardNormal.y));
        }

        private void ApplyPitchTorque()
        {
            if (_ahc.Controls.Pitch != 0) {
                _ahc.Controls.PitchLastChanged += Time.deltaTime;
            }
            float modulatedPitch = Mathf.InverseLerp(0f, TIME_TO_FULL_INPUT, _ahc.Controls.PitchLastChanged) * _ahc.Controls.Pitch;

            _ahc.HeliRigidbody.AddTorque(-_ahc.transform.right * modulatedPitch * _ahc.Settings.PitchSensitivity, ForceMode.Acceleration);
        }
        private void ApplyRollTorque()
        {
            if (_ahc.Controls.Roll != 0)
            {
                _ahc.Controls.RollLastChanged += Time.deltaTime;
            }
            float modulatedRoll = Mathf.InverseLerp(0f, TIME_TO_FULL_INPUT, _ahc.Controls.RollLastChanged) * _ahc.Controls.Roll;
            _ahc.HeliRigidbody.AddTorque(-_ahc.transform.forward * modulatedRoll * _ahc.Settings.RollSensitivity, ForceMode.Acceleration);
        }        
        private void ApplyYawTorque()
        {
            if (_ahc.Controls.Roll != 0)
            {
                _ahc.Controls.RollLastChanged += Time.deltaTime;
            }
            float modulatedRoll = Mathf.InverseLerp(0f, TIME_TO_FULL_INPUT, _ahc.Controls.RollLastChanged) * _ahc.Controls.Roll;
            _ahc.HeliRigidbody.AddTorque(_ahc.transform.up * _ahc.Controls.Yaw * _ahc.Settings.YawSensitivity, ForceMode.Acceleration);
        }
        
        public Vector3 RotorLift()
        {
            if (_ahc.IsUpsideDown())
            {
                return Vector3.zero;
            }

            Vector3 direction = GetLiftDirection();

            float strength = GetLiftStrength(direction, _ahc.Controls.Collective);

            return direction * strength;
        }

        private float GetLiftStrength(Vector3 liftDirection, float collectiveInput)
        {
            float angle = Vector3.Angle(Vector3.up, liftDirection);
            if (angle >= 90)
            {
                return 0;
            }

            float collectiveLift = _baseLiftMagnitude + (_maxLiftVariation * collectiveInput);

            if (angle <= _ahc.Settings.MaxLiftConservationAngle)
            {

                float adjustedBaseLift = collectiveLift / Mathf.Cos(angle * Mathf.Deg2Rad);
                return adjustedBaseLift;
            }
            else
            {
                //Lift slowly decreases after tilting past the MaxTiltAngle.
                float modulation = Mathf.Lerp(0.2f, 1f, 1 - Mathf.InverseLerp(_ahc.Settings.MaxLiftConservationAngle, 90f, angle));
                return collectiveLift * modulation;
            }
        }

        private Vector3 GetLiftDirection()
        {
            float angleHeliUpZenithUp = Vector3.Angle(_ahc.transform.up, Vector3.up);
            if (angleHeliUpZenithUp < 0.25f)
            {
                return Vector3.up;
            }
            Vector3 liftDirection = _ahc.transform.up;
            if (_ahc.ModulateLiftDirection)
            {
                liftDirection = ModulateLiftDirection(angleHeliUpZenithUp);
            }

            Vector3 direction = AHC_Utils.ClampOrientation(Vector3.up, liftDirection, 0, MAX_LIFT_ANGLE);
            return direction;
        }

        private Vector3 ModulateLiftDirection(float angle)
        {
            Vector3 projectedDirection;
            if (angle <= 5f)
            {
                projectedDirection = AHC_Utils.DampenOrientation(Vector3.up, _ahc.transform.up, 5f);
            }
            else
            {
                projectedDirection = AHC_Utils.BoostOrientation(Vector3.up, _ahc.transform.up, 90f);
            }
            return projectedDirection;
        }

        private Vector3 RotorDrag()
        {
            Vector3 veloDragDirection = -_ahc.HeliRigidbody.velocity.normalized;
            float angleDragHeliUp = Vector3.Angle(veloDragDirection, _ahc.transform.up);
            float rotorDragStrength = ComputRotorDragStrength(angleDragHeliUp, _ahc.HeliRigidbody.velocity.magnitude);
            Vector3 rotorDragDirection = ComputeRotorDragDirection(angleDragHeliUp);
            return rotorDragDirection * rotorDragStrength;
        }
        
        private float ComputRotorDragStrength(float angleDragHeliUp, float speed)
        {
            float adjustmentCoefficient;
            if (angleDragHeliUp > 90)
            {
                adjustmentCoefficient = Mathf.Lerp(0, _ahc.Settings.RotorDragCoefficientAbove, Mathf.InverseLerp(90, 180, angleDragHeliUp));
            }
            else
            {
                adjustmentCoefficient = Mathf.Lerp(0, _ahc.Settings.RotorDragCoefficientUnder, Mathf.InverseLerp(0, 90, angleDragHeliUp));
            }
            return Mathf.Min(_maxLiftMagnitude * adjustmentCoefficient * Mathf.Pow(speed, 2f), _maxRotorDragMagnitude);
        }

        private Vector3 ComputeRotorDragDirection(float angleDragHeliUp)
        {

            Vector3 veloDragDirection = -_ahc.HeliRigidbody.velocity.normalized;

            //To avoid killing the momentum when turning or climbing, the direction of the rotor drag needs to be almost perpendicular to the velocity.
            //To achieve that, the drag vector is rotated towards the projection of the helicopter's up vector on the plan orthogonal to the drag's direction.
            //It is rotated to form an angle of ROTOR_DRAG_MAX_DEVIATION_ANGLE with that projected vector. 
            Vector3 heliYaxisOnPlaneOrthoToDrag;
            if (angleDragHeliUp > 90)
            {
                heliYaxisOnPlaneOrthoToDrag = Vector3.ProjectOnPlane(-_ahc.transform.up, veloDragDirection).normalized;
            }
            else
            {
                heliYaxisOnPlaneOrthoToDrag = Vector3.ProjectOnPlane(_ahc.transform.up, veloDragDirection).normalized;
            }

            return AHC_Utils.ClampOrientation(heliYaxisOnPlaneOrthoToDrag, veloDragDirection, 0, ROTOR_DRAG_MAX_DEVIATION_ANGLE);
        }
       
    }
    
}
