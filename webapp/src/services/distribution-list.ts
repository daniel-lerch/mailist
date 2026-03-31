import { useExtensionStore } from "@/stores/extension"

export type CreateDistributionList = {
  alias: string
  flags: {
    overrideRecipient: boolean
    spamFilter: boolean
  }
  recipientsQuery: unknown | null
  sendersQuery: unknown | null
}

export type DistributionList = CreateDistributionList & {
  id: number
  recipientCount: number
  senderCount: number
}

export async function fetchWithAuth(input: RequestInfo, init?: RequestInit): Promise<Response> {
  const extension = useExtensionStore()

  // clone init so we can safely modify headers
  const initCopy: RequestInit = init ? { ...init } : {}
  const headers: HeadersInit = initCopy.headers ? { ...(initCopy.headers as HeadersInit) } : {}
  ;(headers as Record<string, string>)["Authorization"] = `Bearer ${extension.accessToken}`
  initCopy.headers = headers

  let response = await fetch(input, initCopy)
  if (response.status === 401) {
    // try to re-login once and retry
    await extension.login()
    ;(headers as Record<string, string>)["Authorization"] = `Bearer ${extension.accessToken}`
    initCopy.headers = headers
    response = await fetch(input, initCopy)
    if (response.status === 401)
      throw new Error("Unauthorized: Access token is invalid even after re-login")
  }

  return response
}

export async function getDistributionLists(): Promise<DistributionList[]> {
  const extension = useExtensionStore()
  const url = `${extension.backendUrl}/api/distribution-lists`
  const response = await fetchWithAuth(url)
  if (!response.ok) {
    const text = await response.text()
    throw new Error(`Failed to fetch distribution lists: ${response.status} ${text}`)
  }
  return await response.json()
}

export async function getDistributionList(id: number): Promise<DistributionList> {
  const extension = useExtensionStore()
  const url = `${extension.backendUrl}/api/distribution-lists/${id}`
  const response = await fetchWithAuth(url)
  if (!response.ok) {
    const text = await response.text()
    throw new Error(`Failed to fetch distribution list: ${response.status} ${text}`)
  }
  return await response.json()
}

export async function createDistributionList(
  request: CreateDistributionList
): Promise<DistributionList> {
  const extension = useExtensionStore()
  const url = `${extension.backendUrl}/api/distribution-lists`
  const response = await fetchWithAuth(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  })

  if (response.status === 201) {
    return await response.json()
  }

  const text = await response.text()
  throw new Error(`Failed to create distribution list: ${response.status} ${text}`)
}

export async function modifyDistributionList(
  id: number,
  request: CreateDistributionList
): Promise<DistributionList> {
  const extension = useExtensionStore()
  const url = `${extension.backendUrl}/api/distribution-lists/${id}`
  const response = await fetchWithAuth(url, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  })

  if (response.ok) {
    return await response.json()
  }

  const text = await response.text()
  throw new Error(`Failed to modify distribution list: ${response.status} ${text}`)
}

export async function deleteDistributionList(id: number): Promise<void> {
  const extension = useExtensionStore()
  const url = `${extension.backendUrl}/api/distribution-lists/${id}`
  const response = await fetchWithAuth(url, { method: "DELETE" })

  if (response.status === 204) {
    return
  }

  const text = await response.text()
  throw new Error(`Failed to delete distribution list: ${response.status} ${text}`)
}
