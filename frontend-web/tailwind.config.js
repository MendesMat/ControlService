/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: 'hsl(221, 83%, 53%)',
          hover: 'hsl(221, 83%, 45%)',
          soft: 'hsl(221, 83%, 95%)',
          muted: 'hsl(221, 83%, 96%)',
        },
        surface: {
          DEFAULT: 'hsl(0, 0%, 100%)',
          app: 'hsl(215, 18%, 96%)',
          sidebar: 'hsl(0, 0%, 100%)',
        },
        content: {
          primary: 'hsl(215, 25%, 12%)',
          secondary: 'hsl(215, 14%, 48%)',
          tertiary: 'hsl(215, 12%, 65%)',
        },
        border: {
          DEFAULT: 'hsl(215, 15%, 88%)',
          subtle: 'hsl(215, 15%, 93%)',
        },
        status: {
          success: 'hsl(142, 71%, 45%)',
          'success-bg': 'hsl(142, 71%, 95%)',
          alert: 'hsl(38, 92%, 50%)',
          'alert-bg': 'hsl(38, 92%, 94%)',
          error: 'hsl(348, 83%, 47%)',
          'error-bg': 'hsl(348, 83%, 96%)',
          info: 'hsl(221, 83%, 53%)',
          'info-bg': 'hsl(221, 83%, 95%)',
          pending: 'hsl(38, 92%, 50%)',
          'pending-bg': 'hsl(38, 92%, 94%)',
        },
      },
      fontFamily: {
        sans: ['Inter', 'Roboto', 'Segoe UI', 'sans-serif'],
      },
      fontSize: {
        '2xs': ['11px', { lineHeight: '1.4', letterSpacing: '0.04em' }],
        'xs':  ['12px', { lineHeight: '1.5' }],
        'sm':  ['14px', { lineHeight: '1.5' }],
        'base':['16px', { lineHeight: '1.6' }],
        'lg':  ['18px', { lineHeight: '1.5' }],
        'xl':  ['20px', { lineHeight: '1.4' }],
        '2xl': ['24px', { lineHeight: '1.3' }],
        '3xl': ['30px', { lineHeight: '1.2' }],
      },
      boxShadow: {
        'soft':        '0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04)',
        'composed':    '0 4px 6px -1px rgba(0,0,0,0.06), 0 2px 4px -2px rgba(0,0,0,0.04)',
        'card':        '0 1px 2px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.04)',
        'inner-top':   'inset 0 1px 0 rgba(255,255,255,0.15)',
        'sidebar':     '1px 0 0 0 hsl(215,15%,88%)',
      },
    },
  },
  plugins: [],
}
