import './assets/css/app.css';

import { createApp } from 'vue';
import { createPinia } from 'pinia';
import router from './routes';
import { AxiosPinia, AxiosVue } from './plugins/axios';
import { useAuthStore } from '@/stores/auth';
import PrimeVue from 'primevue/config';
import { definePreset } from "@primevue/themes";
import Aura from '@primevue/themes/aura';
import DialogService from 'primevue/dialogservice';
import ConfirmationService from 'primevue/confirmationservice';
import Tooltip from 'primevue/tooltip';
import ToastService from 'primevue/toastservice';
import FocusTrap from 'primevue/focustrap';
import App from './App.vue';

const pinia = createPinia();
pinia.use(AxiosPinia);

const AppTheme = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#ecfeff',
      100: '#d0f9fd',
      200: '#a7f1fa',
      300: '#6be5f5',
      400: '#27cfe9',
      500: '#0cc0df',
      600: '#0c8eae',
      700: '#11728d',
      800: '#175d73',
      900: '#184d61',
      950: '#093243',
    },
    colorScheme: {
      light: {
        primary: {
          color: '{primary.400}',
          inverseColor: '#ffffff',
        },
      },
      dark: {
        primary: {
          color: '{primary.400}',
          inverseColor: '{primary.950}',
        },
      }
    },
  }
});

(async () => {
  const app = createApp(App)
    .use(AxiosVue)
    .use(pinia)
    .use(PrimeVue, {
      theme: {
        preset: AppTheme,
        options: {
          darkModeSelector: 'app-dark',
          cssLayer: {
            name: 'primevue',
            order: 'tailwind-base, primevue, tailwind-utilities, theme'
          }
        }
      }
    })
    .use(DialogService)
    .use(ConfirmationService)
    .use(ToastService)
    .directive('tooltip', Tooltip)
    .directive('focustrap', FocusTrap);

  const { bindCurrentUser } = useAuthStore();
  await bindCurrentUser();

  app.use(router)
    .mount('#app');
})();
