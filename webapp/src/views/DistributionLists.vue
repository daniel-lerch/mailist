<template>
  <div class="p-4">
    <ConfirmDialog />
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-3xl">E-Mail-Verteiler</h1>
      <Button asChild v-slot="slotProps" type="button" severity="primary">
        <RouterLink :to="`/new`" :class="slotProps.class">
          <i class="pi pi-plus"></i>
          Neuer Verteiler
        </RouterLink>
      </Button>
    </div>
    <DataTable :value="distributionLists" removableSort>
      <Column field="alias" header="Alias" sortable />
      <Column field="recipientCount" header="Anzahl Empfänger" sortable />
      <Column field="recipientsQuery" header="Empfänger">
        <template #body="slotProps">
          <PersonFilterCell :filter="slotProps.data.recipientsQuery" />
        </template>
      </Column>
      <Column field="senderCount" header="Erlaubte Absender" sortable>
        <template #body="slotProps">
          <span v-if="slotProps.data.sendersQuery">{{ slotProps.data.senderCount }}</span>
          <span v-else>-</span>
        </template>
      </Column>
      <Column field="sendersQuery" header="Absender">
        <template #body="slotProps">
          <PersonFilterCell v-if="slotProps.data.sendersQuery" :filter="slotProps.data.sendersQuery" />
          <span v-else><i class="pi pi-globe"></i> Alle</span>
        </template>
      </Column>
      <Column>
        <template #body="{ data }">
          <Button asChild v-slot="slotProps" type="button" severity="secondary" variant="text">
            <RouterLink :to="`/${data.id}`" :class="slotProps.class" class="p-button-icon-only">
              <i class="pi pi-pencil"></i>
            </RouterLink>
          </Button>
        </template>
      </Column>
      <Column>
        <template #body="{ data }">
          <Button type="button" icon="pi pi-trash" severity="danger" variant="text" @click="remove(data.id)" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import Column from "primevue/column"
import ConfirmDialog from "primevue/confirmdialog"
import Button from "primevue/button"
import DataTable from "primevue/datatable"
import { ref } from "vue"
import { getDistributionLists, deleteDistributionList } from "@/services/distribution-list"
import PersonFilterCell from "@/components/PersonFilterCell.vue"
import { useExtensionStore } from "@/stores/extension"
import { useConfirm } from "primevue/useconfirm"

const confirm = useConfirm()

const extension = useExtensionStore()
if (extension.moduleId === 0) {
  await extension.load()
}
if (extension.accessToken === "") {
  await extension.login()
}

const distributionLists = ref(await getDistributionLists())

function remove(id: number) {
  confirm.require({
    message: 'Möchtest du diesen Verteiler wirklich löschen?',
    header: 'Löschen bestätigen',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: 'Löschen',
    rejectLabel: 'Abbrechen',
    acceptClass: 'p-button-danger',
    accept: () => removeConfirmed(id),
  })
}

async function removeConfirmed(id: number) {
  try {
    await deleteDistributionList(id)
    distributionLists.value = distributionLists.value.filter((dl) => dl.id !== id)
  } catch (e) {
    console.log(e)
  }
}
</script>
