<script setup>
import { computed } from 'vue';
import { RouterLink } from 'vue-router';

const props = defineProps({
  href: String,
  active: Boolean,
  externalHref: String,
  as: String,
});

const classes = computed(() => {
    return props.active
        ? 'inline-flex items-center px-1 pt-1 border-b-2 border-white text-sm font-medium leading-5 text-white focus:outline-none focus:border-primary-700 transition'
        : 'inline-flex items-center px-1 pt-1 border-b-2 border-transparent text-sm font-medium leading-5 text-white hover:text-gray-700 hover:border-gray-300 focus:outline-none focus:text-gray-700 focus:border-gray-300 transition';
});
</script>

<template>
  <a v-if="externalHref" :href="externalHref" :class="classes" class="w-full text-left">
    <slot />
  </a>

  <button v-else-if="as === 'button'" :class="classes" class="w-full text-left">
    <slot />
  </button>

  <RouterLink v-else :to="href" :class="classes">
    <slot />
  </RouterLink>
</template>
