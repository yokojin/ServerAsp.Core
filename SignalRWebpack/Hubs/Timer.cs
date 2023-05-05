using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;


namespace ServerApp.Services
{
   
    public class TimerF : Hub
    {

         //Возврат возврат время до конца дня 
        public async Task SendMessage(string message)
        {

            string str = "Hi!";
          // Console.WriteLine($"Received message: {message}");
            await Clients.All.SendAsync("Receive", str);
        }
    }
}
