using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

    public class Player :MonoBehaviour
    {
        private List<string> friends;
        private List<string> friendRequests;
        FirebaseAuth auth;
        public void Start()
        {
            InitializeFirebase();
        }
        public void Update()
        {
            // Update logic here
        }
        void InitializeFirebase()
        {
            auth = FirebaseAuth.DefaultInstance;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    FirebaseApp app =FirebaseApp.DefaultInstance;
                    auth = FirebaseAuth.DefaultInstance;
                    Debug.Log("firebase initialized");
                }
                else
                {
                    Debug.LogError("Firebase initialization failed: " + task.Exception);
                }
            });

        }
        public void authenticate(string email, string password)
        {
            
        }

    }
