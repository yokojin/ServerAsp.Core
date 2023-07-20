using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using ProjectApp.Data;
using ServerApp.Services;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using static ServerApp.Controllers.HomeController;
using static ServerApp.Controllers.RefreshTimeController;

namespace ServerApp.Controllers {
   
     
    public class RefreshTimeController : Controller
    {
        private readonly UsersDataContext _userContext;       
        private readonly ILogger<RefreshTimeController> _logger;

        public RefreshTimeController(UsersDataContext context, ILogger<RefreshTimeController> logger)
        {
            _userContext = context;
            _logger = logger;
        }
        //Class для Проверки времени 
        public class TimeString {
             public string?  UserId { get; set; }
             public string?   TimeZone { get; set; }   
             public int? Day {get ; set; }
             public bool OnOffTimer { get; set;}
             public string? Whnd { get; set; }

            public bool isDayFixed { get; set; }

        }

        public class TimeOffData
        {
            public bool IsFixedStart { get; set; }
            public bool IsFixedEnd { get; set; }
            public bool IsFixedDay { get; set; }
        }

        public class Date {
            public int id;
            public DateTime dateTime;
            public  bool isFStart;
        
        }


        public async Task<int> GetId(UsersDataContext context , TimeString timeString)
        {
            Console.WriteLine("========================================================В  GetId=================================================================\n");

            var idUser = await context.Users.Where(x => x.Id == int.Parse(timeString.UserId))
                              .Select(x => x.Id)
                              .FirstOrDefaultAsync();         
           return  int.Parse(idUser.ToString());
        }


        public async Task<string> GetTimeZone(UsersDataContext context,  int userId)
        {
            Console.WriteLine("========================================================В  GetTimeZone=================================================================\n");
            var columnTimezon = await context.Users
                              .Where(x => x.Id == userId)
                               .Select(x => x.TimeZone)
                               .FirstOrDefaultAsync();
            return columnTimezon;
        }


        public async Task<int> QueryFindLastRecord(UsersDataContext context, int userId) {

            var lastMax = await context.UserDate.Where(u => u.UserId == userId && u.IsFixedStart == false
                                        && u.IsFixedEnd == true
                                        && u.IsFixedDay == true).Select(u => (int?)u.Day).DefaultIfEmpty().MaxAsync();
                                                    
            if (lastMax == null) {

                lastMax = await context.UserDate.Where(u => u.UserId == userId && u.IsFixedStart == false
                                            && u.IsFixedEnd == false
                                            && u.IsFixedDay == true).MaxAsync(u => u.Day);

                Console.WriteLine(lastMax.ToString()+"Последний не законченный день");
                return (int)lastMax;
            }

            //Последний законченный день
            var digitDay = await context.UserDate.Where(u => u.UserId == userId && u.IsFixedStart == false && u.IsFixedEnd == true && u.IsFixedDay == true && u.Day == lastMax).Select(u => u.Day).FirstOrDefaultAsync();

            return digitDay;
        }

