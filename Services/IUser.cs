namespace ServerApp.Services
{
     interface IUser
    {
        string GetName();
        string GetLogin();
        string GetEmail();
        string GetPassword();
        string GetPasswordSec();
    }
}
