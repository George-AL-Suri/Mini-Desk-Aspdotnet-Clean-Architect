
import "./styles/style.css";

const app = document.querySelector<HTMLDivElement>("#app");

if (!app) {
  throw new Error("App root element was not found.");
}

app.innerHTML = `
  <main class="min-h-screen bg-slate-50 p-8 text-slate-900">
    <h1 class="text-3xl font-bold">MiniDesk</h1>
    <p class="mt-2 text-slate-600">
      Project setup is ready.
    </p>
  </main>
`;