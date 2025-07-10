export interface BankAccountLedgerModel {
  entryNo: string,
	documentType: string,
	documentNo: string,
	bankAccountNo: string,
	description: string,
	postingDate: Date,
	currencyCode: string,
	amount: number,
	remainingAmount: number,
	amountLCY: number,
	open: boolean,
	debitAmount: number,
  creditAmount: number,
	runningTotalAmount: number,
	runningTotalAmountLCY: number
}
