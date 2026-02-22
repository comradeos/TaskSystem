import { http } from "./http"

export const commentsApi = {
    getByTask: async (taskId) => {
        return await http.get(`/api/comments/${taskId}`)
    },

    create: async (data) => {
        return await http.post("/api/comments", data)
    }
}