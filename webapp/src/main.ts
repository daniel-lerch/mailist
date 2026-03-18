import { createPinia } from "pinia"
import { createApp } from "vue"
import App from "./App.vue"
import PrimeVue from "primevue/config"
import Tooltip from "primevue/tooltip"
import ConfirmationService from "primevue/confirmationservice"
import Aura from "@primeuix/themes/aura"
import router from "./router"

const app = createApp(App)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
  },
})
app.directive("tooltip", Tooltip)
app.use(ConfirmationService)
app.use(router)
app.use(createPinia())
app.mount("#app")
