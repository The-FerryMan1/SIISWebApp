<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { AuthFormField, FormSubmitEvent } from '@nuxt/ui'
import z from 'zod'
import { useOfficeAccountStore } from '../../stores/officeAuth'
import { useDebounceFn } from '@vueuse/core'
import { useRouter } from 'vue-router'

const officeAuth = useOfficeAccountStore()
const toast = useToast()
const router = useRouter()

const fields: AuthFormField[] = [
  {
    name: 'email',
    type: 'string',
    autofocus: true,
    autocomplete: 'on',
    label: 'Email',
    placeholder: 'Enter your email',
    required: true,
    variant: 'outline'
  },
  {
    name: 'password',
    type: 'password',
    label: 'Password',
    placeholder: 'Enter your password',
    required: true,
    variant: 'outline'
  },
]

const schema = z.object({
  email: z.string({ error: 'Invalid email' }).email('Invalid email address'),
  password: z.string({ error: 'Invalid password' }).min(1, { error: 'Password is required' }),
})

type Schema = z.infer<typeof schema>

const onSubmit = async (payload: FormSubmitEvent<Schema>) => {
  try {
    await officeAuth.login(payload.data.email, payload.data.password)
    toast.add({ description: 'You logged in successfully', color: 'success' })
    router.push({ name: 'office-dashboard' })
  } catch (error) {
    toast.add({ description: 'Login failed: Invalid Credentials', color: 'error' })
  }
}

const debounceSubmit = useDebounceFn(onSubmit, 1000)

onMounted(() => {
  if (officeAuth.isAuthenticated()) {
    router.push({ name: 'office-dashboard' })
  }
})
</script>

<template>
  <UMain class="bg-[url('/images/cover-bg.png')] bg-cover bg-center min-h-screen flex flex-col items-center justify-center">
    <UPageCard orientation="vertical" :reverse="true" variant="outline" class="w-full max-w-md p-2">
      <UAuthForm
        :schema
        :fields
        icon="i-lucide-lock"
        title="Office Login"
        description="Enter your office account credentials."
        loading-auto
        @submit="debounceSubmit"
      >
        <template #header>
          <div class="flex flex-col items-center gap-4 pb-4 text-center">
            <img
              src="../../assets/img/brand.png"
              alt="Logo"
              class="h-24 w-auto object-contain"
              loading="lazy"
            />
            <h1 class="text-xl font-black">Student Internship Information System</h1>
            <p class="text-sm text-muted">Office Account Login</p>
          </div>
        </template>
      </UAuthForm>
    </UPageCard>
  </UMain>
</template>
