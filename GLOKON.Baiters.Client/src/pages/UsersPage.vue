<script setup lang="ts">
import { inject, onMounted, ref } from "vue";
import {useToast} from "primevue/usetoast";
import {useConfirm} from "primevue/useconfirm";
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import ButtonGroup from 'primevue/buttongroup';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import InputGroup from 'primevue/inputgroup';
import Select from 'primevue/select';
import InputNumber from 'primevue/inputnumber';
import type { AxiosInstance } from 'axios';
import PlayerData from '@/components/PlayerData.vue';
import Tag from 'primevue/tag';
const confirm = useConfirm();
const toast = useToast();

const isLoading = ref(false);
const $http = inject<AxiosInstance>('axios') as AxiosInstance;
/* eslint-disable  @typescript-eslint/no-explicit-any */
const results = ref<any[]>([]);
const itemQualities = [
  { quality: 0, name: 'Normal', },
  { quality: 1, name: 'Shining', },
  { quality: 2, name: 'Glistening', },
  { quality: 3, name: 'Opulent', },
  { quality: 4, name: 'Radiant', },
  { quality: 5, name: 'Alpha', },
];

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
      $http.post(`/api/bans/${steamId}`)
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

const isSetHeldItemOpen = ref(false);
const setHeldItemOn = ref<number | undefined>(undefined);
const heldItemId = ref<string>('');
const heldItemSize = ref<number>(1);
const heldItemQuality = ref<number>(0);

function openSetHeldItem(steamId?: number): void {
  setHeldItemOn.value = steamId;
  isSetHeldItemOpen.value = true;
}

function closeHeldItem(): void {
  isSetHeldItemOpen.value = false;
  setHeldItemOn.value = undefined;
  heldItemId.value = '';
  heldItemSize.value = 1;
  heldItemQuality.value = 0;
}

function setHeldItem(): void {
  const steamId = sendLetterTo.value ?? '';
  $http.post(`/api/users/held-item/${steamId}`, {
    id: heldItemId.value,
    size: heldItemSize.value,
    quality: heldItemQuality.value,
  })
    .then(() => closeHeldItem())
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t set held item on player(s)');
      toast.add({
        severity: 'error',
        summary: 'Problem Setting Held Item',
        detail: 'There was a problem setting the held item on the player(s)',
        life: 10000,
      });
    })
    .finally(() => fetchData());
}

const isLetterComposeOpen = ref(false);
const sendLetterTo = ref<number | undefined>(undefined);
const letterHeader = ref<string>('');
const letterBody = ref<string>('');
const letterClosing = ref<string>('');
const letterItems = ref<{ id: number, itemId: string }[]>([]);

function composeLetter(steamId?: number): void {
  sendLetterTo.value = steamId;
  isLetterComposeOpen.value = true;
}

function closeComposeLetter(): void {
  isLetterComposeOpen.value = false;
  sendLetterTo.value = undefined;
  letterHeader.value = '';
  letterBody.value = '';
  letterClosing.value = '';
  letterItems.value = [];
}

function addLetterItem(): void {
  letterItems.value.push({ id: letterItems.value.length, itemId: '' });
}

function removeLetterItem(index: number): void {
  letterItems.value.splice(index, 1);
}

