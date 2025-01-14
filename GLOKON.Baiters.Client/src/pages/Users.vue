<script setup lang="ts">
import { inject, onMounted, ref } from "vue";
import {useToast} from "primevue/usetoast";
import {useConfirm} from "primevue/useconfirm";
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import ButtonGroup from 'primevue/buttongroup';
import type { AxiosInstance } from 'axios';
import PlayerData from '@/components/PlayerData.vue';
import Tag from 'primevue/tag';
const confirm = useConfirm();
const toast = useToast();

const isLoading = ref(false);
const $http = inject<AxiosInstance>('axios') as AxiosInstance;
/* eslint-disable  @typescript-eslint/no-explicit-any */
const results = ref<any[]>([]);

function fetchData(): void {
  isLoading.value = true;
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

        results.value = items;
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

function banPlayer(event: Event, steamId: string) {
  confirm.require({
    target: event.currentTarget as HTMLElement | undefined,
    header: 'Ban Player',
    message: 'Are you sure you want to ban this player?',
    icon: 'fas fa-exclamation-triangle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      icon: 'fas fa-ban',
      outlined: true
    },
    acceptProps: {
      label: 'Ban',
      severity: 'danger',
      icon: 'fas fa-ban',
    },
    accept: () => {
      $http.post(`/api/users/ban/${steamId}`)
        .catch((err: Error) => {
          console.error(err, 'Couldn\'t ban player');
          toast.add({
            severity: 'error',
            summary: 'Problem Banning Player',
            detail: 'There was a problem banning the player',
            life: 10000,
          });
        })
        .finally(() => fetchData());
    },
  });
}

function kickPlayer(event: Event, steamId: string) {
  confirm.require({
    target: event.currentTarget as HTMLElement | undefined,
    header: 'Kick Player',
    message: 'Are you sure you want to kick this player?',
    icon: 'fas fa-exclamation-triangle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      icon: 'fas fa-ban',
      outlined: true
    },
    acceptProps: {
      label: 'Kick',
      severity: 'warn',
      icon: 'fas fa-bomb',
    },
    accept: () => {
      $http.post(`/api/users/kick/${steamId}`)
        .catch((err: Error) => {
          console.error(err, 'Couldn\'t kick player');
          toast.add({
            severity: 'error',
            summary: 'Problem Kicking Player',
            detail: 'There was a problem kicking the player',
            life: 10000,
          });
        })
        .finally(() => fetchData());
    },
  });
}

function sendLetter(steamId?: string): void {
  // TODO: Ask for message then send it
}

onMounted(() => {
  fetchData();
});
</script>
<template>
  <div class="mx-auto sm:max-w-7xl sm:px-6 lg:px-8">
    <div class="text-xl font-bold leading-tight mb-4 px-4">
      <i class="fas fa-users mr-2"></i>Users<span v-if="results" class="ml-1">({{ results.length }})</span>
    </div>
    <div class="bg-white overflow-hidden shadow sm:rounded-lg">
      <div class="p-4 flex justify-between sm:justify-end gap-2 flex-wrap">
        <p class="text-sm mr-auto">
          Here you will find all users currently on the server
        </p>
        <Button icon="fas fa-envelope"
                label="Send Letter to Everyone"
                severity="info"
                @click="sendLetter()" />
      </div>
      <DataTable :value="results" data-key="id" paginator :row-hover="true" :loading="isLoading"
                 :rows="50" :rowsPerPageOptions="[25, 50, 100]" stripedRows responsiveLayout="scroll">
        <template #empty>No users found.</template>
        <template #loading>Loading users. Please wait&hellip;</template>
        <Column field="id" header="Steam ID" :sortable="true"></Column>
        <Column field="fisherName" header="Name" :sortable="true">
          <template #body="slotProps">
            <PlayerData
              :last-emote="slotProps.data.lastEmote"
              :cosmetics="slotProps.data.cosmetics"
              :item="slotProps.data.heldItem" />
            {{ slotProps.data.fisherName }}
          </template>
        </Column>
        <Column field="lastEmote" header="Last Emote" :sortable="true">
          <template #body="slotProps">
            <Tag v-if="slotProps.data.lastEmote"
                 class="w-full"
                 icon="fas fa-fw fa-face-smile"
                 severity="secondary"
                 :value="slotProps.data.lastEmote">
            </Tag>
          </template>
        </Column>
        <Column field="isAdmin" header="Admin?" :sortable="true">
          <template #body="slotProps">
            <i v-if="slotProps.data.isAdmin" class="fas fa-fw fa-check"></i>
            <i v-else class="fas fa-fw fa-times"></i>
          </template>
        </Column>
        <Column style="white-space:nowrap;text-align:right;">
          <template #body="slotProps">
            <ButtonGroup>
              <Button icon="fas fa-envelope" severity="info" v-tooltip.bottom="'Send Letter'" @click="sendLetter(slotProps.data.id)" />
              <Button icon="fas fa-bomb" severity="warn" v-tooltip.bottom="'Kick'" @click="kickPlayer($event, slotProps.data.id)" />
              <Button icon="fas fa-ban" severity="danger" v-tooltip.bottom="'Ban'" @click="banPlayer($event, slotProps.data.id)" />
            </ButtonGroup>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
