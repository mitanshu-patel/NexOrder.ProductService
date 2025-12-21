using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NexOrder.ProductService.Shared.Common
{
    public static class CustomHttpResult
    {
        public static IActionResult GetResponse<T>(this CustomResponse<T> customResponse)
        {
            if (customResponse.ResponseCode == System.Net.HttpStatusCode.BadRequest)
            {
                return new BadRequestObjectResult(customResponse.Errors != null ? customResponse.Errors : customResponse.ErrorMessage);
            }
            else if (customResponse.ResponseCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundObjectResult(customResponse.ErrorMessage);
            }
            else if (customResponse.ResponseCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new UnauthorizedResult();
            }
            else if (customResponse.ResponseCode == System.Net.HttpStatusCode.Forbidden)
            {
                return new ForbidResult(customResponse.ErrorMessage);
            }
            else if (customResponse.ResponseCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return new OkObjectResult(customResponse.Data);
        }

        public static CustomResponse<T> NotFound<T>(string errorMessage)
        {
            var customResponse = new CustomResponse<T>
            {
                ErrorMessage = errorMessage,
                ResponseCode = System.Net.HttpStatusCode.NotFound,
            };
            return customResponse;
        }

        public static CustomResponse<T> BadRequest<T>(string errorMessage, Dictionary<string, string>? errors = null)
        {
            var customResponse = new CustomResponse<T>
            {
                ErrorMessage = errorMessage,
                ResponseCode = System.Net.HttpStatusCode.BadRequest,
                Errors = errors,
            };
            return customResponse;
        }

        public static CustomResponse<T> Ok<T>(T result)
        {
            var customResponse = new CustomResponse<T>
            {
                ResponseCode = System.Net.HttpStatusCode.OK,
                Data = result,
            };
            return customResponse;
        }

        public static CustomResponse<T> UnAuthorized<T>(string errorMessage)
        {
            var customResponse = new CustomResponse<T>
            {
                ErrorMessage = errorMessage,
                ResponseCode = System.Net.HttpStatusCode.Unauthorized,
            };
            return customResponse;
        }
    }
}
