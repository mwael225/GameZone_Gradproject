using System;
using System.Collections.Generic;
using System.Linq;

public class User
{
    protected string username , email , password  , status , id;
    List<string> friends = new List<string>();
    public User(string username, string email, string password, string status, string id)
    {
        this.username = username;
        this.email = email;
        this.password = password;
        this.status = status;
        this.id = id;
    }

    public string getusername()
    {
        return username;
    }
    public string getemail()
    {
        return email;
    }
    public string getstatus()
    {
        return status;
    }
    public string getid()
    {
        return id;
    }
 
   

  
}
