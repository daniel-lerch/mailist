import { z } from "zod"

export type SinglePersonFilter = {
  kind: "person"
  personId: number
}

export type GroupFilter = {
  kind: "group"
  groupId: number
  roleIds: number[]
}

export type StatusFilter = {
  kind: "status"
  statusId: number
}

export type PersonFilter = SinglePersonFilter | GroupFilter | StatusFilter

export class MailistFilter {
  readonly query: unknown
  readonly parsedFilters: ReadonlyArray<PersonFilter> | null

  private constructor(query: unknown, parsedFilters: PersonFilter[] | null) {
    this.query = query
    this.parsedFilters = parsedFilters
  }

  static create(filters: PersonFilter[]) {
    return new MailistFilter(getChurchQueryFilter(filters), filters)
  }

  static parse(query: unknown) {
    return new MailistFilter(query, getMailistFilters(query))
  }

  isEmpty() {
    return this.query === null
  }

  isAdvancedFilter() {
    return this.query !== null && this.parsedFilters === null
  }
}

function varObj(name: string) {
  return z.object({ var: z.literal(name) })
}

function getGroupFilter(filter: unknown[]): GroupFilter | null {
  const groupFilter = z
    .tuple([
      z.object({ "==": z.tuple([varObj("ctgroup.id"), z.string()]) }),
      z.object({ "==": z.tuple([varObj("groupmember.groupMemberStatus"), z.literal("active")]) }),
    ])
    .rest(z.unknown())
  const groupFilterResult = groupFilter.safeParse(filter)
  if (!groupFilterResult.success) {
    return null
  }

  const roleIds: number[] = []

  const remainingFilters = groupFilterResult.data.slice(2)
  if (remainingFilters.length > 0) {
    const groupRoleFilter = z.tuple([
      z.object({ oneof: z.tuple([varObj("role.id"), z.array(z.string())]) }),
    ])
    const groupRoleFilterResult = groupRoleFilter.safeParse(remainingFilters)
    if (!groupRoleFilterResult.success) {
      return null
    }

    roleIds.push(...groupRoleFilterResult.data[0].oneof[1].map((id) => parseInt(id)))
  }

  return {
    kind: "group",
    groupId: parseInt(groupFilterResult.data[0]["=="][1]),
    roleIds,
  }
}

function getSinglePersonFilter(filter: unknown[]): SinglePersonFilter | null {
  const singlePersonFilter = z.tuple([
    z.object({ "==": z.tuple([varObj("person.id"), z.string()]) }),
  ])
  const singlePersonFilterResult = singlePersonFilter.safeParse(filter)
  if (!singlePersonFilterResult.success) {
    return null
  }
  return { kind: "person", personId: parseInt(singlePersonFilterResult.data[0]["=="][1]) }
}

function getStatusFilter(filter: unknown[]): StatusFilter | null {
  const statusFilter = z.tuple([
    z.object({ "==": z.tuple([varObj("person.statusId"), z.string()]) }),
  ])
  const statusFilterResult = statusFilter.safeParse(filter)
  if (!statusFilterResult.success) {
    return null
  }
  return { kind: "status", statusId: parseInt(statusFilterResult.data[0]["=="][1]) }
}

function getPersonFilter(filter: unknown[]): PersonFilter | null {
  const groupFilter = getGroupFilter(filter)
  if (groupFilter !== null) return groupFilter

  const singlePersonFilter = getSinglePersonFilter(filter)
  if (singlePersonFilter !== null) return singlePersonFilter

  return getStatusFilter(filter)
}

export function getMailistFilters(query: unknown): PersonFilter[] | null {
  const topLevelAnd = z.object({
    and: z
      .tuple([
        z.object({ "==": z.tuple([varObj("person.isArchived"), z.literal(0)]) }),
        z.object({ isnull: z.tuple([varObj("person.dateOfDeath")]) }),
      ])
      .rest(z.unknown()),
  })
  const topLevelAndResult = topLevelAnd.safeParse(query)
  if (!topLevelAndResult.success) {
    return null
  }

  const remainingFilters = topLevelAndResult.data.and.slice(2)

  const secondLevelOr = z.tuple([
    z.object({
      or: z.array(z.object({ and: z.array(z.unknown()) })),
    }),
  ])
  const secondLevelOrResult = secondLevelOr.safeParse(remainingFilters)
  if (secondLevelOrResult.success) {
    const personFilters = []
    for (const x of secondLevelOrResult.data[0].or) {
      const personFilter = getPersonFilter(x.and)
      if (personFilter === null) {
        return null
      }
      personFilters.push(personFilter)
    }
    return personFilters
  } else {
    const personFilter = getPersonFilter(remainingFilters)
    if (personFilter === null) {
      return null
    }
    return [personFilter]
  }
}

function getChurchQueryFilterPart(filter: PersonFilter): unknown[] {
  if (filter.kind === "group") {
    const group: unknown[] = [
      {
        "==": [{ var: "ctgroup.id" }, `${filter.groupId}`],
      },
      {
        "==": [{ var: "groupmember.groupMemberStatus" }, "active"],
      },
    ]
    if (filter.roleIds.length > 0) {
      group.push({
        oneof: [{ var: "role.id" }, filter.roleIds.map((id) => `${id}`)],
      })
    }
    return group
  } else if (filter.kind === "person") {
    return [
      {
        "==": [{ var: "person.id" }, `${filter.personId}`],
      },
    ]
  } else {
    return [
      {
        "==": [{ var: "person.statusId" }, `${filter.statusId}`],
      },
    ]
  }
}

export function getChurchQueryFilter(filters: PersonFilter[]) {
  if (filters.length === 0) {
    return null
  } else if (filters.length === 1) {
    return {
      and: [
        { "==": [{ var: "person.isArchived" }, 0] },
        { isnull: [{ var: "person.dateOfDeath" }] },
        ...getChurchQueryFilterPart(filters[0]!),
      ],
    }
  } else {
    return {
      and: [
        { "==": [{ var: "person.isArchived" }, 0] },
        { isnull: [{ var: "person.dateOfDeath" }] },
        { or: filters.map((filter) => ({ and: getChurchQueryFilterPart(filter) })) },
      ],
    }
  }
}
