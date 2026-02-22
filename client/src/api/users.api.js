import { http } from "./http"

export const usersApi = {

    getAll: async () => {
        return await http.get("/api/users")
    },

    create: async (user) => {
        return await http.post("/api/users", user)
    }
}