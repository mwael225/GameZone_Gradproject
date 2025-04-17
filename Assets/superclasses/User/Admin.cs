using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSystem
{
    public class Admin : User
    {
        public Admin(string username, string email) : base(username, email)
        {
        }
    }
}
