<script setup>
import { computed } from 'vue';
import { RouterLink } from 'vue-router';

const props = defineProps({
  active: Boolean,
  href: String,
  externalHref: String,
  as: String,
});

const classes = computed(() => {
    return props.active
        ? 'block pl-3 pr-4 py-2 border-l-4 border-primary-400 text-base font-medium text-primary-700 bg-primary-50 focus:outline-none focus:text-primary-800 focus:bg-primary-100 focus:border-primary-700 transition'
        : 'block pl-3 pr-4 py-2 border-l-4 border-transparent text-base font-medium text-white hover:text-gray-800 hover:bg-gray-50 hover:border-gray-300 focus:outline-none focus:text-gray-800 focus:bg-gray-50 focus:border-gray-300 transition';
});
</script>

<template>
    <div>
      <a v-if="externalHref" :href="externalHref" :class="classes" class="w-full text-left">
        <slot />
      </a>

      <button v-else-if="as === 'button'" :class="classes" class="w-full text-left">
          <slot />
      </button>

      <RouterLink v-else :to="href" :class="classes">
        <slot />
      </RouterLink>
    </div>
</template>
