<script setup lang="ts">
import { inject, onMounted, ref } from "vue";
import {useToast} from "primevue/usetoast";
import {useConfirm} from "primevue/useconfirm";
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import ButtonGroup from 'primevue/buttongroup';
import Dialog from 'primevue/dialog';
import ProgressSpinner from 'primevue/progressspinner';
import type { AxiosInstance } from 'axios';
const confirm = useConfirm();
const toast = useToast();

const isLoading = ref(false);
const $http = inject<AxiosInstance>('axios') as AxiosInstance;
/* eslint-disable  @typescript-eslint/no-explicit-any */
const results = ref<any[]>([]);
const colours = ['#101c31', '#ac0029', '#a4aa39', '#008583', '#e69d00'];

function fetchData(): void {
  isLoading.value = true;
  $http.get(`/api/chalk-canvases/`)
    .then((response) => {
      if (response.data) {
        results.value = response.data;
      } else {
        results.value = [];
      }
    })
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t load chalk canvases');
      toast.add({
        severity: 'error',
        summary: 'Problem Loading Chalk Canvases',
        detail: 'There was a problem loading chalk canvases',
        life: 10000,
      });
    })
    .finally(() => {
      isLoading.value = false;
    });
}

const isChalkPreviewOpen = ref(false);
const isGeneratingPreview = ref(false);
const isDarkPreviewBackground = ref(false);
/* eslint-disable  @typescript-eslint/no-explicit-any */
const previewingChalkCanvas = ref<any | undefined>(undefined);
const chalkPreviewB64 = ref<string | undefined>(undefined);
const chalkDimensions = ref<{ width: number; height: number } | undefined>(undefined);

/* eslint-disable  @typescript-eslint/no-explicit-any */
function previewChalk(event: Event, chalkCanvas: any): void {
  isChalkPreviewOpen.value = true;
  isGeneratingPreview.value = true;
  previewingChalkCanvas.value = chalkCanvas;
  const imageWidth = chalkCanvas.maxX - chalkCanvas.minX;
  const imageHeight = chalkCanvas.maxY - chalkCanvas.minY;
  chalkDimensions.value = {
    width: imageWidth,
    height: imageHeight,
  };
  const originX = chalkCanvas.minX;
  const originY = chalkCanvas.minY;
  const imageBuffer = new Uint8ClampedArray(imageWidth * imageHeight * 4);

  // Fill buffer with pixels
  for(const point of chalkCanvas.points) {
    const fixedX = point.position.x - originX;
    const fixedY = point.position.y - originY;
    const coordinate = (fixedY * imageWidth + fixedX) * 4;
    const colourHex = colours[point.colour - 1] ?? colours[0];

    let colourParts = colourHex.substring(1).split('');
    if(colourParts.length == 3){
      // Handle short hex codes
      colourParts = [colourParts[0], colourParts[0], colourParts[1], colourParts[1], colourParts[2], colourParts[2]];
    }

    const colour = Number('0x' + colourParts.join(''));
    imageBuffer[coordinate]     = (colour >>> 16) & 0xFF; // R
    imageBuffer[coordinate + 1] = (colour >>> 8) & 0xFF; // G
    imageBuffer[coordinate + 2] = colour & 0xFF; // B
    imageBuffer[coordinate + 3] = 0xFF; // Alpha
  }

  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');

  canvas.width = imageWidth;
  canvas.height = imageHeight;

  if (ctx) {
    const imageData = ctx.createImageData(imageWidth, imageHeight);
    imageData.data.set(imageBuffer);
    ctx.putImageData(imageData, 0, 0);
    chalkPreviewB64.value = canvas.toDataURL();
  } else {
    console.error('Failed to create canvas context');
  }

  isGeneratingPreview.value = false;
}

function stopPreviewingChalk(): void {
  isChalkPreviewOpen.value = false;
  isGeneratingPreview.value = false;
  previewingChalkCanvas.value = undefined;
  chalkPreviewB64.value = undefined;
  chalkDimensions.value = undefined;
}

function setPreviewBackground(isDark: boolean): void {
  isDarkPreviewBackground.value = isDark;
}

