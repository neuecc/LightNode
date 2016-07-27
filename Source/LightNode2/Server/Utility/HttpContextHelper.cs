using Microsoft.AspNetCore.Http;
using System.Text;

namespace LightNode.Server
{
    internal static class HttpContextHelper
    {
        static readonly Encoding UTF8 = new UTF8Encoding(false);

        public static void EmitStringMessage(this HttpContext context, string message)
        {
            var bytes = UTF8.GetBytes(message);
            context.Response.Headers["Content-Type"] = "text/plain";
            context.Response.Body.Write(bytes, 0, bytes.Length);
        }

        public static void EmitOK(this HttpContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK; // 200
        }

        public static void EmitNoContent(this HttpContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.NoContent; // 204
        }

        public static void EmitBadRequest(this HttpContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest; // 400
        }

        public static void EmitNotFound(this HttpContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound; // 404
            context.EmitStringMessage("404 NotFound");
        }

        public static void EmitMethodNotAllowed(this HttpContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.MethodNotAllowed; // 405
            context.EmitStringMessage("405 MethodNotAllowed");
        }

        public static void EmitNotAcceptable(this HttpContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotAcceptable; // 406
            context.EmitStringMessage("406 NotAcceptable");
        }

        public static void EmitUnsupportedMediaType(this HttpContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.UnsupportedMediaType; // 415
        }

        public static void EmitInternalServerError(this HttpContext context)
        {
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError; // 500
        }
    }
}
