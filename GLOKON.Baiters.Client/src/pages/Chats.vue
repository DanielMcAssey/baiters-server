<script setup lang="ts">
import { inject, onMounted, ref } from "vue";
import {useToast} from "primevue/usetoast";
import ColorPicker from 'primevue/colorpicker';
import VirtualScroller from 'primevue/virtualscroller';
import Select from 'primevue/select';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import InputGroup from 'primevue/inputgroup';
import InputGroupAddon from 'primevue/inputgroupaddon';
import type { AxiosInstance } from 'axios';
const toast = useToast();

const isLoading = ref(false);
const $http = inject<AxiosInstance>('axios') as AxiosInstance;
/* eslint-disable  @typescript-eslint/no-explicit-any */
const results = ref<any[]>([]);
/* eslint-disable  @typescript-eslint/no-explicit-any */
const users = ref<any[]>([]);

const defaultMessageColour = '#ff0000';
const messageColour = ref<string>(defaultMessageColour);
const userToMessage = ref<number | undefined>(undefined);
const messageToSend = ref<string>('');

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

function sendMessage(): void {
  const steamId = userToMessage.value ?? '';
  $http.post(`/api/chats/messages/${steamId}`, {
    message: messageToSend.value.replace('#', ''),
    colour: messageColour.value,
  })
    .then(() => {
      messageColour.value = defaultMessageColour;
      messageToSend.value = '';
      userToMessage.value = undefined;
    })
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t send message');
      toast.add({
        severity: 'error',
        summary: 'Problem Sending Message',
        detail: 'There was a problem sending the message',
        life: 10000,
      });
    });
}

onMounted(() => {
  fetchData();
  fetchUsers();
});
</script>
<template>
  <div class="mx-auto sm:max-w-7xl sm:px-6 lg:px-8">
    <div class="text-xl font-bold leading-tight mb-4 px-4">
      <i class="fas fa-message mr-2"></i>Chat
    </div>
    <div class="bg-white overflow-hidden shadow sm:rounded-lg">
      <div class="p-4">
        <VirtualScroller :items="results" :itemSize="50" class="border border-surface-200 dark:border-surface-700 rounded w-full h-96">
          <template v-slot:item="{ item, options }">
            <div :class="['flex items-center p-2', { 'bg-surface-100 dark:bg-surface-700': options.odd }]" style="height: 50px">
              <p>
                [<span class="text-blue-400 italic">{{ item.senderId }}</span>]
              </p>
              <p class="ml-1 bg-surface-500 px-1 rounded">
                <span class="font-medium" :style="{ color: '#' + (item.colour.length === 8 ? item.colour.slice(0, -2) : item.colour) }">{{ item.senderName }}:</span>
              </p>
              <p class="ml-2">
                {{ item.message }}
              </p>
            </div>
          </template>
        </VirtualScroller>
        <div class="mt-4">
          <form @submit.prevent="sendMessage">
            <InputGroup>
              <Select v-model="userToMessage"
                      :options="users"
                      optionLabel="fisherName"
                      optionValue="steamId"
                      empty-message="No users in server"
                      filter
                      showClear
                      class="max-w-52"
                      placeholder="All Players" />
              <InputText v-model="messageToSend"
                         type="text"
                         placeholder="Message to send" />
              <InputGroupAddon>
                <ColorPicker v-model="messageColour" class="border rounded-lg border-primary-500" />
              </InputGroupAddon>
              <Button type="submit"
                      icon="fas fa-paper-plane"
                      :disabled="!messageToSend || messageToSend.length === 0"
                      label="Send"
                      severity="success"
                      v-tooltip.bottom="'Send Letter'" />
            </InputGroup>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
