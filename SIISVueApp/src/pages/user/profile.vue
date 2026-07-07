<script setup lang="ts">
import { useRouter } from 'vue-router';

const router = useRouter()

const back = () => {
    router.back()
}
</script>

<template>
    <UMain class="space-y-5">
        <div>
            <UButton variant="ghost" label="Back" icon="i-lucide-arrow-left" @click="back" />
        </div>
       

        

            
        <div>
            <h1 class="font-bold text-3xl text-primary my-10">Profile</h1>
        </div>
        <UCard title="Edit Profile">
            <UForm class="space-y-4">
                <UFormField label="Username">
                    <UInput class="w-full" />
                </UFormField>

                 <UFormField label="Email">
                    <UInput class="w-full" disabled/>
                </UFormField>

                <div class="w-full flex justify-end items-center my-8">
                    <UButton label="Save" icon="i-lucide-save"/>
                </div>
            </UForm>
        </UCard>

        <UCard title="Change Password">
            <UForm class="space-y-4">
                <UFormField label="Current Password">
                    <UInput  class="w-full" />
                </UFormField>
                <UFormField label="New Password">
                    <UInput  class="w-full" />
                </UFormField>
                <UFormField label="Confirm Password">
                    <UInput  class="w-full" />
                </UFormField>

                <div class="w-full flex justify-end items-center my-8">
                    <UButton label="Change Password" icon="i-lucide-save"/>
                </div>
            </UForm>
        </UCard>

      
    </UMain>

</template>