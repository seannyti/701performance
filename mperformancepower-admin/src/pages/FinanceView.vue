<script setup lang="ts">
import { onMounted } from 'vue'
import { useOrderStore } from '@/stores/order.store'
import AdminShell from '@/components/layout/AdminShell.vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

const store = useOrderStore()

onMounted(() => store.fetchFinanceStats())

function formatCurrency(n: number) {
  if (n >= 1_000_000) return `$${(n / 1_000_000).toFixed(1)}M`
  if (n >= 1_000) return `$${(n / 1_000).toFixed(1)}K`
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)
}

function formatFull(n: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)
}

function maxRevenue(items: { revenue: number }[]) {
  return Math.max(...items.map(i => i.revenue), 1)
}

const methodColors: Record<string, string> = {
  Cash: '#22c55e',
  Financed: '#3b82f6',
  TradeIn: '#a855f7',
}
</script>

<template>
  <AdminShell>
    <div class="finance">
      <div class="page-header">
        <h1>Finance Overview</h1>
        <p>Revenue, sales, and payment breakdowns.</p>
      </div>

      <div v-if="!store.financeStats" class="finance__loading">Loading...</div>

      <template v-else>
        <!-- KPI cards -->
        <div class="kpi-row">
          <div class="kpi-card kpi-card--red">
            <div class="kpi-card__label">Total Revenue</div>
            <div class="kpi-card__value">{{ formatCurrency(store.financeStats.totalRevenue) }}</div>
          </div>
          <div class="kpi-card">
            <div class="kpi-card__label">Units Sold</div>
            <div class="kpi-card__value">{{ store.financeStats.totalUnitsSold }}</div>
          </div>
          <div class="kpi-card">
            <div class="kpi-card__label">Avg Sale Price</div>
            <div class="kpi-card__value">{{ formatCurrency(store.financeStats.avgSalePrice) }}</div>
          </div>
          <div class="kpi-card kpi-card--green">
            <div class="kpi-card__label">This Month Revenue</div>
            <div class="kpi-card__value">{{ formatCurrency(store.financeStats.thisMonthRevenue) }}</div>
          </div>
          <div class="kpi-card">
            <div class="kpi-card__label">This Month Units</div>
            <div class="kpi-card__value">{{ store.financeStats.thisMonthUnits }}</div>
          </div>
        </div>

        <!-- Monthly revenue chart -->
        <div class="panel">
          <h3 class="panel__title">Monthly Revenue — Last 12 Months</h3>
          <div v-if="store.financeStats.monthlyRevenue.length === 0" class="panel__empty">No data yet.</div>
          <div v-else class="bar-chart">
            <div v-for="m in store.financeStats.monthlyRevenue" :key="m.label" class="bar-chart__col">
              <div class="bar-chart__value">{{ formatCurrency(m.revenue) }}</div>
              <div class="bar-chart__bar" :style="{ height: `${(m.revenue / maxRevenue(store.financeStats!.monthlyRevenue)) * 100}%` }" />
              <div class="bar-chart__label">{{ m.label }}</div>
            </div>
          </div>
        </div>

        <!-- Bottom row -->
        <div class="detail-row">
          <!-- Revenue by category -->
          <div class="panel">
            <h3 class="panel__title">Revenue by Category</h3>
            <div v-if="store.financeStats.revenueByCategory.length === 0" class="panel__empty">No data yet.</div>
            <DataTable v-else :value="store.financeStats.revenueByCategory" size="small">
              <Column field="category" header="Category" />
              <Column field="units" header="Units" />
              <Column header="Revenue">
                <template #body="{ data }">
                  <span style="font-weight:600;">{{ formatFull(data.revenue) }}</span>
                </template>
              </Column>
            </DataTable>
          </div>

          <!-- Payment method breakdown -->
          <div class="panel">
            <h3 class="panel__title">Payment Methods</h3>
            <div v-if="store.financeStats.paymentMethodBreakdown.length === 0" class="panel__empty">No data yet.</div>
            <div v-else class="method-list">
              <div v-for="m in store.financeStats.paymentMethodBreakdown" :key="m.method" class="method-row">
                <div class="method-row__left">
                  <span class="method-dot" :style="{ background: methodColors[m.method] ?? '#555' }" />
                  <span class="method-name">{{ m.method }}</span>
                  <span class="method-count">{{ m.count }} sale{{ m.count !== 1 ? 's' : '' }}</span>
                </div>
                <span class="method-revenue">{{ formatFull(m.revenue) }}</span>
              </div>
            </div>
          </div>

          <!-- Top lenders -->
          <div class="panel">
            <h3 class="panel__title">Top Lenders</h3>
            <div v-if="store.financeStats.topLenders.length === 0" class="panel__empty">No financed deals yet.</div>
            <DataTable v-else :value="store.financeStats.topLenders" size="small">
              <Column field="lender" header="Lender" />
              <Column field="count" header="Deals" />
              <Column header="Total Financed">
                <template #body="{ data }">
                  <span style="font-weight:600;">{{ formatFull(data.totalFinanced) }}</span>
                </template>
              </Column>
            </DataTable>
          </div>
        </div>
      </template>
    </div>
  </AdminShell>
</template>

<style scoped>
.finance { display: flex; flex-direction: column; gap: 20px; }

.page-header h1 { font-size: 1.5rem; font-weight: 700; }
.page-header p { font-size: 0.875rem; color: #9a9a9a; margin-top: 4px; }
.finance__loading { color: #555; font-size: 0.9rem; }

.kpi-row { display: grid; grid-template-columns: repeat(5, 1fr); gap: 12px; }
.kpi-card {
  background: #111; border: 1px solid #222; border-radius: 10px;
  padding: 18px 20px; border-left: 4px solid #333;
}
.kpi-card--red  { border-left-color: #e63946; }
.kpi-card--green { border-left-color: #22c55e; }
.kpi-card__label { font-size: 0.75rem; color: #777; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 8px; }
.kpi-card__value { font-size: 1.75rem; font-weight: 700; color: #f0f0f0; }

.panel { background: #111; border: 1px solid #222; border-radius: 10px; padding: 20px; }
.panel__title {
  font-size: 0.875rem; font-weight: 700; color: #9a9a9a;
  text-transform: uppercase; letter-spacing: 0.04em; margin: 0 0 16px 0;
}
.panel__empty { color: #555; font-size: 0.875rem; }

.bar-chart {
  display: flex; align-items: flex-end; gap: 8px;
  height: 160px; padding-bottom: 28px; position: relative;
}
.bar-chart__col {
  flex: 1; display: flex; flex-direction: column; align-items: center;
  gap: 4px; height: 100%; justify-content: flex-end; position: relative;
}
.bar-chart__value { font-size: 0.6rem; color: #555; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 100%; }
.bar-chart__bar { width: 100%; background: #e63946; border-radius: 4px 4px 0 0; min-height: 2px; transition: height 0.3s ease; }
.bar-chart__label { position: absolute; bottom: -24px; font-size: 0.6rem; color: #555; white-space: nowrap; }

.detail-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; align-items: start; }

.method-list { display: flex; flex-direction: column; gap: 12px; }
.method-row { display: flex; align-items: center; justify-content: space-between; }
.method-row__left { display: flex; align-items: center; gap: 8px; }
.method-dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.method-name { font-size: 0.875rem; color: #f0f0f0; }
.method-count { font-size: 0.75rem; color: #555; }
.method-revenue { font-size: 0.875rem; font-weight: 600; color: #f0f0f0; }
</style>
