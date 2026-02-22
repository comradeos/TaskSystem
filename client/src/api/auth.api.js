import { http } from "./http"

export const authApi = {
    login: async (login, password) => {
        return await http.post("/api/auth/login", {
            login,
            password
        })
    },

    logout: async () => {
        return await http.post("/api/auth/logout")
    }
}