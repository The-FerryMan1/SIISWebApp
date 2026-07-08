import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import ui from '@nuxt/ui/vite';
import path from 'node:path';

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    ui( {
      ui:{
        colors:{
          primary: 'indigo'
        }
      }
    }),
    vueDevTools(),
  ],
  
  base: '/',
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  build: {
    outDir: fileURLToPath(new URL('../SIISMinimalAPI/wwwroot', import.meta.url)),
    emptyOutDir: true,
  },
})
