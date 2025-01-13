import defaultTheme from 'tailwindcss/defaultTheme';
import forms from '@tailwindcss/forms';
import typography from '@tailwindcss/typography';
import containerQueries from '@tailwindcss/container-queries';
import tailwindcssPrimeUi from 'tailwindcss-primeui';

/** @type {import('tailwindcss').Config} */
export default {
    content: [
        './src/**/*.{html,js,vue}',
    ],

    darkMode: 'app-dark', /* Until we support dark mode, its a manual toggle */

    theme: {
        extend: {
            fontFamily: {
                sans: ['Nunito', ...defaultTheme.fontFamily.sans],
            },
        },
    },

    plugins: [forms, typography, containerQueries, tailwindcssPrimeUi],
};
