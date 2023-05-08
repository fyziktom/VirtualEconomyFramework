using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VEFramework.BlockchainIndexerServer.Common
{
    public class HttpResponseException : Exception
    {
        public HttpResponseException(HttpStatusCode code, string message)
        {
            Status = (int)code;
            Value = message;
        }
        public int Status { get; set; } = 500;
        public object Value { get; set; }
    }



    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is HttpResponseException exception)
            {
                context.Result = new ObjectResult(exception.Value)
                {
                    StatusCode = exception.Status,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
