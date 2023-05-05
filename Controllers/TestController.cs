using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;

namespace ServerApp.Controllers {
   
     
    public class TestController : ControllerBase
    {

        public class TestString { 
          
          public string WhatNewinDay { get; set; }
          public string NewKnoledge{ get; set; }
          public string DayPhilosophy{ get; set; }
          public string WhatDone{ get; set; }
          public string WhatNotDone{ get; set; }
          public string Сonclusions{ get; set; }
            //Day: number;  
        }


        [HttpPost]
        //[Authorize]
        public async Task Index([FromBody]TestString data)
        {
            Console.WriteLine($"Receive:{data.WhatNewinDay} {data.NewKnoledge}\r\n");
            await Task.Delay(1000);
        }
    }
}
