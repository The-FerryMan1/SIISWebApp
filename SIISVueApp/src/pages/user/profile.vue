<script setup lang="ts">
import { useRouter } from 'vue-router'
import { UseAuthStore } from '../../stores/auth'
import { storeToRefs } from 'pinia'
import z from 'zod'
import type { FormSubmitEvent } from '@nuxt/ui'
import { ref, useTemplateRef } from 'vue'
import { useDebounceFn } from '@vueuse/core'
import ConfirmationModal from '../../components/confirmationModal.vue'
import { useAxios } from '../../fetch/axios.ts'
import type { AxiosError } from 'axios'

const router = useRouter()
const auth = UseAuthStore()
const { user } = storeToRefs(auth)

const toast = useToast()
const overlay = useOverlay()
const confirmationModal = overlay.create(ConfirmationModal)

const profileSubmitLoading = ref<boolean>(false)
const changePassLoading = ref<boolean>(false)

const back = () => {
  router.back()
}

//edit profile
const profileSchema = z.object({
  username: z.string('invalid username').min(1, 'username is required'),
  email: z.email('invalid email'),
})
type ProfileSchema = z.infer<typeof profileSchema>

const profileState = ref<Partial<ProfileSchema>>({
  email: user.value?.email,
  username: user.value?.username,
})

const profileSubmit = async (event: FormSubmitEvent<ProfileSchema>) => {
  const confirm = await confirmationModal.open({
    title: 'Update user information',
    description: 'Are you sure?',
  })

  if (confirm) {
    try {
      profileSubmitLoading.value = true
      await profileRequest(event.data)
      toast.add({ title: 'User information updated successfully', color: 'success' })
    } catch (error) {
      console.log(error)
    } finally {
      profileSubmitLoading.value = false
    }
  }
}

const profileRequest = useDebounceFn(async (payload: ProfileSchema) => {
  await useAxios.put('/user', payload)
  await auth.useVerify()
})

//change password
const changePassForm = useTemplateRef('changePassForm')
const changePassSchema = z
  .object({
    currentPassword: z.string('Invalid password'),
    newPassword: z.string('Invalid password').min(8, 'Enter alteast 8 characters'),
    confirm: z.string('Invalid password').min(8, 'Enter alteast 8 characters'),
  })
  .refine((data) => data.newPassword == data.confirm, {
    path: ['confirm'],
    error: "Password don't match",
  })

type ChangePassSchema = z.infer<typeof changePassSchema>

const changePassState = ref<Partial<ChangePassSchema>>({
  currentPassword: undefined,
  newPassword: undefined,
  confirm: undefined,
})

const changePassSubmit = async (event: FormSubmitEvent<ChangePassSchema>) => {
  const confirm = await confirmationModal.open()

  if (confirm) {
    try {
      changePassLoading.value = true
      await changePassRequest(event.data)
      toast.add({ title: 'Password changed successfully', color: 'success' })
    } catch (error) {
      const status = error as AxiosError
      if (status.status == 401) {
        toast.add({ title: 'Operation Failed, wrong current password', color: 'error' })
      } else {
        toast.add({ title: 'Operation Failed', color: 'error' })
      }
      console.log(error)
    } finally {
      profileSubmitLoading.value = false
      changePassState.value = {
        confirm: undefined,
        currentPassword: undefined,
        newPassword: undefined,
      }
    }
  }
}

const changePassRequest = useDebounceFn(async (payload: ChangePassSchema) => {
  await useAxios.put('/user/change-password', {
    currentPassword: payload.currentPassword,
    newPassword: payload.newPassword,
  })
  await auth.useVerify()
}, 500)
</script>

  <template>
    <UMain class="space-y-8">
      <div>
        <UButton variant="ghost" label="Back" icon="i-lucide-arrow-left" @click="back" />
      </div>
      <div>
        <h1 class="text-4xl font-black text-primary tracking-tight">Profile</h1>
        <p class="text-muted text-sm mt-1">Manage your account settings and preferences</p>
      </div>

      <div class="grid gap-6 lg:grid-cols-2">
        <UCard title="Edit Profile" variant="outline">
          <UForm
            :schema="profileSchema"
            :state="profileState"
            v-if="user"
            class="space-y-4"
            @submit="profileSubmit"
            loading-auto
          >
            <UFormField name="username" label="Username" required>
              <UInput v-model="profileState.username" placeholder="Enter new username" class="w-full" />
            </UFormField>

            <UFormField name="email" label="Email" required>
              <UInput
                v-model="profileState.email"
                placeholder="Enter new email"
                class="w-full"
                disabled
              />
            </UFormField>

            <div class="w-full flex justify-end items-center">
              <UButton type="submit" label="Save Changes" icon="i-lucide-save" color="primary" />
            </div>
          </UForm>
        </UCard>

        <UCard title="Change Password" variant="outline">
          <UForm
            ref="changePassForm"
            :state="changePassState"
            :schema="changePassSchema"
            @submit="changePassSubmit"
            :loading="changePassLoading"
            class="space-y-4"
            @error="(err) => console.log(err)"
          >
            <UFormField name="currentPassword" label="Current Password" required>
              <UInput
                type="password"
                v-model="changePassState.currentPassword"
                placeholder="Enter current password"
                class="w-full"
              />
            </UFormField>
            <UFormField name="newPassword" label="New Password" required>
              <UInput
                type="password"
                v-model="changePassState.newPassword"
                placeholder="Enter new password"
                class="w-full"
              />
            </UFormField>
            <UFormField name="confirm" label="Confirm Password" required>
              <UInput
                type="password"
                v-model="changePassState.confirm"
                placeholder="Confirm new password"
                class="w-full"
              />
            </UFormField>
            <div class="w-full flex justify-end items-center">
              <UButton type="submit" label="Change Password" icon="i-lucide-lock" color="primary" />
            </div>
          </UForm>
        </UCard>
      </div>
    </UMain>
  </template>
