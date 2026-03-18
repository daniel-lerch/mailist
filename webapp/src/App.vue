<template>
  <div class="flex flex-col min-h-screen">
    <header>
      <Menubar :model="items" breakpoint="480px" class="m-2">
        <template #start>
          <router-link to="/" class="flex items-center gap-2 no-underline">
            <img src="/brand.png" alt="Logo" width="24" height="24" class="inline-block" />
            <span class="text-2xl font-semibold text-inherit">Mailist</span>
          </router-link>
        </template>

        <template #item="{ item, props }">
          <router-link v-slot="{ href, navigate }" :to="item.route" custom>
            <a v-bind="props.action" :href="href" @click="navigate">
              <span v-if="item.icon" :class="item.icon"></span>
              <span>{{ item.label }}</span>
            </a>
          </router-link>
        </template>
      </Menubar>
    </header>
    <main class="grow">
      <ErrorBoundary>
        <Suspense>
          <template #default>
            <router-view v-bind="$attrs" />
          </template>
          <template #fallback>
            <div>Loading...</div>
          </template>
        </Suspense>
      </ErrorBoundary>
    </main>
    <footer>
      <div class="max-w-7xl mx-auto px-4 py-3 text-gray-600">
        <small class="text-sm text-gray-500">
          Copyright &copy; 2022-2026 Daniel Lerch
        </small>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import ErrorBoundary from "@/components/ErrorBoundary.vue"
import Menubar from "primevue/menubar"
import { churchtoolsClient } from "@churchtools/churchtools-client"
import { ref } from "vue"

const churchtoolsUrl = window.settings?.base_url ?? import.meta.env.VITE_CHURCHTOOLS_URL
churchtoolsClient.setBaseUrl(churchtoolsUrl)

const items = ref([
  {
    label: "E-Mail-Verteiler",
    icon: "pi pi-fw pi-inbox",
    route: "/",
  },
  {
    label: "Einstellungen",
    icon: "pi pi-fw pi-cog",
    route: "/setup",
  },
])

</script>
