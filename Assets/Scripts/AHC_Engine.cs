using UnityEngine;

namespace alelavoie
{
    public class AHC_Engine
    {
        private bool _engineOn;
        private bool _throttling = false;
        private float _engineReadiness;
        private float _engineSpeed;
        private AHC _ahc;
        public float EngineSpeed
        {
            get { return _engineSpeed; }
        }
        public bool EngineOn
        {
            get { return _engineOn; }
            set { _engineOn = value; }
        }
        public bool Throttling
        {
            get { return _ahc.EnableThrottling && _throttling; }
            set { _throttling = value; }
        }


        public AHC_Engine (AHC ahc)
        {
            _ahc = ahc;
            initEngine();
        }
        
        public void ProcessState()
        {
            
            if (_engineOn && _engineReadiness < 1f)
            {
                _engineReadiness += 0.002f;
                _engineSpeed = Mathf.Pow(_engineReadiness, 2f);
            }
            else if (!_engineOn && _engineReadiness > 0)
            {
                _engineReadiness -= 0.002f;
                _engineSpeed = Mathf.Pow(_engineReadiness, 2f);
            }
        }

        private void initEngine()
        {
            if (_ahc.Settings.EngineRunning)
            {
                _engineOn = true;
                _engineReadiness = 1f;
                _engineSpeed = 1f;
            }
            else
            {
                _engineOn = false;
                _engineReadiness = 0;
                _engineSpeed = 0;
            }
        }
    }
}

