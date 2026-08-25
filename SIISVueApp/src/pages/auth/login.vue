<script setup lang="ts">
import type { AuthFormField, FormSubmitEvent } from '@nuxt/ui'
import z from 'zod'
import { UseAuthStore } from '../../stores/auth'
import { useRouter } from 'vue-router'

const auth = UseAuthStore()
const toast = useToast()
const router = useRouter()

const fields: AuthFormField[] = [
  {
    name: 'identifier',
    type: 'string',
    autofocus: true,
    autocomplete: 'on',
    label: 'Email or Username',
    placeholder: 'Enter your email or username',
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
  identifier: z.string({ error: 'Invalid email or username' }).min(1, 'Email or username is required'),
  password: z.string({ error: 'Invalid password' }).min(1, { error: 'Password is required' }),
})

type Schema = z.infer<typeof schema>

const onSubmit = async (payload: FormSubmitEvent<Schema>) => {
  try {
    const result = await auth.useLogin({ identifier: payload.data.identifier, password: payload.data.password })
    toast.add({ description: 'You logged in successfully', color: 'success' })
    if (result.role === 'office') {
      router.push({ name: 'office-dashboard' })
    } else {
      router.push({ name: 'dashboard' })
    }
  } catch (error) {
    toast.add({ description: 'Login failed: Invalid Credentials', color: 'error' })
  }
}
</script>

<template>
  <UMain class="bg-[url('/cover-bg.png')] bg-cover bg-center min-h-screen flex flex-col items-center justify-center">
    <UPageCard orientation="vertical" :reverse="true" variant="outline" class="w-full max-w-md p-2">
      <UAuthForm
        :schema
        :fields
        icon="i-lucide-lock"
        title="Login"
        description="Enter your credentials to access your account."
        loading-auto
        @submit="onSubmit"
      >
        <template #header>
          <div class="flex flex-col items-center gap-4 pb-4 text-center">
            <img
              src="../../assets/img/brand.png"
              alt="Illustration"
              class="h-24 w-auto object-contain"
              loading="lazy"
            />
            <h1 class="text-xl font-black">Student Internship Information System</h1>
          </div>
        </template>
      </UAuthForm>
    </UPageCard>
  </UMain>
</template>