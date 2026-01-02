import { useChurchtoolsStore } from "@/stores/churchtools"
import {
  getMailistFilters,
  type GroupFilter,
  type SinglePersonFilter,
  type StatusFilter,
} from "./filter"

export type SinglePersonFilterWithNames = SinglePersonFilter & {
  name: string | null
}

export type GroupFilterWithNames = GroupFilter & {
  name: string | null
  roles: (string | null)[]
}

export type StatusFilterWithNames = StatusFilter & {
  name: string | null
}

export type PersonFilterWithNames =
  | SinglePersonFilterWithNames
  | GroupFilterWithNames
  | StatusFilterWithNames

/*
export class MailistFilterWithNames {
  readonly query: unknown
  readonly parsedFilters: ReadonlyArray<PersonFilterWithNames> | null

  private constructor(query: unknown, parsedFilters: PersonFilterWithNames[] | null) {
    this.query = query
    this.parsedFilters = parsedFilters
  }

  static parse(query: unknown) {
    
  }

  isAdvancedFilter() {
    return this.query !== null && this.parsedFilters === null
  }
}
*/

export async function getMailistFiltersWithNames(
  query: unknown
): Promise<PersonFilterWithNames[] | null> {
  const filters = getMailistFilters(query)
  if (filters === null) {
    return null
  }

  const cache = useChurchtoolsStore()
  await cache.refreshIfInvalid()

  const filtersWithNames = []

  for (const filter of filters) {
    if (filter?.kind === "group") {
      const group = cache.groups.find((g) => g.id === filter.groupId)
      filtersWithNames.push({
        ...filter,
        name: group?.name ?? null,
        roles: filter.roleIds.map(
          (roleId) => group?.roles?.find((role) => role.groupTypeRoleId === roleId)?.name ?? null
        ),
      })
    } else if (filter?.kind === "person") {
      filtersWithNames.push({
        ...filter,
        name: cache.persons.find((p) => p.id === filter.personId)?.name ?? null,
      })
    } else {
      filtersWithNames.push({
        ...filter,
        name: cache.statuses.find((s) => s.id === filter.statusId)?.name ?? null,
      })
    }
  }

  return filtersWithNames
}
