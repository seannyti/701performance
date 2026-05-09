<script setup lang="ts">
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
</script>

<template>
  <Toast position="top-right" />
  <ConfirmDialog />
  <RouterView />
</template>

<style>
body {
  background: #0a0a0a;
  color: #ffffff;
}

/* ── Excel-style grid tables ──────────────────────────────────────────────── */
.pp-table .p-datatable-table {
  border-collapse: collapse;
}

.pp-table .p-datatable-table-container {
  border: 1px solid #2a2a2a;
  overflow: hidden;
}

/* Header cells */
.pp-table .p-datatable-thead > tr > th {
  background: #161616 !important;
  padding: 0.45rem 0.75rem !important;
  font-size: 0.68rem !important;
  font-weight: 700 !important;
  text-transform: uppercase !important;
  letter-spacing: 0.06em !important;
  color: #666 !important;
  white-space: nowrap;
  border-right: 1px solid #2e2e2e !important;
  border-bottom: 2px solid #333 !important;
}
.pp-table .p-datatable-thead > tr > th:last-child {
  border-right: none !important;
}

/* Body cells */
.pp-table .p-datatable-tbody > tr > td {
  padding: 0.4rem 0.75rem !important;
  font-size: 0.82rem !important;
  border-right: 1px solid #282828 !important;
  border-bottom: 1px solid #242424 !important;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.pp-table .p-datatable-tbody > tr > td:last-child {
  border-right: none !important;
}
.pp-table .p-datatable-tbody > tr:last-child > td {
  border-bottom: none !important;
}

/* Zebra striping */
.pp-table .p-datatable-tbody > tr:nth-child(even) > td {
  background: #111 !important;
}
.pp-table .p-datatable-tbody > tr:nth-child(odd) > td {
  background: #0d0d0d !important;
}

/* Row hover */
.pp-table .p-datatable-tbody > tr:hover > td {
  background: rgba(229, 57, 53, 0.09) !important;
  cursor: pointer;
}
</style>
