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
        <template #footer>
          <a href="/office-login">office login</a>
        </template>
      </UAuthForm>
    </UPageCard>
  </UMain>

  <!-- <div
          class="w-full h-125 flex justify-center items-start relative"
          :style="{
            backgroundImage: 'url(../../assets/img/cover-bg.png)',
            backgroundSize: 'cover',
            backgroundPosition: 'center'
          }"
        >
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
        </div> -->
</template>
