import { defineConfig } from 'astro/config';
export default defineConfig({
  site: 'https://dayronobregon.github.io/wallanoti',
  // Integrations: if you want to enable Starlight, add it back and ensure
  // the package is available in your registry or use a git/tarball install.
  outDir: './dist'
});
