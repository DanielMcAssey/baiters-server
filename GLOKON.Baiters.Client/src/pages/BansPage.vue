<script setup lang="ts">
import { inject, onMounted, ref } from "vue";
import {useToast} from "primevue/usetoast";
import {useConfirm} from "primevue/useconfirm";
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
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
  $http.get(`/api/bans/`)
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

function removeBan(event: Event, steamId: number): void {
  confirm.require({
    target: event.currentTarget as HTMLElement | undefined,
    header: 'Remove Ban',
    message: 'Are you sure you want to remove this ban?',
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
      $http.delete(`/api/bans/${steamId}`)
        .catch((err: Error) => {
          console.error(err, 'Couldn\'t remove ban');
          toast.add({
            severity: 'error',
            summary: 'Problem Removing Ban',
            detail: 'There was a problem removing the ban',
            life: 10000,
          });
        })
        .finally(() => fetchData());
    },
  });
}

const isCreateBanOpen = ref(false);
const createBanFor = ref<string>('');
const createBanReason = ref<string>('');

function openCreateBan(): void {
  isCreateBanOpen.value = true;
}

function closeCreateBan(): void {
  isCreateBanOpen.value = false;
  createBanFor.value = '';
  createBanReason.value = '';
}

function createBan(): void {
  $http.post(`/api/bans/${createBanFor.value}`, {
    reason: createBanReason.value,
  })
    .then(() => closeCreateBan())
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t ban player');
      toast.add({
        severity: 'error',
        summary: 'Problem Setting Banning Player',
        detail: 'There was a problem banning the player',
        life: 10000,
      });
    })
    .finally(() => fetchData());
}

onMounted(() => {
  fetchData();
});
</script>
<template>
  <div class="mx-auto sm:max-w-7xl sm:px-6 lg:px-8">
    <div class="text-xl font-bold leading-tight mb-4 px-4">
      <i class="fas fa-ban mr-2"></i>Bans<span v-if="results" class="ml-1">({{ results.length }})</span>
    </div>
    <div class="bg-white overflow-hidden shadow sm:rounded-lg">
      <div class="p-4 flex justify-between sm:justify-end gap-2 flex-wrap">
        <p class="text-sm mr-auto">
          Here you will find all actors currently spawned
        </p>
        <Button icon="fas fa-rotate-right"
                severity="primary"
                :loading="isLoading"
                @click="fetchData()" />
        <Button icon="fas fa-plus"
                label="Ban SteamID"
                severity="info"
                @click="openCreateBan()" />
      </div>
      <DataTable :value="results" data-key="id" paginator :row-hover="true" :loading="isLoading"
                 :rows="50" :rowsPerPageOptions="[25, 50, 100]" stripedRows responsiveLayout="scroll">
        <template #empty>No users found.</template>
        <template #loading>Loading users. Please wait&hellip;</template>
        <Column field="createdAt" header="At" :sortable="true"></Column>
        <Column field="steamId" header="Steam ID" :sortable="true"></Column>
        <Column field="playerName" header="Player Name" :sortable="true"></Column>
        <Column field="reason" header="Reason" :sortable="true"></Column>
        <Column style="white-space:nowrap;text-align:right;">
          <template #body="slotProps">
            <ButtonGroup>
              <Button icon="fas fa-trash" severity="danger" v-tooltip.bottom="'Remove Ban'" @click="removeBan($event, slotProps.data.steamId)" />
            </ButtonGroup>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
  <Dialog v-model:visible="isCreateBanOpen" modal header="Ban Player" @close="closeCreateBan" :style="{ width: '30rem' }">
    <form @submit.prevent="createBan">
      <div class="grid grid-cols-6 gap-2">
        <div class="col-span-6">
          <label for="ban_steam_id" class="font-semibold">Ban SteamID</label>
          <InputText id="ban_steam_id"
                     type="text"
                     class="w-full mt-1"
                     name="ban_steam_id"
                     v-model="createBanFor"
                     placeholder="SteamID of the player to ban"
                     required
                     autocomplete="off" />
        </div>
        <div class="col-span-6">
          <label for="ban_reason" class="font-semibold">Reason (Optional)</label>
          <Textarea id="ban_reason"
                    class="w-full mt-1"
                    name="ban_reason"
                    v-model="createBanReason"
                    placeholder="An optional reason for banning the SteamID"
                    required
                    rows="5" />
        </div>
        <div class="col-span-6 flex justify-end gap-2">
          <Button type="button" icon="fas fa-ban" label="Cancel" severity="secondary" @click="closeCreateBan" />
          <Button type="submit" icon="fas fa-plus" label="Ban" severity="success" />
        </div>
      </div>
    </form>
  </Dialog>
</template>
