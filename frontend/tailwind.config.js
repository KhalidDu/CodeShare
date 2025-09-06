/** @type {import('tailwindcss').Config} */
import forms from '@tailwindcss/forms'

export default {
  darkMode: "class",
  content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
  plugins: [
    forms,
  ],
}
