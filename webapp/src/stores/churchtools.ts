import type { Group, Person, Status } from "@/utils/ct-types"
import { churchtoolsClient } from "@churchtools/churchtools-client"
import { defineStore } from "pinia"

export const useChurchtoolsStore = defineStore("churchtools", {
  state: () => ({
    timestamp: 0,
    groups: [] as {
      id: number
      name: string
      roles: { id: number; name: string; groupTypeRoleId: number }[]
    }[],
    persons: [] as { id: number; name: string }[],
    statuses: [] as { id: number; name: string }[],
  }),
  actions: {
    async refreshIfInvalid() {
      if (Date.now() - this.timestamp > 5 * 60 * 1000) {
        const groupsTask = churchtoolsClient
          .getAllPages<Group>("/groups", { include: ["roles"] })
          .then(
            (data) =>
              (this.groups = data.map((g) => ({
                id: g.id,
                name: g.name,
                roles:
                  g.roles?.map((r) => ({
                    id: r.id,
                    name: r.nameTranslated,
                    groupTypeRoleId: r.groupTypeRoleId,
                  })) || [],
              })))
          )

        const personsTask = churchtoolsClient.getAllPages<Person>("/persons").then(
          (data) =>
            (this.persons = data.map((p) => {
              if (p.nickname)
                return { id: p.id, name: `${p.firstName} (${p.nickname}) ${p.lastName}` }
              else return { id: p.id, name: `${p.firstName} ${p.lastName}` }
            }))
        )

        const statusesTask = churchtoolsClient
          .get<Status[]>("/statuses")
          .then((data) => (this.statuses = data.map((s) => ({ id: s.id, name: s.nameTranslated }))))

        await Promise.all([groupsTask, personsTask, statusesTask])

        this.timestamp = Date.now()
      }
    },
  },
})
