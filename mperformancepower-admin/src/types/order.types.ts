export type OrderStatus = 'Pending' | 'Completed' | 'Delivered' | 'Cancelled'
export type PaymentMethod = 'Cash' | 'Financed' | 'TradeIn'

export interface Order {
  id: number
  vehicleId: number
  vehicleName: string
  inquiryId: number | null
  customerName: string
  customerEmail: string
  customerPhone: string
  salePrice: number
  paymentMethod: PaymentMethod
  downPayment: number | null
  loanAmount: number | null
  loanTermMonths: number | null
  apr: number | null
  lenderName: string | null
  status: OrderStatus
  notes: string | null
  createdAt: string
  deliveredAt: string | null
}

export interface OrderListItem {
  id: number
  vehicleId: number
  vehicleName: string
  customerName: string
  customerEmail: string
  salePrice: number
  paymentMethod: PaymentMethod
  status: OrderStatus
  createdAt: string
}

export interface CreateOrderDto {
  vehicleId: number
  inquiryId?: number | null
  customerName: string
  customerEmail: string
  customerPhone: string
  salePrice: number
  paymentMethod: PaymentMethod
  downPayment?: number | null
  loanAmount?: number | null
  loanTermMonths?: number | null
  apr?: number | null
  lenderName?: string | null
  status: OrderStatus
  notes?: string | null
}

export type UpdateOrderDto = Omit<CreateOrderDto, 'vehicleId' | 'inquiryId'>

export interface FinanceStats {
  totalRevenue: number
  totalUnitsSold: number
  avgSalePrice: number
  thisMonthRevenue: number
  thisMonthUnits: number
  revenueByCategory: { category: string; revenue: number; units: number }[]
  paymentMethodBreakdown: { method: string; count: number; revenue: number }[]
  monthlyRevenue: { year: number; month: number; label: string; revenue: number; units: number }[]
  topLenders: { lender: string; count: number; totalFinanced: number }[]
}
