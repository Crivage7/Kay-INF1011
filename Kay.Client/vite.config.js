import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// Adresse de l'API en développement (profil « http » de launchSettings.json).
const API = process.env.KAY_API_URL || 'http://localhost:5180';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 56352,
        strictPort: true,
        // Écoute sur toutes les interfaces : un 2e ordinateur du réseau peut ouvrir http://<ip>:56352.
        host: true,
        // Le client appelle des URL relatives ; Vite les relaie vers l'API (pas de CORS en dev).
        proxy: {
            '/api': API,
            '/hubs': { target: API, ws: true },
        },
    },
})
