<template>
  <div class="flex flex-col gap-2">
    <div v-if="filters.length === 0" class="py-2 mx-auto">
      <Message v-if="props.modelValue.isAdvancedFilter()" severity="warn" variant="simple"
        icon="pi pi-exclamation-triangle">
        Wenn du hier Filterkriterien hinzufügst, wird der erweiterte Filter überschrieben.
      </Message>
      <span v-else class="italic">
        Füge Filterkriterien hinzu um Empfänger auszuwählen.
      </span>
    </div>
    <div v-for="(filter, index) in filters" :key="index">
      <div v-if="filter.kind === 'group'">
        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-users"></i>
          </InputGroupAddon>
          <Select v-model="filter.groupId" :options="groups" filter optionLabel="name" optionValue="id"
            placeholder="Gruppe auswählen" />
          <InputGroupAddon>
            <Button type="button" icon="pi pi-trash" severity="danger" variant="text" @click="removeFilter(index)" />
          </InputGroupAddon>
        </InputGroup>
        <RoleCheckboxGroup v-model="filter.roleIds" :roles="groups.find(g => g.id === filter.groupId)?.roles || []" />
      </div>
      <div v-else-if="filter.kind === 'person'">
        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-user"></i>
          </InputGroupAddon>
          <Select v-model="filter.personId" :options="persons" filter optionLabel="name" optionValue="id"
            placeholder="Person auswählen" />
          <InputGroupAddon>
            <Button type="button" icon="pi pi-trash" severity="danger" variant="text" @click="removeFilter(index)" />
          </InputGroupAddon>
        </InputGroup>
      </div>
      <div v-else>
        <InputGroup>
          <InputGroupAddon>
            <i class="pi pi-id-card"></i>
          </InputGroupAddon>
          <Select v-model="filter.statusId" :options="statuses" optionLabel="name" optionValue="id"
            placeholder="Status auswählen" />
          <InputGroupAddon>
            <Button type="button" icon="pi pi-trash" severity="danger" variant="text" @click="removeFilter(index)" />
          </InputGroupAddon>
        </InputGroup>
      </div>
    </div>
    <ButtonGroup>
      <Button type="button" label="Gruppe" icon="pi pi-plus" variant="text" @click="addGroupFilter" />
      <Button type="button" label="Person" icon="pi pi-plus" variant="text" @click="addSinglePersonFilter" />
      <Button type="button" label="Status" icon="pi pi-plus" variant="text" @click="addStatusFilter" />
    </ButtonGroup>
  </div>
</template>

<script setup lang="ts">
import { MailistFilter, type GroupFilter, type SinglePersonFilter, type StatusFilter } from "@/services/churchquery";
import type { Group, GroupRole, Person, Status } from "@/utils/ct-types";
import { churchtoolsClient } from "@churchtools/churchtools-client";
import Button from "primevue/button";
import ButtonGroup from "primevue/buttongroup";
import InputGroup from "primevue/inputgroup";
import InputGroupAddon from "primevue/inputgroupaddon";
import Message from "primevue/message";
import Select from "primevue/select";
import { onMounted, ref, watch } from "vue";
import RoleCheckboxGroup from "./RoleCheckboxGroup.vue";

const props = defineProps<{ modelValue: MailistFilter }>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: MailistFilter): void
}>()

const filters = ref<(SinglePersonFilter | GroupFilter | StatusFilter)[]>(
  props.modelValue.parsedFilters?.slice() || []
)

const groups = ref<{ id: number, name: string, roles?: GroupRole[] }[]>([])
const persons = ref<{ id: number, name: string }[]>([])
const statuses = ref<{ id: number, name: string }[]>([])

onMounted(() => {
  churchtoolsClient.getAllPages<Group>("/groups", { include: ["roles"] })
    .then((data) => groups.value = data.map(g => ({ id: g.id, name: g.name, roles: g.roles })))

  churchtoolsClient.getAllPages<Person>("/persons")
    .then((data) => persons.value = data.map(p => {
      if (p.nickname)
        return { id: p.id, name: `${p.firstName} (${p.nickname}) ${p.lastName}` }
      else
        return { id: p.id, name: `${p.firstName} ${p.lastName}` }
    }))

  churchtoolsClient.get<Status[]>("/statuses")
    .then((data) => statuses.value = data.map(s => ({ id: s.id, name: s.nameTranslated })))
})

watch(filters, (newValue) => {
  const filter = MailistFilter.create(newValue)
  emit('update:modelValue', filter)
}, { deep: true })

function addGroupFilter() {
  filters.value.push({ kind: "group", groupId: 0, roleIds: [] })
}

function addSinglePersonFilter() {
  filters.value.push({ kind: "person", personId: 0 })
}

function addStatusFilter() {
  filters.value.push({ kind: "status", statusId: 0 })
}

function removeFilter(index: number) {
  filters.value.splice(index, 1)
}
</script>