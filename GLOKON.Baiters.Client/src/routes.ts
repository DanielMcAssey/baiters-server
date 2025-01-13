import { createWebHistory, createRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import Home from '@/pages/Home.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: Home, meta: { title: 'Home' } },
    { path: '/users', component: () => import('@/pages/Users.vue'), meta: { title: 'Users', requiresAuth: true } },
    { path: '/actors', component: () => import('@/pages/Actors.vue'), meta: { title: 'Actors', requiresAuth: true } },
    { path: '/plugins', component: () => import('@/pages/Plugins.vue'), meta: { title: 'Plugins', requiresAuth: true } },
    { path: '/error', component: () => import('@/pages/Error.vue'), meta: { title: 'Error' } },
    { path: '/:pathMatch(.*)*', component: () => import('@/pages/NotFound.vue'), meta: { title: 'Not Found' } },
  ],
});

router.beforeEach((to) => {
  const auth = useAuthStore();

  if (to.meta?.requiresAuth && !auth.isAuthenticated) {
    return {
      path: '/',
    };
  }

  document.title = (to.meta?.title ?? 'Unknown') + ' - Baiters Administration';
});

export default router;
