﻿using AccountingServer.DBAccess;
using AccountingServer.Entities;
using Dapper;
using System.Text;

namespace AccountingServer.Endpoints
{
    struct DateInterval
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
    public class LedgerQuery
    {
        public DateTime ToDate { get; set; }
        public string? ExtraAccList { get; set; }
    }

    public class CustInvQuery
    {
        public string Company { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class VendorInvQuery
    {
        public string Company { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool? OnlyOpen { get; set; } = false;
    }
    public static class InvoicingEndpoints
    {
        public static void MapInvoicingEndpoints(this IEndpointRouteBuilder app)
        {
            const string sqlPurchInvoicesAndExpenses = @"
with InvCTE as(
select
	pih.[No_] as InvoiceNo,
    pih.[Buy-from Vendor No_] as BuyFromID,
    pih.[Buy-from Vendor Name] as BuyFromName,
    pih.[Pay-to Vendor No_] as PayToID,
    pih.[Pay-to Name] as PayToName,
    pih.[Your Reference] as YourRefOrderID,
    pih.[Posting Date] as PostingDate,
    pil.[Line No_] as InvLineNo,
    pil.[No_] as ArticleNo,
    pil.Description as ArticleName,
    pil.Quantity as InvoicedQty
from
    [dbo].[CDF$Purch_ Inv_ Header$437dbf0e-84ff-417a-965d-ed2bb9650972] pih
	inner join dbo.[CDF$Purch_ Inv_ Line$437dbf0e-84ff-417a-965d-ed2bb9650972] pil on pih.[No_] = pil.[Document No_]
where
    pil.Type = 2
),
-- Step 1: Aggregate charges and pivot them
ChargeSummary as (
    select
        subquery.[Document No_] as InvoiceNo,
        subquery.[Document Line No_] as DocLineNo,
        ve.[Item No_] as ItemNo,
        ve.[Valued Quantity] as ValuedQuantity,
        ve.[Item Charge No_] as ChargeName,
        SUM(ve.[Cost Amount (Actual)]) as TotalCostAmount
    from 
        [CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ve
    inner join 
        [CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] subquery 
        on ve.[Item Ledger Entry No_] = subquery.[Item Ledger Entry No_]
    where 
        subquery.[Source Code] <> 'INVTADJMT'
        and ve.[Cost Amount (Actual)] <> 0
    group by 
        subquery.[Document No_], 
        subquery.[Document Line No_], 
        ve.[Item No_],
        ve.[Valued Quantity],
        ve.[Item Charge No_]
),
ChargePivot as (
    select
        InvoiceNo,
        DocLineNo,
        ItemNo,
        ValuedQuantity,
        ISNULL([ ], 0) as PUACost,
        ISNULL(SEAFREIGHT, 0) as SeafreightCost,
        ISNULL(DUTY, 0) as DutyCost,
        ISNULL(STORAGE, 0) as StorageCost,
        ISNULL(OUTFREIGHT, 0) as OutfreightCost,
        ISNULL(INLANDFREIGHT, 0) as InlandFreightCost,
        ISNULL(SMALLCOST, 0) as SmallCost,
        ISNULL(UVYARNCOST, 0) as UVYarnCost,
        ISNULL(QUALDISCOUNT, 0) as QualDiscountCost
    from
        ChargeSummary
    pivot
    (
        sum(TotalCostAmount)
        for ChargeName in (
            [ ], 
            SEAFREIGHT, 
            DUTY, 
            STORAGE, 
            OUTFREIGHT, 
            INLANDFREIGHT, 
            SMALLCOST, 
            UVYARNCOST, 
            QUALDISCOUNT
        )
    ) as p
)

-- Step 2: Combine invoice lines with charge data
select 
    InvCTE.*,
    ISNULL(cp.PUACost, 0) as PUACost,
    ISNULL(cp.SeafreightCost, 0) as SeafreightCost,
    ISNULL(cp.DutyCost, 0) as DutyCost,
    ISNULL(cp.StorageCost, 0) as StorageCost,
    ISNULL(cp.OutfreightCost, 0) as OutfreightCost,
    ISNULL(cp.InlandFreightCost, 0) as InlandFreightCost,
    ISNULL(cp.SmallCost, 0) as SmallCost,
    ISNULL(cp.UVYarnCost, 0) as UVYarnCost,
    ISNULL(cp.QualDiscountCost, 0) as QualDiscountCost
from
	InvCTE
	left join ChargePivot cp on 
		InvCTE.InvoiceNo = cp.InvoiceNo collate database_default
		and InvCTE.InvLineNo = cp.DocLineNo
		and InvCTE.ArticleNo = cp.ItemNo collate database_default
		and InvCTE.InvoicedQty = cp.ValuedQuantity
";
            const string OrderBy = "order by InvCTE.PostingDate, InvCTE.InvoiceNo, InvCTE.InvLineNo";

            app.MapGet("/api/PurchInvAndExpenses", async (DynamicsDBContext context) =>
                {
                    try
                    {
                        using var conn = context.Create();
                        var lines = await conn.QueryAsync<PIAndExpenses>(sqlPurchInvoicesAndExpenses);
                        return Results.Ok(lines);
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(context.connString + "; " + ex.Message, statusCode: 500);
                    }
                }
            );

            app.MapGet("/api/PurchInvAndExpenses/{invNo}", async (DynamicsDBContext context, string invNo) =>
            {
                StringBuilder sqlWithInvNo = new(sqlPurchInvoicesAndExpenses);
                sqlWithInvNo.AppendLine($"where InvCTE.InvoiceNo = @InvoiceNo");
                sqlWithInvNo.AppendLine(OrderBy);

                using var conn = context.Create();
                var lines = await conn.QueryAsync<PIAndExpenses>(sqlWithInvNo.ToString(), new { InvoiceNo = invNo });
                return Results.Ok(lines);
            }
            );

            app.MapGet("/api/PurchInvAndExpenses/betweenDates", async (DynamicsDBContext context, [AsParameters] DateInterval betweenDates) =>
            {
                StringBuilder sqlWithInvNo = new(sqlPurchInvoicesAndExpenses);
                sqlWithInvNo.AppendLine($"where InvCTE.PostingDate between @fromDate and @toDate");
                sqlWithInvNo.AppendLine(OrderBy);

                using var conn = context.Create();
                var lines = await conn.QueryAsync<PIAndExpenses>(sqlWithInvNo.ToString(), new { fromDate = betweenDates.DateFrom, toDate = betweenDates.DateTo });
                return Results.Ok(lines);
            }
            );

            app.MapGet("/api/BankAccountLedgerItems", async (DynamicsDBContext context, [AsParameters] LedgerQuery query) =>
            {
                string sqlBankAccountLedgerEntries = $@"
select
	[Entry No_] as EntryNo, 
	[Document Type] as DocumentType,
	[Document No_] as DocumentNo,
	[Bank Account No_] as BankAccountNo, 
	[Description],
	[Posting Date] as PostingDate, 
	CASE [Currency Code] 
		WHEN '' THEN 'EUR'
		ELSE [Currency Code]
	END as CurrencyCode,
	Amount, 
	[Remaining Amount] as RemainingAmount,
	[Amount (LCY)] as AmountLCY,
	[Open],
	[Debit Amount] as DebitAmount,
    [Credit Amount] as CreditAmount,
	Sum(Amount) OVER (PARTITION BY [Bank Account No_] ORDER BY [Posting Date], [Entry No_]) AS RunningTotalAmount,
	Sum([Amount (LCY)]) OVER (PARTITION BY [Bank Account No_] ORDER BY [Posting Date], [Entry No_]) AS RunningTotalAmountLCY
from
	[TRANSPACNAV21].[dbo].[CDF$Bank Account Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where
	[Bank Account No_] in ('00010588253', '00010043333', '00018000262', '00010058708', '11210EUR', '11210GBP', '11210SEK', '11210USD', '11220EUR', '11220GBP', '11220SEK', '11220USD' @MoreAccntList)
    AND [Posting Date] <= '{query.ToDate:yyyy-MM-dd}'
order by
    [Posting Date] Desc
";
                try
                {
                    using var conn = context.Create();
                    string sql = "";
                    if (query.ExtraAccList == null || query.ExtraAccList.Length == 0)
                        sql = sqlBankAccountLedgerEntries.Replace("@MoreAccntList", "");
                    else
                        sql = sqlBankAccountLedgerEntries.Replace("@toDate", query.ToDate.ToString("yyyy/MM/dd")).Replace("@MoreAccntList", ", " + query.ExtraAccList);
                    var lines = await conn.QueryAsync<BankAccountLedgerEntries>(sql);
                    return Results.Ok(lines);
                    
                }
                catch (Exception ex)
                {
                    return Results.Problem(context.connString + "; " + ex.Message, statusCode: 500);
                }
                
            }
            );

            app.MapGet("/api/CustomerInvoices", async (DynamicsDBContext context, [AsParameters] CustInvQuery query) =>
            {
                using var conn = context.Create();

                string DefCurSQL = $"select [LCY Code] from [{query.Company}$General Ledger Setup$437dbf0e-84ff-417a-965d-ed2bb9650972]"; 
                string? DefCur = conn.ExecuteScalar<string>(DefCurSQL);
                if (DefCur == null)
                {
                    return Results.Problem($"Default currency for {query.Company} could not be retrieved.", statusCode: 500);
                }

                StringBuilder sql = new($@"
select
    cle.[Document No_] as InvoiceNo,
    cle.[Customer No_] as CustomerNo,
    cust.[Name] as CustomerName,
    cast(cle.[Posting Date] as Date) as InvoiceDate,
    cle.[Description],
    cast(cle.[Due Date] as Date) as DueDate,
    (select SUM(Amount) from [{query.Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
     where [Entry Type] = 1
            and [Cust_ Ledger Entry No_] = cle.[Entry No_] 
            and [Posting Date] = cle.[Posting Date]) as Amount,
    case cle.[Currency Code]    
        when '' then '{DefCur}'
        else cle.[Currency Code]
    end As Cur,
    (select SUM([Amount (LCY)]) from [{query.Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
     where [Entry Type] = 1
        and [Cust_ Ledger Entry No_] = cle.[Entry No_]
        and [Posting Date] = cle.[Posting Date]) as AmountLCY,
    cast(ClosedBy.[Posting Date] as Date) as PaymentDate,
    ClosedBy.[Document No_] as PaymentDocNo
from 
    [dbo].[{query.Company}$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] cle
    left join [dbo].[{query.Company}$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ClosedBy 
        on ClosedBy.[Entry No_] = cle.[Closed by Entry No_]
    inner join [{query.Company}$Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] cust on cle.[Customer No_] = cust.[No_]
where
    cle.[Posting Date] BETWEEN '{query.FromDate:yyyy-MM-dd}' AND '{query.ToDate:yyyy-MM-dd}'
    AND cle.[Document Type] = 2");

                var lines = await conn.QueryAsync<CustomerInvoices>(sql.ToString());
                return Results.Ok(lines);
            }
            );

            app.MapGet("/api/LateCustomerInvoices", async (DynamicsDBContext context, string Company) =>
            {
                using var conn = context.Create();

                string DefCurSQL = $"select [LCY Code] from [{Company}$General Ledger Setup$437dbf0e-84ff-417a-965d-ed2bb9650972]";
                string? DefCur = conn.ExecuteScalar<string>(DefCurSQL);
                if (DefCur == null)
                {
                    return Results.Problem($"Default currency for {Company} could not be retrieved.", statusCode: 500);
                }

                StringBuilder sql = new($@"
select
	cle.[Document No_] as InvoiceNo,
	cle.[Customer No_] as CustomerNo,
	cust.[Name] as CustomerName,
	cast(cle.[Posting Date] as Date) as InvoiceDate,
	cle.[Description],
	cast(cle.[Due Date] as Date) as DueDate,
	(select SUM(Amount) from [{Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
	                    where [Entry Type] = 1 
						and [Cust_ Ledger Entry No_] = cle.[Entry No_] 
						and [Posting Date] = cle.[Posting Date]) as Amount,
	CASE cle.[Currency Code] 
	    WHEN '' THEN '{DefCur}'
		ELSE cle.[Currency Code]
	END As Cur,
	(select SUM([Amount (LCY)]) from [{Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
	                            where [Entry Type] = 1
								and [Cust_ Ledger Entry No_] = cle.[Entry No_]
								and [Posting Date] = cle.[Posting Date]) as AmountLCY,
    DATEDIFF(DAY, cle.[Due Date], GETDATE()) as LateDays
from
	[{Company}$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] cle
	inner join [{Company}$Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] cust on cle.[Customer No_] = cust.[No_]
where 
	[Open] = 1
	and (cle.[Document Type] = 2 OR cle.[Document Type] = 3)");

                var lines = await conn.QueryAsync<CustomerInvoices>(sql.ToString());
                return Results.Ok(lines);
            }
            );

            app.MapGet("/api/VendorInvoices", async (DynamicsDBContext context, [AsParameters] VendorInvQuery query) =>
            {
                using var conn = context.Create();

                string DefCurSQL = $"select [LCY Code] from [{query.Company}$General Ledger Setup$437dbf0e-84ff-417a-965d-ed2bb9650972]";
                string? DefCur = conn.ExecuteScalar<string>(DefCurSQL);
                if (DefCur == null)
                {
                    return Results.Problem($"Default currency for {query.Company} could not be retrieved.", statusCode: 500);
                }

                StringBuilder sql = new($@"
select
	vle.[Document No_] as InvoiceNo,
	vle.[Vendor No_] as VendorNo,
	vendor.[Name] as VendorName,
	cast(vle.[Posting Date] as Date) as InvoiceDate,
	vle.[Description],
	cast(vle.[Due Date] as Date) as DueDate,
	vle.[External Document No_] as ExtInvNo,
	(select SUM(-1 * Amount) from [{query.Company}$Detailed Vendor Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
	                    where [Ledger Entry Amount] = 1 
						      and [Vendor Ledger Entry No_] = vle.[Entry No_] 
						      and [Posting Date] = vle.[Posting Date]) as Amount,
	CASE vle.[Currency Code] 
		WHEN '' THEN '{DefCur}' 
		ELSE vle.[Currency Code]
	END As Cur,
	(select SUM(-1 * [Amount (LCY)]) from [{query.Company}$Detailed Vendor Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
	                    where [Ledger Entry Amount] = 1 
						      and [Vendor Ledger Entry No_] = vle.[Entry No_] 
						      and [Posting Date] = vle.[Posting Date]) as AmountLCY,
	cast(ClosedBy.[Posting Date] as Date) as PaymentDate,
	ClosedBy.[Document No_] as PaymentDocNo
from 
	[dbo].[{query.Company}$Vendor Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] vle
	left join [dbo].[{query.Company}$Vendor Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ClosedBy on ClosedBy.[Entry No_] = vle.[Closed by Entry No_]
	inner join [dbo].[{query.Company}$Vendor$437dbf0e-84ff-417a-965d-ed2bb9650972] vendor on vle.[Vendor No_] = vendor.[No_]
where 
	(vle.[Document Type] = 2 OR vle.[Document Type] = 3)
    AND vle.[Posting Date] BETWEEN '{query.FromDate:yyyy-MM-dd}' AND '{query.ToDate:yyyy-MM-dd}'");

                if (query.OnlyOpen.HasValue && query.OnlyOpen.Value)
                    sql.AppendLine("AND vle.[Open] = 1");
                
                var lines = await conn.QueryAsync<VendorInvoices>(sql.ToString());
                return Results.Ok(lines);
            }
            );

            app.MapGet("/api/GetEnabledComps", async (DynamicsDBContext context, string UserName) =>
            {
                using var conn = context.Create();
                string sql = $"select [AccessTo] from [SatV2].[Webpages].[WebUserRights] where [UserName] = '{UserName}'";
                var lines = await conn.QueryAsync<string>(sql);
                return Results.Ok(lines);
            });

            app.MapGet("/api/CustAgedInvoices", async(DynamicsDBContext context, string Company) =>
            {
                using var conn = context.Create();

                string DefCurSQL = $"select [LCY Code] from [{Company}$General Ledger Setup$437dbf0e-84ff-417a-965d-ed2bb9650972]";
                string? DefCur = conn.ExecuteScalar<string>(DefCurSQL);
                if (DefCur == null)
                {
                    return Results.Problem($"Default currency for {Company} could not be retrieved.", statusCode: 500);
                }

                StringBuilder sql = new($@"
WITH EntryDetails AS (
    SELECT
        cle.[Entry No_] AS EntryNo,
        cle.[Document No_] AS InvoiceNo,
        cle.[Customer No_] AS CustomerNo,
        cust.[Name] AS CustomerName,
        CAST(cle.[Posting Date] AS DATE) AS InvoiceDate,
        cle.[Description] AS Descr,
        CAST(cle.[Due Date] AS DATE) AS DueDate,
        CASE cle.[Currency Code]
            WHEN '' THEN '{DefCur}'
            ELSE cle.[Currency Code]
        END AS Cur,
        DATEDIFF(DAY, cle.[Due Date], GETDATE()) AS LateDays,
        (
            SELECT SUM(Amount)
            FROM [{Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] dcle
            WHERE dcle.[Entry Type] = 1
              AND dcle.[Cust_ Ledger Entry No_] = cle.[Entry No_]
              AND dcle.[Posting Date] = cle.[Posting Date]
        ) AS Amount,

        (
            SELECT SUM([Amount (LCY)])
            FROM [{Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] dcle
            WHERE dcle.[Entry Type] = 1
              AND dcle.[Cust_ Ledger Entry No_] = cle.[Entry No_]
              AND dcle.[Posting Date] = cle.[Posting Date]
        ) AS AmountLCY

    FROM
        [{Company}$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] cle
        INNER JOIN [{Company}$Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] cust
            ON cle.[Customer No_] = cust.[No_]
    WHERE
        cle.[Open] = 1
        AND cle.[Document Type] IN (2, 3)
)

SELECT
    InvoiceNo,
    CustomerNo,
    CustomerName,
    InvoiceDate,
    Descr,
    DueDate,
    Cur,
    LateDays,

    CASE WHEN LateDays < 30 THEN Amount ELSE NULL END AS [Acc30],
    CASE WHEN LateDays BETWEEN 30 AND 60 THEN Amount ELSE NULL END AS [Acc3060],
    CASE WHEN LateDays BETWEEN 61 AND 90 THEN Amount ELSE NULL END AS [Acc6090],
	CASE WHEN LateDays > 90 THEN Amount ELSE NULL END AS [Acc90],

	CASE WHEN LateDays < 30 THEN AmountLCY ELSE NULL END AS [LCY30],
    CASE WHEN LateDays BETWEEN 30 AND 60 THEN AmountLCY ELSE NULL END AS [LCY3060],
    CASE WHEN LateDays BETWEEN 61 AND 90 THEN AmountLCY ELSE NULL END AS [LCY6090],
	CASE WHEN LateDays > 90 THEN AmountLCY ELSE NULL END AS [LCY90]
FROM EntryDetails");

                var lines = await conn.QueryAsync<CustAgedInvoices>(sql.ToString());
                return Results.Ok(lines);
            }
            );
        }
    }
}
