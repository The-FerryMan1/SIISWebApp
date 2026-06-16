<script setup lang="ts">
import type { AuthFormField, FormSubmitEvent } from '@nuxt/ui';
import z from 'zod';
import { UseAuthStore } from '../../stores/auth';
import { useDebounceFn } from '@vueuse/core';
import { useRouter } from 'vue-router';

const auth = UseAuthStore();
const toast = useToast();
const router = useRouter();

const fields: AuthFormField[] = [
    {
        name: 'email',
        type: 'email',
        autofocus: true,
        autocomplete: 'on',
        label: 'Email',
        placeholder: 'Enter your email',
        required: true
    },
    {
        name: 'password',
        type: 'password',
        label: 'Password',
        placeholder: 'Enter your password',
        required: true
    },
];


const schema = z.object({
    email: z.email({error: 'Invalid email'}),
    password: z.string({error: 'Invalid password'}).min(1, {error: 'Password is required'}),
});

type Schema = z.infer<typeof schema>;

const onSubmit = async(paylaod: FormSubmitEvent<Schema>)=>{
    try {
        await auth.useLogin(paylaod.data);
        toast.add({description:'Login successfully', color: 'success'})
        router.push({name: 'dashboard'});
    } catch (error) {
        toast.add({description:'Login failed: Invalid Credentials', color: 'error'})
    }
}

const debounceSubmit = useDebounceFn(onSubmit, 1000)
</script>

<template>
    <UPageSection>
        <template #body>
            <UPageCard orientation="horizontal" :reverse="true" variant="outline">
                <UAuthForm :schema :fields icon="i-lucide-lock" title="Login"
                    description="Enter your credentials to access your account."
                    loading-auto
                    @submit="debounceSubmit"
                    >
                </UAuthForm>

                <img src="https://picsum.photos/704/1294" alt="Illustration"
                    class="w-full rounded-lg object-cover size-140 " loading="lazy" />
            </UPageCard>
        </template>


    </UPageSection>
</template>