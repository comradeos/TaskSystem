import { http } from "./http"

export const projectsApi = {

    getAll: async () => {
        return await http.get("/api/projects")
    },

    getById: async (id) => {
        return await http.get(`/api/projects/${id}`)
    },

    create: async (name) => {
        return await http.post("/api/projects", { name })
    }
}