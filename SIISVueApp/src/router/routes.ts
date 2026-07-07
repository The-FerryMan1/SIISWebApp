import type { RouteRecordRaw } from 'vue-router'

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('../layouts/GuestLayout.vue'),
    children: [
      {
        path: 'login',
        component: () => import('../pages/auth/login.vue'),
        name: 'login',
      },
      {
        path: 'registration',
        component: () => import('../pages/onBoarding/onBoarding.vue'),
        name: 'registration',
      },
      {
        path: 'registration-review',
        component: () => import('../pages/onBoarding/onBoardingReview.vue'),
        name: 'registration-review',
      },
    ],
  },
  {
    path: '/admin',
    component: () => import('../layouts/MainLayout.vue'),
    children: [
      {
        path: '',
        component: () => import('../pages/admin/dashboard.vue'),
        name: 'dashboard',
        meta: { isRequiresAuth: true },
      },
      {
        path: '/application',
        component: () => import('../pages/admin/application.vue'),
        name: 'application',
        meta: { isRequiresAuth: true },
      },
      {
        path: '/application/details/:uuid',
        component: () => import('../pages/admin/applicationDetails.vue'),
        name: 'application-details',
        meta: { isRequiresAuth: true },
      },
      {
        path: '/application/details/:uuid/edit',
        component: () => import('../pages/admin/applicationEdit.vue'),
        name: 'application-edit',
        meta: { isRequiresAuth: true },
      },
      {
        path: '/office',
        component: () => import('../pages/office/office.vue'),
        name: 'office',
        meta: { isRequiresAuth: true },
      },
      {
        path: '/ojt',
        component: () => import('../pages/ojt/ojt.vue'),
        name: 'ojt',
        meta: { isRequiresAuth: true },
      },
      {
        path: '/endorsement',
        component: () => import('../pages/endorsement/endorsement.vue'),
        name: 'endorsement',
        meta: { isRequiresAuth: true },
      },
       {
        path: '/profile',
        component: () => import('../pages/user/profile.vue'),
        name: 'profile',
        meta: { isRequiresAuth: true },
      },
    ],
  },
]
