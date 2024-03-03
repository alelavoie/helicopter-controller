using UnityEngine;

namespace alelavoie
{
    public class AHC_Settings
    {
        public float MotorStrength;
        public float MaxLiftConservationAngle;
        public float PitchSensitivity;
        public float RollSensitivity;
        public float YawSensitivity;
        public float RotorDragCoefficientUnder;
        public float RotorDragCoefficientAbove;

        public bool AltitudeStabilizer;

        public bool EngineRunning;
        
        private float _angularDrag;

        public AHC_Settings(AHC ahc)
        {
            _angularDrag = ahc.HeliRigidbody.angularDrag;
            MotorStrength = Mathf.Lerp(0.01f, 0.8f, ahc.LiftStrength);

            MaxLiftConservationAngle = ahc.MaxLiftConservationAngle;

            PitchSensitivity = Mathf.Lerp(0.02f, 0.3f, ahc.PitchSensitivity) * _angularDrag;
            RollSensitivity = Mathf.Lerp(0.02f, 0.4f, ahc.RollSensitivity) * _angularDrag;
            YawSensitivity = Mathf.Lerp(0.02f, 0.2f, ahc.YawSensitivity) * _angularDrag;

            RotorDragCoefficientUnder = Mathf.Lerp(0.002f, 0.005f, ahc.RotorDragCoefficientUnder);
            RotorDragCoefficientAbove = Mathf.Lerp(0.001f, 0.003f, ahc.RotorDragCoefficientAbove);

            AltitudeStabilizer = ahc.AltitudeStabilizer;
            EngineRunning = ahc.EngineRunning;

        }

    }
}