        public async Task<int> GetDay(UsersDataContext context,  int userId)
        {
            int needDay = 0;
            Console.WriteLine("========================================================В GetDay Попал=================================================================\n");
            //Получаем день который начался 

            var dayListALl = await context.UserDate.Where(u => u.UserId == userId).ToListAsync();

            if (dayListALl != null)
            {

                foreach (var li in dayListALl)
                {
                    Console.WriteLine($"{li.UserId}/{li.IsFixedStart}/{li.IsFixedEnd}/{li.IsFixedDay}/{li.Day}\n");

                    if (li.IsFixedStart == true && li.IsFixedEnd == false && li.IsFixedDay == false)
                    {
                        //День который включен и идёт
                         needDay = li.Day;
                    }
                    if (li.IsFixedStart == false && li.IsFixedEnd == false && li.IsFixedDay == true)
                    {
                        //День который включен, который зафиксировал пользователь но ещё не кончился 
                         needDay = li.Day;
                    }

                    if (li.IsFixedStart == false && li.IsFixedEnd == true && li.IsFixedDay == true)
                    {
                        //День который включен, который закончился
                         needDay = li.Day; //+1
                    }
                    Console.WriteLine($"День который найден {needDay}");

                }
                Console.WriteLine("Записей нет вернуть 0\n");
                return needDay;
            }
            else {
                Console.WriteLine("Записей нет вернуть 0\n");
                return needDay;

            }
           
            /*
            var day = await context.UserDate.Where(u => u.UserId == userId
            && u.IsFixedStart == true
            && u.IsFixedEnd == false
            && u.IsFixedDay == false)              
                .ToListAsync();

            Console.WriteLine($"Проверка на записи этого пользователя {userId}\n");

            var CheckFirsttday = await context.UserDate.Where(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (CheckFirsttday == null)
            {

                Console.WriteLine("Записей нет вернуть 0");
                return needDay;
            }
            else {         
                int? Day = await QueryFindLastRecord(context, userId);
                if (Day != null)
                {
                    return needDay= (int)Day;
                }            
            }
           
            var lastDays = await context.UserDate.Where(u => u.UserId == userId).MaxAsync(u => u.Day);

            if (lastDays == null) { Console.WriteLine($"нет записей\n"); }

            if (day == null)
            {
              //  var lastDays = await context.UserDate.Where(u => u.UserId == userId).MaxAsync(u => u.Day);

                //
                var lastDay = await context.UserDate
                    .Where(u => u.UserId == userId && u.IsFixedStart == true && u.IsFixedEnd == false && u.IsFixedDay == false)
                    .Select(u => u.WhatNewinDay)
                    .FirstOrDefaultAsync();

                Console.WriteLine($"{lastDay}" + " / !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! пришедший \n");
                 needDay = int.Parse(lastDay);

                if (lastDay == null) { Console.WriteLine($"нет записей\n"); }


                return needDay;
            }
            if(day != null)
            {

                var id = await context.UserDate.Where(x => x.Id == userId).FirstOrDefaultAsync();

                var dayList = await context.UserDate.Where(u => u.UserId == userId).ToArrayAsync();


                if (dayList == null && id==null) { Console.WriteLine($"нет записей\n");  }

                foreach (var li in dayList)
                {

                    if (li.IsFixedStart == false && li.IsFixedEnd == true && li.IsFixedDay == false  && li.Day == lastDays)
                    {

                        needDay = li.Day+1;
                        Console.WriteLine($"{needDay}" + " / День который нужен! первое условие если день закончился сам\n");
                        
                        return needDay;

                    }

                    if (li.IsFixedStart == false && li.IsFixedEnd == true && li.IsFixedDay == true  && li.Day == lastDays 
                        || li.IsFixedStart == false && li.IsFixedEnd == false && li.IsFixedDay == true && li.Day == lastDays)
                    {

                        needDay = li.Day;
                        Console.WriteLine($"{needDay}" + " / День который нужен! второе условие день который пользователь завершил сам или он закончился \n");
                        
                        return needDay;

                    }

                    else
                    {

                      //  //  needDay=li.Day;
                      //  Console.WriteLine($"{needDay}" + " / Начальный день спринта\n");

                      //  Console.WriteLine("========================================================Вышел из  GetDay=================================================================\n");
                      ////  return needDay;
                    }




                }
            }
            
          //  Console.WriteLine(needDay + " / День который нужен\n");


            
            return needDay;
           */

        }


        public async Task<string> GetWhnd(UsersDataContext context,  int userId, int day)
        {

            Console.WriteLine("========================================================В   GetWhnd=================================================================\n");
            var CheckRecords = await context.UserDate.Where(u=> u.UserId == userId).FirstOrDefaultAsync();

            if (CheckRecords == null) {

                Console.WriteLine("Я в нужнем месте");
                return null;
            }

            var WhnW = await context.UserDate.
               Where(u => u.UserId == userId && u.IsFixedStart == false && u.IsFixedEnd == true && u.IsFixedDay == true)
               .Select(u => u.WhatNewinDay)
               .FirstOrDefaultAsync();
            //Отредактировать условие таким образом чтобы находило данные за последние завершённый или начатый день или день который начался 
            if (WhnW == null)
            {
                var days = await context.UserDate.Where(u => u.UserId == userId).MaxAsync(u => u.Day);


                WhnW = await context.UserDate
                    .Where(u => u.UserId == userId && u.IsFixedStart == false && u.IsFixedEnd == true && u.IsFixedDay == false && u.Day==days)
                    .Select(u => u.WhatNewinDay)
                    .FirstOrDefaultAsync();
            }

            Console.WriteLine($"{WhnW} " + " получил данные из базы\n");

            return WhnW;
        }

