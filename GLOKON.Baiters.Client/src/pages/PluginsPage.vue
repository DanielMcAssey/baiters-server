<script setup lang="ts">
import { inject, onMounted, ref } from "vue";
import {useToast} from "primevue/usetoast";
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import type { AxiosInstance } from 'axios';
const toast = useToast();

const isLoading = ref(false);
const $http = inject<AxiosInstance>('axios') as AxiosInstance;
/* eslint-disable  @typescript-eslint/no-explicit-any */
const results = ref<any[]>([]);

function fetchData(): void {
  isLoading.value = true;
  $http.get(`/api/plugins/`)
    .then((response) => {
      if (response.data) {
        results.value = response.data;
      } else {
        results.value = [];
      }
    })
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t load plugins');
      toast.add({
        severity: 'error',
        summary: 'Problem Loading Plugins',
        detail: 'There was a problem loading plugins',
        life: 10000,
      });
    })
    .finally(() => {
      isLoading.value = false;
    });
}

onMounted(() => {
  fetchData();
});
</script>
<template>
  <div class="mx-auto sm:max-w-7xl sm:px-6 lg:px-8">
    <div class="text-xl font-bold leading-tight mb-4 px-4">
      <i class="fas fa-plug mr-2"></i>Plugins
    </div>
    <div class="bg-white overflow-hidden shadow sm:rounded-lg">
      <div class="p-4 flex justify-between sm:justify-end gap-2 flex-wrap">
        <p class="text-sm mr-auto">
          Here you will find all plugins enabled on the server
        </p>
      </div>
      <DataTable :value="results" data-key="id" paginator :row-hover="true" :loading="isLoading"
                 :rows="50" :rowsPerPageOptions="[25, 50, 100]" stripedRows responsiveLayout="scroll">
        <template #empty>No plugins enabled.</template>
        <template #loading>Loading plugins. Please wait&hellip;</template>
        <Column field="name" header="Name" :sortable="true"></Column>
        <Column field="description" header="Description" :sortable="true"></Column>
        <Column field="version" header="Version" :sortable="true"></Column>
        <Column field="author" header="Author" :sortable="true"></Column>
      </DataTable>
    </div>
  </div>
</template>
