/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        // Arooba Brand Palette — inspired by Egyptian earth tones & craftsmanship
        arooba: {
          50: '#fef7ee',
          100: '#fdecd3',
          200: '#fad5a5',
          300: '#f6b86d',
          400: '#f19332',
          500: '#ee7711', // Primary brand orange
          600: '#df5d07',
          700: '#b94508',
          800: '#93370e',
          900: '#772f0f',
          950: '#401505',
        },
        earth: {
          50: '#f9f6f1',
          100: '#f0e9db',
          200: '#e2d2b8',
          300: '#d0b48e',
          400: '#c09a6c',
          500: '#b38455',
          600: '#a66f49',
          700: '#8a583e',
          800: '#714938',
          900: '#5d3d30',
          950: '#321f18',
        },
        nile: {
          50: '#eefbf4',
          100: '#d5f6e3',
          200: '#aeebcb',
          300: '#79daac',
          400: '#42c288',
          500: '#1fa76d',
          600: '#138858',
          700: '#106d48',
          800: '#10563b',
          900: '#0e4732',
          950: '#07281c',
        },
        sand: {
          50: '#fdfaf5',
          100: '#f9f1e3',
          200: '#f2e0c4',
          300: '#e9c99c',
          400: '#dfab72',
          500: '#d79352',
          600: '#c97d41',
          700: '#a76337',
          800: '#865032',
          900: '#6c432b',
          950: '#3a2115',
        },
      },
      fontFamily: {
        display: ['Cairo', 'sans-serif'],
        body: ['IBM Plex Sans Arabic', 'sans-serif'],
        mono: ['IBM Plex Mono', 'monospace'],
      },
      borderRadius: {
        'xl': '0.875rem',
        '2xl': '1rem',
        '3xl': '1.5rem',
      },
      animation: {
        'slide-up': 'slideUp 0.3s ease-out',
        'slide-down': 'slideDown 0.3s ease-out',
        'fade-in': 'fadeIn 0.4s ease-out',
        'pulse-soft': 'pulseSoft 2s ease-in-out infinite',
      },
      keyframes: {
        slideUp: {
          '0%': { transform: 'translateY(10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        slideDown: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        pulseSoft: {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.7' },
        },
      },
    },
  },
  plugins: [],
};
