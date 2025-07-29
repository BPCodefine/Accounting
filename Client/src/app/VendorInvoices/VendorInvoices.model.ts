export interface VendorInvoices {
  invoiceNo: string,
  vendorNo: string,
  vendorName: string,
  description: string,
  extInvNo: string,
  invoiceDate: Date,
  dueDate: Date,
  amount: number,
  cur: string,
  amountLCY: number,
  paymentDate: string,
  paymentDocNo: string
}
