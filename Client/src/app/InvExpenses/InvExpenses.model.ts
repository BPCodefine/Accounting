export interface InvExpensesModel {
  invoiceNo: string;
  buyFromID: string;
  buyFromName: string;
  payToID: string;
  payToName: string;
  yourRefOrderID: string;
  postingDate: string;
  invLineNo: string;
  articleNo: string;
  articleName: string;
  invoicedQty: number;
  puaCost: number;
  seafreightCost: number;
  dutyCost: number;
  storageCost: number;
  outfreightCost: number;
  inlandFreightCost: number;
  smallCost: number;
  uVYarnCost: number;
  qualDiscountCost: number;
}
