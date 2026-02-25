using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VehicleSales.Queries;

public sealed record PagedRequest(
    [property: Range(0, int.MaxValue)]
    [property: Description("0 based page number")]
    int PageNumber,
    [property: Range(1, 100)]
    int PageSize);
