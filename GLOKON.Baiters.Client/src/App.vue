<script setup lang="ts">
import { ref, watchEffect, computed, inject, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ApplicationMark from '@/components/ApplicationMark.vue';
import Banner from '@/components/Banner.vue';
import Dropdown from '@/components/Dropdown.vue';
import DropdownLink from '@/components/DropdownLink.vue';
import NavLink from '@/components/NavLink.vue';
import ResponsiveNavLink from '@/components/ResponsiveNavLink.vue';
import { useAuthStore } from '@/stores/auth';
import ConfirmPopup from 'primevue/confirmpopup';
import ConfirmDialog from 'primevue/confirmdialog';
import DynamicDialog from 'primevue/dynamicdialog';
import Toast from 'primevue/toast';
import type { AxiosInstance } from 'axios';
import { useToast } from 'primevue/usetoast';

const auth = useAuthStore();
const router = useRouter();
const route = useRoute();
const toast = useToast();
const showingNavigationDropdown = ref(false);
const showFlashBanner = ref(false);
const flashBannerStyle = ref('success');
const flashBannerMessage = ref(null);
const $http = inject<AxiosInstance>('axios') as AxiosInstance;
/* eslint-disable  @typescript-eslint/no-explicit-any */
const serverInfo = ref<any>(undefined);

const isActiveRoute = computed(() => (pathToCheck: string) => {
  return route.matched.some(({ path }) => {
    if (!path) {
      return pathToCheck === '/';
    }

    if (pathToCheck.endsWith('*')) {
      // Remove last * and append / to name so we can match routes correctly, for example /actor/ with /actor/ but not /actor-game/
      return (path + '/').startsWith(pathToCheck.slice(0, -1));
    } else {
      return path === pathToCheck;
    }
  });
});

function fetchServerInfo(): void {
  $http.get(`/api/servers/`)
    .then((response) => {
      if (response.data) {
        serverInfo.value = response.data;
      } else {
        serverInfo.value = undefined;
      }
    })
    .catch((err: Error) => {
      console.error(err, 'Couldn\'t fetch server info');
      toast.add({
        severity: 'error',
        summary: 'Problem Loading Server Info',
        detail: 'There was a problem loading server info',
        life: 10000,
      });
    });
}

function logout(): void {
  window.location.href = '/logout';
}

watch(() => auth.user, (user) => {
  if (user) {
    fetchServerInfo();
  }
}, { immediate: true });

router.afterEach(() => {
  if (auth.user) {
    fetchServerInfo();
  }
});

watchEffect(async () => {
  // TODO: Get this working
  //flashBannerStyle.value = page.props.flash?.bannerStyle || 'success';
  //flashBannerMessage.value = page.props.flash?.banner;
  //showFlashBanner.value = true;
});
</script>

<template>
  <div>
    <Banner v-if="showFlashBanner && flashBannerMessage" :style="flashBannerStyle" :is-closeable="true" @close="showFlashBanner = false">
      {{ flashBannerMessage }}
    </Banner>

    <div class="min-h-screen bg-surface-100">
      <nav class="bg-primary-400 border-b border-primary-500 shadow">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex justify-between h-16">
            <div class="flex">
              <div class="shrink-0 flex items-center">
                <a href="https://github.com/DanielMcAssey/baiters-server" target="_blank">
                  <ApplicationMark class="block h-9 w-auto" />
                </a>
              </div>

              <div class="hidden space-x-4 sm:-my-px sm:ml-5 sm:flex" v-if="auth.isAuthenticated">
                <NavLink href="/" :active="isActiveRoute('/')">
                  Home
                </NavLink>
                <NavLink href="/users" :active="isActiveRoute('/users/*')">
                  Users
                </NavLink>
                <NavLink href="/actors" :active="isActiveRoute('/actors/*')">
                  Actors
                </NavLink>
                <NavLink href="/chats" :active="isActiveRoute('/chats/*')">
                  Chat
                </NavLink>
                <NavLink href="/plugins" :active="isActiveRoute('/plugins/*')">
                  Plugins
                </NavLink>
              </div>
            </div>

            <div class="hidden sm:flex">
              <div class="space-x-8 sm:-my-px sm:ml-10 sm:flex" v-if="!auth.isAuthenticated">
                <NavLink external-href="/login">
                  <img src="@/assets/img/steam_login.png" class="max-h-full" alt="Login with Steam" />
                </NavLink>
              </div>

              <template v-else>
                <div class="space-x-4 sm:-my-px sm:ml-5 sm:flex">
                  <NavLink as="button" v-if="serverInfo" @click="fetchServerInfo">
                    <div>
                      <div class="font-medium text-base">
                        <strong>Lobby:</strong> {{ serverInfo.lobbyCode }}
                      </div>
                      <div class="font-medium text-sm">
                        <strong>Players:</strong> {{ serverInfo.playerCount }}/{{ serverInfo.maxPlayers }}
                      </div>
                    </div>
                  </NavLink>
                </div>

                <div class="sm:flex sm:items-center sm:ml-3">
                  <div class="relative">
                    <Dropdown align="right" width="48">
                      <template #trigger>
                      <span class="inline-flex rounded-md">
                        <button type="button" class="inline-flex items-center px-3 py-2 border border-transparent text-sm leading-4 font-medium rounded-md text-gray-500 bg-white hover:text-gray-700 focus:outline-none transition">
                          {{ auth.me?.name ?? 'Unknown' }}

                          <svg class="ml-2 -mr-0.5 h-4 w-4"
                               xmlns="http://www.w3.org/2000/svg"
                               viewBox="0 0 20 20"
                               fill="currentColor">
                            <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
                          </svg>
                        </button>
                      </span>
                      </template>

                      <template #content>
                        <div class="block px-4 py-2 text-xs text-gray-400">
                          Manage Account
                        </div>

                        <form method="POST" @submit.prevent="logout">
                          <DropdownLink as="button">
                            Log Out
                          </DropdownLink>
                        </form>
                      </template>
                    </Dropdown>
                  </div>
                </div>
              </template>
            </div>

            <div class="-mr-2 flex items-center sm:hidden">
              <button class="inline-flex items-center justify-center p-2 rounded-md text-white hover:text-gray-500 hover:bg-gray-100 focus:outline-none focus:bg-gray-100 focus:text-gray-500 transition" @click="showingNavigationDropdown = ! showingNavigationDropdown">
                <svg class="h-6 w-6"
                     stroke="currentColor"
                     fill="none"
                     viewBox="0 0 24 24">
                  <path :class="{'hidden': showingNavigationDropdown, 'inline-flex': ! showingNavigationDropdown }"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M4 6h16M4 12h16M4 18h16" />
                  <path :class="{'hidden': ! showingNavigationDropdown, 'inline-flex': showingNavigationDropdown }"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>
        </div>

        <div :class="{'block': showingNavigationDropdown, 'hidden': ! showingNavigationDropdown}" class="sm:hidden">
          <div class="pt-2 pb-3 space-y-1" v-if="auth.isAuthenticated">
            <ResponsiveNavLink href="/" :active="isActiveRoute('/')">
              Home
            </ResponsiveNavLink>
            <ResponsiveNavLink href="/users" :active="isActiveRoute('/users/*')">
              Users
            </ResponsiveNavLink>
            <ResponsiveNavLink href="/actors" :active="isActiveRoute('/actors/*')">
              Actors
            </ResponsiveNavLink>
            <ResponsiveNavLink href="/chats" :active="isActiveRoute('/chats/*')">
              Chat
            </ResponsiveNavLink>
            <ResponsiveNavLink href="/plugins" :active="isActiveRoute('/plugins/*')">
              Plugins
            </ResponsiveNavLink>
          </div>

          <div class="pt-4 pb-1 border-t border-gray-200" v-else>
            <ResponsiveNavLink external-href="/login">
              <img src="@/assets/img/steam_login.png" class="h-full mx-auto" alt="Login with Steam" />
            </ResponsiveNavLink>
          </div>

          <template v-if="auth.isAuthenticated">
            <ResponsiveNavLink as="button" class="border-t border-gray-200" @click="fetchServerInfo" v-if="serverInfo">
              <div class="font-medium text-base">
                <strong>Lobby:</strong> {{ serverInfo.lobbyCode }}
              </div>
              <div class="font-medium text-sm">
                <strong>Players:</strong> {{ serverInfo.playerCount }}/{{ serverInfo.maxPlayers }}
              </div>
            </ResponsiveNavLink>

            <div class="pt-4 pb-1 border-t border-gray-200">
              <div class="flex items-center px-4">
                <div>
                  <div class="font-medium text-base text-white">
                    {{ auth.me?.name ?? 'Unknown' }}
                  </div>
                  <div class="font-medium text-sm text-white italic">
                    {{ auth.me?.steamId ?? 'Unknown' }}
                  </div>
                </div>
              </div>

              <div class="mt-3 space-y-1">
                <form method="POST" @submit.prevent="logout">
                  <ResponsiveNavLink as="button">
                    Log Out
                  </ResponsiveNavLink>
                </form>
              </div>
            </div>
          </template>
        </div>
      </nav>

      <main class="mt-8">
        <RouterView />
      </main>

      <Toast />
      <ConfirmDialog group="modal" />
      <ConfirmPopup />
      <DynamicDialog />
    </div>
  </div>
</template>
