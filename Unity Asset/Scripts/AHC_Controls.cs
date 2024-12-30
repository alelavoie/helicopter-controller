using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEditor;
using static UnityEngine.InputSystem.InputAction;

namespace alelavoie
{
    public class AHC_Controls
    {
        public float Pitch
        {
            get;
            set;
        }

        public float Roll
        {
            get;
            set;
        }

        public float Yaw
        {
            get;
            set;
        }
        public float PitchLastChanged
        {
            get;
            set;
        }
        public float RollLastChanged
        {
            get;
            set;
        }
        public float YawLastChanged
        {
            get;
            set;
        }

        public float Collective
        {
            get;
            set;
        }
        private readonly AHC _heli;
        public AHC_Controls(AHC ahc)
        {
            _heli = ahc;
        }
        
    }
}
