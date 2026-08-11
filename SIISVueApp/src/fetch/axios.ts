import axios from 'axios'

export const useAxios = axios.create({
  baseURL: '/api/',
  withCredentials: true,
})
