using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

[Authorize]
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly IMapper Mapper;
    protected readonly IRequestContext RequestContext;

    public BaseController(IMapper mapper, IRequestContext requestContext)
    {
        Mapper = mapper;
        RequestContext = requestContext;
    }

    // Yhtenäistetyt vastausmetodit
    protected IActionResult Success<T>(T data) =>
        Ok(new { error = false, data });

    protected IActionResult Created<T>(T data, string actionName, object routeValues) =>
        CreatedAtAction(actionName, routeValues, new { error = false, data });

    protected IActionResult Error(string message, int statusCode = 500) =>
        StatusCode(statusCode, new { error = true, message });

    protected IActionResult NotFoundError(string message = "Resource not found") =>
        NotFound(new { error = true, message });

    protected IActionResult BadRequestError(string message) =>
        BadRequest(new { error = true, message });
}

