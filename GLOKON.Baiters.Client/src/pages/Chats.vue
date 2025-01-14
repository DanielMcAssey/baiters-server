<script setup lang="ts">
import { inject, onMounted, ref } from "vue";
import {useToast} from "primevue/usetoast";
import {useConfirm} from "primevue/useconfirm";
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import ButtonGroup from 'primevue/buttongroup';
import type { AxiosInstance } from 'axios';
const confirm = useConfirm();
const toast = useToast();

const isLoading = ref(false);
const $http = inject<AxiosInstance>('axios') as AxiosInstance;
/* eslint-disable  @typescript-eslint/no-explicit-any */
const results = ref<any[]>([]);
/* eslint-disable  @typescript-eslint/no-explicit-any */
const users = ref<any[]>([]);

function fetchData(): void {
  isLoading.value = true;
  $http.get(`/api/chats/`)
    .then((response) => {
      if (response.data) {
        results.value = response.data;
      } else {
        results.value = [];
      }
    })
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t load players');
      toast.add({
        severity: 'error',
        summary: 'Problem Loading Players',
        detail: 'There was a problem loading players',
        life: 10000,
      });
    })
    .finally(() => {
      isLoading.value = false;
    });
}

function fetchUsers(): void {
  $http.get(`/api/users/`)
    .then((response) => {
      if (response.data) {
        const responseData = response.data;
        const items = [];

        for (const id of Object.keys(responseData)) {
          items.push({
            id: id,
            ...responseData[id],
          });
        }

        users.value = items;
      } else {
        users.value = [];
      }
    })
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t load players');
      toast.add({
        severity: 'error',
        summary: 'Problem Loading Players',
        detail: 'There was a problem loading players',
        life: 10000,
      });
    });
}

function sendMessage(steamId?: string): void {
  // TODO: Ask for message then send it
}

onMounted(() => {
  fetchData();
  fetchUsers();
});
</script>
<template>
  <div class="mx-auto sm:max-w-7xl sm:px-6 lg:px-8">
    <div class="text-xl font-bold leading-tight mb-4 px-4">
      <i class="fas fa-message mr-2"></i>Chat Messages<span v-if="results" class="ml-1">({{ results.length }})</span>
    </div>
    <div class="bg-white overflow-hidden shadow sm:rounded-lg">
      <div class="p-4 flex justify-between sm:justify-end gap-2 flex-wrap">
        <p class="text-sm mr-auto">
          Here you will the server chat
        </p>
      </div>
      <DataTable :value="results" data-key="id" paginator :row-hover="true" :loading="isLoading"
                 :rows="50" :rowsPerPageOptions="[25, 50, 100]" stripedRows responsiveLayout="scroll">
        <template #empty>
          No chats found.
        </template>
        <template #loading>
          Loading chats. Please wait&hellip;
        </template>
        <Column field="senderName" header="Name" :sortable="true"></Column>
        <Column field="message" header="Message" :sortable="true"></Column>
        <Column field="senderId" header="Steam ID" :sortable="true"></Column>
      </DataTable>
    </div>
  </div>
</template>
