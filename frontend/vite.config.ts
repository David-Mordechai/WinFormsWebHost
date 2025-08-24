import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  build: {
    outDir: '../WinFormsWebHost/AppUi',
    emptyOutDir: true,
    rollupOptions: {
      onwarn(warning, warn) {
        // Filter out PURE annotation warnings from signalr
        if (
          warning.message.indexOf('/*#__PURE__*/') !== -1 &&
          /@microsoft\/signalr/.test(warning.loc?.file || '')
        ) {
          return
        }
        warn(warning)
      }
    }
  }
})
