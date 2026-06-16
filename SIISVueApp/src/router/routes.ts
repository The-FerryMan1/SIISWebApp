import type { RouteRecordRaw } from "vue-router";

export const routes: RouteRecordRaw[] = [
    {
        path: '/',
        component: ()=>import('../layouts/GuestLayout.vue'),
        children: [
            {
                path: '',
                component: ()=>import('../pages/auth/login.vue'),
                name: 'login',
            }
        ]
    },
    {
        path: '/admin',
        component: ()=>import('../layouts/MainLayout.vue'),
        children: [
            {
                path: '',
                component: ()=>import('../pages/admin/dashboard.vue'),
                name: 'dashboard',
                meta:{isRequiresAuth: true}
            }
        ]
    },
]