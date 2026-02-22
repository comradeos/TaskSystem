import { http } from "./http"

export const tasksApi = {

    getByProject: async (projectId, params = {}) => {

        const filteredParams = Object.entries(params)
            .filter(([_, value]) =>
                value !== undefined &&
                value !== "" &&
                value !== null
            )
            .reduce((acc, [key, value]) => {
                acc[key] = value
                return acc
            }, {})

        const query = new URLSearchParams(filteredParams).toString()

        return await http.get(
            `/api/tasks/${projectId}${query ? `?${query}` : ""}`
        )
    },

    getById: async (id) => {
        return await http.get(`/api/tasks/by-id/${id}`)
    },

    update: async (id, data) => {
        return await http.put(`/api/tasks/${id}`, data)
    },

    create: async (data) => {
        return await http.post("/api/tasks", data)
    }
}