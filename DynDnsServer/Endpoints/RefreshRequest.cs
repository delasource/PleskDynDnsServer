using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace DynDnsServer.Endpoints;

public class RefreshRequest
{
    /// <summary>
    /// Resource
    /// </summary>
    [Required]
    [FromQuery(Name = "r")]
    [QueryParam, BindFrom("r")]
    public string? Resource { get; init; }

    /// <summary>
    /// Security Hash
    /// </summary>
    [Required]
    [FromQuery(Name = "h")]
    [QueryParam, BindFrom("h")]
    public string? SecurityHash { get; init; }
}