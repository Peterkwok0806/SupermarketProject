export interface OrderRequest {
    fullName: string
    phone:string
    address:string
    remark: string
}

export interface OrderEntity {
  id: number;
  totalAmount: number;
  status: OrderStatus;
  fullName: string;
  phone: string;
  address: string;
  remark?: string;
  createdAt: Date;
  orderItems:OrderItem[];
}

export interface OrderItem{
productId:number;
productName: string;
productPhoto: string;
quantity: number;
unitPrice:number;
}

export enum OrderStatus {
  Pending = 0,
  Paid = 1,
  Processing = 2,
  Shipped = 3,
  Completed = 4,
  Cancelled = 5
}