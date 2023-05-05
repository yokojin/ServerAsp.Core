using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ServerApp.Controllers
{
    public class HomeController : Controller
    {


        class Person
        {
            public string Name { get; }
            public int Age { get; set; }
            public Person(string name, int age)
            {
                Name = name;
                Age = age;
            }
        }
        public async Task Index()
        {
            Person gri = new Person("Gri", 35);

            

            await Response.WriteAsJsonAsync (gri);

        }
    }
}