        public async Task<bool?> GettimeOff_On(UsersDataContext context,  int userId)
        {
            Console.WriteLine("========================================================В   GettimeOff_On=================================================================\n");
            Console.WriteLine($"ID пользователя  GettimeOFF: {userId}\n");
            var PersonYesNo = await context.UserDate.FirstOrDefaultAsync(p => p.UserId == userId);
           
            //Понять включить или выключить таймер            

            //Найден пользователь или нет
            if (PersonYesNo != null)
            {
                //Get last day 
                var timeOff_On = await context.UserDate.Where(u => u.UserId == userId && u.Day == context.UserDate.Where(u => u.UserId == userId).Max(d => d.Day)).ToListAsync();


                foreach (var li in timeOff_On) {

                    if (li.IsFixedStart == true && li.IsFixedEnd == false && li.IsFixedDay == false)
                    {
                        return  true;
                    }
                    if (li.IsFixedStart == false && li.IsFixedEnd == true && li.IsFixedDay == false 
                        || li.IsFixedStart == false && li.IsFixedEnd == true && li.IsFixedDay == true)
                    {
                        return false;
                    }



                }
            }
            else {
                
                return null;

            }
            return null;
        }

        public async Task<bool> CheckingTheDay(string timezone, DateTime date_2) {

            Console.WriteLine("========================================================В   CheckingTheDay=================================================================\n");

            Console.WriteLine($"{timezone} -- сюда пришла TimeZone далее будем проверять дату по часовому поясу и дату за которую произошла запись");
           // далее будем проверять дату по часовому поясу и дату за которую произошла запись, если сейчас новая дата то блокировать редактирование дня 

            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[timezone];
            ZonedDateTime now = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);

            // Получение даты и времени в нужном формате
            Instant utcInstant = now.ToInstant();
            DateTimeOffset utcDateTimeOffset = utcInstant.ToDateTimeOffset();
            DateTime utcDateTime = utcDateTimeOffset.UtcDateTime;
            Console.WriteLine($"{utcDateTime}");

            Console.WriteLine($"Время даты которая зафиксирована {date_2.Date}");

            if (utcDateTime.Minute > date_2.Minute )
            {
                Console.WriteLine($"{utcDateTime.Minute}  CheckingTheDay- время которое больше положенного  \n");
                return false;
            }
            else
            {
               Console.WriteLine($"{date_2.Minute}  CheckingTheDay время которое установлено у пользователя не пришло \n");
                return  true;
            }
        }


        public async Task CheckEndDay() {
            
            //Проверка дня если закончился то переключить на следующий 
        
        }

