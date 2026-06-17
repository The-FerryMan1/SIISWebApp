import { defineStore } from "pinia";
import { ref } from "vue";
import { useAxios } from "../fetch/axios";




export type Applicaton = {
    id: number,
    applicationUUID: string,
    fullName: string,
    status: string,
    createdAt: Date,
    updatedAt: Date | null
}



export const useApplicationStore = defineStore('applicaton', ()=>{
    const applications = ref<Applicaton[] | null>([]);



    const applicationInit = async() =>{
        await getAllAsync();
    }

    const getAllAsync = async()=>{
        try {
            const {data} = await useAxios.get('/application')
            applications.value = data
        } catch (error) {
            console.log(error)
        }
    }

    return {
        applications,
        applicationInit,
        getAllAsync
    }
})