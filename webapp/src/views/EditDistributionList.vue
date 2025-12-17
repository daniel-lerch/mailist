<template>
  <div class="px-2 max-w-3xl mx-auto">
    <h1 v-if="initialValue === undefined" class="py-4 text-3xl">Neue Verteilerliste</h1>
    <h1 v-else class="py-4 text-3xl">Verteilerliste bearbeiten</h1>

    <form @submit.prevent="save">
      <div class="flex flex-col gap-2">
        <label for="alias">Alias</label>
        <InputGroup>
          <InputText v-model="alias" id="alias" fluid required />
          <InputGroupAddon>@example.org</InputGroupAddon>
        </InputGroup>
      </div>
      <Divider />
      <div class="flex items-center gap-3">
        <Checkbox v-model="newsletter" inputId="newsletter" binary />
        <label for="newsletter">Newsletter</label>
      </div>
      <Divider />
      <div class="mb-2">Empfänger</div>
      <div class="mb-4 rounded-lg bg-gray-100 dark:bg-gray-950">
        <SelectButton v-model="mode" :options="modes" optionLabel="label" :allowEmpty="false" />
        <PersonFilterEditor v-if="mode.name === 'default'" v-model="recipientsQuery" class="p-2" />
        <AdvancedFilterEditor v-else-if="mode.name === 'advanced'" v-model="recipientsQuery" class="p-2" />
      </div>
      <div class="mb-2">Erlaubte Absender</div>
      <div class="mb-4 rounded-lg bg-gray-100 dark:bg-gray-950">
        <SelectButton v-model="senderFilterMode" :options="modes" optionLabel="label" :allowEmpty="false" />
        <PersonFilterEditor v-if="senderFilterMode.name === 'default'" v-model="recipientsQuery" class="p-2" />
        <AdvancedFilterEditor v-else-if="senderFilterMode.name === 'advanced'" v-model="recipientsQuery" class="p-2" />
      </div>
      <Message v-if="error" severity="error" variant="simple" class="mb-4">{{ error }}</Message>
      <div class="flex flex-wrap gap-4">
        <Button type="button" label="Abbrechen" severity="secondary" variant="text" @click="cancel" />
        <Button type="submit" label="Speichern" :loading="loading" />
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import AdvancedFilterEditor from '@/components/AdvancedFilterEditor.vue';
import Button from 'primevue/button';
import Checkbox from 'primevue/checkbox';
import Divider from 'primevue/divider';
import InputGroup from 'primevue/inputgroup';
import InputGroupAddon from 'primevue/inputgroupaddon';
import InputText from 'primevue/inputtext';
import Message from 'primevue/message';
import SelectButton from 'primevue/selectbutton';
import PersonFilterEditor from '@/components/PersonFilterEditor.vue';
import { createDistributionList, getDistributionList, modifyDistributionList } from '@/services/distribution-list';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useExtensionStore } from '@/stores/extension';

const props = defineProps<{
  id?: string
}>()

const router = useRouter()

const extension = useExtensionStore()
if (extension.moduleId === 0) {
  await extension.load()
}
if (extension.accessToken === "") {
  await extension.login()
}

const initialValue = props.id ? await getDistributionList(parseInt(props.id)) : undefined

const modes = [{ name: "default", label: "Normaler Filter" }, { name: "advanced", label: "Erweiterter Filter (JSON)" }]
const mode = ref(modes[0]!)
const senderFilterMode = ref(modes[0]!)

const alias = ref(initialValue?.alias ?? "")
const newsletter = ref(initialValue?.newsletter ?? false)
const recipientsQuery = ref(initialValue?.recipientsQuery ?? null)

const loading = ref(false)
const error = ref<string | null>(null)

function cancel() {
  router.push("/")
}

async function save() {
  loading.value = true
  try {
    if (initialValue === undefined) {
      await createDistributionList({
        alias: alias.value,
        newsletter: newsletter.value,
        recipientsQuery: recipientsQuery.value,
      })
    } else {
      await modifyDistributionList(initialValue.id, {
        alias: alias.value,
        newsletter: newsletter.value,
        recipientsQuery: recipientsQuery.value,
      })
    }
    router.push("/")
  } catch (e) {
    error.value = e instanceof Error ? e.message : String(e)
  } finally {
    loading.value = false
  }
}
</script>
