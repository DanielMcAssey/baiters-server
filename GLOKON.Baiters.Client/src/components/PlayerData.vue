<script setup lang="ts">
import { ref } from 'vue';
import Popover from 'primevue/popover';
import Tag from "primevue/tag";

defineProps({
  cosmetics: {
    type: Object,
  },
  item: {
    type: Object,
  },
});

const cosmeticsPanel = ref();
const itemPanel = ref();

function toggleCosmetics(event: Event) {
  event.preventDefault();
  cosmeticsPanel.value.toggle(event);
}

function showCosmetics(event: Event) {
  event.preventDefault();
  cosmeticsPanel.value.show(event);
}

function hideCosmetics(event: Event) {
  event.preventDefault();
  cosmeticsPanel.value.hide(event);
}

function toggleItem(event: Event) {
  event.preventDefault();
  itemPanel.value.toggle(event);
}

function showItem(event: Event) {
  event.preventDefault();
  itemPanel.value.show(event);
}

function hideItem(event: Event) {
  event.preventDefault();
  itemPanel.value.hide(event);
}
</script>
<template>
  <Popover ref="cosmeticsPanel" :pt="{ root: '!bg-slate-100', content: '!p-3' }">
    <div class="max-w-sm text-xs" v-if="cosmetics">
      <p><strong>Title:</strong> {{ cosmetics.title }}</p>
      <p><strong>Eye:</strong> {{ cosmetics.eye }}</p>
      <p><strong>Nose:</strong> {{ cosmetics.nose }}</p>
      <p><strong>Mouth:</strong> {{ cosmetics.mouth }}</p>
      <p><strong>Undershirt:</strong> {{ cosmetics.undershirt }}</p>
      <p><strong>Overshirt:</strong> {{ cosmetics.overshirt }}</p>
      <p><strong>Legs:</strong> {{ cosmetics.legs }}</p>
      <p><strong>Hat:</strong> {{ cosmetics.hat }}</p>
      <p><strong>Species:</strong> {{ cosmetics.species }}</p>
      <p><strong>Accessories:</strong> {{ cosmetics.accessory.join(', ') }}</p>
      <p><strong>Pattern:</strong> {{ cosmetics.pattern }}</p>
      <p><strong>PrimaryColor:</strong> {{ cosmetics.primaryColor }}</p>
      <p><strong>SecondaryColor:</strong> {{ cosmetics.secondaryColor }}</p>
      <p><strong>Tail:</strong> {{ cosmetics.tail }}</p>
      <p v-if="cosmetics.bobber"><strong>Bobber:</strong> {{ cosmetics.bobber }}</p>
    </div>
  </Popover>
  <Popover ref="itemPanel" :pt="{ root: '!bg-slate-100', content: '!p-3' }">
    <div class="max-w-sm text-xs" v-if="item">
      <p><strong>ID:</strong> {{ item.id }}</p>
      <p><strong>Size:</strong> {{ item.size }}</p>
      <p><strong>Quality:</strong> {{ item.quality }}</p>
    </div>
  </Popover>
  <div class="flex items-center gap-2">
    <Tag v-if="cosmetics"
         icon="fas fa-fw fa-shirt"
         severity="info"
         value="Cosmetics"
         @click="toggleCosmetics"
         @mouseenter="showCosmetics"
         @mouseleave="hideCosmetics">
    </Tag>
    <Tag v-if="item"
         icon="fas fa-fw fa-crown"
         severity="default"
         value="Held Item"
         @click="toggleItem"
         @mouseenter="showItem"
         @mouseleave="hideItem">
    </Tag>
  </div>
</template>