function removeChalk(event: Event, id: number): void {
  confirm.require({
    target: event.currentTarget as HTMLElement | undefined,
    header: 'Remove Chalk Canvas',
    message: 'Are you sure you want to remove this chalk canvas?',
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
      $http.delete(`/api/chalk-canvases/${id}`)
        .catch((err: Error) => {
          console.error(err, 'Couldn\'t remove chalk canvas');
          toast.add({
            severity: 'error',
            summary: 'Problem Removing Chalk',
            detail: 'There was a problem removing the chalk canvas',
            life: 10000,
          });
        })
        .finally(() => fetchData());
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
      <i class="fas fa-chalkboard mr-2"></i>Chalk Canvases<span v-if="results" class="ml-1">({{ results.length }})</span>
    </div>
    <div class="bg-white overflow-hidden shadow sm:rounded-lg">
      <div class="p-4 flex justify-between sm:justify-end gap-2 flex-wrap">
        <p class="text-sm mr-auto">
          Here you will find all chalk canvases currently in-game
        </p>
      </div>
      <DataTable :value="results" data-key="id" paginator :row-hover="true" :loading="isLoading"
                 :rows="50" :rowsPerPageOptions="[25, 50, 100]" stripedRows responsiveLayout="scroll">
        <template #empty>No chalk canvases found.</template>
        <template #loading>Loading chalk canvases. Please wait&hellip;</template>
        <Column field="id" header="ID" :sortable="true"></Column>
        <Column field="pointsCount" header="Cells" :sortable="true"></Column>
        <Column style="white-space:nowrap;text-align:right;">
          <template #body="slotProps">
            <ButtonGroup>
              <Button icon="fas fa-magnifying-glass"
                      severity="info"
                      v-tooltip.bottom="'Preview Chalk'"
                      @click="previewChalk($event, slotProps.data)" />
              <Button icon="fas fa-trash"
                      severity="danger"
                      v-tooltip.bottom="'Remove Chalk'"
                      @click="removeChalk($event, slotProps.data.id)" />
            </ButtonGroup>
          </template>
        </Column>
      </DataTable>
      <Dialog v-model:visible="isChalkPreviewOpen" modal header="Chalk Preview" @close="stopPreviewingChalk" :style="{ width: '30rem' }">
        <div class="grid grid-cols-6 gap-2">
          <div class="col-span-6 text-center" v-if="isGeneratingPreview">
            <ProgressSpinner class="w-52 h-52"
                             strokeWidth="5"
                             fill="transparent"
                             animationDuration=".5s"
                             aria-label="Generating Chalk Preview" />
            <p class="text-center italic w-full mt-2">
              Generating preview&hellip;
            </p>
          </div>
          <template v-else-if="chalkDimensions && chalkPreviewB64">
            <div class="col-span-6 flex justify-between">
              <div class="flex flex-col justify-center gap-1">
                <p class="font-bold text-xl">
                  {{ previewingChalkCanvas?.id }}
                </p>
                <p class="text-xs">
                  <strong>Cells:</strong>&nbsp;{{ previewingChalkCanvas?.pointsCount }}
                </p>
              </div>
              <ButtonGroup>
                <Button icon="fas fa-moon"
                        severity="primary"
                        label="Dark"
                        size="small"
                        :variant="isDarkPreviewBackground ? undefined : 'outlined'"
                        @click="setPreviewBackground(true)" />
                <Button icon="fas fa-sun"
                        severity="primary"
                        label="Light"
                        size="small"
                        :variant="isDarkPreviewBackground ? 'outlined' : undefined"
                        @click="setPreviewBackground(false)" />
              </ButtonGroup>
            </div>
            <div class="col-span-6 rounded" :class="{ 'bg-surface-800': isDarkPreviewBackground, 'bg-surface-200': !isDarkPreviewBackground }">
              <img :src="chalkPreviewB64"
                   alt="Chalk Preview"
                   class="object-contain w-full h-full"
                   :height="chalkDimensions.height"
                   :width="chalkDimensions.width" />
            </div>
          </template>
          <p class="col-span-6 text-center italic" v-else>
            Failed to generate preview
          </p>
          <div class="col-span-6 flex justify-end gap-2">
            <Button type="button" icon="fas fa-times" label="Close" severity="secondary" @click="stopPreviewingChalk" />
          </div>
        </div>
      </Dialog>
    </div>
  </div>
</template>
