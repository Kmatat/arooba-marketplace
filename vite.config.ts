import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@auth': path.resolve(__dirname, './src/app/auth'),
      '@vendors': path.resolve(__dirname, './src/app/vendors'),
      '@products': path.resolve(__dirname, './src/app/products'),
      '@orders': path.resolve(__dirname, './src/app/orders'),
      '@customers': path.resolve(__dirname, './src/app/customers'),
      '@finance': path.resolve(__dirname, './src/app/finance'),
      '@admin': path.resolve(__dirname, './src/app/admin'),
      '@shared': path.resolve(__dirname, './src/app/shared'),
    },
  },
});
