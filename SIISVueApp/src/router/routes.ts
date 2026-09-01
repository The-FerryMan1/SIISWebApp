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
        meta: { title: 'Login' },
      },
      {
        path: 'office-login',
        redirect: '/login',
        name: 'office-login',
      },
      {
        path: 'registration/:token',
        component: () => import('../pages/onBoarding/onBoarding.vue'),
        name: 'registration',
        meta: { title: 'Registration' },
      },
      {
        path: 'registration-review',
        component: () => import('../pages/onBoarding/onBoardingReview.vue'),
        name: 'registration-review',
        meta: { title: 'Registration Review' },
      },
      {
        path: '',
        component: () => import('../pages/home/home.vue'),
        name: 'home',
        meta: { title: 'Home' },
      },
    ],
  },
  {
    path: '/office',
    component: () => import('../layouts/OfficeLayout.vue'),
    children: [
      {
        path: '',
        component: () => import('../pages/office/officeDashboard.vue'),
        name: 'office-dashboard',
        meta: { isRequiresOfficeAuth: true, title: 'Dashboard' },
      },
      {
        path: 'reports',
        component: () => import('../pages/office/reports.vue'),
        name: 'office-reports',
        meta: { isRequiresOfficeAuth: true, title: 'Reports' },
      },
      {
        path: 'requirements',
        component: () => import('../pages/office/requirements.vue'),
        name: 'office-requirements',
        meta: { isRequiresOfficeAuth: true, title: 'Requirements' },
      },
      {
        path: 'logs',
        component: () => import('../pages/office/logs.vue'),
        name: 'office-logs',
        meta: { isRequiresOfficeAuth: true, title: 'Logs' },
      },
      {
        path: 'progress/:uuid',
        component: () => import('../pages/progress.vue'),
        name: 'office-progress',
        meta: { isRequiresOfficeAuth: true, title: 'Progress' },
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
        meta: { isRequiresAuth: true, title: 'Dashboard' },
      },
      {
        path: 'opg-dashboard',
        component: () => import('../pages/admin/opgDashboard.vue'),
        name: 'opg-dashboard',
        meta: { isRequiresAuth: true, title: 'OPG Dashboard' },
      },
      {
        path: 'analytics',
        component: () => import('../pages/admin/analytics.vue'),
        name: 'analytics',
        meta: { isRequiresAuth: true, title: 'Analytics' },
      },
      {
        path: 'application',
        component: () => import('../pages/admin/application.vue'),
        name: 'application',
        meta: { isRequiresAuth: true, title: 'Applications' },
      },
      {
        path: 'application/details/:uuid',
        component: () => import('../pages/admin/applicationDetails.vue'),
        name: 'application-details',
        meta: { isRequiresAuth: true, title: 'Application Details' },
      },
      {
        path: 'application/details/:uuid/edit',
        component: () => import('../pages/admin/applicationEdit.vue'),
        name: 'application-edit',
        meta: { isRequiresAuth: true, title: 'Edit Application' },
      },
      {
        path: 'office',
        component: () => import('../pages/office/office.vue'),
        name: 'office',
        meta: { isRequiresAuth: true, title: 'Offices' },
      },
      {
        path: 'ojt',
        component: () => import('../pages/ojt/ojt.vue'),
        name: 'ojt',
        meta: { isRequiresAuth: true, title: 'OJTs' },
      },
      {
        path: 'ojt/:uuid',
        component: () => import('../pages/ojt/ojtview/ojtview.vue'),
        name: 'ojt-details',
        meta: { isRequiresAuth: true, title: 'OJT Details' },
      },
      {
        path: 'endorsement',
        component: () => import('../pages/endorsement/endorsement.vue'),
        name: 'endorsement',
        meta: { isRequiresAuth: true, title: 'Endorsement' },
      },
      {
        path: 'endorsement-by-school',
        component: () => import('../pages/admin/endorsementBySchool.vue'),
        name: 'endorsement-by-school',
        meta: { isRequiresAuth: true, title: 'Endorsement by School' },
      },
      {
        path: 'endorsement-settings',
        component: () => import('../pages/admin/endorsementSettings.vue'),
        name: 'endorsement-settings',
        meta: { isRequiresAuth: true, title: 'Endorsement Settings' },
      },
      {
        path: 'profile',
        component: () => import('../pages/user/profile.vue'),
        name: 'profile',
        meta: { isRequiresAuth: true, title: 'Profile' },
      },
      {
        path: 'registration-token-generator',
        component: () => import('../pages/registration/registrationtoken.vue'),
        name: 'registration-generator',
        meta: { isRequiresAuth: true, title: 'Registration Tokens' },
      },
      {
        path: 'report',
        component: () => import('../pages/report/reportpage.vue'),
        name: 'report',
        meta: { isRequiresAuth: true, title: 'Reports' },
      },
      {
        path: 'student-import',
        component: () => import('../pages/admin/studentImport.vue'),
        name: 'student-import',
        meta: { isRequiresAuth: true, title: 'Student Import' },
      },
      {
        path: 'requirements',
        component: () => import('../pages/admin/requirements.vue'),
        name: 'requirements',
        meta: { isRequiresAuth: true, title: 'Requirements' },
      },
      {
        path: 'logs',
        component: () => import('../pages/admin/logs.vue'),
        name: 'logs',
        meta: { isRequiresAuth: true, title: 'Logs' },
      },
      {
        path: 'progress/:uuid',
        component: () => import('../pages/progress.vue'),
        name: 'progress',
        meta: { isRequiresAuth: true, title: 'Progress' },
      }
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('../pages/notFound/notfound.vue'),
    meta: { title: 'Not Found' },
  },
]
