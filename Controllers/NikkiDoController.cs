using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NodaTime;
using NodaTime.Extensions;
using ProjectApp.Data;
using System;
using System.Data.Common;
using static ServerApp.Controllers.NikkiDoController;

namespace ServerApp.Controllers
{
    

    public class NikkiDoController : Controller
    {
        private readonly UsersDataContext _userContext;
        private readonly UserData _userDateContext;
        private readonly ILogger<NikkiDoController> _logger;
       
        public NikkiDoController(UsersDataContext context,  ILogger<NikkiDoController> logger ) {
           
            _userContext = context;
            _logger = logger;

        }

        public class DataStart
        {
            public string? UserId  { get; set; }
            public string? WhatNewinDay { get; set; }
            public bool IsFixedStart { get; set; }
            public bool IsFixedEnd { get; set;}
           
            
        }

        public class TimerEnd {

            public string? UserId { get; set; }
            public int Day { get; set; }
           
            public bool IsFixedEnd { get; set; }

        }

        public class DataEnd
        {
            public string? UserId { get; set; }
            
            public string? WhatNewinDay { get; set; }
            
            public string? NewKnoledge { get; set; }
            
            public string? DayPhilosophy { get; set; }
            public string? WhatDone { get; set; }
            public string? WhatNotDone { get; set; }
            public string? Сonclusions { get; set; }
           


        }


        public static DateTimeOffset GetDateTime(string tz) {

            Console.WriteLine($"{tz} -- сюда пришла TimeZone");

            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[tz];
            ZonedDateTime now = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);

            Instant utcInstant = now.ToInstant();
            DateTimeOffset utcDateTimeOffset = utcInstant.ToDateTimeOffset();

            Console.WriteLine($"{utcDateTimeOffset}");



            return utcDateTimeOffset;
        }


        //Получение дня 
        public async Task<int> GetDay(UsersDataContext context, int userId)
        {


            //Получить список всех дней и проверить какой последний 

            //Получаем день
           // var day = await context.UserDate.Where(u => u.UserId == userId
           //&& u.IsFixedStart == true
           //&& u.IsFixedEnd == false
           //&& u.IsFixedDay == false)
           //    .Select(u => u.Day)
           //    .FirstOrDefaultAsync();

            var dayList = await context.UserDate.Where(u => u.UserId == userId && u.Day == context.UserDate.Where(u => u.UserId == userId).Max(d => d.Day)).ToListAsync();
            int needDay = 0;
            

            if(dayList == null ) {

                Console.WriteLine("Дней нет ");
                needDay = 0;

            }
            else

            {
                foreach (var li in dayList)
                {
                    //Принвжтии старт передача нового дня от последнего максимального иначе если день не зафиксирован и начат блокировать кнопку
                    if (li.IsFixedStart == false && li.IsFixedEnd == true && li.IsFixedDay == true
                        || li.IsFixedStart == false && li.IsFixedEnd == true && li.IsFixedDay == false || li.IsFixedStart == false && li.IsFixedEnd == false && li.IsFixedDay == true)
                    {

                        needDay = li.Day + 1;

                    }
                    else { 
                    
                        
                    
                    }





                }
            }
            Console.WriteLine($"{needDay.ToString()}" + " Полученный день\n" );
            return needDay;

        }


