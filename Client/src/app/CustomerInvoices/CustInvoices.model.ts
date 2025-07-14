export interface CustInvoices {
  invoiceNo: string;
  customerNo: string;
  customerName: string;
  description: string;
  invoiceDate: string;
  dueDate: string;
  amount: number;
  cur: string;
  amountLCY: number;
  paymentDate: string;
  paymentDocNo: string;
  lateDays: number;
}
