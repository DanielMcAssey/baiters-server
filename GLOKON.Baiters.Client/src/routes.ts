import { createWebHistory, createRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import Home from '@/pages/HomePage.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: Home, meta: { title: 'Home', requiresAuth: true } },
    { path: '/users', component: () => import('@/pages/UsersPage.vue'), meta: { title: 'Users', requiresAuth: true } },
    { path: '/actors', component: () => import('@/pages/ActorsPage.vue'), meta: { title: 'Actors', requiresAuth: true } },
    { path: '/bans', component: () => import('@/pages/BansPage.vue'), meta: { title: 'Bans', requiresAuth: true } },
    { path: '/chalk-canvases', component: () => import('@/pages/ChalkCanvasesPage.vue'), meta: { title: 'Chalk Canvases', requiresAuth: true } },
    { path: '/chats', component: () => import('@/pages/ChatsPage.vue'), meta: { title: 'Chats', requiresAuth: true } },
    { path: '/plugins', component: () => import('@/pages/PluginsPage.vue'), meta: { title: 'Plugins', requiresAuth: true } },
    { path: '/error', component: () => import('@/pages/ErrorPage.vue'), meta: { title: 'Error' } },
    { path: '/unauthorized', component: () => import('@/pages/UnauthorizedPage.vue'), meta: { title: 'Unauthorized' } },
    { path: '/:pathMatch(.*)*', component: () => import('@/pages/NotFoundPage.vue'), meta: { title: 'Not Found' } },
  ],
});

router.beforeEach((to) => {
  const auth = useAuthStore();

  if (to.meta?.requiresAuth && !auth.isAuthenticated) {
    return {
      path: '/unauthorized',
    };
  }

  if (to.path === '/unauthorized' && auth.isAuthenticated) {
    return {
      path: '/',
    };
  }

  document.title = (to.meta?.title ?? 'Unknown') + ' - Baiters Administration';
});

export default router;
