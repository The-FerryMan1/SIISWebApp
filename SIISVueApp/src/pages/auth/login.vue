<script setup lang="ts">
import type { AuthFormField, FormSubmitEvent } from '@nuxt/ui'
import z from 'zod'
import { UseAuthStore } from '../../stores/auth'
import { useDebounceFn } from '@vueuse/core'
import { useRouter } from 'vue-router'

const auth = UseAuthStore()
const toast = useToast()
const router = useRouter()

const fields: AuthFormField[] = [
  {
    name: 'username',
    type: 'string',
    autofocus: true,
    autocomplete: 'on',
    label: 'Username',
    placeholder: 'Enter your username',
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
  username: z.string({ error: 'Invalid username' }).min(1, 'username is required'),
  password: z.string({ error: 'Invalid password' }).min(1, { error: 'Password is required' }),
})

type Schema = z.infer<typeof schema>

const onSubmit = async (paylaod: FormSubmitEvent<Schema>) => {
  try {
    await auth.useLogin(paylaod.data)
    toast.add({ description: 'You logged in successfully', color: 'success' })
    router.push({ name: 'dashboard' })
  } catch (error) {
    toast.add({ description: 'Login failed: Invalid Credentials', color: 'error' })
  }
}

const debounceSubmit = useDebounceFn(onSubmit, 1000)
</script>

<template>
  <UMain class="bg-[url('/images/cover-bg.png')] bg-cover bg-center min-h-screen flex flex-col items-center justify-center p-4">
    <UPageCard orientation="vertical" variant="outline" class="w-full max-w-md">
      <template #header>
        <div class="flex flex-col items-center gap-4 pb-4 text-center">
          <img
            src="../../assets/img/brand.png"
            alt="SIIS"
            class="h-20 w-auto object-contain"
            loading="lazy"
          />
          <div>
            <h1 class="text-xl font-black text-primary">Student Internship Information System</h1>
            <p class="text-sm text-muted mt-1">Sign in to your account</p>
          </div>
        </div>
      </template>

      <UAuthForm
        :schema
        :fields
        icon="i-lucide-lock"
        title="Welcome back"
        description="Enter your credentials to access your account."
        loading-auto
        @submit="debounceSubmit"
      />
    </UPageCard>
  </UMain>
</template>
