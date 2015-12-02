﻿using Lockstep;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Lockstep {
    public abstract class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }
    
        string replayLoadScene;
        static int hashFrame;
        static long prevHash;
        static long stateHash;
        static bool hashChecked;

        public abstract NetworkHelper MainNetworkHelper {
            get;
        }

        public virtual LSManager[] Managers {
            get {
                return new LSManager[0];
            }
        }
    
        protected void Start () {
            Instance = this;
            LockstepManager.Initialize (this);
            Startup ();
        }
    
        protected virtual void Startup () {

        }

        protected void FixedUpdate () {
            LockstepManager.Simulate ();
            if (ReplayManager.IsPlayingBack) {
                if (hashChecked == false) {
                    if (LockstepManager.FrameCount == hashFrame) {
                        hashChecked = true;
                        long newHash = AgentController.GetStateHash ();
                        if (newHash != prevHash) {
                            Debug.Log ("Desynced!");
                        } else {
                            Debug.Log ("Synced!");
                        }
                    }
                }
            } else {
                hashFrame = LockstepManager.FrameCount - 1;
                prevHash = stateHash;
                stateHash = AgentController.GetStateHash ();
                hashChecked = false;
            }
        }
    
        private float timeToNextSimulate;

        protected void Update () {
            timeToNextSimulate -= Time.smoothDeltaTime * Time.timeScale;
            if (timeToNextSimulate <= float.Epsilon) {
                timeToNextSimulate = LockstepManager.BaseDeltaTime;
            }
            LockstepManager.Visualize ();
            CheckInput ();
        }
    
        protected virtual void CheckInput () {
        
        }

        void LateUpdate () {
            LockstepManager.LateVisualize ();
        }

        public static void StartGame () {
            Instance.OnStartGame ();
        }

        protected virtual void OnStartGame () {
        
        }
    
        void OnDisable () {
            //LockstepManager.Deactivate ();
        }

        void OnApplicationQuit () {
            LockstepManager.Quit ();
        }
    
        void OnGUI () {
        
            if (CommandManager.sendType == SendState.Network) {
                return;
            }
            if (ReplayManager.IsPlayingBack) {
                if (GUILayout.Button ("Play")) {
                    ReplayManager.Stop ();
                    Application.LoadLevel (Application.loadedLevel);
                }
            } else {
                if (GUILayout.Button ("Replay")) {
                    ReplayManager.Save ();
                    ReplayManager.Play ();
                    Application.LoadLevel (Application.loadedLevel);
                }
            }
        }
    }
}