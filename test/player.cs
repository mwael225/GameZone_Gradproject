using System;

public class Player : User
{
    public Player(string username, string email, string password, string role, string id) : base(username, email, password, role, id)
    {
        
    }
    
    public void friendrequest()
    {
        email = "friendrequest";
    }
    public void test()
    {
        Console.WriteLine(email);
    }
}

