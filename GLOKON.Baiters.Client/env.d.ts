/// <reference types="vite/client" />
import 'pinia';
import 'vue';
import { AxiosInstance } from 'axios';

declare module 'vue' {
  interface ComponentCustomProperties {
    $http: typeof AxiosInstance;
  }
}

declare module 'pinia' {
  export interface PiniaCustomProperties {
    $http: typeof AxiosInstance;
  }
}
