import { defineStore } from 'pinia';
import { AxiosError } from 'axios';

type User = {
  steamId: string,
  name: string,
  isAdmin: boolean,
};

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: undefined as User | undefined,
  }),
  actions: {
    async bindCurrentUser() {
      try {
        const userResult = await this.$http.get<User>('/api/me');
        if (userResult.data) {
          this.user = userResult.data as User;
        }
      } catch (err) {
        if (err instanceof AxiosError && err.response) {
          console.error('Failed to get current user', err.response.status);
        } else {
          console.error('Failed to get current user with unknown error', err);
        }
      }
    },
  },
  getters: {
    isAuthenticated(): boolean {
      return !!this.user;
    },
    me(): User | undefined {
      return this.user;
    }
  },
});
