﻿using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DatingApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">This is used if we want to do something while the action is executed</param>
        /// <param name="next">This allows us to run some code after the action has been executed. </param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
           
            var resultContext = await next();

       
            var userId = int.Parse(resultContext.HttpContext.User
                .FindFirst(ClaimTypes.NameIdentifier).Value);

          
            var repo = resultContext.HttpContext.RequestServices
                .GetService<IDatingRepository>();

            var user = await repo.GetUser(userId, true);
            user.LastActive = DateTime.Now;
            await repo.SaveAll();
        }
    }
}
