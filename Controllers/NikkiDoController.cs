using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectApp.Data;
using System.Data.Common;

namespace ServerApp.Controllers
{
    public class NikkiDoController : ControllerBase
    {
        private readonly UsersDataContext _userContext;
        private readonly ILogger<NikkiDoController> _logger;
        public NikkiDoController(UsersDataContext context, ILogger<NikkiDoController> logger ) {
            _userContext = context;
            _logger = logger;
        }

        public class DataStart
        {
            public string? UserId  { get; set; }

            public string? WhatNewinDay { get; set; }

            public int Day { get; set; }
           
            public bool IsFixedStart { get; set; }
            public bool IsFixedEnd { get; set;}
            
        }
        [HttpPost]
        [Authorize]
        //Сюда должны придти первые данные
        public async Task<ActionResult> FirstFixData([FromBody] DataStart dataStart)
        {
            Console.WriteLine("Запись за день");
            Console.WriteLine($"Day {dataStart.Day}\n" + $"UserId: {dataStart.UserId}\n" + $"WhatNewinDay: {dataStart.WhatNewinDay}\n" + $"WhatNewinDay: {dataStart.IsFixedStart}\n");
            var context = new UsersDataContext();
            if (dataStart.UserId == null) {

                Console.WriteLine("Отсутствует id пользователя");
                //Отправить данные в базу 
                User? Person = await context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(dataStart.UserId));
            }
           
            if(dataStart.UserId != null)
            {
                var DataRec = new UserData
                {
                    UserId=int.Parse(dataStart.UserId),
                    WhatNewinDay = dataStart.WhatNewinDay,
                    Day = dataStart.Day,
                    IsFixedStart = dataStart.IsFixedStart,
                    IsFixedEnd = dataStart.IsFixedEnd,
                };
                    context.UserDate.Add(DataRec);
                    await context.SaveChangesAsync();

                return Ok("Work");
            }
            else
            {
                return BadRequest("Not work");

            }
            

        }
        [HttpPost]
        [Authorize]
        public async Task<object> CheckData([FromBody] DataStart dataStart)
        {
            var context = new UsersDataContext();
            var contextData= new UserData();
            if (dataStart.UserId != null)
            {
                Console.WriteLine("Отсутствует id пользователя");
                //Отправить данные в базу 
                User? Person = await context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(dataStart.UserId));
            }

            try
            {
                var linqRow = from userDay in context.UserDate
                              where userDay.UserId == int.Parse(dataStart.UserId)                          
                              select userDay;

                string? wND = null;
                int? Day = null;

                foreach (var user in linqRow)
                {

                    Console.WriteLine($"{user.Id}\n" + $"{user.UserId}\n" + $"{user.WhatNewinDay}\n" + $"{user.Day}\n");


                    if (user.IsFixedStart == true && user.IsFixedDay == false && user.IsFixedEnd == false)
                    {
                        wND = user.WhatNewinDay;
                        Day= user.Day;
                    }
                }             
                Console.WriteLine(dataStart.WhatNewinDay + "dsa");
                var response = new
                {                 
                    userId = int.Parse(dataStart.UserId),               
                    WhatInDay = wND,
                    day = Day,
                    
            };
                      
                return Results.Json(response);
            }   catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return Results.BadRequest();
            }
           
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> NextDate([FromBody] DataStart dataStart)
        {
            //Переключить на следующий день
            if (dataStart == null)
            {
                return Ok();
            }
            else
            {
                return BadRequest();

            }
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> PrevDate([FromBody] DataStart dataStart)
        {

            //Переключить на предыдущий день
            if (dataStart == null)
            {
                return Ok();
            }
            else
            {
                return BadRequest();

            }
        }




        [HttpPost]
        [Authorize]
        //Сюда должны придти первые данные
        public async Task<ActionResult> SecondFixData([FromBody] DataStart dataStart)
        {
            Console.WriteLine("Фиксация дня!");
            Console.WriteLine($"Day {dataStart.Day}\n" + $"UserId: {dataStart.UserId}\n" + $"WhatNewinDay: {dataStart.WhatNewinDay}\n" + $"WhatNewinDay: {dataStart.IsFixedStart}\n");
            var context = new UsersDataContext();
            if (dataStart.UserId == null)
            {

                Console.WriteLine("Отсутствует id пользователя");
                //Отправить данные в базу 
                User? Person = await context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(dataStart.UserId));
            }

            if (dataStart.UserId != null)
            {
                var DataRec = new UserData
                {
                    UserId = int.Parse(dataStart.UserId),
                    WhatNewinDay = dataStart.WhatNewinDay,
                    Day = dataStart.Day,
                    IsFixedStart = dataStart.IsFixedStart,
                    IsFixedEnd = dataStart.IsFixedEnd,
                };
                context.UserDate.Add(DataRec);
                await context.SaveChangesAsync();

                return Ok("Work");
            }
            else
            {
                return BadRequest("Not work");

            }


        }

    }
}