        [HttpPost]
        [Authorize]
        //Сюда должны придти первые данные
        //Сюда придёт первая запись дня 
        public async Task<ActionResult> FirstFixData([FromBody] DataStart dataStart)
        {
            Console.WriteLine("===========================================================FirstFixData CONTROLLER Begin===========================================================\n");
            Console.WriteLine($"Проверка на приход ---- UserId: {dataStart.UserId}\n" + $"WhatNewinDay: {dataStart.WhatNewinDay}\n" +  $"IsFixedStart: {dataStart.IsFixedStart}\n " + $"IsFixedStart: {dataStart.IsFixedEnd}\n");

            try
            {


                var context2 = new UsersDataContext();
                Console.WriteLine($"Запись за день  +  {dataStart.UserId}");
                var TimeZone = await context2.Users
                              .Where(x => x.Id == int.Parse(dataStart.UserId))
                               .Select(x => x.TimeZone)
                               .FirstOrDefaultAsync();

                Console.WriteLine($"{GetDateTime(TimeZone)}\n");


                Console.WriteLine("Запись за день");
                Console.WriteLine($"UserId: {dataStart.UserId}\n" + $"WhatNewinDay: {dataStart.WhatNewinDay}\n" + $"IsFixedStart: {dataStart.IsFixedStart}\n");
                var context = new UsersDataContext();

                string? dT = null;
                DateTime? dDateTime = null;

                //Если записей по этому id нет делаем первую запись если нет будет проверять сначало на последнее число которое записано и его булевы переменные 
                var UsersDateIn = await context.UserDate.FirstOrDefaultAsync(u => u.UserId == int.Parse(dataStart.UserId));
                  Console.WriteLine($"Попал ли я сюда\n");


                //Это только запись первого дня для пользователя 
                if (UsersDateIn == null)
                {

                    var columnName = await context.Users
                               .Where(x => x.Id == int.Parse(dataStart.UserId))
                                .Select(x => x.TimeZone)
                                .FirstOrDefaultAsync();




                    Console.WriteLine($"{columnName} ---- ЧТо здесь пришло ");
                    //Делаем запись и отмечаем что старт дан

                    var updatePersRecord = new UserData
                    {
                        UserId = int.Parse(dataStart.UserId),
                        WhatNewinDay = dataStart.WhatNewinDay,
                        Day = 1,
                        IsFixedStart = true,
                        IsFixedEnd = false,
                        IsFixedDay = false,
                        Date = GetDateTime(columnName),

                    };

                    context.UserDate.Add(updatePersRecord);
                    await context.SaveChangesAsync();

                    //var response = new {
                    //     UserId, 
                    //     WhatNewinDay,
                    //      IsFixedStart 
                    //      IsFixedEnd                     
                    //  };

                    Console.WriteLine("===========================================================FirstFixData CONTROLLER END===========================================================\n");
                    return Ok(" First day Fixed");



                }
                else
                {
                    

                    var columnTimeZ = await context.Users
                               .Where(x => x.Id == int.Parse(dataStart.UserId))
                                .Select(x => x.TimeZone)
                                .FirstOrDefaultAsync();
                    // var addNewRecord = context.UserDate.Where(x=>x.)                    
                    //Логика для того как увеличивать день

                    Console.WriteLine($"{columnTimeZ} ---- ЧТо здесь пришло  я попал в след условие");

                    var DataRec = new UserData
                    {
                        UserId = int.Parse(dataStart.UserId),
                        WhatNewinDay = dataStart.WhatNewinDay,
                        //Добавить следующий день
                        Day = await  GetDay(context, int.Parse(dataStart.UserId)),                    
                        IsFixedStart = dataStart.IsFixedStart,
                        IsFixedEnd = dataStart.IsFixedEnd,
                        IsFixedDay = false,
                        Date = GetDateTime(columnTimeZ),
                    };
                    context.UserDate.Add(DataRec);
                    await context.SaveChangesAsync();
                    Console.WriteLine("===========================================================FirstFixData CONTROLLER END===========================================================\n");
                    return Ok("Do next record");

                }

            }
            catch (UnauthorizedAccessException) {

                return Unauthorized("Токен истек или недействителен");

            }
            catch (Exception ex)
            {
                // Обработка других исключений
                return StatusCode(500, ex.Message);
            }


        }

        //Зафиксировать день и после перейти на следующий с возможностью переключатся на предыдущий 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> StopDay([FromBody] DataEnd dataEnd) {


