import axios from 'axios';
import { markRaw } from 'vue';
import type { App } from 'vue';

const instance = axios.create({
  withCredentials: true,
  headers: {
    common: {
      'X-Requested-With': 'XMLHttpRequest',
    }
  }
});

export const AxiosVue = {
  install: (app: App) => {
    app.config.globalProperties.$http = instance;
    app.provide('axios', instance);
  }
};
export const AxiosPinia = () => {
  return { $http: markRaw(instance), };
};