        public async Task<bool> IsValidToken() {

            string access_token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            // Остальная логика метода...

            // Создание экземпляра JwtSecurityTokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();
            // Валидация и чтение токена
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = AuthOptions.ISSUER,
                ValidAudience = AuthOptions.AUDIENCE,
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey()
            };
            try
            {
                // Валидация токена и чтение его свойств
                var claimsPrincipal = tokenHandler.ValidateToken(access_token, tokenValidationParameters, out var validatedToken);

                // Проверка времени истечения токена
                if (validatedToken.ValidTo < DateTime.UtcNow)
                {
                    // Время истекло, возвращаем ошибку
                    Console.WriteLine("access_token время истекло");
                    return true;
                }
                else { 
                
                   
                }
            }
            catch (SecurityTokenException)
            {
               
            }
                return  false;
        }



        public async void BlockDayOrNot(TimeString timeString) {

            Console.WriteLine("========================================================В  BlockDay=================================================================\n");
            var context = new UsersDataContext();
            //Получаю UserId
            int idResult = await GetId(context, timeString);
            Console.WriteLine($"Id пользователя:{idResult} \n");
            //Получение часового пояса
            string Timezone = await GetTimeZone(context, idResult);
            var UserYesNo = await context.UserDate.FirstOrDefaultAsync(p => p.UserId == int.Parse(timeString.UserId) && p.UserId != null);
            DateTime dateTimeRecord = new DateTime();
            if(UserYesNo==null)
             
            {
                Console.WriteLine("Ещё пока нет дней\n");

            }
            if (UserYesNo != null)
            {


                var UserFixDate_1_check = await context.UserDate.Where(u => u.UserId == int.Parse(timeString.UserId) &&  u.IsFixedStart == false && u.IsFixedEnd == true && u.IsFixedDay == false
                ).ToListAsync();

               // var UserFixDate_2_check = await context.UserDate.Where(u => u.UserId == int.Parse(timeString.UserId) && u.IsFixedStart == false && u.IsFixedEnd == true && u.IsFixedDay == true)
               //.ToListAsync();



               // if (UserFixDate_1_check != null) { Console.WriteLine($"Если такого дня нет то :  что то сделать  {timeString.UserId} \n"); }
                //В этом цмкле мы проверяем есть ли не заблокированные дни если есть то перезаписываем его
                foreach (var item in UserFixDate_1_check)
                {
                    Console.WriteLine($"Дата и время которые зафиксировал пользователь за запустившийся день:{item.Date} и id - {item.UserId} \n");
                    //Получить Timezone и идущйи день 
                    dateTimeRecord = item.Date.Date;
                    bool CheckDay = await CheckingTheDay(Timezone, dateTimeRecord);
                    if (CheckDay == false)
                    {
                        Console.WriteLine("День истёк!!!!!! " +  $"{CheckDay}" +" \n");
                        //Логика если день истёк
                        // Сделать update в базе если наступил следующий день 

                        //Добавленно два пункта оба true 
                        item.IsFixedStart = false;
                        item.IsFixedDay = true;
                        item.IsFixedEnd = true;

                        await context.SaveChangesAsync();
                        
                        
                    }
                    else
                    {
                        Console.WriteLine("Продолжается\n");
                        
                    }
                    
                }


                //здесь находим по коллекцию по другому условию
                /*
                foreach (var item in UserFixDate_2_check)
                {
                    Console.WriteLine($"Здесь цикл с другим условием:{item.Date} и id - {item.UserId} \n");
                    //Получить Timezone и идущйи день 
                    dateTimeRecord = item.Date;
                    bool CheckDay = await CheckingTheDay(Timezone, dateTimeRecord);
                    if (CheckDay == false)
                    {
                        Console.WriteLine("День истёк!!!!!! " + $"{CheckDay}" + " \n");
                        //Логика если день истёк
                        // Сделать update в базе если наступил следующий день 

                        //Добавленно два пункта оба true 
                        item.IsFixedStart = false;
                        item.IsFixedDay = false;
                        item.IsFixedEnd = true;

                        await context.SaveChangesAsync();
                        Console.WriteLine("========================================================Вышел из  BlockDay=================================================================\n");

                    }
                    else
                    {
                        Console.WriteLine("Продолжается\n");
                    }
               
                }
                */

            }
            Console.WriteLine("========================================================Вышел из  BlockDay=================================================================\n");

        }

        public async Task<bool> DayFixedOrNot(UsersDataContext context, TimeString timeString, string timezone, int day) {

            //Проверить даты с последней записью если последняя запись закончена то показать ожидание старта и отдавать следующий день 
            bool falseTrue = true;
            //Console.WriteLine("========================================================В   CheckingTheDay=================================================================\n");
            //Console.WriteLine($"{timezone} -- сюда пришла TimeZone далее будем проверять дату по часовому поясу и дату за которую произошла запись");
            //// далее будем проверять дату по часовому поясу и дату за которую произошла запись, если сейчас новая дата то блокировать редактирование дня 
            //DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[timezone];
            //ZonedDateTime now = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);
            //// Получение даты и времени в нужном формате
            //Instant utcInstant = now.ToInstant();
            //DateTimeOffset utcDateTimeOffset = utcInstant.ToDateTimeOffset();
            //DateTime utcDateTime = utcDateTimeOffset.UtcDateTime;
            //Console.WriteLine($"{utcDateTime}");





            DateTimeOffset getData = await context.UserDate.Where(u => u.UserId == int.Parse(timeString.UserId) && u.IsFixedStart == false && u.IsFixedEnd == true &&
             u.IsFixedDay == true && u.Day == day).Select(u => u.Date.LocalDateTime).FirstOrDefaultAsync();
         
            Console.WriteLine($"{getData}" +" Время дня пользователя\n");

            string userTimeZoneId = timezone;

            // Получаем временную зону из идентификатора
            DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb[userTimeZoneId];

            // Конвертируем getData в объект Instant
            Instant getDataInstant = Instant.FromDateTimeOffset(getData);

            // Конвертируем getDataInstant в объект ZonedDateTime с использованием userTimeZone
            ZonedDateTime getDataUserTimeZone = getDataInstant.InZone(userTimeZone);
            Console.WriteLine(getDataUserTimeZone.TimeOfDay + " Время из базы в часовом поясе пользователя\n");

            // Получаем текущее время в часовой пояс пользователя
            ZonedDateTime userDateTime = SystemClock.Instance.GetCurrentInstant().InZone(userTimeZone);
            Console.WriteLine(userDateTime.TimeOfDay + " Текущее время в часовом поясе пользователя\n");

            // Сравниваем времена
            if (getDataUserTimeZone.TimeOfDay > userDateTime.TimeOfDay)
            {
                Console.WriteLine("Время из базы позже текущего времени пользователя.");
            }
            else if (getDataUserTimeZone.TimeOfDay < userDateTime.TimeOfDay)
            {
                Console.WriteLine("Время из базы раньше текущего времени пользователя.");
            }
            else
            {
                Console.WriteLine("Время из базы равно текущему времени пользователя.");
            }


           


            //var getDay= await context.UserDate.Where(u=> u.UserId == int.Parse(timeString.UserId) && u.Day==day).ToListAsync();
            //bool falseTrue = true;

            //foreach(var item in getDay)
            //{
            //    if (item.IsFixedDay == true || item.IsFixedEnd == true)
            //    {


            //        falseTrue= false;
            //    }
            //    else {
            //        falseTrue= true;
            //    }
            //}

            return falseTrue;
        }


        //Обновление даты времени, и вывод данных поэтому времени 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> TimeChecker([FromBody] TimeString timeString)
        {
            Console.WriteLine("===========================================================TimeChecker CONTROLLER Begin===========================================================\n");
            try
            {
            
                // Возвращаемые данные
                var context = new UsersDataContext();
                bool? OnOff = null;             
                //Получаю UserId
                int idResult = await GetId(context, timeString);
                Console.WriteLine($"Получаем ID пользователя : {idResult}\n");
                Console.WriteLine("========================================================Вышел  GetId=================================================================\n");
                //Получение часового пояса
                string Timezone = await GetTimeZone(context, idResult);
                Console.WriteLine($"Получаем Часовой пояс : {Timezone}\n");
                Console.WriteLine("========================================================Вышел  GetTimeZone=================================================================\n");
                //Получение дня
                int day = await GetDay(context, idResult);
                Console.WriteLine($"Получаем день который идёт сейчас : {day}\n");
                Console.WriteLine("========================================================Вышел из  GetDay=================================================================\n");
                //Получение что узнать за день
                string? WhnW = await GetWhnd(context,  idResult, day);
                Console.WriteLine("========================================================Вышел   GetWhnd=================================================================\n");              
                //Сразу выводить включение или выключение таймера
                OnOff = await GettimeOff_On(context, idResult);
                Console.WriteLine($"========================================================Вышел   GettimeOff_On =====================\n");
                //Получить коллекцию последнего дня человека
                var LastDayUser = await context.UserDate.Where(x => x.UserId == idResult).ToListAsync();
                //  bool isDayFixed = await DayFixedOrNot(context, timeString, Timezone, day);// Найти последний день у пользователя и проверить заблокирован ли он если он закончился или его завершили то увеличить передаваемое число на 1  

                BlockDayOrNot(timeString);            
                Console.WriteLine("========================================================Вышел из  BlockDay===============================================================\n");                           
                Console.WriteLine($"Что пользователь хочет за этот день сделать: {WhnW}" + $" и кнопка {OnOff.ToString()}\n");

                //Получить дату сегодня и получить дату дня который идёт у пользователя
                if (OnOff == null) {

                    Console.WriteLine($"Значение бул  кнопка {(OnOff != null ? OnOff.ToString() : "null")}\n\n");
                }


                //Console.WriteLine("Сколько записей по дня у этого пользователя: ");

                var CountDay =   context.UserDate.Where(x => x.UserId== idResult).Select(x => x.Day).ToList();



                 
                Console.WriteLine($"Данные которые идут на отображение: \n 1.Id -{idResult}\n 2.TimeZone -{Timezone}\n 3.Day -{day}\n 4.OnOff -{OnOff}\n 5.Whnd -{WhnW}\n");

                var response = new
                {
                    UserId = idResult, // Возвращаемый идентификатор пользователя
                    TimeZone = Timezone, // Возвращаемый часовой пояс
                    Day = day,   // День пользователя
                    OnOffTimer = OnOff, // включение выключение таймера
                    Whnd = WhnW,

                    //нужно ещё отправлять зафиксирован ли день 
                };

                Console.WriteLine($"Receive Timezone: {Timezone}\n" + $"{day}---какой день\n" + $"Данные записи в этот день:{WhnW}\n" + $"{OnOff}");
                Console.WriteLine("===========================================================TimeChecker CONTROLLER END===========================================================\n");
                // Верните данные в формате JSON
                return Json(response);

            }
            catch (SecurityTokenException)
            {
                // Некорректный токен, возвращаем ошибку
                return Unauthorized("Токен некорректен");
            }        
         
        }

       
    }
}