            //Отправляю ID и по нему выбираю какой день идёт и после фиксирую этот день и возвращаю следующий пустой 
            try
            {


                var context = new UsersDataContext();
                Console.WriteLine("Определяем часовой пояс" + $"{dataEnd.UserId}");
                var columnRecord = await context.Users.Where(x => x.Id == int.Parse(dataEnd.UserId))
                               .Select(x => x.TimeZone)
                               .FirstOrDefaultAsync();

                Console.WriteLine($"{GetDateTime(columnRecord)}\n");


                Console.WriteLine("Запись за день");
                Console.WriteLine($"UserId: {dataEnd.UserId}\n" + $"WhatNewinDay: {dataEnd.WhatNewinDay}\n");
                var context2 = new UsersDataContext();

                //string? dT = null;
                //DateTime? dDateTime = null;

                var updateStopDayforUser = await context.UserDate.Where(s => s.UserId == int.Parse(dataEnd.UserId) &&  s.IsFixedDay == false && s.IsFixedEnd==false).ToListAsync();

                foreach (var userDate in updateStopDayforUser)
                {
                    Console.WriteLine($"Button stop is use {userDate.UserId} " + $"{userDate.WhatNewinDay} \n");
                }


                //Определить запись по id и булевым переменным
                var userDateEnd = await context.UserDate.Where(u => u.UserId == int.Parse(dataEnd.UserId) && u.IsFixedStart == true && u.IsFixedDay == false && u.IsFixedEnd == false)
                    .Select(u => new
                    {
                        Id = u.Id,
                        Column1 = u.UserId,
                        Column2 = u.WhatNewinDay,
                        Column3 = u.NewKnoledge,
                        Column4 = u.DayPhilosophy,
                        Column5 = u.WhatDone,

                    })
                    .FirstOrDefaultAsync();

                ////  Console.WriteLine($"{int.Parse(UsersDateIn)}\n");

                //    if( userDateEnd != null )
                //{
                //    string row = $"{userDateEnd.Id} - {userDateEnd.Column1} -  {userDateEnd.Column2} - {userDateEnd.Column3} - {userDateEnd.Column4}";


                //    Console.WriteLine($"{row}");
                //}
                var response = new
                {

                    UserId = dataEnd.UserId, // Возвращаемый идентификатор пользователя
                    


                };

                Console.WriteLine($"{dataEnd.UserId}\n" + $"{dataEnd.WhatNewinDay}\n" + $"{dataEnd.NewKnoledge}\n" + 
                    $"{dataEnd.DayPhilosophy}\n" + $"{dataEnd.WhatDone}\n" + $"{dataEnd.WhatNotDone}\n" + $"{dataEnd.Сonclusions}\n");

                Console.WriteLine("===========================================================Stop (Record is Fixed) CONTROLLER END===========================================================\n");

                return Ok("Record is fixed");

                //  return Ok("Do next record and change on next day");

     

    }
            catch (UnauthorizedAccessException)
            {

                return Unauthorized("Токен истек или недействителен");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на сервер");
                // Обработка других исключений
                return StatusCode(500, ex.Message);
            }
        }



        //Проверка на конец спринта
        [HttpPost]
        [Authorize]
        public async Task<object> CheckData([FromBody] DataStart dataStart)
        {        
            
            var context = new UsersDataContext();
            var contextData= new UserData();
                     
            if (dataStart.UserId != null)
            {
               // Console.WriteLine("Отсутствует id пользователя");
                //Отправить данные в базу 
                User? Person = await context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(dataStart.UserId));
            }

            

            try
            {
                var linqTime = from userDay in context.Users
                              where userDay.Id == int.Parse(dataStart.UserId)
                               select userDay;

                string?  timeZone = null;
                foreach (var time in linqTime)
                {

                    timeZone = time.TimeZone;

                }


               
                DateTimeZone tz = DateTimeZoneProviders.Tzdb[timeZone];
                Offset offset = tz.GetUtcOffset(Instant.FromUtc(1900, 1, 1, 0, 0));
                ZonedDateTime now = SystemClock.Instance.GetCurrentInstant().InZone(tz);
                // DateTimeOffset now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                Console.WriteLine(offset.ToString());


                var endOfDay = now.Date.PlusDays(1).AtStartOfDayInZone(tz);

                // get the duration until the end of the day
                 var duration = endOfDay - now;
                Console.WriteLine("Осталось до конца дня: ");
                Console.WriteLine($"{duration.Hours}" + ":" + $"{duration.Minutes}" + ":" + $"{duration.Seconds}\n");

                //Получаем время
                Console.WriteLine("Текущее время: ");
                 Console.WriteLine($"{now.Hour}" + ":" + $"{now.Minute}" + ":" + $"{now.Second}");





               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
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
                    Console.WriteLine($"{user.Id} - " + $"{user.UserId}-" + $"{user.WhatNewinDay}-" + $"{user.Day}\n");
                    if (user.IsFixedStart == true && user.IsFixedDay == false && user.IsFixedEnd == false)
                    {
                        wND = user.WhatNewinDay;
                        Day= user.Day;
                    }                 
                }             
             
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



        /*
        [HttpPost]
        [Authorize]
        //Сюда должны придти первые данные
        public async Task<ActionResult> SecondFixData([FromBody] DataStart dataStart)
        {
            Console.WriteLine("Фиксация дня!");
            Console.WriteLine($"UserId: {dataStart.UserId}\n" + $"WhatNewinDay: {dataStart.WhatNewinDay}\n" + $"WhatNewinDay: {dataStart.IsFixedStart}\n");
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
        */

        
        [HttpPost]
        [Authorize]
        //Проверка времени если время закончилось фиксация дня и автоматический вывод следуещего дня и так до 30 дней 
        //Переделать таймер на расчёт времени на сервере польователю обновлять время в зависимости от времени на сервере
        public async Task<object> TimerChecker([FromBody] TimerEnd dataEnd)
        {


            var context = new UsersDataContext();
            var contextData = new UserData();

            if (dataEnd.UserId != null)
            {
                // Console.WriteLine("Отсутствует id пользователя");
                //Отправить данные в базу 
                User? Person = await context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(dataEnd.UserId));
            }



            try
            {
                var linqTime = from userDay in context.Users
                               where userDay.Id == int.Parse(dataEnd.UserId)
                               select userDay;

                string? timeZone = null;
                foreach (var time in linqTime)
                {

                    timeZone = time.TimeZone;

                }



                DateTimeZone tz = DateTimeZoneProviders.Tzdb[timeZone];
                Offset offset = tz.GetUtcOffset(Instant.FromUtc(1900, 1, 1, 0, 0));
                ZonedDateTime now = SystemClock.Instance.GetCurrentInstant().InZone(tz);
                // DateTimeOffset now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                Console.WriteLine(offset.ToString());


                var endOfDay = now.Date.PlusDays(1).AtStartOfDayInZone(tz);

                // get the duration until the end of the day
                var duration = endOfDay - now;
                Console.WriteLine("Осталось до конца дня: ");
                Console.WriteLine($"{duration.Hours}" + ":" + $"{duration.Minutes}" + ":" + $"{duration.Seconds}\n");

                //Получаем время
                Console.WriteLine("Текущее время: ");
                Console.WriteLine($"{now.Hour}" + ":" + $"{now.Minute}" + ":" + $"{now.Second}");






            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }


            if (dataEnd.UserId != null)
            {
               
              
                return Ok("Work");
            }
            else
            {
                return BadRequest("Not work");

            }

        }
        

    }
}
