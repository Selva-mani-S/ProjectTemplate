namespace Solution.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [AllowAnonymous]
    [Route("Error/{statusCode:int}")]
    public IActionResult HandleStatusCode(int statusCode)
    {
        ViewData["StatusCode"] = statusCode;
        return View("Error");
    }
}
