using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using ProjectApp.Data;
using ServerApp.Services;
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
             public string Whnd { get; set; }

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
        
        }


        public async Task<int> GetId(UsersDataContext context , TimeString timeString)
        {
            
            var idUser = await context.Users.Where(x => x.Id == int.Parse(timeString.UserId))
                              .Select(x => x.Id)
                              .FirstOrDefaultAsync();         
           return  int.Parse(idUser.ToString());
        }


        public async Task<string> GetTimeZone(UsersDataContext context,  int userId)
        {
            var columnTimezon = await context.Users
                              .Where(x => x.Id == userId)
                               .Select(x => x.TimeZone)
                               .FirstOrDefaultAsync();
            return columnTimezon;
        }

        public async Task<int> GetDay(UsersDataContext context,  int userId)
        {
            var day = await context.UserDate.Where(u => u.UserId == userId
            && u.IsFixedStart == true
            && u.IsFixedEnd == false
            && u.IsFixedDay == false)
                .Select(u => u.Day)
                .FirstOrDefaultAsync();

            return day;
        }


        public async Task<string> GetWhnd(UsersDataContext context,  int userId)
        {
            var WhnW = await context.UserDate.
               Where(u => u.UserId == userId && u.IsFixedStart == true && u.IsFixedEnd == false && u.IsFixedDay == false)
               .Select(u => u.WhatNewinDay)
               .FirstOrDefaultAsync();

            return WhnW;
        }

        public async Task<List<TimeOffData>?> GettimeOff_On(UsersDataContext context,  int userId)
        {


            var PersonYesNo = await context.UserDate.FirstOrDefaultAsync(p => p.UserId == userId);

           // Console.WriteLine($"{PersonYesNo.Id}"+" Здесь надо определить что пользователь не найден или найден ");

            if (PersonYesNo != null)
            {

                var timeOff_On = await context.UserDate.Where(u => u.UserId == userId && u.IsFixedStart == true && u.IsFixedEnd == false && u.IsFixedDay == false)
                 .Select(u => new TimeOffData
                 {
                     IsFixedStart = u.IsFixedStart,
                     IsFixedEnd = u.IsFixedEnd,
                     IsFixedDay = u.IsFixedDay
                 })
                 .ToListAsync();
                return timeOff_On;

            }
            else {

                return null;

            }
            

          
        }

        public async Task<bool> CheckingTheDay(string timezone, DateTime date_2) {



            Console.WriteLine($"{timezone} -- сюда пришла TimeZone далее будем проверять дату по часовому поясу и дату за которую произошла запись");
           // далее будем проверять дату по часовому поясу и дату за которую произошла запись, если сейчас новая дата то блокировать редактирование дня 

            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[timezone];
            ZonedDateTime now = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);

            // Получение даты и времени в нужном формате
            Instant utcInstant = now.ToInstant();
            DateTimeOffset utcDateTimeOffset = utcInstant.ToDateTimeOffset();
            DateTime utcDateTime = utcDateTimeOffset.UtcDateTime;
            Console.WriteLine($"{utcDateTime}");

            Console.WriteLine($"Время даты которая зафиксирована {date_2}");

            if (utcDateTime.Day > date_2.Day)
            {

                return false;
            }
            else
            {
                return true;
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






        //Сюда будет приходить запрос на часовой пояс 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> TimeChecker([FromBody] TimeString timeString)
        {          
            try
            {
                // Валидация токена и чтение его свойств


              //  Проверка времени истечения токена
                //if (await IsValidToken() == true)
                //{
                //    // Время истекло, возвращаем ошибку
                    
                //    return Unauthorized("Истекло время");
                //}
                //else
                //{
                   
                //    Console.WriteLine("Принимаю Данные!");
                   
                //}

                // Продолжение выполнения метода TimeChecker
                // ...

                // Возвращаемые данные
                var context = new UsersDataContext();
                bool OnOff = false;             
                //Получаю UserId
                int idResult = await GetId(context, timeString);
                //Получение часового пояса
                string Timezone = await GetTimeZone(context, idResult);
                //Получение дня
                int day = await GetDay(context, idResult);
                //Получение что узнать за день
                string WhnW = await GetWhnd(context,  idResult);
                Console.WriteLine($"Receive Timezone: {Timezone}\n");
                //Лист для логики отображения дня 
                List<TimeOffData> timeOff_ = await GettimeOff_On(context, idResult);

                if (timeOff_ == null)
                {
                    OnOff = false;
                }
                else
                {
                    foreach (var item in timeOff_)
                    {

                        Console.WriteLine($"Начало дня: {item.IsFixedStart}" + $" Конец дня: {item.IsFixedEnd}" + $" День заблокирован: {item.IsFixedDay}");
                        if (item.IsFixedStart == true && item.IsFixedEnd == false && item.IsFixedDay == false)
                        {
                            OnOff = true;
                        }
                        else
                        {
                            OnOff = false;
                        }


                    }
                }
                Console.WriteLine($"Что пользователь хочет за этот день сделать: {WhnW}\n");

                //Получить дату сегодня и получить дату дня который идёт у пользователя

                var UserYesNo = await context.UserDate.FirstOrDefaultAsync(p => p.UserId == int.Parse(timeString.UserId) && p.UserId != null);
                DateTime dateTimeRecord= new DateTime();
                  if(UserYesNo != null) {
                   
                            
                        var UserFixDate = await context.UserDate.Where(u => u.UserId == int.Parse(timeString.UserId) && u.IsFixedStart == true && u.IsFixedEnd == false && u.IsFixedDay == false)
                   .Select(u => new Date
                        {dateTime = u.Date,
                          id=u.UserId    
                        })
                   .ToListAsync();

                    foreach (var item in UserFixDate) {
                        Console.WriteLine($"Дата и время которые зафиксировал пользователь за запустившийся день:{item.dateTime} и id - {item.id} \n");
                        //Получить Timezone и идущйи день 
                        dateTimeRecord = item.dateTime;
                        bool CheckDay = await CheckingTheDay(Timezone, dateTimeRecord);
                        if (CheckDay == false)
                        {
                            Console.WriteLine("День истёк!!!!!!\n");
                            //Логика если день истёк
                        }
                        else {
                            Console.WriteLine("Продолжается\n");
                        }

                    }

                    

                  }

                  


                var response = new
                {
                    UserId = idResult, // Возвращаемый идентификатор пользователя
                    TimeZone = Timezone, // Возвращаемый часовой пояс
                    Day = day,   // День пользователя
                    OnOffTimer = OnOff, // включение выключение таймера
                    Whnd = WhnW,
                };

                Console.WriteLine($"Receive Timezone: {Timezone}\n" + $"{day}---какой день\n" + $"Данные записи в этот день:{WhnW}\n" + $"{OnOff}");

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
