import { defineConfig } from 'astro/config';
import starlight from '@starlight/astro';

export default defineConfig({
  site: 'https://dayronobregon.github.io/wallanoti',
  integrations: [starlight()],
  outDir: './dist'
});
