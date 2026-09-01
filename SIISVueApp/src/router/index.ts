import { createRouter, createWebHistory } from 'vue-router'
import { routes } from './routes'
import { UseAuthStore } from '../stores/auth'
import { useOfficeAccountStore } from '../stores/officeAuth'

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to, from) => {
  const auth = UseAuthStore()
  const officeAuth = useOfficeAccountStore()
  officeAuth.init()
  const requiresAuth = to.meta.isRequiresAuth
  const requiresOfficeAuth = to.meta.isRequiresOfficeAuth
  const isAdminAuth = auth.authInit()
  const isOfficeAuth = officeAuth.isAuthenticated()

  document.title = `SIIS - ${to.meta?.title || to.name?.toString().toLocaleUpperCase() || 'Home'}`

  if (isAdminAuth && to.path == '/') {
    return { name: 'dashboard' }
  }

  if (requiresAuth && !isAdminAuth) {
    return { path: '/login' }
  }

  if (requiresOfficeAuth && !isOfficeAuth) {
    return { path: '/login' }
  }
})

export default router
