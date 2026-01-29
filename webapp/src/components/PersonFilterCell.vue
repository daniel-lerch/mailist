<template>
  <div v-if="props.filter !== null && parsedFilters === null">
    <i class="pi pi-code"></i>
    Erweiterter Filter
  </div>
  <div v-else-if="parsedFilters !== null">
    <div v-for="(f, index) in parsedFilters" :key="index" class="mb-2">
      <div v-if="f.kind === 'group'">
        <div v-if="f.name">
          <i class="pi pi-users"></i>
          {{ f.name }}
          <span class="text-sm text-gray-500">#{{ f.groupId }}</span>
          <div v-if="f.roles.length > 0">
            <Tag v-for="(role, idx) in f.roles" :key="f.roleIds[idx]" :severity="role ? 'secondary' : 'danger'"
              :value="role ?? `Unbekannte Rolle (ID: ${f.roleIds[idx]})`" rounded />
          </div>
        </div>
        <div v-else class="text-red-600">
          <i class="pi pi-users"></i>
          Unbekannte Gruppe (ID: {{ f.groupId }})
        </div>
      </div>
      <div v-else-if="f.kind === 'person'">
        <div v-if="f.name">
          <i class="pi pi-user"></i>
          {{ f.name }}
          <span class="text-sm text-gray-500">#{{ f.personId }}</span>
        </div>
        <div v-else class="text-red-600">
          <i class="pi pi-user"></i>
          Unbekannte Person (ID: {{ f.personId }})
        </div>
      </div>
      <div v-else>
        <div v-if="f.name">
          <i class="pi pi-id-card"></i>
          {{ f.name }}
          <span class="text-sm text-gray-500">#{{ f.statusId }}</span>
        </div>
        <div v-else class="text-red-600">
          <i class="pi pi-id-card"></i>
          Unbekannter Status (ID: {{ f.statusId }})
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import Tag from 'primevue/tag';
import { getMailistFiltersWithNames } from '@/services/filter-with-names';

const props = defineProps<{
  filter: unknown
}>()

const parsedFilters = await getMailistFiltersWithNames(props.filter)

</script>