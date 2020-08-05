using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            //dodaje header z errorem
           response.Headers.Add("Application-Error", message);
            //odkrywa header z errorem dla Angulara
           response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
           //allow origin
           response.Headers.Add("Access-Control-Allow-Origin", "*");

        }
    }
}