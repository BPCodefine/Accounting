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

            app.MapGet("/username", (HttpContext context) =>
            {
                if (OperatingSystem.IsWindows())
                {
                    // Retrieve the Windows identity from the current user
                    if (context.User.Identity is WindowsIdentity windowsIdentity)
                    {
                        return Results.Ok(windowsIdentity.Name); // Returns the SSPI context user name
                    }
                    else
                    {
                        return Results.Ok("Error getting Windows Identity");
                    }
                }
                else
                {
                    return Results.Problem("WindowsIdentity.Name is only supported on Windows platforms.", statusCode: 500);
                }
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
