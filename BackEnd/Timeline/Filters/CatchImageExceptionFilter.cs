using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services.Imaging;

namespace Timeline.Filters
{
    public class CatchImageExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ImageException e)
            {
                context.Result = new  BadRequestObjectResult(e.Error switch
                {
                    ImageException.ErrorReason.CantDecode => new CommonResponse(ErrorCodes.Image.CantDecode, Resource.MessageImageDecodeFailed),
                    ImageException.ErrorReason.UnmatchedFormat => new CommonResponse(ErrorCodes.Image.UnmatchedFormat, Resource.MessageImageFormatUnmatch),
                    ImageException.ErrorReason.BadSize => new CommonResponse(ErrorCodes.Image.BadSize, Resource.MessageImageBadSize),
                    _ => new CommonResponse(ErrorCodes.Image.Unknown, Resource.MessageImageUnknownError)
                });
            }
        }
    }
}
