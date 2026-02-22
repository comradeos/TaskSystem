const API_BASE = "http://localhost:5001"

function getToken() {
    return localStorage.getItem("sessionToken")
}

function getErrorMessage(result, fallback) {
    if (result?.data?.detail) return result.data.detail
    if (result?.data?.title) return result.data.title
    if (result?.error?.detail) return result.error.detail
    if (result?.error?.title) return result.error.title

    return fallback
}

async function request(url, options = {}) {
    const token = getToken()

    const headers = {
        "Content-Type": "application/json",
        ...options.headers
    }

    if (token) {
        headers["X-Session-Token"] = token
    }

    const response = await fetch(`${API_BASE}${url}`, {
        ...options,
        headers
    })

    // 401: сессия умерла
    if (response.status === 401) {
        localStorage.removeItem("sessionToken")
        window.location.href = "/login"
        return
    }

    let result = null
    try {
        result = await response.json()
    } catch {
        throw new Error("Server returned invalid JSON")
    }

    if (!response.ok) {
        throw new Error(getErrorMessage(result, "Request failed"))
    }

    if (result?.result === false) {
        throw new Error(getErrorMessage(result, "Operation failed"))
    }

    return result.data
}

export const http = {
    get: (url) => request(url),

    post: (url, body) =>
        request(url, {
            method: "POST",
            body: JSON.stringify(body)
        }),

    put: (url, body) =>
        request(url, {
            method: "PUT",
            body: JSON.stringify(body)
        })
}