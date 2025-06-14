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
            

        }
        public void authenticate(string email, string password)
        {
            
        }

    }