function sendLetter(): void {
  const steamId = sendLetterTo.value ?? '';
  $http.post(`/api/users/letter/${steamId}`, {
    header: letterHeader.value,
    body: letterBody.value,
    closing: letterClosing.value,
    items: letterItems.value.map((item) => item.itemId),
  })
    .then(() => closeComposeLetter())
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t send letter to player(s)');
      toast.add({
        severity: 'error',
        summary: 'Problem Sending Letter',
        detail: 'There was a problem sending the letter to the player(s)',
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
                @click="composeLetter()" />

        <Button icon="fas fa-icons"
                label="Set Held Item for Everyone"
                severity="info"
                @click="openSetHeldItem()" />
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
        <Column field="position" header="Position" :sortable="true">
          <template #body="slotProps">
            <Tag :pt="{ root: { style: 'white-space:preserve nowrap' } }" icon="fas fa-location-dot" severity="info" size="small">
              X: {{ slotProps.data.position.x.toFixed(2) }}<br />
              Y: {{ slotProps.data.position.y.toFixed(2) }}<br />
              Z: {{ slotProps.data.position.z.toFixed(2) }}
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
              <Button icon="fas fa-envelope" severity="info" v-tooltip.bottom="'Send Letter'" @click="composeLetter(slotProps.data.id)" />
              <Button icon="fas fa-icons" severity="info" v-tooltip.bottom="'Set Held Item'" @click="openSetHeldItem(slotProps.data.id)" />
              <Button icon="fas fa-bomb" severity="warn" v-tooltip.bottom="'Kick'" @click="kickPlayer($event, slotProps.data.id)" />
              <Button icon="fas fa-ban" severity="danger" v-tooltip.bottom="'Ban'" @click="banPlayer($event, slotProps.data.id)" />
            </ButtonGroup>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
  <Dialog v-model:visible="isSetHeldItemOpen" modal header="Set Held Item" @close="closeHeldItem" :style="{ width: '30rem' }">
    <form @submit.prevent="setHeldItem">
      <div class="grid grid-cols-6 gap-2">
        <div class="col-span-6">
          <label for="item_set_on" class="font-semibold">Set On <span v-if="setHeldItemOn"> (SteamID)</span></label>
          <InputText id="item_set_on"
                     type="text"
                     class="w-full mt-1"
                     name="item_set_on"
                     :value="setHeldItemOn ?? 'All Players'"
                     disabled
                     autocomplete="off" />
        </div>
        <div class="col-span-4">
          <label for="item_id" class="font-semibold">Item ID</label>
          <InputText id="item_id"
                     type="text"
                     class="w-full mt-1"
                     name="item_id"
                     v-model="heldItemId"
                     placeholder="Item ID"
                     required
                     autocomplete="off" />
        </div>
        <div class="col-span-2">
          <label for="item_size" class="font-semibold">Size</label>
          <InputNumber id="item_size"
                       class="w-full mt-1"
                       name="item_size"
                       v-model="heldItemSize"
                       mode="decimal"
                       showButtons
                       :allowEmpty="false"
                       :min="0.0"
                       :max="12000.0"
                       :minFractionDigits="0"
                       :maxFractionDigits="3"
                       placeholder="Item Size"
                       required
                       autocomplete="off" />
        </div>
        <div class="col-span-6">
          <label for="item_quality" class="font-semibold">Quality</label>
          <Select v-model="heldItemQuality"
                  id="item_quality"
                  name="item_quality"
                  class="w-full mt-1"
                  :options="itemQualities"
                  optionLabel="name"
                  optionValue="quality"
                  placeholder="Item Quality" />
        </div>
        <div class="col-span-6 flex justify-end gap-2">
          <Button type="button" icon="fas fa-ban" label="Cancel" severity="secondary" @click="closeHeldItem" />
          <Button type="submit" icon="fas fa-icons" label="Set Item" severity="success" />
        </div>
      </div>
    </form>
  </Dialog>
  <Dialog v-model:visible="isLetterComposeOpen" modal header="Compose Letter" @close="closeComposeLetter" :style="{ width: '30rem' }">
    <form @submit.prevent="sendLetter">
      <div class="grid grid-cols-6 gap-2">
        <div class="col-span-6">
          <label for="recipient" class="font-semibold">Recipient <span v-if="sendLetterTo"> (SteamID)</span></label>
          <InputText id="recipient"
                     type="text"
                     class="w-full mt-1"
                     name="recipient"
                     :value="sendLetterTo ?? 'All Players'"
                     disabled
                     autocomplete="off" />
        </div>
        <div class="col-span-6">
          <label for="header" class="font-semibold">Header</label>
          <InputText id="header"
                     type="text"
                     class="w-full mt-1"
                     name="header"
                     v-model="letterHeader"
                     placeholder="Letter title"
                     required
                     autocomplete="off" />
        </div>
        <div class="col-span-6">
          <label for="body" class="font-semibold">Message</label>
          <Textarea id="body"
                    class="w-full mt-1"
                    name="body"
                    v-model="letterBody"
                    placeholder="Your message"
                    required
                    rows="5" />
        </div>
        <div class="col-span-6">
          <label for="closing" class="font-semibold">Closing</label>
          <InputText id="closing"
                     type="text"
                     class="w-full mt-1"
                     name="closing"
                     v-model="letterClosing"
                     placeholder="Letter sign-off"
                     required
                     autocomplete="off" />
        </div>
        <div class="col-span-6">
          <label for="items" class="font-semibold">Items ({{ letterItems.length }})</label>
          <Button type="button"
                  icon="fas fa-plus"
                  label="Add Item"
                  severity="info"
                  size="small"
                  class="ml-2"
                  @click="addLetterItem" />
          <div class="grid grid-cols-6 gap-2 mt-2">
            <p class="col-span-6 text-center italic" v-if="letterItems.length === 0">
              Letter has no items
            </p>
            <InputGroup v-for="(item, index) in letterItems" :key="item.id" class="col-span-6">
              <InputText :id="'items_' + index"
                         type="text"
                         class="w-full"
                         :name="'items_' + index"
                         v-model="item.itemId"
                         placeholder="Item ID"
                         required
                         autocomplete="off" />
              <Button type="button"
                      icon="fas fa-trash"
                      severity="danger"
                      @click="removeLetterItem(index)" />
            </InputGroup>
          </div>
        </div>
        <div class="col-span-6 flex justify-end gap-2">
          <Button type="button" icon="fas fa-ban" label="Cancel" severity="secondary" @click="closeComposeLetter" />
          <Button type="submit" icon="fas fa-envelope" label="Send" severity="success" />
        </div>
      </div>
    </form>
  </Dialog>
</template>
