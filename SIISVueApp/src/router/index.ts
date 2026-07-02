import { createRouter, createWebHistory } from 'vue-router'
import { routes } from './routes'
import { UseAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach(async (to, from) => {
  const auth = UseAuthStore()
  const requiresAuth = to.meta.isRequiresAuth
  const isAuth = auth.authInit()

  document.title = `SIIS - ${to.name?.toString().toLocaleUpperCase()}`

  if (isAuth && to.path == '/') {
    return { name: 'dashboard' }
  }

  if (requiresAuth && !isAuth) {
    return { path: '/' }
  }
})

export default router
