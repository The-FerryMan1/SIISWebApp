import axios from 'axios'

export const useAxios = axios.create({
  baseURL: 'http://localhost:5233',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})
