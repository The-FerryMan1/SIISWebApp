import { defineStore } from "pinia";
import { ref } from "vue";
import { useAxios } from "../fetch/axios";
import type { AxiosError } from "axios";


export interface RegistrationToken {
    id: number,
    uuid: string,
    expDate: string,
    createdAt: string,
}

export const useRegistrationToken = defineStore('registration-token', () => {
    
    const tokens = ref<RegistrationToken[]>()
    
    const registrationTokenError = ref()
    
    const GetAllTokens = async()=>{
        try {
            const {data} = await useAxios('/registrationtoken')
            tokens.value = data
        } catch (error) {
            const err = error as AxiosError
            registrationTokenError.value = err
            console.log(err)
        }    
    }

    const createRegistrationToken = async (expDate: {expDate: string}) =>{
        try {
            await useAxios.post("/registrationtoken", expDate)
        } catch (error) {
            const err = error as AxiosError
            registrationTokenError.value = err
            console.log(err)
        }
    }

    return {
        tokens,
        registrationTokenError,
        GetAllTokens,
        createRegistrationToken
    }

})