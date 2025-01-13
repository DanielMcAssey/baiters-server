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

function fetchData(): void {
  isLoading.value = true;
  $http.get(`/api/actors/`)
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

        results.value = items;
      } else {
        results.value = [];
      }
    })
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t load actors');
      toast.add({
        severity: 'error',
        summary: 'Problem Loading Actors',
        detail: 'There was a problem loading actors',
        life: 10000,
      });
    })
    .finally(() => {
      isLoading.value = false;
    });
}

function spawnActor(event: Event, type: string): void {
  confirm.require({
    target: event.currentTarget as HTMLElement | undefined,
    header: 'Spawn Actor',
    message: 'Are you sure you want to spawn this actor?',
    icon: 'fas fa-exclamation-triangle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      icon: 'fas fa-ban',
      outlined: true
    },
    acceptProps: {
      label: 'Spawn',
      severity: 'success',
      icon: 'fas fa-wrench',
    },
    accept: () => {
      $http.post(`/api/actors/spawn/${type}`)
        .catch((err: Error) => {
          console.error(err, 'Couldn\'t spawn actor');
          toast.add({
            severity: 'error',
            summary: 'Problem Spawning Actor',
            detail: 'There was a problem spawning the actor',
            life: 10000,
          });
        });
    },
  });
}

function removeActor(event: Event, id: number): void {
  confirm.require({
    target: event.currentTarget as HTMLElement | undefined,
    header: 'Remove Actor',
    message: 'Are you sure you want to remove this actor?',
    icon: 'fas fa-exclamation-triangle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      icon: 'fas fa-ban',
      outlined: true
    },
    acceptProps: {
      label: 'Remove',
      severity: 'danger',
      icon: 'fas fa-trash',
    },
    accept: () => {
      $http.delete(`/api/actors/${id}`)
        .catch((err: Error) => {
          console.error(err, 'Couldn\'t remove actor');
          toast.add({
            severity: 'error',
            summary: 'Problem Removing Actor',
            detail: 'There was a problem removing the actor',
            life: 10000,
          });
        });
    },
  });
}

onMounted(() => {
  fetchData();
});
</script>
<template>
  <div class="mx-auto sm:max-w-7xl sm:px-6 lg:px-8">
    <div class="text-xl font-bold leading-tight mb-4 px-4">
      <i class="fas fa-sitemap mr-2"></i>Actors
    </div>
    <div class="bg-white overflow-hidden shadow sm:rounded-lg">
      <div class="p-4 flex justify-between sm:justify-end gap-2 flex-wrap">
        <p class="text-sm mr-auto">
          Here you will find all actors currently spawned
        </p>
        <Button icon="fas fa-wrench"
                label="Spawn Actor"
                severity="info"
                @click="spawnActor($event, 'ambient_bird')" />
      </div>
      <DataTable :value="results" data-key="id" paginator :row-hover="true" :loading="isLoading"
                 :rows="50" :rowsPerPageOptions="[25, 50, 100]" stripedRows responsiveLayout="scroll">
        <template #empty>No users found.</template>
        <template #loading>Loading users. Please wait&hellip;</template>
        <Column field="id" header="ID" :sortable="true"></Column>
        <Column field="type" header="Type" :sortable="true"></Column>
        <Column field="spawnTime" header="Spawned At" :sortable="true"></Column>
        <Column field="zone" header="Zone" :sortable="true"></Column>
        <Column style="white-space:nowrap;text-align:right;">
          <template #body="slotProps">
            <ButtonGroup>
              <Button icon="fas fa-trash" severity="danger" v-tooltip.bottom="'Remove Actor'" @click="removeActor($event, slotProps.data.id)" />
            </ButtonGroup>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
