import tailwindcss from 'tailwindcss';

/** @type {import('tailwindcss').Config} */
export default tailwindcss({
  content: [
    './index.html',
    './src/**/*.{vue,js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {},
  },
  plugins: [
    '@tailwindcss/forms',
  ],
});
