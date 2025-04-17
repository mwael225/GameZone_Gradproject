using UnityEngine;
using System.Threading.Tasks;

namespace GameSystem
{
    public abstract class User
    {
        protected string username;
        protected string email;
        protected string status;

        public string Username => username;
        public string Email => email;
        public string Status => status;

        protected User(string username, string email)
        {
            this.username = username;
            this.email = email;
            this.status = "Offline";
        }
        
        public virtual async Task<bool> LoadFromDatabase()
        {
            // Base implementation for loading from database
            return true;
        }
        
        public virtual async Task<bool> SaveToDatabase()
        {
            // Base implementation for saving to database
            return true;
        }
    }
} 