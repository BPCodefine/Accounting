using AccountingServer.DBAccess;
using AccountingServer.Entities;
using Dapper;
using System.Security.Principal;

namespace AccountingServer.Endpoints
{
    public static class GeneralEndpoints
    {
        public static void MapGeneralEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => "Hello World!");

            app.MapGet("/api/username", (HttpContext context) =>
            {
                var username = context.User.Identity?.Name; // e.g., DOMAIN\username
                return Results.Ok(new { username });
            });

            const string sqlVendors = @"
SELECT
   [No_] as No,
   [Name],
   [Search Name] as SearchName,
   [Country_Region Code] as CountryRegionCode
FROM
    [dbo].[CDF$Vendor$437dbf0e-84ff-417a-965d-ed2bb9650972]
WHERE
	Name <> ''
ORDER BY
    Name";

            app.MapGet("/api/Vendors", async (DynamicsDBContext context) =>
            {
                using var conn = context.Create();
                var lines = await conn.QueryAsync<Vendors>(sqlVendors.ToString());
                return Results.Ok(lines);
            }
            );
        }
    }
}
