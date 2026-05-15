export interface OrderRequest {
    fullName: string
    phone:string
    address:string
    remark: string
}

export interface OrderEntity {
  id: number;
  totalAmount: number;
  status: number;
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
