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
  },
  {
    name: 'password',
    type: 'password',
    label: 'Password',
    placeholder: 'Enter your password',
    required: true,
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
  <UPageSection>
    <template #header>
      <UColorModeSwitch />
    </template>
    <template #body>
      <UPageCard orientation="horizontal" :reverse="true" variant="outline">
        <UAuthForm
          :schema
          :fields
          icon="i-lucide-lock"
          title="Login"
          description="Enter your credentials to access your account."
          loading-auto
          @submit="debounceSubmit"
        >
        </UAuthForm>
        <div class="w-full h-125 flex justify-center items-start relative">
          \
          <div class="absolute p-20">
            <img
              src="../../assets/img/brand.png"
              alt="Illustration"
              class="w-full rounded-lg object-contain size-35 z-1"
              loading="lazy"
            />
            <h1 class="text-nowrap text-xl font-black p-5">
              Student Internship Information System
            </h1>
          </div>
          <img
            src="../../assets/img/cover-bg.png"
            alt="Illustration"
            class="w-full rounded-lg object-cover h-full z-0 absolute"
            loading="lazy"
          />
        </div>
      </UPageCard>
    </template>
  </UPageSection>
</template>
