using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectApp.Data;
using ServerApp.Services;
using System.Linq;

namespace ServerApp.Controllers
{
    public class RegController : Controller
    {

        private readonly UsersDataContext context;

        public RegController(UsersDataContext _context)
        {
            this.context = _context;
        }
        public record class Person(string FirstName, 
            string LastName, 
            string EmailAddres, string Password,
            string PasswordSec);
        public record class Role(int R);
        
        [HttpPost]
        public async Task<ActionResult> UpdateReg([FromBody]Person pers)
        {

            Console.WriteLine("Запрос на регистрацию");
            var context = new UsersDataContext();

           // Console.WriteLine(pers.FirstName + "\n" + pers.LastName +"\n" + pers.EmailAddres + "\n" + pers.Password + "\n");


            string searchedEmail = pers.EmailAddres;
            
            var linqNames = from user in context.Users
                            where user.Email == searchedEmail
                            select user;

            User? Person = await context.Users.FirstOrDefaultAsync(u=> u.UserName == pers.FirstName && u.Email == pers.EmailAddres);

            foreach (var user in linqNames)
            {
              
                Console.WriteLine($"{user.UserName}\n" + $"{user.LastName}\n" + $"{user.Email}\n" + $"{user.Password}\n");

                

            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userRole = context.Roles.SingleOrDefault(r => r.Name == "User");




            if (userRole != null && Person == null)
            {
                var Newuser = new User
                {
                    UserName = pers.FirstName,
                    LastName = pers.LastName,
                    Email = pers.EmailAddres,
                    Password = pers.Password,
                    RoleId = userRole.Id,
                };


                context.Users.Add(Newuser);
                await context.SaveChangesAsync();
                return Ok("User registrated!");
                // await Task.Delay(1000);
            }
            else {
                return BadRequest("-Пользователь уже существует");
            }
                
            
        }
    }
}
