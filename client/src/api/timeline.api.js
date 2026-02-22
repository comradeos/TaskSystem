import { http } from "./http"

export const timelineApi = {

    getTaskHistory: async (taskId) => {
        return await http.get(`/api/tasks/${taskId}/history`)
    },

    getProjectHistory: async (projectId) => {
        return await http.get(`/api/projects/${projectId}/history`)
    }
}